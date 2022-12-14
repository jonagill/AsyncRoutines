using System;

namespace AsyncRoutines
{
#if PROMISES
    public interface IRoutinePromise : IYieldInstruction, IDisposable, Promises.ICancelablePromise {}
#else
    public interface IRoutinePromise : IYieldInstruction, IDisposable {}
#endif
}
