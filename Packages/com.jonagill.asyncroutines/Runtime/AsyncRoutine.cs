using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Assertions;

[assembly: InternalsVisibleTo("AsyncRoutines.Tests")]

namespace AsyncRoutines
{
    public static class AsyncRoutine
    {
        #region Running Routines
        
        public static IAsyncRoutinePromise RunRoutine(Behaviour behaviour, IEnumerator<IYieldInstruction> routine)
        {
            return AsyncRoutineRunner.DefaultRunner.Run(behaviour, routine);
        }
        
        public static IAsyncRoutinePromise RunRoutine(IEnumerator<IYieldInstruction> routine)
        {
            return AsyncRoutineRunner.DefaultRunner.Run(null, routine);
        }
        
        #endregion
        
        #region Queuing Updates
        
        public static IDisposable QueueUpdate(
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(null, updateCallback, updatePhase);
        }
        
        public static IDisposable QueueUpdate(
            Action<float> updateCallback,
            float targetRateHz,
            UpdatePhase updatePhase = UpdatePhase.Update,
            bool randomizeStartTime = true)
        {
            return new AsyncUpdateRoutine(null, targetRateHz, updateCallback, updatePhase, randomizeStartTime);
        }
        
        public static IDisposable QueueUpdate30Hz(
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(null, 30f, updateCallback, updatePhase, true);
        }
        
        public static IDisposable QueueUpdate10Hz(
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(null, 10f, updateCallback, updatePhase, true);
        }
        
        public static IDisposable QueueUpdate1Hz(
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(null, 1f, updateCallback, updatePhase, true);
        }

        
        public static IDisposable QueueUpdate(
            Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(context, updateCallback, updatePhase);
        }
        
        public static IDisposable QueueUpdate(
            Behaviour context,
            Action<float> updateCallback,
            float targetRateHz,
            UpdatePhase updatePhase = UpdatePhase.Update,
            bool randomizeStartTime = true)
        {
            return new AsyncUpdateRoutine(context, targetRateHz, updateCallback, updatePhase, randomizeStartTime);
        }
        
        public static IDisposable QueueUpdate30Hz(
            Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(context, 30f, updateCallback, updatePhase, true);
        }
        
        public static IDisposable QueueUpdate10Hz(
            Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(context, 10f, updateCallback, updatePhase, true);
        }
        
        public static IDisposable QueueUpdate1Hz(
            Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(context, 1f, updateCallback, updatePhase, true);
        }
        
        #endregion
        
        #region Next Frame Callbacks

        public static IAsyncRoutinePromise InvokeNextUpdate(
            MonoBehaviour context, 
            Action callback)
        {
            return InvokeNextFrame(context, callback, UpdatePhase.Update);
        }
        
        public static IAsyncRoutinePromise InvokeNextPostUpdate(
            MonoBehaviour context, 
            Action callback)
        {
            return InvokeNextFrame(context, callback, UpdatePhase.PostUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeNextLateUpdate(
            MonoBehaviour context, 
            Action callback)
        {
            return InvokeNextFrame(context, callback, UpdatePhase.LateUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeNextFixedUpdate(
            MonoBehaviour context, 
            Action callback)
        {
            return InvokeNextFrame(context, callback, UpdatePhase.FixedUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeNextPreRender(
            MonoBehaviour context, 
            Action callback)
        {
            return InvokeNextFrame(context, callback, UpdatePhase.PreRender);
        }
        
        public static IAsyncRoutinePromise InvokeNextEndOfFrame(
            MonoBehaviour context, 
            Action callback)
        {
            return InvokeNextFrame(context, callback, UpdatePhase.EndOfFrame);
        }
        
        public static IAsyncRoutinePromise InvokeNextUpdate(Action callback)
        {
            return InvokeNextFrame(null, callback, UpdatePhase.Update);
        }
        
        public static IAsyncRoutinePromise InvokeNextPostUpdate(Action callback)
        {
            return InvokeNextFrame(null, callback, UpdatePhase.PostUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeNextLateUpdate(Action callback)
        {
            return InvokeNextFrame(null, callback, UpdatePhase.LateUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeNextFixedUpdate(Action callback)
        {
            return InvokeNextFrame(null, callback, UpdatePhase.FixedUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeNextPreRender(Action callback)
        {
            return InvokeNextFrame(null, callback, UpdatePhase.PreRender);
        }
        
        public static IAsyncRoutinePromise InvokeNextEndOfFrame(Action callback)
        {
            return InvokeNextFrame(null, callback, UpdatePhase.EndOfFrame);
        }

        public static IAsyncRoutinePromise InvokeNextFrame(
            MonoBehaviour context,
            Action callback,
            UpdatePhase updatePhase)
        {
            Assert.IsNotNull(callback);
            return AsyncRoutineRunner.DefaultRunner.Run(context, InvokeNextFrameRoutine(callback, updatePhase));
        }
        
        private static IEnumerator<IYieldInstruction> InvokeNextFrameRoutine(Action callback, UpdatePhase updatePhase)
        {
            yield return AsyncYield.NextFrame(updatePhase);
            callback.Invoke();
        }
        
        #endregion
        
        #region Delayed Callbacks Callbacks

        public static IAsyncRoutinePromise InvokeInUpdate(
            MonoBehaviour context,
            float delay,
            Action callback)
        {
            return InvokeAfterDelay(context, delay, callback, UpdatePhase.Update);
        }
        
        public static IAsyncRoutinePromise InvokeInPostUpdate(
            MonoBehaviour context,
            float delay,
            Action callback)
        {
            return InvokeAfterDelay(context, delay, callback, UpdatePhase.PostUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeInLateUpdate(
            MonoBehaviour context,
            float delay, 
            Action callback)
        {
            return InvokeAfterDelay(context, delay, callback, UpdatePhase.LateUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeInFixedUpdate(
            MonoBehaviour context,
            float delay, 
            Action callback)
        {
            return InvokeAfterDelay(context, delay, callback, UpdatePhase.FixedUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeInPreRender(
            MonoBehaviour context,
            float delay, 
            Action callback)
        {
            return InvokeAfterDelay(context, delay, callback, UpdatePhase.PreRender);
        }
        
        public static IAsyncRoutinePromise InvokeInEndOfFrame(
            MonoBehaviour context,
            float delay, 
            Action callback)
        {
            return InvokeAfterDelay(context, delay, callback, UpdatePhase.EndOfFrame);
        }
        
        public static IAsyncRoutinePromise InvokeInUpdate(float delay, Action callback)
        {
            return InvokeAfterDelay(null, delay, callback, UpdatePhase.Update);
        }
        
        public static IAsyncRoutinePromise InvokeInPostUpdate(float delay, Action callback)
        {
            return InvokeAfterDelay(null, delay, callback, UpdatePhase.PostUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeInLateUpdate(float delay, Action callback)
        {
            return InvokeAfterDelay(null, delay, callback, UpdatePhase.LateUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeInFixedUpdate(float delay, Action callback)
        {
            return InvokeAfterDelay(null, delay, callback, UpdatePhase.FixedUpdate);
        }
        
        public static IAsyncRoutinePromise InvokeInPreRender(float delay, Action callback)
        {
            return InvokeAfterDelay(null, delay, callback, UpdatePhase.PreRender);
        }
        
        public static IAsyncRoutinePromise InvokeInEndOfFrame(float delay, Action callback)
        {
            return InvokeAfterDelay(null, delay, callback, UpdatePhase.EndOfFrame);
        }

        public static IAsyncRoutinePromise InvokeAfterDelay(
            MonoBehaviour context,
            float delay,
            Action callback,
            UpdatePhase updatePhase)
        {
            Assert.IsNotNull(callback);
            return AsyncRoutineRunner.DefaultRunner.Run(context, InvokeAfterDelayRoutine(delay, callback, updatePhase));
        }
        
        private static IEnumerator<IYieldInstruction> InvokeAfterDelayRoutine(float delay, Action callback, UpdatePhase updatePhase)
        {
            yield return AsyncYield.Wait(delay, updatePhase);
            callback.Invoke();
        }
        
        #endregion
    }
}