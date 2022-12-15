using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace AsyncRoutines
{
    public partial class AsyncRoutineRunner
    {
        private sealed class AsyncRoutine
        {
            private const string RELEASE_NAME = "AsyncRoutine";
            private static readonly IYieldInstruction DefaultNullYield = AsyncYield.NextUpdate;
            
            public IEnumerator<IYieldInstruction> Coroutine { get; private set; }
            public IYieldInstruction CurrentYieldInstruction { get; private set; }
            public IAsyncRoutinePromise Promise => routinePromise;

            private readonly AsyncRoutinePromise routinePromise;
            private readonly Behaviour context;
            private readonly bool hasContext;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            public string Name { get; private set; }
#else
            public string Name => RELEASE_NAME;
#endif
            public bool IsCanceled { get; private set; }

            public bool IsPaused
            {
                get
                {
                    // A routine without a context cannot be paused
                    if (!hasContext)
                    {
                        return false;
                    }

                    // A routine where the context has been destroyed cannot be paused
                    if (context == null)
                    {
                        return false;
                    }

                    // A routine with a valid context is paused when the context is disabled
                    return !context.isActiveAndEnabled;
                }
            }

            public AsyncRoutine(Behaviour context, IEnumerator<IYieldInstruction> coroutine)
            {
                Assert.IsNotNull(coroutine);

                this.Coroutine = coroutine;
                this.CurrentYieldInstruction = null;
                this.routinePromise = new AsyncRoutinePromise();
                this.context = context;
                this.hasContext = context != null;

                routinePromise.Canceled(() => IsCanceled = true);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                var coroutineName = coroutine.GetType().Name;
                if (hasContext)
                {
                    Name = $"AsyncRoutine ({context.GetType().Name} {context.name}) {coroutineName}";
                }
                else
                {
                    Name = $"AsyncRoutine {coroutineName}";
                }
#endif
            }

            public IYieldInstruction Step()
            {
                Assert.IsFalse(IsCanceled, "You cannot call Step() on a canceled AsyncRoutine.");

                try
                {
                    // Try to step the coroutine
                    IEnumerator<IYieldInstruction> coroutine = Coroutine;
                    if (coroutine.MoveNext())
                    {
                        var currentYieldInstruction = coroutine.Current;
                        if (currentYieldInstruction != null)
                        {
                            CurrentYieldInstruction = currentYieldInstruction;
                        }
                        else
                        {
                            // If someone yielded null, use our default yield instruction
                            CurrentYieldInstruction = DefaultNullYield;
                        }

                        return CurrentYieldInstruction;
                    }
                }
                catch (Exception e)
                {
                    // The coroutine threw an exception.
                    // Log the exception and report the the coroutine failed to complete.
                    if (!UnitTestsRunning)
                    {
                        Debug.LogException(e);
                    }
                    
                    routinePromise.Throw(e);

                    return null;
                }

                try
                {
                    // If we get to here, to coroutine has completed.
                    // Report the completion to any listeners
                    CurrentYieldInstruction = null;
                    if (routinePromise.IsPending)
                    {
                        routinePromise.Complete();
                    }
                    else
                    {
                        Debug.LogError($"Coroutine completed, but its promise is no longer pending: {this}");
                    }
                }
                catch (Exception e)
                {
                    // Catch exceptions thrown when completing the promise to avoid external systems breaking
                    // the async routine runner
                    Debug.LogException(e);
                }

                return null;
            }

            public bool ShouldCancel()
            {
                if (hasContext && context == null)
                {
                    // Cancel the routine if our context has been destroyed since we were created
                    return true;
                }

                return false;
            }

            public void Cancel()
            {
                Assert.IsFalse(IsCanceled, "You cannot cancel an already canceled AsyncRoutine.");

                try
                {
                    routinePromise.Cancel();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            public override string ToString()
            {
                var yieldLabel = CurrentYieldInstruction != null ? $" ({CurrentYieldInstruction})" : string.Empty;
                var pausedLabel = IsPaused ? " Paused" : string.Empty;
                var stateLabel = string.Empty;
                if (routinePromise.HasSucceeded)
                {
                    stateLabel = " (Completed)";
                }
                else if (routinePromise.IsCanceled)
                {
                    stateLabel = " (Canceled)";
                }
                else if (routinePromise.HasException)
                {
                    Exception exception = null;
                    routinePromise.Catch(e => exception = e);
                    stateLabel = $" (Exception: {exception})";
                }

                return $"[{Name}{yieldLabel}{stateLabel}{pausedLabel}]";
            }
        }
    }
}