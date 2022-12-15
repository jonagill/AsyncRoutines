using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AsyncRoutines.Tests
{
    public class AsyncRoutineRunnerTests
    {
        private AsyncRoutineRunner runner;
        private MockTimeProvider timeProvider;
        private Behaviour context;
        
        [SetUp]
        public void Setup()
        {
            AsyncRoutineRunner.UnitTestsRunning = true;
            
            runner = new AsyncRoutineRunner();
            timeProvider = new MockTimeProvider();
            AsyncYield.PushTimeProvider(timeProvider);

            context = new GameObject("Context").AddComponent<Camera>();
        }

        [TearDown]
        public void Teardown()
        {
            AsyncRoutineRunner.UnitTestsRunning = false;
            
            runner.Dispose();
            AsyncYield.RemoveTimeProvider(timeProvider);
            
            if (context != null)
            {
                Object.DestroyImmediate(context.gameObject);   
            }
        }

        [Test]
        public void CanCompleteSimpleRoutine()
        {
            IEnumerator<IYieldInstruction> WaitOneFrame()
            {
                yield return AsyncYield.NextUpdate;
            }

            var routinePromise = runner.Run(WaitOneFrame());
            Assert.IsTrue(routinePromise.IsPending);
            
            runner.StepRoutines(UpdatePhase.Update);
            
            Assert.IsTrue(routinePromise.HasSucceeded);
        }
        
        [Test]
        public void CanTickRoutineAcrossMultipleFrames()
        {
            IEnumerator<IYieldInstruction> WaitThreeFrames()
            {
                yield return AsyncYield.NextUpdate;
                yield return AsyncYield.NextUpdate;
                yield return AsyncYield.NextUpdate;
            }

            var routinePromise = runner.Run(WaitThreeFrames());
            Assert.IsTrue(routinePromise.IsPending);
            
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsPending);
            
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsPending);
            
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.HasSucceeded);
        }

        
        [Test]
        public void YieldNullTreatedAsNextUpdate()
        {
            IEnumerator<IYieldInstruction> WaitThreeFrames()
            {
                yield return null;
                yield return null;
                yield return null;
            }

            var routinePromise = runner.Run(WaitThreeFrames());
            Assert.IsTrue(routinePromise.IsPending);
            
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsPending);
            
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsPending);
            
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.HasSucceeded);
        }

        [Test]
        public void RoutinesCanChangeQueue()
        {
            IEnumerator<IYieldInstruction> SwapQueues()
            {
                yield return AsyncYield.NextUpdate;
                yield return AsyncYield.NextEndOfFrame;
            }

            var routinePromise = runner.Run(SwapQueues());
            Assert.IsTrue(routinePromise.IsPending);
            
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsPending);
            
            // This should not have advanced anything, since our
            // routine is now on the EndOfFrame queue
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsPending);
            
            runner.StepRoutines(UpdatePhase.EndOfFrame);
            Assert.IsTrue(routinePromise.HasSucceeded);
        }
        
        [Test]
        public void RoutinesCanBeDeferred()
        {
            IEnumerator<IYieldInstruction> Wait1Second()
            {
                yield return AsyncYield.Wait(1f);
            }

            var routinePromise = runner.Run(Wait1Second());
            Assert.IsTrue(routinePromise.IsPending);
            
            // This should not have advanced anything, since our
            // routine is deferred for 1s
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsPending);

            timeProvider.Time = 1.5f;
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.HasSucceeded);
        }
        
        [Test]
        public void RoutinesCanBeDeferredRealTime()
        {
            IEnumerator<IYieldInstruction> Wait1Second()
            {
                yield return AsyncYield.WaitRealtime(1f);
            }

            var routinePromise = runner.Run(Wait1Second());
            Assert.IsTrue(routinePromise.IsPending);
            
            // This should not have advanced anything, since our
            // routine is deferred for 1s
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsPending);

            timeProvider.RealTimeSinceStartup = 1.5f;
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.HasSucceeded);
        }
        
        [Test]
        public void RoutinesCanBeExternallyCanceled()
        {
            IEnumerator<IYieldInstruction> WaitOneFrame()
            {
                yield return AsyncYield.NextUpdate;
            }

            var routinePromise = runner.Run(WaitOneFrame());
            Assert.IsTrue(routinePromise.IsPending);
            
            routinePromise.Cancel();
            
            Assert.IsTrue(routinePromise.IsCanceled);
            
            // The runner will not remove the routine until the next step
            Assert.AreEqual(1, runner.Count);
            runner.StepRoutines(UpdatePhase.Update);
            
            Assert.AreEqual(0, runner.Count);
        }
        
        [Test]
        public void RoutinesPauseWhileContextDisabled()
        {
            int counter = 0;
            IEnumerator<IYieldInstruction> Count()
            {
                while (true)
                {
                    counter++;
                    yield return AsyncYield.NextUpdate;
                }
            }

            runner.Run(context, Count());
            Assert.AreEqual(1, counter);
            
            runner.StepRoutines(UpdatePhase.Update);
            Assert.AreEqual(2, counter);

            // Disabling the context behaviour should pause the routine
            context.enabled = false;
            runner.StepRoutines(UpdatePhase.Update);
            Assert.AreEqual(2, counter);

            // Enabling the context behaviour should resume the routine
            context.enabled = true;
            runner.StepRoutines(UpdatePhase.Update);
            Assert.AreEqual(3, counter);
            
            // Disabling the context GameObject should pause the routine
            context.gameObject.SetActive(false);
            runner.StepRoutines(UpdatePhase.Update);
            Assert.AreEqual(3, counter);
            
            // Enabling the context GameObject should resume the routine
            context.gameObject.SetActive(true);
            runner.StepRoutines(UpdatePhase.Update);
            Assert.AreEqual(4, counter);
        }
        
        [Test]
        public void RoutinesCancelWhenContextDestroyed()
        {
            IEnumerator<IYieldInstruction> Loop()
            {
                while (true)
                {
                    yield return AsyncYield.NextUpdate;
                }
            }

            var routinePromise = runner.Run(context, Loop());
            
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsPending);
            Assert.AreEqual(1, runner.Count);

            Object.DestroyImmediate(context.gameObject);
            
            // The routine will not be cleared up until we step again
            Assert.IsTrue(routinePromise.IsPending);
            Assert.AreEqual(1, runner.Count);

            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsCanceled);
            Assert.AreEqual(0, runner.Count);
        }
        
        [Test]
        public void DeferredRoutinesCancelWhenContextDestroyed()
        {
            IEnumerator<IYieldInstruction> Wait1s()
            {
                yield return AsyncYield.Wait(1f);
            }

            var routinePromise = runner.Run(context, Wait1s());
            
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsPending);
            Assert.AreEqual(1, runner.Count);

            Object.DestroyImmediate(context.gameObject);
            
            // Nothing should have changed since we are still deferred
            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsPending);
            Assert.AreEqual(1, runner.Count);

            timeProvider.Time = 1.5f;

            runner.StepRoutines(UpdatePhase.Update);
            Assert.IsTrue(routinePromise.IsCanceled);
            Assert.AreEqual(0, runner.Count);
        }
        
        [Test]
        public void RoutineExceptionCancelsRoutine()
        {
            IEnumerator<IYieldInstruction> ThrowIn1Frame()
            {
                yield return AsyncYield.NextUpdate;
                throw new Exception("Failure");
            }
            
            var routinePromise = runner.Run(ThrowIn1Frame());
            
            Assert.IsTrue(routinePromise.IsPending);
            
            runner.StepRoutines(UpdatePhase.Update);
            
            Assert.IsTrue(routinePromise.HasException);
        }
        
        [Test]
        public void RoutineThatImmediatelyCompletesDoesntQueue()
        {
            IEnumerator<IYieldInstruction> Break()
            {
                yield break;
            }
            
            var routinePromise = runner.Run(Break());

            Assert.IsTrue(routinePromise.HasSucceeded);
            Assert.AreEqual(0, runner.Count);
        }
    }
}