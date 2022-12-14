namespace AsyncRoutines 
{
    public interface ITimeProvider
    {
        float Time { get; }
        float DeltaTime { get; }
        float FixedDeltaTime { get; }
        float FrameCount { get; }
        float RealTimeSinceStartup { get; }
    }
}
