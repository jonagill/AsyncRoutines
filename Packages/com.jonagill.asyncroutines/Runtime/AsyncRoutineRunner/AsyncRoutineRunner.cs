using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace AsyncRoutines
{
    public partial class AsyncRoutineRunner : IAsyncRoutineRunner
    {
        private struct CurrentRunnerScope : IDisposable
        {
            private AsyncRoutineRunner runner;
            public CurrentRunnerScope(AsyncRoutineRunner runner)
            {
                this.runner = runner;
                CurrentRunnerStack.Push(runner);
            }
            
            public void Dispose()
            {
                Assert.AreEqual(runner, CurrentRunnerStack.Peek());
                CurrentRunnerStack.Pop();
            }
        }
        
        internal static bool UnitTestsRunning = false;
        
        public static readonly UpdatePhase[] UpdatePhases = (UpdatePhase[]) Enum.GetValues(typeof(UpdatePhase));
        public static readonly AsyncRoutineRunner DefaultRunner = new AsyncRoutineRunner();
        
        /// <summary>
        /// Stack containing any AsyncRoutineRunners that are currently stepping their own routines
        /// </summary>
        private static readonly Stack<AsyncRoutineRunner> CurrentRunnerStack = new Stack<AsyncRoutineRunner>();

        /// <summary>
        /// Returns the AsyncRoutineRunner currently stepping its routines or the default runner if no runner is currently stepping.
        /// </summary>
        public static AsyncRoutineRunner CurrentOrDefaultRunner => CurrentRunnerStack.Count > 0 ? CurrentRunnerStack.Peek() : DefaultRunner;

        private AsyncRoutineQueue[] queues;
        private bool isDisposed;
        
        public event Action OnRoutineStarted;

        public int Count
        {
            get
            {
                var count = 0;
                foreach (var queue in queues)
                {
                    count += queue.Count;
                }

                return count;
            }
        }

        public AsyncRoutineRunner()
        {
            queues = new AsyncRoutineQueue[UpdatePhases.Length];
            for (var i = 0; i < UpdatePhases.Length; i++)
            {
                queues[i] = new AsyncRoutineQueue(UpdatePhases[i]);
            }

            SceneManager.sceneUnloaded += OnSceneUnloaded;
            
#if UNITY_EDITOR
            // Don't allow routines to run across play mode state changes
            // It causes too many issues
            EditorApplication.playModeStateChanged += OnEditorPlayModeStateChanged;
#endif
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            
            ClearAllRoutines();
            
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnEditorPlayModeStateChanged;
#endif

            isDisposed = true;
        }
        
        public IAsyncRoutinePromise Run(IEnumerator<IYieldInstruction> routine)
        {
            return Run(null, routine);
        }

        public IAsyncRoutinePromise Run(Behaviour context, IEnumerator<IYieldInstruction> routine)
        {
            using (new CurrentRunnerScope(this))
            {
                var asyncRoutine = new AsyncRoutine(context, routine);
                var firstYield = asyncRoutine.Step();

                if (firstYield != null)
                {
                    GetQueueForPhase(firstYield.UpdatePhase).InsertRoutine(asyncRoutine);
                }

                OnRoutineStarted?.Invoke();

                return asyncRoutine.Promise;
            }
        }

        /// <summary>
        /// Step any routines that were yielding until the given phase.
        /// Should generally only be called once per frame per phase.
        /// </summary>
        public void StepRoutines(UpdatePhase updatePhase)
        {
            using (new CurrentRunnerScope(this))
            {
                var queue = GetQueueForPhase( updatePhase );
                queue.Step();
                FlushQueuedRoutines( queue );
            }
        }

        /// <summary>
        /// Clear any routines that have had their context destroyed and will never resume.
        /// Called automatically when changing scenes, but can be called at other times
        /// when lots of objects have been destroyed to aid garbage collection.
        /// </summary>
        public void ClearExpiredRoutines()
        {
            using (new CurrentRunnerScope(this))
            {
                foreach (var queue in queues)
                {
                    queue.ClearExpiredCoroutines();
                }
            }
        }

        public void ClearAllRoutines()
        {
            using (new CurrentRunnerScope(this))
            {
                for (var i = 0; i < UpdatePhases.Length; i++)
                {
                    // Destroy the previous queues and build new ones
                    queues[i].Dispose();
                    queues[i] = new AsyncRoutineQueue(UpdatePhases[i]);
                }
            }
        }

        private void FlushQueuedRoutines(AsyncRoutineQueue queue)
        {
            foreach (var buffer in queue.QueuedInsertBuffers)
            {
                if (buffer.Routines.Count == 0)
                {
                    continue;
                }
                
                var destinationQueue = GetQueueForPhase(buffer.DestinationPhase);
                destinationQueue.InsertRoutines(buffer.Routines, buffer.DestinationSubQueue);
                buffer.Routines.Clear();
            }
        }
        
        private void OnSceneUnloaded(Scene scene)
        {
            ClearExpiredRoutines();
        }
        
#if UNITY_EDITOR
        private void OnEditorPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode || 
                change == PlayModeStateChange.ExitingPlayMode)
            {
                ClearAllRoutines();
            } 
        }        
#endif
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private AsyncRoutineQueue GetQueueForPhase(UpdatePhase updatePhase)
        {
            return queues[(int)updatePhase];
        }
        
                
#if UNITY_EDITOR
        public int EditorGetCount(UpdatePhase updatePhase)
        {
            return GetQueueForPhase(updatePhase).Count;
        } 
        
        public void EditorGetRoutines(
            UpdatePhase updatePhase, 
            out IEnumerable<IAsyncRoutine> nonDeferredRoutines, 
            out IEnumerable<IAsyncRoutine> deferredRoutines, 
            out IEnumerable<IAsyncRoutine> deferredRealTimeRoutines)
        {
            var queue = GetQueueForPhase(updatePhase);
            nonDeferredRoutines = queue.EditorGetRoutines(AsyncRoutineQueue.SubQueueType.NonDeferred);
            deferredRoutines = queue.EditorGetRoutines(AsyncRoutineQueue.SubQueueType.Deferred);
            deferredRealTimeRoutines = queue.EditorGetRoutines(AsyncRoutineQueue.SubQueueType.DeferredRealTime);
        } 
#endif
    }
}

