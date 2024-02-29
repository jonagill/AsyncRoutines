using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace AsyncRoutines
{
    /// <summary>
    /// Wraps an update callback in a coroutine that invokes it repeatedly until the routine is cancelled.
    /// </summary>
    public class AsyncUpdateRoutine : IDisposable
    {
        private IAsyncRoutinePromise routine;
        private Behaviour context;
        private Action<float> updateCallback;
        private UpdatePhase updatePhase;
        private float targetRateHz;
        private bool randomizeStartTime;
        private IAsyncRoutineRunner runner;
        
        public AsyncUpdateRoutine(
            Action<float> updateCallback, 
            UpdatePhase updatePhase,
            IAsyncRoutineRunner runner = null) 
            : this(null, -1f, updateCallback, updatePhase, false, runner) { }

        public AsyncUpdateRoutine(
            Behaviour context, 
            Action<float> updateCallback, 
            UpdatePhase updatePhase,
            IAsyncRoutineRunner runner = null) 
            : this(context, -1f, updateCallback, updatePhase, false, runner) { }
        
        public AsyncUpdateRoutine(
            float updateFrequency,
            Action<float> updateCallback, 
            UpdatePhase updatePhase,
            bool randomizeStartTime = false,
            IAsyncRoutineRunner runner = null)
            : this(null, updateFrequency, updateCallback, updatePhase, randomizeStartTime, runner) { }

        public AsyncUpdateRoutine(
            Behaviour context,
            float targetRateHz,
            Action<float> updateCallback, 
            UpdatePhase updatePhase,
            bool randomizeStartTime = false,
            IAsyncRoutineRunner runner = null)
        {
            this.context = context;
            this.updateCallback = updateCallback;
            this.updatePhase = updatePhase;
            this.targetRateHz = targetRateHz;
            this.randomizeStartTime = randomizeStartTime;
            this.runner = runner ?? AsyncRoutineRunner.CurrentOrDefaultRunner;

            QueueRoutine();
        }
        
        private void QueueRoutine()
        {
            routine = runner.Run(context, UpdateRoutine());
            routine.Catch(OnException);
        }
        
        private IEnumerator<IYieldInstruction> UpdateRoutine()
        {
            IYieldInstruction yield;
            if (targetRateHz > 0f)
            {
                yield = new YieldAtRate(targetRateHz, updatePhase, randomizeStartTime);
            }
            else
            {
                yield = new YieldNextFrame(updatePhase);
            }
            
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (targetRateHz > Application.targetFrameRate && Application.targetFrameRate > 0)
            {
                Debug.LogWarning(
                    $"{nameof(AsyncUpdateRoutine)} created targeting an update rate of {targetRateHz}hz, which is higher than the current framerate of {Application.targetFrameRate} fps.");
            }

            var fpsLabel = targetRateHz > 0f ? string.Empty : $", {targetRateHz}hz";
            var profilerTag = $"[AsyncUpdateRoutine {updateCallback.Method.DeclaringType.Name}.{updateCallback.Method.Name} ({updatePhase}{fpsLabel})]";
#endif
            
            float lastUpdateTime = AsyncYield.TimeProvider.Time;
            while (true)
            {
                yield return yield;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Profiler.BeginSample(profilerTag);
                try
#endif
                {
                    var timeNow = AsyncYield.TimeProvider.Time;
                    var deltaTime = timeNow - lastUpdateTime;
                    updateCallback(deltaTime);
                    lastUpdateTime = timeNow;
                }
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                finally
                {

                    Profiler.EndSample();
                }
#endif
            }
        }

        private void OnException(Exception e)
        {
            IEnumerator<IYieldInstruction> RequeueInOneFrame()
            {
                yield return new YieldNextFrame(updatePhase);
                QueueRoutine();
            }

            // If an exception is thrown while updating,
            // we don't want to lose our entire update
            // Log the exception and requeue the routine next frame
            Debug.LogException(e);
            runner.Run(context, RequeueInOneFrame());
        }

        public void Dispose()
        {
            if (routine != null)
            {
                routine.CancelIfPending();
                routine = null;
            }
        }
    }
}