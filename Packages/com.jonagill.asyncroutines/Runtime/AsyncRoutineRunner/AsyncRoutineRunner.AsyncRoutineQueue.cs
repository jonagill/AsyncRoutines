using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Promises;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Profiling;

namespace AsyncRoutines
{
    public partial class AsyncRoutineRunner
    {
        private class AsyncRoutineQueue : IDisposable
        {
            private enum ModificationType
            {
                None,
                Remove,
                Reinsert
            }

            private enum UpdateType
            {
                Full,
                RemoveOnly
            }

            public enum SubQueueType
            {
                NonDeferred,
                Deferred,
                DeferredRealTime
            }

            /// <summary>
            /// Message type for passing coroutines that have left one queue and
            /// need to be inserted back into another queue. 
            /// </summary>
            public struct QueuedInsertBuffer
            {
                public UpdatePhase DestinationPhase;
                public SubQueueType DestinationSubQueue;
                public List<AsyncRoutine> Routines;
            }

            private static readonly SubQueueType[] SubQueueTypes = (SubQueueType[]) Enum.GetValues(typeof(SubQueueType));
            
            private readonly UpdatePhase updatePhase;
            private readonly string profilerStepTag;

            // Scratch buffer for inserting coroutines one at a time
            private readonly AsyncRoutine[] singleRoutineScratchBuffer = new AsyncRoutine[1];

            // The list of currently coroutines that are not deferred by a yield instruction
            private readonly List<AsyncRoutine> nonDeferredCoroutines = new List<AsyncRoutine>();
            private Stack<int> nonDeferredCoroutineEmptyIndices = new Stack<int>();

            // The list of coroutines deferred by a yield instruction
            private readonly List<AsyncRoutine> deferredCoroutines = new List<AsyncRoutine>();
            private Stack<int> deferredCoroutineEmptyIndices = new Stack<int>();

            // The list of coroutines deferred in real time by a yield instruction
            private readonly List<AsyncRoutine> deferredRealTimeCoroutines = new List<AsyncRoutine>();
            private Stack<int> deferredRealTimeCoroutineEmptyIndices = new Stack<int>();

            private bool isUpdating;
            private bool isDisposed;
            
            public QueuedInsertBuffer[,] QueuedInsertBuffers { get; }
            public int Count
            {
                get
                {
                    int count = 0;
                    count += nonDeferredCoroutines.Count - nonDeferredCoroutineEmptyIndices.Count;
                    count += deferredCoroutines.Count - deferredCoroutineEmptyIndices.Count;
                    count += deferredRealTimeCoroutines.Count - deferredRealTimeCoroutineEmptyIndices.Count;

                    foreach (var buffer in QueuedInsertBuffers)
                    {
                        count += buffer.Routines.Count;
                    }

                    return count;
                }
            }

            public AsyncRoutineQueue(UpdatePhase updatePhase)
            {
                this.updatePhase = updatePhase;
                profilerStepTag = $"AsyncRoutineQueue.Step ({updatePhase})";

                QueuedInsertBuffers = new QueuedInsertBuffer[UpdatePhases.Length, SubQueueTypes.Length];
                for (var i = 0; i < UpdatePhases.Length; i++)
                {
                    for (var j = 0; j < SubQueueTypes.Length; j++)
                    {
                        QueuedInsertBuffers[i, j] = new QueuedInsertBuffer()
                        {
                            DestinationPhase = UpdatePhases[i],
                            DestinationSubQueue = SubQueueTypes[j],
                            Routines = new List<AsyncRoutine>()
                        };
                    }
                }
            }

            public void InsertRoutine(AsyncRoutine routine)
            {
                Assert.IsNotNull(routine);

                var currentYield = routine.CurrentYieldInstruction;
                Assert.IsNotNull(currentYield);
                Assert.AreEqual(updatePhase, currentYield.UpdatePhase);

                singleRoutineScratchBuffer[0] = routine;
                InsertRoutines(singleRoutineScratchBuffer, GetSubQueueFromYieldInstruction(currentYield));
                singleRoutineScratchBuffer[0] = null;
            }

            public void InsertRoutines(IList<AsyncRoutine> routines, SubQueueType subQueue)
            {
                if (isUpdating)
                {
                    QueueInsert(routines);
                    return;
                }

                GetBuffersForSubQueue(subQueue, out var routineBuffer, out var emptyIndices);
                for (var i = 0; i < routines.Count; i++)
                {
                    var routine = routines[i];
                    if (emptyIndices.Count > 0)
                    {
                        var index = emptyIndices.Pop();
                        routineBuffer[index] = routine;
                    }
                    else
                    {
                        routineBuffer.Add(routine);
                    }
                }
            }

            public void Step()
            {
                isUpdating = true;
                try
                {
                    Profiler.BeginSample(profilerStepTag);
                    UpdateRoutines(SubQueueType.NonDeferred, UpdateType.Full);
                    UpdateRoutines(SubQueueType.Deferred, UpdateType.Full, AsyncYield.TimeProvider.Time);
                    UpdateRoutines(SubQueueType.DeferredRealTime, UpdateType.Full, AsyncYield.TimeProvider.RealTimeSinceStartup);
                    Profiler.EndSample();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    isUpdating = false;
                }
            }
            
            public void ClearExpiredCoroutines()
            {
                UpdateRoutines(SubQueueType.NonDeferred, UpdateType.RemoveOnly);
                UpdateRoutines(SubQueueType.Deferred, UpdateType.RemoveOnly);
                UpdateRoutines(SubQueueType.DeferredRealTime, UpdateType.RemoveOnly);
                
                foreach (var queue in QueuedInsertBuffers)
                {
                    var routines = queue.Routines;
                    for (var i = routines.Count-1; i >= 0; i--)
                    {
                        if (routines[i].ShouldCancel())
                        {
                            routines[i].Cancel();
                            routines.RemoveAt(i);
                        }
                    }
                }
            }

            public void Dispose()
            {
                if (isDisposed)
                {
                    return;
                }
                
                CancelAndClearRoutines(nonDeferredCoroutines);
                CancelAndClearRoutines(deferredCoroutines);
                CancelAndClearRoutines(deferredRealTimeCoroutines);

                foreach (var buffer in QueuedInsertBuffers)
                {
                    CancelAndClearRoutines(buffer.Routines);
                }
                
                isDisposed = true;
            }

            private void QueueInsert(IList<AsyncRoutine> routines)
            {
                for (var i = 0; i < routines.Count; i++)
                {
                    QueueInsert(routines[i]);
                }
            }
            
            private void QueueInsert(AsyncRoutine routine)
            {
                Assert.IsNotNull(routine);

                var currentYield = routine.CurrentYieldInstruction;
                Assert.IsNotNull(currentYield);

                var subQueueType = GetSubQueueFromYieldInstruction(currentYield);
                QueuedInsertBuffers[(int)currentYield.UpdatePhase, (int)subQueueType]
                    .Routines
                    .Add(routine);
            }

            private void CancelAndClearRoutines(IList<AsyncRoutine> routines)
            {
                for (var i = 0; i < routines.Count; i++)
                {
                    var routine = routines[i];
                    if (routine != null && !routine.IsCanceled)
                    {
                        routine.Cancel();
                    }
                }
                routines.Clear();
            }

            private void UpdateRoutines(SubQueueType subQueue, UpdateType updateType, float currentTime = -1f)
            {
                GetBuffersForSubQueue(subQueue, out var routineBuffer, out var emptyIndices);
                var routinesCount = routineBuffer.Count;
                if (emptyIndices.Count == routinesCount)
                {
                    // If all our indices are empty, there's nothing to update
                    return;
                }

                for (var i = 0; i < routinesCount; i++)
                {
                    var routine = routineBuffer[i];
                    if (routine == null)
                    {
                        // This was an empty index
                        continue;
                    }
                    
                    if (routine.IsCanceled)
                    {
                        // This routine is canceled -- remove it from the buffer
                        routineBuffer[i] = null;
                        emptyIndices.Push(i);
                        continue;
                    }

                    if (routine.IsPaused)
                    {
                        // This routine is paused -- don't operate on it for now
                        continue;
                    }

                    bool isDeferred;
                    switch (subQueue)
                    {
                        case SubQueueType.Deferred:
                            isDeferred = routine.CurrentYieldInstruction.DeferPollUntilTime > currentTime;
                            break;
                        case SubQueueType.DeferredRealTime:
                            isDeferred = routine.CurrentYieldInstruction.DeferPollUntilRealTime > currentTime;
                            break;
                        default:
                            isDeferred = false;
                            break;
                    }

                    if (isDeferred)
                    {
                        // This routine is deferred and should not be checked until we reach its current time
                        continue;
                    }

                    ModificationType requiredModification;
                    if (updateType == UpdateType.RemoveOnly)
                    {
                        // Check if this routine has expired and should be canceled and removed
                        // We are intentionally doing this after checking deferral so that we don't
                        // have to do an expensive lifetime check for every deferred routine ever frame
                        if (routine.ShouldCancel())
                        {
                            routine.Cancel();
                            requiredModification = ModificationType.Remove;
                        }
                        else
                        {
                            requiredModification = ModificationType.None;
                        }
                    }
                    else
                    {
                        requiredModification = StepRoutine(routine);
                    }

                    if (requiredModification == ModificationType.Remove ||
                        requiredModification == ModificationType.Reinsert)
                    {
                        routineBuffer[i] = null;
                        emptyIndices.Push(i);
                    }

                    if (requiredModification == ModificationType.Reinsert)
                    {
                        QueueInsert(routine);
                    }
                }
            }

            private ModificationType StepRoutine(AsyncRoutine routine)
            {
                Assert.IsNotNull(routine);
                
                var currentYield = routine.CurrentYieldInstruction;
                Assert.IsNotNull(currentYield);
                
                var requiredModification = ModificationType.None;

                var currentDeferUntil = currentYield.DeferPollUntilTime;
                var currentDeferUntilRealtime = currentYield.DeferPollUntilRealTime;
                
                Assert.IsTrue(currentDeferUntil <= AsyncYield.TimeProvider.Time, "Cannot step routine before its specified defer time.`");
                Assert.IsTrue(currentDeferUntilRealtime <= AsyncYield.TimeProvider.RealTimeSinceStartup, "Cannot step routine before its specified deferred time.");

                if (currentYield.Poll())
                {
                    Profiler.BeginSample(routine.Name);
                    var newYield = routine.Step();
                    Profiler.EndSample();

                    if (newYield == null)
                    {
                        // The routine has completed (or failed). Either way, remove it
                        requiredModification = ModificationType.Remove;
                    }
                    else
                    {
                        if (newYield.UpdatePhase != updatePhase ||
                            newYield.DeferPollUntilTime != currentDeferUntil ||
                            newYield.DeferPollUntilRealTime != currentDeferUntilRealtime)
                        {
                            // The routine has changed update phases or defer times
                            // Reinsert it into the correct place in its new queue
                            requiredModification = ModificationType.Reinsert;
                        }
                    }
                }

                return requiredModification;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private SubQueueType GetSubQueueFromYieldInstruction(IYieldInstruction yield)
            {
                if (yield.DeferPollUntilTime > 0.0f)
                {
                    return SubQueueType.Deferred;
                }
                
                if (yield.DeferPollUntilRealTime > 0.0f)
                {
                    return SubQueueType.DeferredRealTime;
                }

                return SubQueueType.NonDeferred;
            }
            
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void GetBuffersForSubQueue(SubQueueType subQueue, out IList<AsyncRoutine> routineBuffer, out Stack<int> emptyIndices)
            {
                switch (subQueue)
                {
                    case SubQueueType.NonDeferred:
                        routineBuffer = nonDeferredCoroutines;
                        emptyIndices = nonDeferredCoroutineEmptyIndices;
                        break;
                    case SubQueueType.Deferred:
                        routineBuffer = deferredCoroutines;
                        emptyIndices = deferredCoroutineEmptyIndices;
                        break;
                    case SubQueueType.DeferredRealTime:
                        routineBuffer = deferredRealTimeCoroutines;
                        emptyIndices = deferredRealTimeCoroutineEmptyIndices;
                        break;
                    default:
                        throw new NotImplementedException($"Unexpected subqueue type {subQueue}");
                }
            }
        }
    }
}