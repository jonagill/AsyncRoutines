using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsyncRoutines
{
    public static class BehaviourExt
    {
        public static IAsyncRoutinePromise RunRoutine(this Behaviour behaviour, IEnumerator<IYieldInstruction> routine)
        {
            return AsyncRoutine.RunRoutine(behaviour, routine);
        }

        public static IDisposable QueueUpdate(
            this Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(context, updateCallback, updatePhase);
        }
        
        public static IDisposable QueueUpdate(
            this Behaviour context,
            Action<float> updateCallback,
            float targetRateHz,
            UpdatePhase updatePhase = UpdatePhase.Update,
            bool randomizeStartTime = true)
        {
            return new AsyncUpdateRoutine(context, targetRateHz, updateCallback, updatePhase, randomizeStartTime);
        }
        
        public static IDisposable QueueUpdate30Hz(
            this Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(context, 30f, updateCallback, updatePhase, true);
        }
        
        public static IDisposable QueueUpdate10Hz(
            this Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(context, 10f, updateCallback, updatePhase, true);
        }
        
        public static IDisposable QueueUpdate1Hz(
            this Behaviour context,
            Action<float> updateCallback,
            UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new AsyncUpdateRoutine(context, 1f, updateCallback, updatePhase, true);
        }
    }
}
