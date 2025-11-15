using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using EnhancedUI.Editor.Utilities;

namespace EnhancedUI.Editor.Tools
{
    /// <summary>
    /// Health check and validation tool for Enhanced UI Framework setup
    /// </summary>
    public class HealthCheckWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private List<HealthIssue> _issues = new List<HealthIssue>();
        private bool _hasRunCheck = false;

        [MenuItem("Tools/Enhanced UI/Analysis Tools/Health Check", false, 200)]
        public static void ShowWindow()
        {
            var window = GetWindow<HealthCheckWindow>("Health Check");
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void OnGUI()
        {
            EnhancedUIEditorGUIUtility.DrawWindowHeader("Health Check", "d_TestPassed");

            GUILayout.Space(10);

            // Run check button
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Run Health Check", EnhancedUIEditorStyles.PrimaryButton, GUILayout.Width(200)))
                {
                    RunHealthCheck();
                }

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(10);

            // Results
            if (_hasRunCheck)
            {
                DrawResults();
            }
            else
            {
                EnhancedUIEditorGUIUtility.DrawMessageBox(
                    "Click 'Run Health Check' to validate your Enhanced UI Framework setup.\n\nThis will check for common configuration issues, missing references, and performance recommendations.",
                    MessageType.Info
                );
            }
        }

        private void RunHealthCheck()
        {
            _issues.Clear();
            _hasRunCheck = true;

            // Check settings
            CheckSettings();

            // Check containers in scene
            CheckContainers();

            // Check canvas setup
            CheckCanvasSetup();

            // Check asset loaders
            CheckAssetLoaders();

            // Performance recommendations
            CheckPerformanceSettings();

            Debug.Log($"[Health Check] Completed. Found {_issues.Count} issue(s).");
        }

        private void DrawResults()
        {
            // Summary
            var errorCount = _issues.Count(i => i.Severity == IssueSeverity.Error);
            var warningCount = _issues.Count(i => i.Severity == IssueSeverity.Warning);
            var infoCount = _issues.Count(i => i.Severity == IssueSeverity.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawStatBox("Errors", errorCount.ToString(), errorCount > 0 ? EnhancedUIEditorStyles.ErrorColor : Color.white);
                DrawStatBox("Warnings", warningCount.ToString(), warningCount > 0 ? EnhancedUIEditorStyles.WarningColor : Color.white);
                DrawStatBox("Info", infoCount.ToString(), EnhancedUIEditorStyles.InfoColor);
            }

            GUILayout.Space(10);

            // Issue list
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            if (_issues.Count == 0)
            {
                using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
                {
                    GUILayout.Space(20);
                    EditorGUILayout.LabelField("✓ All Checks Passed!", EnhancedUIEditorStyles.HeaderLabel);
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField("Your Enhanced UI Framework setup looks good!", EditorStyles.centeredGreyMiniLabel);
                    GUILayout.Space(20);
                }
            }
            else
            {
                foreach (var issue in _issues)
                {
                    DrawIssue(issue);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawStatBox(string label, string value, Color color)
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.StatusBox))
            {
                var originalColor = GUI.contentColor;
                GUI.contentColor = color;
                EditorGUILayout.LabelField(value, EnhancedUIEditorStyles.HeaderLabel);
                GUI.contentColor = originalColor;

                EditorGUILayout.LabelField(label, EnhancedUIEditorStyles.CenteredLabel);
            }
        }

        private void DrawIssue(HealthIssue issue)
        {
            GUIStyle boxStyle;
            string icon;

            switch (issue.Severity)
            {
                case IssueSeverity.Error:
                    boxStyle = EnhancedUIEditorStyles.ErrorBox;
                    icon = "❌";
                    break;
                case IssueSeverity.Warning:
                    boxStyle = EnhancedUIEditorStyles.WarningBox;
                    icon = "⚠";
                    break;
                default:
                    boxStyle = EnhancedUIEditorStyles.InfoBox;
                    icon = "ℹ";
                    break;
            }

            using (new EditorGUILayout.VerticalScope(boxStyle))
            {
                EditorGUILayout.LabelField($"{icon} {issue.Title}", EnhancedUIEditorStyles.BoldLabel);

                GUILayout.Space(3);

                EditorGUILayout.LabelField(issue.Description, EditorStyles.wordWrappedLabel);

                if (!string.IsNullOrEmpty(issue.Suggestion))
                {
                    GUILayout.Space(3);
                    EditorGUILayout.LabelField($"💡 {issue.Suggestion}", EditorStyles.wordWrappedMiniLabel);
                }

                if (issue.FixAction != null)
                {
                    GUILayout.Space(5);

                    if (GUILayout.Button("Fix Automatically", GUILayout.Height(25)))
                    {
                        issue.FixAction();
                        RunHealthCheck(); // Re-run check
                    }
                }
            }

            GUILayout.Space(5);
        }

        private void CheckSettings()
        {
            var settings = EnhancedUISettings.Instance;

            if (settings == null)
            {
                _issues.Add(new HealthIssue
                {
                    Severity = IssueSeverity.Error,
                    Title = "EnhancedUISettings not found",
                    Description = "The EnhancedUISettings asset is missing. This is required for the framework to function.",
                    Suggestion = "Run the Setup Wizard to create the settings asset.",
                    FixAction = () => SetupWizard.ShowWindow()
                });
                return;
            }

            // Check memory budget
            if (settings.memoryBudgetMB < 10)
            {
                _issues.Add(new HealthIssue
                {
                    Severity = IssueSeverity.Warning,
                    Title = "Low Memory Budget",
                    Description = $"Cache memory budget is set to {settings.memoryBudgetMB}MB which may be too low for larger applications.",
                    Suggestion = "Consider increasing to at least 50MB for better performance."
                });
            }

            // Check caching settings
            if (!settings.enableSmartCaching)
            {
                _issues.Add(new HealthIssue
                {
                    Severity = IssueSeverity.Info,
                    Title = "Smart Caching Disabled",
                    Description = "Smart caching with LRU is disabled. Enable it for better memory management.",
                    Suggestion = "Enable 'enableSmartCaching' in EnhancedUISettings."
                });
            }
        }

        private void CheckContainers()
        {
            // Check for containers in scene
            var pageContainers = FindObjectsOfType<PageContainer>();
            var modalContainers = FindObjectsOfType<ModalContainer>();
            var sheetContainers = FindObjectsOfType<SheetContainer>();

            if (pageContainers.Length == 0 && modalContainers.Length == 0 && sheetContainers.Length == 0)
            {
                _issues.Add(new HealthIssue
                {
                    Severity = IssueSeverity.Warning,
                    Title = "No Containers Found",
                    Description = "No PageContainer, ModalContainer, or SheetContainer found in the scene.",
                    Suggestion = "Run the Setup Wizard to create containers."
                });
            }

            // Check for duplicate container names
            var allContainers = new List<IUIContainer>();
            allContainers.AddRange(pageContainers);
            allContainers.AddRange(modalContainers);
            allContainers.AddRange(sheetContainers);

            var duplicateNames = allContainers
                .GroupBy(c => c.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            foreach (var name in duplicateNames)
            {
                _issues.Add(new HealthIssue
                {
                    Severity = IssueSeverity.Error,
                    Title = "Duplicate Container Name",
                    Description = $"Multiple containers with name '{name}' found. Container names must be unique.",
                    Suggestion = "Rename containers to have unique names."
                });
            }
        }

        private void CheckCanvasSetup()
        {
            var canvases = FindObjectsOfType<Canvas>();

            if (canvases.Length == 0)
            {
                _issues.Add(new HealthIssue
                {
                    Severity = IssueSeverity.Error,
                    Title = "No Canvas Found",
                    Description = "No Canvas component found in the scene. A Canvas is required for UI rendering.",
                    Suggestion = "Create a Canvas in the scene."
                });
                return;
            }

            // Check for Canvas with no Event System
            var eventSystems = FindObjectsOfType<UnityEngine.EventSystems.EventSystem>();

            if (eventSystems.Length == 0)
            {
                _issues.Add(new HealthIssue
                {
                    Severity = IssueSeverity.Error,
                    Title = "No EventSystem Found",
                    Description = "No EventSystem found in the scene. This is required for UI interaction.",
                    Suggestion = "Add an EventSystem to the scene.",
                    FixAction = () =>
                    {
                        var go = new GameObject("EventSystem");
                        go.AddComponent<UnityEngine.EventSystems.EventSystem>();
                        go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    }
                });
            }
        }

        private void CheckAssetLoaders()
        {
            // Check if Addressables is available
#if !EUI_ADDRESSABLE_SUPPORT
            _issues.Add(new HealthIssue
            {
                Severity = IssueSeverity.Info,
                Title = "Addressables Not Enabled",
                Description = "Addressables support is not enabled. Add 'EUI_ADDRESSABLE_SUPPORT' to scripting define symbols to enable it.",
                Suggestion = "Install Addressables package and add the scripting define symbol for better asset management."
            });
#endif

            // Check if UniTask is available
#if !EUI_UNITASK_SUPPORT
            _issues.Add(new HealthIssue
            {
                Severity = IssueSeverity.Info,
                Title = "UniTask Not Enabled",
                Description = "UniTask support is not enabled. Add 'EUI_UNITASK_SUPPORT' to scripting define symbols to enable it.",
                Suggestion = "Install UniTask package and add the scripting define symbol for better async performance."
            });
#endif
        }

        private void CheckPerformanceSettings()
        {
            var settings = EnhancedUISettings.Instance;
            if (settings == null) return;

            // Check pool sizes
            if (settings.poolConfigurations != null && settings.poolConfigurations.Count == 0)
            {
                _issues.Add(new HealthIssue
                {
                    Severity = IssueSeverity.Info,
                    Title = "No Pool Configurations",
                    Description = "No object pools configured. Pooling frequently used screens can improve performance.",
                    Suggestion = "Configure pools for commonly used screens in EnhancedUISettings."
                });
            }

            // Check interaction settings
            if (settings.enableInteractionInTransition == true)
            {
                _issues.Add(new HealthIssue
                {
                    Severity = IssueSeverity.Warning,
                    Title = "Interaction During Transitions Enabled",
                    Description = "User interaction is allowed during transitions. This may cause unexpected behavior.",
                    Suggestion = "Consider disabling 'enableInteractionInTransition' in EnhancedUISettings."
                });
            }
        }

        private enum IssueSeverity
        {
            Error,
            Warning,
            Info
        }

        private class HealthIssue
        {
            public IssueSeverity Severity;
            public string Title;
            public string Description;
            public string Suggestion;
            public System.Action FixAction;
        }
    }
}
