namespace AsyncRoutines
{
    /// <summary>
    /// Yield instruction that defers execution only until the next time it is checked
    /// </summary>
    public sealed class YieldNextFrame : IYieldInstruction
    {
        public UpdatePhase UpdatePhase { get; }

        public float DeferPollUntilTime => 0.0f;
        public float DeferPollUntilRealTime => 0.0f;
        
        public YieldNextFrame(UpdatePhase updatePhase)
        {
            UpdatePhase = updatePhase;
        }
        
        public bool Poll() => true;

        public override string ToString()
        {
            return $"{nameof(YieldNextFrame)} ({UpdatePhase})";
        }
    }
}