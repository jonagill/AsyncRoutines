using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsyncRoutines
{
    public static class AsyncRoutine
    {
        public static IAsyncRoutinePromise RunRoutine(Behaviour behaviour, IEnumerator<IYieldInstruction> routine)
        {
            return AsyncRoutineRunner.DefaultRunner.Run(behaviour, routine);
        }
        
        public static IAsyncRoutinePromise RunRoutine(IEnumerator<IYieldInstruction> routine)
        {
            return AsyncRoutineRunner.DefaultRunner.Run(null, routine);
        }
    }
}