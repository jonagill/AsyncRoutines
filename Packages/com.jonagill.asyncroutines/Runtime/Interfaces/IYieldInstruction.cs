namespace AsyncRoutines 
{
    /// <summary>
    /// The shared interface for all yield instructions
    /// that yield the execution of a running coroutine
    /// </summary>
    public interface IYieldInstruction
    {
        /// <summary>
        /// Which phase of the frame update the blocked routine will resume on
        /// </summary>
        UpdatePhase UpdatePhase { get; }

        /// <summary>
        /// Checks if the yield has expired. Should only be called once per frame, as it may update state internally
        /// </summary>
        /// <returns>Returns true if this yield has expired and the blocked routine can resume</returns>
        bool Poll();
        
        /// <summary>
        /// The soonest timestamp that the yield instruction could feasibly be completed by.
        /// Poll() will not be called until this timestamp has been reached.
        /// </summary>
        float DeferPollUntilTime { get; }
        
        /// <summary>
        /// The soonest real timestamp that the yield instruction could feasibly be completed by.
        /// Poll() will not be called until this timestamp has been reached.
        /// </summary>
        float DeferPollUntilRealTime { get; }
    }
}
