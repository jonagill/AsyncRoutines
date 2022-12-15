// Uncomment to freeze the editor while the game is paused, allowing use of the UI Debugger
//#define DEBUGGING_UI

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
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

        private class RoutineElement : VisualElement
        {
            public readonly Box box;
            public readonly VisualElement topRow;
            public readonly VisualElement bottomRow;
            public readonly Label nameLabel;
            public readonly Button cancelButton;
            public readonly ObjectField objectField;

            public Action Clicked;

            public RoutineElement()
            {
                box = new Box();
                topRow = new VisualElement();
                bottomRow = new VisualElement();
                cancelButton = new Button();
                nameLabel = new Label();
                objectField = new ObjectField();

                this.Add(box);
                
                box.Add(topRow);
                box.Add(bottomRow);
                box.style.borderBottomColor = new Color(0f, 0f, 0f, .5f);
                box.style.flexShrink = 0f;
                SetMargins(box, ELEMENT_MARGINS);
                SetPadding(box, ELEMENT_PADDING);
                
                topRow.Add(nameLabel);
                topRow.Add(cancelButton);
                topRow.style.flexDirection = FlexDirection.Row;
                topRow.style.justifyContent = Justify.SpaceBetween;
                
                bottomRow.Add(objectField);

                nameLabel.style.flexShrink = 1f;
                nameLabel.style.overflow = Overflow.Hidden;
                
                cancelButton.text = "Cancel";
                cancelButton.clicked += OnClick;

                objectField.label = "Context";
                objectField.allowSceneObjects = true;
                objectField.objectType = typeof(Behaviour);
            }

            public void OnClick()
            {
                Clicked?.Invoke();
            }
        }

        private AsyncRoutineRunner runner;
        private HelpBox warningBox;
        private Label activeRoutineLabel;
        private ListView updatePhasePane;
        private ListView activeRoutinePane;

        private UpdatePhase selectedUpdatePhase;
        private readonly List<AsyncRoutineRunner.IAsyncRoutine> routines = new List<AsyncRoutineRunner.IAsyncRoutine>();

        private void OnEnable()
        {
            EditorApplication.update += Update;
        }

        private void OnDisable()
        {
            EditorApplication.update -= Update;
            routines.Clear();
        }
        
        private void Update()
        {
            if (runner == null)
            {
                Close();
                return;
            }
            
#if DEBUGGING_UI
            if (EditorApplication.isPaused)
            {
                return;
            }
#endif
            
            if (Application.isPlaying && FindObjectOfType<AsyncRoutineUpdater>() == null)
            {
                warningBox.text = $"No active {nameof(AsyncRoutineUpdater)} component was found. Your routines will not be updated!";
                warningBox.style.display = DisplayStyle.Flex;
            }
            else
            {
                warningBox.style.display = DisplayStyle.None;
            }

            runner.EditorGetRoutines(
                selectedUpdatePhase,
                out var nonDeferredRoutines, 
                out var deferredRoutines,
                out var deferredRealtimeRoutines);
            
            routines.Clear();
            routines.AddRange(nonDeferredRoutines);
            routines.AddRange(deferredRoutines);
            routines.AddRange(deferredRealtimeRoutines);

            activeRoutineLabel.text = $"Total routines: {runner.Count}";
            updatePhasePane.RefreshItems();
            activeRoutinePane.RefreshItems();
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
            updatePhasePane.selectedIndex = 0;

            activeRoutinePane = new ListView();
            splitView.Add(activeRoutinePane);
            activeRoutinePane.fixedItemHeight = 60f;
            activeRoutinePane.makeItem = () => new RoutineElement();
            activeRoutinePane.bindItem = (item, index) =>
            {
                var routineElement = (RoutineElement) item;
                var routine = routines[index];
                
                var routineLabel = routine.ToString();
                routineElement.nameLabel.text = routineLabel;
                routineElement.nameLabel.tooltip = routineLabel;
                routineElement.Clicked = () => CancelRoutine(index);

                if (routine.Context != null)
                {
                    routineElement.objectField.SetValueWithoutNotify(routine.Context);
                    routineElement.objectField.style.display = DisplayStyle.Flex;
                }
                else
                {
                    routineElement.objectField.style.display = DisplayStyle.None;
                }
            };
            activeRoutinePane.itemsSource = routines;
        }

        private void CancelRoutine(int index)
        {
            if (index >= 0 && index < routines.Count)
            {
                routines[index].Cancel();
            }
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