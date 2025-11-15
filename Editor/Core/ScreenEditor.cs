using UnityEngine;
using UnityEditor;
using EnhancedUI.Editor.Utilities;
using EnhancedUI.Editor.Tools;

namespace EnhancedUI.Editor.Core
{
    /// <summary>
    /// Enhanced custom editor for Screen (base class for Page, Modal, Sheet)
    /// </summary>
    [CustomEditor(typeof(Screen), true)]
    [CanEditMultipleObjects]
    public class ScreenEditor : UnityEditor.Editor
    {
        private bool _showTransitionSettings = true;
        private bool _showRuntimeInfo = false;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var screen = target as Screen;
            if (screen == null) return;

            // Header
            DrawHeader(screen);

            GUILayout.Space(10);

            // Runtime information (only in play mode)
            if (Application.isPlaying)
            {
                DrawRuntimeInformation(screen);
                GUILayout.Space(10);
            }

            // Transition Settings
            DrawTransitionSettings();

            GUILayout.Space(10);

            // Quick Actions
            DrawQuickActions(screen);

            serializedObject.ApplyModifiedProperties();
        }

        protected virtual void DrawHeader(Screen screen)
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.HeaderBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var icon = EditorGUIUtility.IconContent("ScriptableObject Icon");
                    GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));

                    using (new EditorGUILayout.VerticalScope())
                    {
                        var screenType = screen.GetType().Name;
                        EditorGUILayout.LabelField($"{screenType} Screen", EnhancedUIEditorStyles.HeaderLabel);
                        EditorGUILayout.LabelField(screen.name, EditorStyles.miniLabel);
                    }

                    GUILayout.FlexibleSpace();

                    // Status badges
                    if (Application.isPlaying)
                    {
                        if (screen.IsInitialized)
                        {
                            EnhancedUIEditorStyles.DrawStatusBadge("Initialized", EnhancedUIEditorStyles.SuccessColor, 100);
                        }
                        else
                        {
                            EnhancedUIEditorStyles.DrawStatusBadge("Not Initialized", EnhancedUIEditorStyles.WarningColor, 100);
                        }
                    }
                }
            }
        }

        protected virtual void DrawRuntimeInformation(Screen screen)
        {
            _showRuntimeInfo = EnhancedUIEditorStyles.DrawSectionHeader(
                "🎮 Runtime Information",
                _showRuntimeInfo
            );

            if (!_showRuntimeInfo) return;

            GUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EnhancedUIEditorGUIUtility.DrawStatsRow(
                    "Initialized", screen.IsInitialized ? "Yes" : "No",
                    "Identifier", string.IsNullOrEmpty(screen.Identifier) ? "None" : screen.Identifier,
                    "Interactable", screen.Interactable ? "Yes" : "No"
                );

                GUILayout.Space(5);

                EnhancedUIEditorGUIUtility.DrawStatsRow(
                    "Alpha", screen.Alpha.ToString("F2"),
                    "Active", screen.gameObject.activeSelf ? "Yes" : "No",
                    "Enabled", screen.enabled ? "Yes" : "No"
                );

                GUILayout.Space(5);

                // Component info
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("RectTransform:", EditorStyles.boldLabel, GUILayout.Width(120));
                    EditorGUILayout.LabelField(screen.RectTransform != null ? "✓" : "✗", GUILayout.Width(80));

                    EditorGUILayout.LabelField("CanvasGroup:", EditorStyles.boldLabel, GUILayout.Width(120));
                    EditorGUILayout.LabelField(screen.CanvasGroup != null ? "✓" : "✗", GUILayout.Width(80));
                }
            }
        }

        private void DrawTransitionSettings()
        {
            _showTransitionSettings = EnhancedUIEditorStyles.DrawSectionHeader(
                "🎬 Transition Animations",
                _showTransitionSettings
            );

            if (!_showTransitionSettings) return;

            GUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.HelpBox(
                    "Configure transition animations for this screen. Each transition type can have different animations.",
                    MessageType.Info
                );

                GUILayout.Space(5);

                // Push Enter Animation
                var pushEnterAnimationContainer = serializedObject.FindProperty("pushEnterAnimationContainer");
                EditorGUILayout.PropertyField(pushEnterAnimationContainer, new GUIContent(
                    "Push Enter Animation",
                    "Animation played when this screen enters (push)"
                ), true);

                GUILayout.Space(3);

                // Push Exit Animation
                var pushExitAnimationContainer = serializedObject.FindProperty("pushExitAnimationContainer");
                EditorGUILayout.PropertyField(pushExitAnimationContainer, new GUIContent(
                    "Push Exit Animation",
                    "Animation played when this screen exits (push)"
                ), true);

                GUILayout.Space(3);

                // Pop Enter Animation
                var popEnterAnimationContainer = serializedObject.FindProperty("popEnterAnimationContainer");
                EditorGUILayout.PropertyField(popEnterAnimationContainer, new GUIContent(
                    "Pop Enter Animation",
                    "Animation played when this screen enters (pop)"
                ), true);

                GUILayout.Space(3);

                // Pop Exit Animation
                var popExitAnimationContainer = serializedObject.FindProperty("popExitAnimationContainer");
                EditorGUILayout.PropertyField(popExitAnimationContainer, new GUIContent(
                    "Pop Exit Animation",
                    "Animation played when this screen exits (pop)"
                ), true);
            }
        }

        protected virtual void DrawQuickActions(Screen screen)
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.LabelField("⚡ Quick Actions", EnhancedUIEditorStyles.BoldLabel);

                GUILayout.Space(5);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Open Control Center", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        EnhancedUIControlCenter.ShowWindow();
                    }

                    if (GUILayout.Button("View Documentation", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        Application.OpenURL("https://github.com/yourstudio/enhanced-ui-framework");
                    }
                }

                if (Application.isPlaying)
                {
                    GUILayout.Space(5);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUI.enabled = screen.IsInitialized;
                        if (GUILayout.Button("Toggle Interactable", GUILayout.Height(25)))
                        {
                            screen.Interactable = !screen.Interactable;
                        }
                        GUI.enabled = true;

                        if (GUILayout.Button("Open Event Tracker", GUILayout.Height(25)))
                        {
                            EventTrackerWindow.ShowWindow();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Enhanced custom editor for Page
    /// </summary>
    [CustomEditor(typeof(Page))]
    [CanEditMultipleObjects]
    public class PageEditor : ScreenEditor
    {
        protected override void DrawHeader(Screen screen)
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.HeaderBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var icon = EditorGUIUtility.IconContent("d_SceneAsset Icon");
                    GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));

                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField("📄 Page Screen", EnhancedUIEditorStyles.HeaderLabel);
                        EditorGUILayout.LabelField(screen.name, EditorStyles.miniLabel);
                    }

                    GUILayout.FlexibleSpace();

                    // Status badges
                    if (Application.isPlaying)
                    {
                        if (screen.IsInitialized)
                        {
                            EnhancedUIEditorStyles.DrawStatusBadge("Initialized", EnhancedUIEditorStyles.SuccessColor, 100);
                        }
                        else
                        {
                            EnhancedUIEditorStyles.DrawStatusBadge("Not Initialized", EnhancedUIEditorStyles.WarningColor, 100);
                        }
                    }
                }

                // Additional info for Page
                if (Application.isPlaying)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox(
                        "Pages are displayed in PageContainer with stack-based navigation.\n" +
                        "Use Push/Pop operations to manage page transitions.",
                        MessageType.Info
                    );
                }
            }
        }
    }

    /// <summary>
    /// Enhanced custom editor for Modal
    /// </summary>
    [CustomEditor(typeof(Modal))]
    [CanEditMultipleObjects]
    public class ModalEditor : ScreenEditor
    {
        protected override void DrawHeader(Screen screen)
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.HeaderBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var icon = EditorGUIUtility.IconContent("d_winbtn_win_max");
                    GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));

                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField("🔲 Modal Screen", EnhancedUIEditorStyles.HeaderLabel);
                        EditorGUILayout.LabelField(screen.name, EditorStyles.miniLabel);
                    }

                    GUILayout.FlexibleSpace();

                    // Status badges
                    if (Application.isPlaying)
                    {
                        if (screen.IsInitialized)
                        {
                            EnhancedUIEditorStyles.DrawStatusBadge("Initialized", EnhancedUIEditorStyles.SuccessColor, 100);
                        }
                        else
                        {
                            EnhancedUIEditorStyles.DrawStatusBadge("Not Initialized", EnhancedUIEditorStyles.WarningColor, 100);
                        }
                    }
                }

                // Additional info for Modal
                if (Application.isPlaying)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox(
                        "Modals are displayed in ModalContainer with stack-based navigation.\n" +
                        "They typically appear on top of pages with a backdrop.",
                        MessageType.Info
                    );
                }
            }
        }
    }

    /// <summary>
    /// Enhanced custom editor for Sheet
    /// </summary>
    [CustomEditor(typeof(Sheet))]
    [CanEditMultipleObjects]
    public class SheetEditor : ScreenEditor
    {
        protected override void DrawHeader(Screen screen)
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.HeaderBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var icon = EditorGUIUtility.IconContent("d_GridLayoutGroup Icon");
                    GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));

                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField("📋 Sheet Screen", EnhancedUIEditorStyles.HeaderLabel);
                        EditorGUILayout.LabelField(screen.name, EditorStyles.miniLabel);
                    }

                    GUILayout.FlexibleSpace();

                    // Status badges
                    if (Application.isPlaying)
                    {
                        if (screen.IsInitialized)
                        {
                            EnhancedUIEditorStyles.DrawStatusBadge("Initialized", EnhancedUIEditorStyles.SuccessColor, 100);
                        }
                        else
                        {
                            EnhancedUIEditorStyles.DrawStatusBadge("Not Initialized", EnhancedUIEditorStyles.WarningColor, 100);
                        }
                    }
                }

                // Additional info for Sheet
                if (Application.isPlaying)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox(
                        "Sheets are displayed in SheetContainer with queue-based navigation.\n" +
                        "Multiple sheets can be shown simultaneously (e.g., bottom sheets, side panels).",
                        MessageType.Info
                    );
                }
            }
        }
    }
}
