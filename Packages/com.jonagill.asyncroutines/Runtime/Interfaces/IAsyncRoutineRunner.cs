using System;
using System.Collections.Generic;
using UnityEngine;

namespace AsyncRoutines
{
    public interface IAsyncRoutineRunner : IDisposable
    {
        /// <summary>
        /// Starts an async routine with no context object. This routine will run until it completes,
        /// it is manually stopped, or the runner itself is destroyed.
        /// </summary>
        IAsyncRoutinePromise Run(IEnumerator<IYieldInstruction> routine);
        
        /// <summary>
        /// Starts an async routine with the given Behaviour as its context.
        /// This routine will run until it completes, it is manually stopped, the context is destroyed, or the runner itself is destroyed.
        /// If the context or its host GameObject are disabled, the routine will pause until they are re-enabled.
        /// </summary>
        IAsyncRoutinePromise Run(Behaviour context, IEnumerator<IYieldInstruction> routine);

        /// <summary>
        /// Immediately scan through all active routines and remove any routines that have had their host object destroyed.
        /// This can be called after destroying a large number of objects to clean up any dangling references and allow GC to run.
        /// </summary>
        void ClearExpiredRoutines();
    }
}