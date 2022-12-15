namespace AsyncRoutines
{
    public class PlayModeTimeProvider : ITimeProvider
    {
        public float Time => UnityEngine.Time.time;
        public float DeltaTime => UnityEngine.Time.deltaTime;
        public float FixedDeltaTime => UnityEngine.Time.fixedDeltaTime;
        public int FrameCount => UnityEngine.Time.frameCount;
        public float RealTimeSinceStartup => UnityEngine.Time.realtimeSinceStartup;
    }
}
