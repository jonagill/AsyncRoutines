using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AsyncRoutines
{
#if UNITY_2022_1_OR_NEWER
    public class AsyncRoutineDebuggerWindow : EditorWindow
    {
        private const float WINDOW_PADDING = 5f;
        private const float ELEMENT_PADDING = 2f;
        private const float ELEMENT_MARGINS = 5f;
        
        [MenuItem("Window/Async Routines/Debugger Window", false, 9999)]
        public static void ShowEditorForDefaultRunner()
        {
            ShowEditor(AsyncRoutineRunner.DefaultRunner);
        }

        public static void ShowEditor(AsyncRoutineRunner runner)
        {
            var window = GetWindow<AsyncRoutineDebuggerWindow>();
            window.titleContent = new GUIContent("Async Routine Debugger");
            window.runner = runner;
        }

        private AsyncRoutineRunner runner;
        private HelpBox warningBox;
        private Label activeRoutineLabel;
        private ListView updatePhasePane;
        private ListView rightPane;

        private UpdatePhase selectedUpdatePhase;

        private void OnEnable()
        {
            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
        }
        
        private void Update()
        {
            if (runner == null)
            {
                Close();
                return;
            }

            if (Application.isPlaying && FindObjectOfType<AsyncRoutineUpdater>() == null)
            {
                warningBox.text = $"No active {nameof(AsyncRoutineUpdater)} component was found. Your routines will not be updated!";
                warningBox.style.display = DisplayStyle.Flex;
            }
            else
            {
                warningBox.style.display = DisplayStyle.None;
            }
            

            activeRoutineLabel.text = $"Total routines: {runner.Count}";
            updatePhasePane.RefreshItems();
            rightPane.RefreshItems();
        }

        public void CreateGUI()
        {
            SetPadding(rootVisualElement, WINDOW_PADDING);
            
            warningBox = new HelpBox("", HelpBoxMessageType.Warning);
            SetMargins(warningBox, ELEMENT_MARGINS);
            rootVisualElement.Add(warningBox);

            var infoBox = new Box();
            SetMargins(infoBox, ELEMENT_MARGINS);
            SetPadding(infoBox, ELEMENT_PADDING);
            activeRoutineLabel = new Label("Total routines: 0");
            infoBox.Add(activeRoutineLabel);
            rootVisualElement.Add(infoBox);

            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            rootVisualElement.Add(splitView);

            updatePhasePane = new ListView();
            SetPadding(updatePhasePane, ELEMENT_PADDING);
            
            splitView.Add(updatePhasePane);
            updatePhasePane.makeItem = () =>
            {
                var label = new Label();
                SetPadding(label, ELEMENT_PADDING);
                label.style.justifyContent = Justify.Center;
                return label;
            };
            updatePhasePane.bindItem = (item, index) =>
            {
                var updatePhase = AsyncRoutineRunner.UpdatePhases[index];
                var activeRoutineCount = runner != null ? runner.EditorGetCount(updatePhase) : 0;
                (item as Label).text = $"{AsyncRoutineRunner.UpdatePhases[index].ToString()} ({activeRoutineCount})";
            };
            updatePhasePane.onSelectionChange += (items) =>
            {
                selectedUpdatePhase = AsyncRoutineRunner.UpdatePhases[updatePhasePane.selectedIndex];
            };
            updatePhasePane.itemsSource = AsyncRoutineRunner.UpdatePhases;

            rightPane = new ListView();
            splitView.Add(rightPane);
            rightPane.makeItem = () => new Label();
            rightPane.bindItem = (item, index) => { (item as Label).text = Random.value.ToString(); };
        }

        private static void SetPadding(VisualElement visualElement, float padding)
        {
            visualElement.style.paddingTop = padding;
            visualElement.style.paddingBottom = padding;
            visualElement.style.paddingLeft = padding;
            visualElement.style.paddingRight = padding;
        }
        
        private static void SetMargins(VisualElement visualElement, float margins)
        {
            visualElement.style.marginTop = margins;
            visualElement.style.marginBottom = margins;
            visualElement.style.marginLeft = margins;
            visualElement.style.marginRight = margins;
        }
    }
#endif
}