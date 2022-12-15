using UnityEditor;

namespace AsyncRoutines
{
    [InitializeOnLoad]
    public static class EditorAsyncRoutineUpdater
    {
        private static EditorTimeProvider timeProvider = new EditorTimeProvider();
        private static bool isActive;
        
        static EditorAsyncRoutineUpdater()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Activate();
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
            {
                Deactivate();
            }
            else if (change == PlayModeStateChange.ExitingPlayMode)
            {
                Activate();
            }
        }

        private static void Activate()
        {
            if (isActive)
            {
                return;
            }

            EditorApplication.update += OnEditorUpdate;
            AsyncRoutineRunner.DefaultRunner.OnRoutineStarted += EditorApplication.QueuePlayerLoopUpdate;
            AsyncYield.PushTimeProvider(timeProvider);
            isActive = true;
        }

        private static void Deactivate()
        {
            if (!isActive)
            {
                return;
            }

            EditorApplication.update -= OnEditorUpdate;
            AsyncRoutineRunner.DefaultRunner.OnRoutineStarted -= EditorApplication.QueuePlayerLoopUpdate;
            AsyncYield.RemoveTimeProvider(timeProvider);
            isActive = false;
        }

        private static void OnEditorUpdate()
        {
            var runner = AsyncRoutineRunner.DefaultRunner;

            // We don't get distinct frame timing phases in editor, so just update all the phases in order
            foreach (var updatePhase in AsyncRoutineRunner.UpdatePhases)
            {
                runner.StepRoutines(updatePhase);
            }
            
            // Keep updating the editor as long as there are routines running
            if (runner.Count > 0)
            {
                EditorApplication.QueuePlayerLoopUpdate();
            }
        }
    }
}
