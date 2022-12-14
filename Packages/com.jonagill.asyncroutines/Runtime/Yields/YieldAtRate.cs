using UnityEngine.Assertions;

namespace AsyncRoutines
{
    /// <summary>
    /// Yield instruction that can be yielded repeatedly to attempt to resume the utilizing routine
    /// a given number of times per second. Can randomize the start time to distribute the workload
    /// if multiple yields are kicked off on the same frame.
    /// </summary>
    public sealed class YieldAtRate : IYieldInstruction
    {
        private readonly float secondsBetweenResumes;
        private readonly float startTimeOffset;
        private readonly bool useRealTime;
        
        private float firstResumeTime;
        private int timesResumed;

        public UpdatePhase UpdatePhase { get; }
        public float TargetRateHz { get; }
        
        public float DeferPollUntilTime { get; private set; }
        public float DeferPollUntilRealTime { get; private set; }
        
        public YieldAtRate(
            float targetRateHz, 
            UpdatePhase updatePhase = UpdatePhase.Update, 
            bool randomizeStartTime = true, 
            bool useRealTime = true)
        {
            Assert.IsTrue(targetRateHz > 0.0f, "Target update rate must be greater than zero.");
            UpdatePhase = updatePhase;
            TargetRateHz = targetRateHz;
            
            DeferPollUntilTime = 0f;
            DeferPollUntilRealTime = 0f;
            
            secondsBetweenResumes = 1.0f / targetRateHz;
            startTimeOffset = randomizeStartTime ? UnityEngine.Random.value * -secondsBetweenResumes : 0f;
            timesResumed = 0;
            this.useRealTime = useRealTime;

            // Set the first time at which we should resume
            var timeProvider = AsyncYield.TimeProvider;
            var time = useRealTime ? timeProvider.RealTimeSinceStartup : timeProvider.Time;
            firstResumeTime = time + secondsBetweenResumes + startTimeOffset;
            
            UpdateDeferTime();
        }

        public bool Poll()
        {
            timesResumed++;
            UpdateDeferTime();
            return true;
        }

        private void UpdateDeferTime()
        {
            // Set our defer time based on the initial resume time and the number of times we have resumed so far
            // Do this rather than just adding secondsBetweenResumes each time to avoid compounding errors
            // over the course of the routine's life
            var nextPollTime = firstResumeTime + (timesResumed * secondsBetweenResumes);
            if (useRealTime)
            {
                DeferPollUntilRealTime = nextPollTime;
            }
            else
            {
                DeferPollUntilTime = nextPollTime;
            }
        }

        public override string ToString()
        {
            return $"{nameof(YieldAtRate)} ({UpdatePhase}, {TargetRateHz}hz)";
        }
    }
}