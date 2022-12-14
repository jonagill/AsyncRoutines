namespace AsyncRoutines
{
    public class MockTimeProvider : ITimeProvider
    {
        public float Time { get; set; }
        public float DeltaTime { get; set; }
        public float FixedDeltaTime { get; set; }
        public int FrameCount { get; set; }
        public float RealTimeSinceStartup { get; set; }
    }    
}
