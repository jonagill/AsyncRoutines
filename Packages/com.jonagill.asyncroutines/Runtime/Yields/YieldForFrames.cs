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

        private readonly int deferUntilFrame;

        public YieldForFrames(int frames, UpdatePhase updatePhase = UpdatePhase.Update)
        {
            Assert.IsTrue(frames >= 0f, "Cannot yield for negative frames.");
            UpdatePhase = updatePhase;
            deferUntilFrame = AsyncYield.TimeProvider.FrameCount + frames;
        }

        public bool Poll()
        {
            return AsyncYield.TimeProvider.FrameCount >= deferUntilFrame;
        }

        public override string ToString()
        {
            return $"{nameof(YieldForFrames)} ({UpdatePhase}, {deferUntilFrame})";
        }
    }
}