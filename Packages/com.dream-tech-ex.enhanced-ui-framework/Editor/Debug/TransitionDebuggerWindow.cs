using System.Linq;
using UnityEngine;
using UnityEditor;
using EnhancedUI.Editor.Utilities;
using EnhancedUI.Utilities;

namespace EnhancedUI.Editor.Tools
{
    /// <summary>
    /// Transition debugger for frame-by-frame analysis
    /// </summary>
    public class TransitionDebuggerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showCompleted = true;
        private bool _showOngoing = true;

        // Throttled repaint
        private double _lastRepaintTime;
        private const double REPAINT_INTERVAL = 0.1; // 10 times per second

        [MenuItem("Tools/Enhanced UI/Debug Tools/Transition Debugger", false, 101)]
        public static void ShowWindow()
        {
            var window = GetWindow<TransitionDebuggerWindow>("Transition Debugger");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                DrawNotPlayingMessage();
                return;
            }

            var bridge = EditorBridge.Instance;
            if (bridge == null)
            {
                EditorGUILayout.HelpBox("EditorBridge not available.", MessageType.Warning);
                return;
            }

            // Header
            DrawHeader(bridge);

            GUILayout.Space(10);

            // Toolbar
            DrawToolbar();

            GUILayout.Space(10);

            // Transition list
            DrawTransitionList(bridge);

            // Throttled repaint (10 times/sec instead of unlimited)
            if (EditorApplication.timeSinceStartup - _lastRepaintTime > REPAINT_INTERVAL)
            {
                Repaint();
                _lastRepaintTime = EditorApplication.timeSinceStartup;
            }
        }

        private void DrawNotPlayingMessage()
        {
            EnhancedUIEditorGUIUtility.DrawWindowHeader("Transition Debugger", "Animation.Record");

            GUILayout.Space(20);

            EnhancedUIEditorGUIUtility.DrawMessageBox(
                "Transition Debugger is only available in Play Mode.\n\nEnter Play Mode to debug transitions in real-time.",
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("Enter Play Mode", EnhancedUIEditorStyles.PrimaryButton))
            {
                EditorApplication.isPlaying = true;
            }
        }

        private void DrawHeader(EditorBridge bridge)
        {
            using (new EditorGUILayout.HorizontalScope(EnhancedUIEditorStyles.HeaderBox))
            {
                var icon = EditorGUIUtility.IconContent("Animation.Record");
                GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));

                GUILayout.Label("Transition Debugger", EnhancedUIEditorStyles.HeaderLabel);

                GUILayout.FlexibleSpace();

                var completedCount = bridge.TransitionHistory.Count(t => t.IsCompleted);
                var ongoingCount = bridge.TransitionHistory.Count - completedCount;

                EnhancedUIEditorStyles.DrawStatusBadge($"{completedCount} Complete", EnhancedUIEditorStyles.SuccessColor, 100);
                GUILayout.Space(5);
                EnhancedUIEditorStyles.DrawStatusBadge($"{ongoingCount} Ongoing", EnhancedUIEditorStyles.WarningColor, 100);
            }
        }

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Label("Show:", GUILayout.Width(40));

                _showOngoing = GUILayout.Toggle(_showOngoing, "Ongoing", EditorStyles.toolbarButton, GUILayout.Width(70));
                _showCompleted = GUILayout.Toggle(_showCompleted, "Completed", EditorStyles.toolbarButton, GUILayout.Width(80));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Clear Transitions",
                        "Clear transition history?",
                        "Yes",
                        "No"))
                    {
                        EditorBridge.Instance.TransitionHistory.ToList().Clear();
                    }
                }
            }
        }

        private void DrawTransitionList(EditorBridge bridge)
        {
            var transitions = bridge.TransitionHistory
                .Where(t => (_showOngoing && !t.IsCompleted) || (_showCompleted && t.IsCompleted))
                .ToList();

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                // Header
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    EditorGUILayout.LabelField("Container", EditorStyles.boldLabel, GUILayout.Width(120));
                    EditorGUILayout.LabelField("Transition", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Type", EditorStyles.boldLabel, GUILayout.Width(100));
                    EditorGUILayout.LabelField("Duration", EditorStyles.boldLabel, GUILayout.Width(70));
                    EditorGUILayout.LabelField("Status", EditorStyles.boldLabel, GUILayout.Width(80));
                }

                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(300));

                if (transitions.Count == 0)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("No transitions to display", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    foreach (var transition in transitions.TakeLast(50))
                    {
                        DrawTransitionRow(transition);
                    }
                }

                EditorGUILayout.EndScrollView();

                // Stats
                if (transitions.Count > 0)
                {
                    GUILayout.Space(5);

                    var completedTransitions = transitions.Where(t => t.IsCompleted).ToList();

                    if (completedTransitions.Count > 0)
                    {
                        var avgDuration = completedTransitions.Average(t => t.Duration);
                        var minDuration = completedTransitions.Min(t => t.Duration);
                        var maxDuration = completedTransitions.Max(t => t.Duration);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EnhancedUIEditorGUIUtility.DrawStatBox("Avg Duration", $"{avgDuration:F3}s");
                            GUILayout.Space(5);
                            EnhancedUIEditorGUIUtility.DrawStatBox("Min", $"{minDuration:F3}s");
                            GUILayout.Space(5);
                            EnhancedUIEditorGUIUtility.DrawStatBox("Max", $"{maxDuration:F3}s");
                        }
                    }
                }
            }
        }

        private void DrawTransitionRow(EditorBridge.TransitionRecord transition)
        {
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.Height(20)))
            {
                // Container
                EditorGUILayout.LabelField(transition.ContainerName, EditorStyles.miniLabel, GUILayout.Width(120));

                // Transition description
                var transitionText = $"{transition.FromScreen} → {transition.ToScreen}";
                EditorGUILayout.LabelField(transitionText, EditorStyles.miniLabel);

                // Type
                EditorGUILayout.LabelField(transition.TransitionType, EditorStyles.miniLabel, GUILayout.Width(100));

                // Duration
                if (transition.IsCompleted)
                {
                    var durationColor = transition.Duration < 0.5f
                        ? EnhancedUIEditorStyles.SuccessColor
                        : (transition.Duration < 1f ? EnhancedUIEditorStyles.WarningColor : EnhancedUIEditorStyles.ErrorColor);

                    var originalColor = GUI.contentColor;
                    GUI.contentColor = durationColor;
                    EditorGUILayout.LabelField($"{transition.Duration:F3}s", EditorStyles.miniLabel, GUILayout.Width(70));
                    GUI.contentColor = originalColor;
                }
                else
                {
                    var elapsed = Time.realtimeSinceStartup - transition.StartTime;
                    EditorGUILayout.LabelField($"{elapsed:F3}s", EditorStyles.miniLabel, GUILayout.Width(70));
                }

                // Status
                var status = transition.IsCompleted ? "Complete" : "Ongoing";
                var statusColor = transition.IsCompleted
                    ? EnhancedUIEditorStyles.SuccessColor
                    : EnhancedUIEditorStyles.WarningColor;

                EnhancedUIEditorStyles.DrawStatusBadge(status, statusColor, 80);
            }
        }
    }
}
