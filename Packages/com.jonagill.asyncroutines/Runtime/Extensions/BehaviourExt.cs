using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsyncRoutines
{
    public static class BehaviourExt
    {
        #region Running Routines
        
        public static IAsyncRoutinePromise RunRoutine(this Behaviour behaviour, IEnumerator<IYieldInstruction> routine)
        {
            return AsyncRoutine.RunRoutine(behaviour, routine);
        }

        /// <summary>
        /// Cancel the existing routine if one exists, then start a new routine
        /// </summary>
        public static IAsyncRoutinePromise RunSoloRoutine(
            this Behaviour behaviour, 
            ref IAsyncRoutinePromise routinePromise,
            IEnumerator<IYieldInstruction> routine)
        {
            return AsyncRoutine.RunSoloRoutine(ref routinePromise, routine);
        }
        
        #endregion
        
        #region Queuing Updates

        public static IDisposable QueueUpdate(
            this Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return AsyncRoutine.QueueUpdate(context, updateCallback, updatePhase);
        }
        
        public static IDisposable QueueUpdate(
            this Behaviour context,
            Action<float> updateCallback,
            float targetRateHz,
            UpdatePhase updatePhase = UpdatePhase.Update,
            bool randomizeStartTime = true)
        {
            return AsyncRoutine.QueueUpdate(context, updateCallback, targetRateHz, updatePhase, randomizeStartTime);
        }
        
        public static IDisposable QueueUpdate30Hz(
            this Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return AsyncRoutine.QueueUpdate30Hz(context, updateCallback, updatePhase);
        }
        
        public static IDisposable QueueUpdate10Hz(
            this Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return AsyncRoutine.QueueUpdate10Hz(context, updateCallback, updatePhase);
        }
        
        public static IDisposable QueueUpdate1Hz(
            this Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return AsyncRoutine.QueueUpdate1Hz(context, updateCallback, updatePhase);
        }
        
        #endregion
        
        #region Next Frame Callbacks
        
        public static IAsyncRoutinePromise InvokeNextUpdate(
            this MonoBehaviour context, 
            Action callback)
        {
            return AsyncRoutine.InvokeNextUpdate(context, callback);
        }
        
        public static IAsyncRoutinePromise InvokeNextPostUpdate(
            this MonoBehaviour context, 
            Action callback)
        {
            return AsyncRoutine.InvokeNextPostUpdate(context, callback);
        }
        
        public static IAsyncRoutinePromise InvokeNextLateUpdate(
            this MonoBehaviour context, 
            Action callback)
        {
            return AsyncRoutine.InvokeNextLateUpdate(context, callback);
        }
        
        public static IAsyncRoutinePromise InvokeNextFixedUpdate(
            this MonoBehaviour context, 
            Action callback)
        {
            return AsyncRoutine.InvokeNextFixedUpdate(context, callback);
        }
        
        public static IAsyncRoutinePromise InvokeNextPreRender(
            this MonoBehaviour context, 
            Action callback)
        {
            return AsyncRoutine.InvokeNextPreRender(context, callback);
        }
        
        public static IAsyncRoutinePromise InvokeNextEndOfFrame(
            this MonoBehaviour context, 
            Action callback)
        {
            return AsyncRoutine.InvokeNextEndOfFrame(context, callback);
        }
        
        public static IAsyncRoutinePromise InvokeNextFrame(
            this MonoBehaviour context,
            Action callback,
            UpdatePhase updatePhase)
        {
            return AsyncRoutine.InvokeNextFrame(context, callback, updatePhase);
        }
        
        private static IEnumerator<IYieldInstruction> InvokeNextFrameRoutine(Action callback, UpdatePhase updatePhase)
        {
            yield return AsyncYield.NextFrame(updatePhase);
            callback.Invoke();
        }
        
        #endregion
        
        #region Delayed Callbacks
        
        public static IAsyncRoutinePromise InvokeInUpdate(
            this MonoBehaviour context,
            float delay,
            Action callback)
        {
            return AsyncRoutine.InvokeInUpdate(context, delay, callback);
        }
        
        public static IAsyncRoutinePromise InvokeInPostUpdate(
            this MonoBehaviour context,
            float delay,
            Action callback)
        {
            return AsyncRoutine.InvokeInPostUpdate(context, delay, callback);
        }
        
        public static IAsyncRoutinePromise InvokeInLateUpdate(
            this MonoBehaviour context,
            float delay, 
            Action callback)
        {
            return AsyncRoutine.InvokeInLateUpdate(context, delay, callback);
        }
        
        public static IAsyncRoutinePromise InvokeInFixedUpdate(
            this MonoBehaviour context,
            float delay, 
            Action callback)
        {
            return AsyncRoutine.InvokeInFixedUpdate(context, delay, callback);
        }
        
        public static IAsyncRoutinePromise InvokeInPreRender(
            this MonoBehaviour context,
            float delay, 
            Action callback)
        {
            return AsyncRoutine.InvokeInPreRender(context, delay, callback);
        }
        
        public static IAsyncRoutinePromise InvokeInEndOfFrame(
            this MonoBehaviour context,
            float delay, 
            Action callback)
        {
            return AsyncRoutine.InvokeInEndOfFrame(context, delay, callback);
        }

        public static IAsyncRoutinePromise InvokeAfterDelay(
            this MonoBehaviour context,
            float delay,
            Action callback,
            UpdatePhase updatePhase)
        {
            return AsyncRoutine.InvokeAfterDelay(context, delay, callback, updatePhase);
        }
        
        #endregion
    }
}
