using System;
using UnityEngine.Assertions;

namespace AsyncRoutines
{
    /// <summary>
    /// Yield instruction that defers execution for the given number of seconds
    /// </summary>
    public sealed class YieldForSeconds : IYieldInstruction
    {
        public UpdatePhase UpdatePhase { get; }
        public float DeferPollUntilTime { get; }
        public float DeferPollUntilRealTime { get; }

        public YieldForSeconds(float seconds, UpdatePhase updatePhase = UpdatePhase.Update, bool useRealTime = false)
        {
            Assert.IsTrue(seconds >= 0f, "Cannot yield for negative seconds.");
            UpdatePhase = updatePhase;
            
            if (useRealTime)
            {
                DeferPollUntilTime = 0f;
                DeferPollUntilRealTime = AsyncYield.TimeProvider.RealTimeSinceStartup + seconds;
            }
            else
            {
                DeferPollUntilTime = AsyncYield.TimeProvider.Time + seconds;
                DeferPollUntilRealTime = 0f;
            }
        }

        public bool Poll() => true;

        public override string ToString()
        {
            var isRealtime = DeferPollUntilRealTime > 0f;
            var timeStampText = isRealtime ? $"{DeferPollUntilRealTime}s (Realtime)" : $"{DeferPollUntilTime}s";
            return $"{nameof(YieldForSeconds)} ({UpdatePhase}, {timeStampText})";
        }
    }
}