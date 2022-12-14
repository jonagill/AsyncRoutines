using System;
using UnityEngine.Assertions;

namespace AsyncRoutines
{
    /// <summary>
    /// Yield instruction that defers execution for the given number of frames
    /// </summary>
    public sealed class YieldForFrames : IYieldInstruction
    {
        public UpdatePhase UpdatePhase { get; }
        public float DeferPollUntilTime { get; }
        public float DeferPollUntilRealTime { get; }

        private int framesRemaining;

        public YieldForFrames(int frames, UpdatePhase updatePhase = UpdatePhase.Update)
        {
            Assert.IsTrue(frames >= 0f, "Cannot yield for negative frames.");
            UpdatePhase = updatePhase;
            framesRemaining = frames;
        }

        public bool Poll()
        {
            framesRemaining--;
            return framesRemaining <= 0;
        }

        public override string ToString()
        {
            return $"{nameof(YieldForFrames)} ({UpdatePhase}, {framesRemaining})";
        }
    }
}