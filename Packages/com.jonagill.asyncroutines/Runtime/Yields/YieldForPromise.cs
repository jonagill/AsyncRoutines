using System;
using Promises;
using UnityEngine.Assertions;

namespace AsyncRoutines
{
    /// <summary>
    /// Yield instruction that defers execution until the given Promise is no longer pending
    /// </summary>
    public sealed class YieldForPromise : IYieldInstruction
    {
        public UpdatePhase UpdatePhase { get; }

        public float DeferPollUntilTime => 0.0f;
        public float DeferPollUntilRealTime => 0.0f;

        private readonly IPromise promise;
        
        public YieldForPromise(IPromise promise, UpdatePhase updatePhase = UpdatePhase.Update)
        {
            Assert.IsNotNull(promise);
            
            UpdatePhase = updatePhase;
            this.promise = promise;
        }
        
        public bool Poll() => !promise.IsPending;

        public override string ToString()
        {
            return $"{nameof(YieldForPromise)} ({UpdatePhase})";
        }
    }
}