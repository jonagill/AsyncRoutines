using System;
using UnityEngine.Assertions;

namespace AsyncRoutines
{
    /// <summary>
    /// Yield instruction that defers execution until the given function returns true
    /// </summary>
    public sealed class YieldUntil : IYieldInstruction
    {
        public UpdatePhase UpdatePhase { get; }

        public float DeferPollUntilTime => 0.0f;
        public float DeferPollUntilRealTime => 0.0f;

        private readonly Func<bool> condition;
        
        public YieldUntil(Func<bool> condition, UpdatePhase updatePhase = UpdatePhase.Update)
        {
            Assert.IsNotNull(condition);
            
            UpdatePhase = updatePhase;
            this.condition = condition;
        }
        
        public bool Poll() => condition();

        public override string ToString()
        {
            return $"{nameof(YieldUntil)} ({UpdatePhase})";
        }
    }
}