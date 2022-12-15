using UnityEngine;
using UnityEngine.Assertions;

namespace AsyncRoutines
{
    /// <summary>
    /// Component that handles updating all AsyncRoutines.
    /// Should exist in your scene in order for AsyncRoutines to update properly.
    /// </summary>
    public class AsyncRoutineUpdater : MonoBehaviour
    {
        public AsyncRoutineRunner Runner => _customRunner ?? AsyncRoutineRunner.DefaultRunner;
        private AsyncRoutineRunner _customRunner = null;
        
        public void SetCustomRunner(AsyncRoutineRunner runner)
        {
            Assert.IsNotNull(runner);
            _customRunner = runner;
        }

        private void OnEnable()
        {
            CustomUpdatePhases.PostUpdatePhase.OnPostUpdate += OnPostUpdate;
            CustomUpdatePhases.PreRenderPhase.OnPreRender += OnPreRender;
            CustomUpdatePhases.EndOfFramePhase.OnEndOfFrame += OnEndOfFrame;
        }

        private void OnDisable()
        {
            CustomUpdatePhases.PostUpdatePhase.OnPostUpdate -= OnPostUpdate;
            CustomUpdatePhases.PreRenderPhase.OnPreRender -= OnPreRender;   
            CustomUpdatePhases.EndOfFramePhase.OnEndOfFrame -= OnEndOfFrame;
        }

        private void Update()
        {
            Runner.StepRoutines(UpdatePhase.Update);
        }

        private void OnPostUpdate()
        {
            Runner.StepRoutines(UpdatePhase.PostUpdate);
        }
        
        private void FixedUpdate()
        {
            Runner.StepRoutines(UpdatePhase.FixedUpdate);
        }
        
        private void LateUpdate()
        {
            Runner.StepRoutines(UpdatePhase.LateUpdate);
        }

        private void OnPreRender()
        {
            Runner.StepRoutines(UpdatePhase.PreRender);
        }
        
        private void OnEndOfFrame()
        {
            Runner.StepRoutines(UpdatePhase.EndOfFrame);
        }
    }
}
