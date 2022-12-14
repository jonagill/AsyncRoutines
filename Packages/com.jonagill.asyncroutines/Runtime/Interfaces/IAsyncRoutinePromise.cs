using System;

namespace AsyncRoutines
{
    public interface IAsyncRoutinePromise : IYieldInstruction, IDisposable, Promises.ICancelablePromise {}
}
