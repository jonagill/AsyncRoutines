using Promises;

namespace AsyncRoutines
{
    public partial class AsyncRoutineRunner
    {
        private class AsyncRoutinePromise : CancelablePromise, IAsyncRoutinePromise
        {
            public UpdatePhase UpdatePhase { get; set; }

            public float DeferPollUntilTime => 0.0f;
            public float DeferPollUntilRealTime => 0.0f;

            public bool Poll() => !IsPending;
            public void Dispose() => Cancel();
        }
    }
}
