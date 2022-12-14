namespace AsyncRoutines 
{
    public interface ITimeProvider
    {
        float Time { get; }
        float DeltaTime { get; }
        float FixedDeltaTime { get; }
        int FrameCount { get; }
        float RealTimeSinceStartup { get; }
    }
}
