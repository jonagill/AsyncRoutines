using System.Collections.Generic;
using UnityEngine;

namespace AsyncRoutines
{
    public static class BehaviourExt
    {
        public static IAsyncRoutinePromise RunRoutine(this Behaviour behaviour, IEnumerator<IYieldInstruction> routine)
        {
            return AsyncRoutine.RunRoutine(behaviour, routine);
        }
    }
}
