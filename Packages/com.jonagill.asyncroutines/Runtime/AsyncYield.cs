﻿using System;
using System.Collections.Generic;
using UnityEngine.Assertions;

namespace AsyncRoutines
{
    public static class AsyncYield
    {
        static AsyncYield()
        {
            // Always default to the play mode time provider being available
            defaultTimeProvider = new PlayModeTimeProvider();
            PushTimeProvider(defaultTimeProvider);
        }
        
        public static IYieldInstruction NextUpdate { get; } = new YieldNextFrame(UpdatePhase.Update);
        public static IYieldInstruction NextPostUpdate { get; } = new YieldNextFrame(UpdatePhase.PostUpdate);
        public static IYieldInstruction NextFixedUpdate { get; } = new YieldNextFrame(UpdatePhase.FixedUpdate);
        public static IYieldInstruction NextLateUpdate { get; } = new YieldNextFrame(UpdatePhase.LateUpdate);
        public static IYieldInstruction NextEndOfFrame { get; } = new YieldNextFrame(UpdatePhase.EndOfFrame);
        public static IYieldInstruction NextPreRender { get; } = new YieldNextFrame(UpdatePhase.PreRender);

        public static IYieldInstruction NextFrame(UpdatePhase updatePhase)
        {
            switch (updatePhase)
            {
                case UpdatePhase.Update:
                    return NextUpdate;
                case UpdatePhase.PostUpdate:
                    return NextPostUpdate;
                case UpdatePhase.FixedUpdate:
                    return NextFixedUpdate;
                case UpdatePhase.LateUpdate:
                    return NextLateUpdate;
                case UpdatePhase.PreRender:
                    return NextPreRender;
                case UpdatePhase.EndOfFrame:
                    return NextEndOfFrame;
                default:
                    throw new ArgumentException();
            }
        }

        public static IYieldInstruction Wait(float seconds, UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new YieldForSeconds(seconds, updatePhase, false);
        }
        
        public static IYieldInstruction WaitRealtime(float seconds, UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new YieldForSeconds(seconds, updatePhase, true);
        }
        
        public static IYieldInstruction WaitFrames(int frames, UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new YieldForFrames(frames, updatePhase);
        }

        public static IYieldInstruction Until(Func<bool> condition, UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new YieldUntil(condition, updatePhase);
        }

        public static IYieldInstruction WaitForPromise(Promises.IPromise promise, UpdatePhase updatePhase = UpdatePhase.Update)
        {
            return new YieldForPromise(promise, updatePhase);
        }
        
        #region Time Provider
        
        /// <summary>
        /// The current time provider used by yield instructions.
        /// </summary>
        public static ITimeProvider TimeProvider => activeTimeProvider;

        private static ITimeProvider defaultTimeProvider;
        private static ITimeProvider activeTimeProvider;
        private static readonly List<ITimeProvider> timeProviderStack = new List<ITimeProvider>();

        /// <summary>
        /// Override the time provider used by yield instructions.
        /// </summary>
        public static void PushTimeProvider(ITimeProvider timeProvider)
        {
            Assert.IsNotNull(timeProvider);
            timeProviderStack.Add(timeProvider);
            activeTimeProvider = timeProvider;
        }
        
        public static void RemoveTimeProvider(ITimeProvider timeProvider)
        {
            Assert.AreNotEqual(defaultTimeProvider, timeProvider);
            if (timeProviderStack.Count > 1)
            {
                timeProviderStack.Remove(timeProvider);
                activeTimeProvider = timeProviderStack[timeProviderStack.Count - 1];
            }
        }

        #endregion
    }
}