using UnityEditor;

namespace AsyncRoutines
{
    public class EditorTimeProvider : ITimeProvider
    {
        static EditorTimeProvider()
        {
            frameCount = 0;
            lastUpdateTime = -1f;
            deltaTime = -1f;

            EditorApplication.update += OnUpdate;
        }
        
        private static int frameCount;
        private static float lastUpdateTime;
        private static float deltaTime;

        private static void OnUpdate()
        {
            var time = (float) EditorApplication.timeSinceStartup;
            if (lastUpdateTime > 0f)
            {
                deltaTime = time - lastUpdateTime;
            }
            else
            {
                deltaTime = 0f;
            }
            
            lastUpdateTime = time;
            frameCount++;
        }

        public float Time => (float) EditorApplication.timeSinceStartup;
        public float DeltaTime => deltaTime;
        public float FixedDeltaTime => deltaTime;
        public float FrameCount => frameCount;
        public float RealTimeSinceStartup => (float) EditorApplication.timeSinceStartup;
    }
}
