using UnityEngine;
using UnityEditor;
using EnhancedUI.Editor.Utilities;
using EnhancedUI.Editor.Tools;

namespace EnhancedUI.Editor.Core
{
    /// <summary>
    /// Enhanced custom editor for EnhancedUISettings with beautiful categorized layout
    /// </summary>
    [CustomEditor(typeof(EnhancedUISettings))]
    public class EnhancedUISettingsEditor : UnityEditor.Editor
    {
        private bool _showPerformanceSettings = true;
        private bool _showPoolingSettings = true;
        private bool _showInteractionSettings = true;
        private bool _showPlatformSettings = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Header
            DrawHeader();

            GUILayout.Space(10);

            // Performance Settings
            DrawPerformanceSettings();

            GUILayout.Space(10);

            // Pooling Settings
            DrawPoolingSettings();

            GUILayout.Space(10);

            // Interaction Settings
            DrawInteractionSettings();

            GUILayout.Space(10);

            // Platform Settings
            DrawPlatformSettings();

            GUILayout.Space(10);

            // Actions
            DrawActions();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.HeaderBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var icon = EditorGUIUtility.IconContent("Settings");
                    GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));

                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField("Enhanced UI Settings", EnhancedUIEditorStyles.HeaderLabel);
                        EditorGUILayout.LabelField("Global configuration for Enhanced UI Framework", EditorStyles.miniLabel);
                    }

                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void DrawPerformanceSettings()
        {
            _showPerformanceSettings = EnhancedUIEditorStyles.DrawSectionHeader(
                "⚡ Performance Settings",
                _showPerformanceSettings
            );

            if (!_showPerformanceSettings) return;

            GUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.HelpBox(
                    "Configure caching and performance monitoring options.",
                    MessageType.Info
                );

                GUILayout.Space(5);

                // Memory Budget
                var memoryBudgetMB = serializedObject.FindProperty("memoryBudgetMB");
                EditorGUILayout.PropertyField(memoryBudgetMB, new GUIContent(
                    "Memory Budget (MB)",
                    "Maximum memory budget for screen cache in MB"
                ));

                if (memoryBudgetMB.intValue < 10)
                {
                    EditorGUILayout.HelpBox(
                        "⚠ Low memory budget may cause frequent cache evictions.",
                        MessageType.Warning
                    );
                }

                GUILayout.Space(5);

                // Smart Caching
                var enableSmartCaching = serializedObject.FindProperty("enableSmartCaching");
                EditorGUILayout.PropertyField(enableSmartCaching, new GUIContent(
                    "Enable Smart Caching",
                    "Use LRU caching for better memory management"
                ));

                // Debug Logging
                var enableDebugLog = serializedObject.FindProperty("enableDebugLog");
                EditorGUILayout.PropertyField(enableDebugLog, new GUIContent(
                    "Enable Debug Logging",
                    "Log detailed information for debugging"
                ));
            }
        }

        private void DrawPoolingSettings()
        {
            _showPoolingSettings = EnhancedUIEditorStyles.DrawSectionHeader(
                "🔄 Pooling Settings",
                _showPoolingSettings
            );

            if (!_showPoolingSettings) return;

            GUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.HelpBox(
                    "Configure object pooling for frequently used screens to improve performance.",
                    MessageType.Info
                );

                GUILayout.Space(5);

                // Object Pooling Toggle
                var enableObjectPooling = serializedObject.FindProperty("enableObjectPooling");
                EditorGUILayout.PropertyField(enableObjectPooling, new GUIContent("Enable Object Pooling"));

                // Pool Configs Array
                var poolConfigurations = serializedObject.FindProperty("poolConfigurations");
                EditorGUILayout.PropertyField(poolConfigurations, new GUIContent("Pool Configurations"), true);

                if (poolConfigurations.arraySize == 0)
                {
                    GUILayout.Space(5);
                    EditorGUILayout.HelpBox(
                        "💡 Add pool configurations for screens that are shown/hidden frequently (e.g., modals, tooltips).",
                        MessageType.Info
                    );
                }
            }
        }

        private void DrawInteractionSettings()
        {
            _showInteractionSettings = EnhancedUIEditorStyles.DrawSectionHeader(
                "👆 Interaction Settings",
                _showInteractionSettings
            );

            if (!_showInteractionSettings) return;

            GUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.HelpBox(
                    "Control user interaction behavior during transitions.",
                    MessageType.Info
                );

                GUILayout.Space(5);

                // Enable Interaction In Transition
                var enableInteractionInTransition = serializedObject.FindProperty("enableInteractionInTransition");
                EditorGUILayout.PropertyField(enableInteractionInTransition, new GUIContent(
                    "Enable Interaction In Transition",
                    "Allow user input during screen transitions"
                ));

                if (enableInteractionInTransition.boolValue)
                {
                    EditorGUILayout.HelpBox(
                        "⚠ Allowing interaction during transitions may cause unexpected navigation behavior.",
                        MessageType.Warning
                    );
                }

                GUILayout.Space(5);

                // Control Interaction Of All Containers
                var controlInteractionOfAllContainers = serializedObject.FindProperty("controlInteractionOfAllContainers");
                EditorGUILayout.PropertyField(controlInteractionOfAllContainers, new GUIContent(
                    "Control Interaction Of All Containers",
                    "Disable interaction in ALL containers when one is transitioning"
                ));
            }
        }

        private void DrawPlatformSettings()
        {
            _showPlatformSettings = EnhancedUIEditorStyles.DrawSectionHeader(
                "📱 Platform Settings",
                _showPlatformSettings
            );

            if (!_showPlatformSettings) return;

            GUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.HelpBox(
                    "Platform-specific features and adaptations.",
                    MessageType.Info
                );

                GUILayout.Space(5);

                // Enable Safe Area
                var enableSafeArea = serializedObject.FindProperty("enableSafeArea");
                EditorGUILayout.PropertyField(enableSafeArea, new GUIContent(
                    "Enable Safe Area",
                    "Automatically adapt UI to device safe area (notch, home indicator)"
                ));

                // Enable Back Button
                var enableBackButton = serializedObject.FindProperty("enableBackButton");
                EditorGUILayout.PropertyField(enableBackButton, new GUIContent(
                    "Enable Back Button",
                    "Handle Android back button globally"
                ));

                // Enable Orientation Management
                var enableOrientationManagement = serializedObject.FindProperty("enableOrientationManagement");
                EditorGUILayout.PropertyField(enableOrientationManagement, new GUIContent(
                    "Enable Orientation Management",
                    "Track and manage screen orientation changes"
                ));
            }
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.LabelField("⚡ Quick Actions", EnhancedUIEditorStyles.BoldLabel);

                GUILayout.Space(5);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reset to Defaults", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        if (EditorUtility.DisplayDialog(
                            "Reset Settings",
                            "Reset all settings to default values?",
                            "Yes",
                            "No"))
                        {
                            ResetToDefaults();
                        }
                    }

                    if (GUILayout.Button("Open Control Center", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        EnhancedUIControlCenter.ShowWindow();
                    }
                }

                GUILayout.Space(5);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Run Health Check", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        HealthCheckWindow.ShowWindow();
                    }

                    if (GUILayout.Button("Documentation", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        Application.OpenURL("https://github.com/yourstudio/enhanced-ui-framework");
                    }
                }
            }
        }

        private void ResetToDefaults()
        {
            var settings = target as EnhancedUISettings;
            if (settings == null) return;

            Undo.RecordObject(settings, "Reset Settings to Defaults");

            settings.memoryBudgetMB = 100;
            settings.enableSmartCaching = true;
            settings.enableDebugLog = false;
            settings.poolConfigurations = new System.Collections.Generic.List<PoolConfig>();
            settings.enableInteractionInTransition = false;
            settings.controlInteractionOfAllContainers = true;
            settings.enableSafeArea = true;
            settings.enableBackButton = true;
            settings.enableOrientationManagement = false;

            EditorUtility.SetDirty(settings);
            serializedObject.Update();
        }
    }
}
