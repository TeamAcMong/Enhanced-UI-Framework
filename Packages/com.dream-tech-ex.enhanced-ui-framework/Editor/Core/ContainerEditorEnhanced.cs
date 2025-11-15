using System.Linq;
using UnityEngine;
using UnityEditor;
using EnhancedUI.Editor.Utilities;
using EnhancedUI.Editor.Tools;
using EnhancedUI.Utilities;

namespace EnhancedUI.Editor.Core
{
    /// <summary>
    /// Base class for enhanced container editors with beautiful UI
    /// </summary>
    public abstract class ContainerEditorEnhancedBase : UnityEditor.Editor
    {
        private bool _showRuntimeInfo = true;
        private bool _showPerformance = true;
        private bool _showQuickActions = true;

        // Throttled repaint
        private double _lastRepaintTime;
        private const double REPAINT_INTERVAL = 0.1; // 10 times per second instead of 60+

        // Cached performance metrics
        private EditorBridge.PerformanceMetrics _cachedMetrics;
        private float _lastMetricsUpdateTime;
        private const float METRICS_UPDATE_INTERVAL = 0.5f; // Update twice per second

        // Cached transition data
        private int _cachedTransitionCount;
        private float _cachedAvgTransitionDuration;
        private float _lastTransitionQueryTime;
        private const float TRANSITION_QUERY_INTERVAL = 1f; // Update once per second

        protected abstract string ContainerIconName { get; }
        protected abstract string ContainerTypeName { get; }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw header
            DrawHeader();

            GUILayout.Space(10);

            // Draw configuration section
            DrawConfiguration();

            // Draw runtime information (play mode only)
            if (Application.isPlaying)
            {
                GUILayout.Space(10);
                DrawRuntimeInformation();

                GUILayout.Space(10);
                DrawPerformanceMetrics();

                GUILayout.Space(10);
                DrawQuickActions();
            }
            else
            {
                GUILayout.Space(10);
                EnhancedUIEditorGUIUtility.DrawMessageBox(
                    "Enter Play Mode to see runtime information and debug tools.",
                    MessageType.Info
                );
            }

            serializedObject.ApplyModifiedProperties();

            // Throttled repaint in play mode (10 times/sec instead of 60+)
            if (Application.isPlaying)
            {
                if (EditorApplication.timeSinceStartup - _lastRepaintTime > REPAINT_INTERVAL)
                {
                    Repaint();
                    _lastRepaintTime = EditorApplication.timeSinceStartup;
                }
            }
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.HeaderBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    // Icon
                    var icon = EditorGUIUtility.IconContent(ContainerIconName);
                    GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));

                    // Title and status
                    using (new EditorGUILayout.VerticalScope())
                    {
                        var container = target as IUIContainer;
                        var containerName = container != null ? container.Name : target.name;

                        EditorGUILayout.LabelField(containerName, EnhancedUIEditorStyles.HeaderLabel);

                        if (Application.isPlaying && container != null)
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                // Status badge
                                var status = container.IsInTransition ? "Transitioning" : "Idle";
                                var statusColor = container.IsInTransition
                                    ? EnhancedUIEditorStyles.TransitionColor
                                    : EnhancedUIEditorStyles.SuccessColor;

                                EnhancedUIEditorStyles.DrawStatusBadge(status, statusColor, 100);

                                GUILayout.Space(5);

                                // Interactable status
                                var interactableStatus = container.Interactable ? "Interactive" : "Blocked";
                                var interactableColor = container.Interactable
                                    ? EnhancedUIEditorStyles.SuccessColor
                                    : EnhancedUIEditorStyles.DisabledColor;

                                EnhancedUIEditorStyles.DrawStatusBadge(interactableStatus, interactableColor, 100);
                            }
                        }
                        else
                        {
                            EditorGUILayout.LabelField(ContainerTypeName, EditorStyles.miniLabel);
                        }
                    }

                    GUILayout.FlexibleSpace();
                }
            }
        }

        private void DrawConfiguration()
        {
            _showRuntimeInfo = EnhancedUIEditorStyles.DrawSectionHeader("⚙ Configuration", _showRuntimeInfo);

            if (_showRuntimeInfo)
            {
                GUILayout.Space(5);

                using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
                {
                    DrawDefaultInspector();
                }
            }
        }

        protected abstract void DrawRuntimeInformation();

        private void DrawPerformanceMetrics()
        {
            var bridge = EditorBridge.Instance;
            if (bridge == null) return;

            _showPerformance = EnhancedUIEditorStyles.DrawSectionHeader("📊 Performance Metrics", _showPerformance);

            if (_showPerformance)
            {
                GUILayout.Space(5);

                using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
                {
                    // Update cached metrics periodically instead of every frame
                    if (Time.realtimeSinceStartup - _lastMetricsUpdateTime > METRICS_UPDATE_INTERVAL)
                    {
                        _cachedMetrics = bridge.GetRecentPerformanceMetrics(1f);
                        _lastMetricsUpdateTime = Time.realtimeSinceStartup;
                    }

                    if (_cachedMetrics.SampleCount > 0)
                    {
                        // Draw FPS
                        EnhancedUIEditorGUIUtility.DrawPerformanceMetric(
                            "Average FPS",
                            _cachedMetrics.AverageFPS,
                            55f, // Good threshold
                            40f  // Bad threshold
                        );

                        EnhancedUIEditorGUIUtility.DrawPerformanceMetric(
                            "Min FPS",
                            _cachedMetrics.MinFPS,
                            50f,
                            30f
                        );

                        // Draw Frame Time
                        EnhancedUIEditorGUIUtility.DrawPerformanceMetric(
                            "Avg Frame Time",
                            _cachedMetrics.AverageFrameTime,
                            20f,  // Good < 20ms
                            33f,  // Bad > 33ms
                            "ms"
                        );

                        // Draw Memory
                        EnhancedUIEditorGUIUtility.DrawLabeledField(
                            "Memory Usage",
                            $"{_cachedMetrics.AverageMemoryUsage:F2} MB"
                        );

                        GUILayout.Space(5);

                        // Cached transition count - update periodically instead of every frame
                        var container = target as IUIContainer;
                        if (container != null)
                        {
                            if (Time.realtimeSinceStartup - _lastTransitionQueryTime > TRANSITION_QUERY_INTERVAL)
                            {
                                _cachedTransitionCount = bridge.TransitionHistory
                                    .Count(t => t.ContainerName == container.Name && t.IsCompleted);

                                _cachedAvgTransitionDuration = _cachedTransitionCount > 0
                                    ? bridge.TransitionHistory
                                        .Where(t => t.ContainerName == container.Name && t.IsCompleted)
                                        .Average(t => t.Duration)
                                    : 0f;

                                _lastTransitionQueryTime = Time.realtimeSinceStartup;
                            }

                            EnhancedUIEditorGUIUtility.DrawLabeledField(
                                "Total Transitions",
                                _cachedTransitionCount.ToString()
                            );

                            if (_cachedTransitionCount > 0)
                            {
                                EnhancedUIEditorGUIUtility.DrawLabeledField(
                                    "Avg Transition Duration",
                                    $"{_cachedAvgTransitionDuration:F3}s"
                                );
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No performance data available yet...", EditorStyles.centeredGreyMiniLabel);
                    }
                }
            }
        }

        protected abstract void DrawQuickActions();
    }

    /// <summary>
    /// Enhanced editor for PageContainer
    /// </summary>
    [CustomEditor(typeof(PageContainer))]
    public class PageContainerEditor : ContainerEditorEnhancedBase
    {
        protected override string ContainerIconName => "SceneAsset Icon";
        protected override string ContainerTypeName => "Page Container";

        protected override void DrawRuntimeInformation()
        {
            var pageContainer = target as PageContainer;
            if (pageContainer == null) return;

            bool showInfo = EnhancedUIEditorStyles.DrawSectionHeader("📄 Runtime State", true);

            if (showInfo)
            {
                GUILayout.Space(5);

                using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
                {
                    // Stats row
                    EnhancedUIEditorGUIUtility.DrawStatsRow(
                        "Page Count",
                        pageContainer.PageCount.ToString(),
                        "Current",
                        pageContainer.CurrentPage != null ? pageContainer.CurrentPage.name : "None",
                        "In Transition",
                        pageContainer.IsInTransition ? "Yes" : "No"
                    );

                    GUILayout.Space(10);

                    // Page stack visualization
                    if (pageContainer.PageCount > 0)
                    {
                        EditorGUILayout.LabelField("Page Stack:", EnhancedUIEditorStyles.BoldLabel);

                        // Draw stack (would need access to internal _pages list)
                        // For now, show current page
                        if (pageContainer.CurrentPage != null)
                        {
                            using (new EditorGUILayout.HorizontalScope(EnhancedUIEditorStyles.SectionBox))
                            {
                                EditorGUILayout.LabelField($"→ {pageContainer.CurrentPage.name}");

                                if (GUILayout.Button("Select", GUILayout.Width(60)))
                                {
                                    Selection.activeGameObject = pageContainer.CurrentPage.gameObject;
                                }
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No pages in stack", EditorStyles.centeredGreyMiniLabel);
                    }
                }
            }
        }

        protected override void DrawQuickActions()
        {
            var pageContainer = target as PageContainer;
            if (pageContainer == null) return;

            bool showActions = EnhancedUIEditorStyles.DrawSectionHeader("⚡ Quick Actions", true);

            if (showActions)
            {
                GUILayout.Space(5);

                using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Pop Page", EnhancedUIEditorStyles.SecondaryButton))
                        {
                            if (pageContainer.PageCount > 0)
                            {
                                pageContainer.Pop(true);
                            }
                        }

                        if (GUILayout.Button("Clear Stack", EnhancedUIEditorStyles.SecondaryButton))
                        {
                            if (EditorUtility.DisplayDialog(
                                "Clear Page Stack",
                                "Are you sure you want to clear the entire page stack?",
                                "Yes",
                                "No"))
                            {
                                // Pop all pages
                                while (pageContainer.PageCount > 0)
                                {
                                    pageContainer.Pop(false);
                                }
                            }
                        }
                    }

                    GUILayout.Space(5);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Select Current Page", EnhancedUIEditorStyles.SecondaryButton))
                        {
                            if (pageContainer.CurrentPage != null)
                            {
                                Selection.activeGameObject = pageContainer.CurrentPage.gameObject;
                            }
                        }

                        if (GUILayout.Button("Open Control Center", EnhancedUIEditorStyles.SecondaryButton))
                        {
                            EnhancedUIControlCenter.ShowWindow();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Enhanced editor for ModalContainer
    /// </summary>
    [CustomEditor(typeof(ModalContainer))]
    public class ModalContainerEditor : ContainerEditorEnhancedBase
    {
        protected override string ContainerIconName => "winbtn_win_max";
        protected override string ContainerTypeName => "Modal Container";

        protected override void DrawRuntimeInformation()
        {
            var modalContainer = target as ModalContainer;
            if (modalContainer == null) return;

            bool showInfo = EnhancedUIEditorStyles.DrawSectionHeader("🔲 Runtime State", true);

            if (showInfo)
            {
                GUILayout.Space(5);

                using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
                {
                    // Stats
                    EnhancedUIEditorGUIUtility.DrawStatsRow(
                        "Modal Count",
                        modalContainer.ModalCount.ToString(),
                        "Status",
                        modalContainer.IsInTransition ? "Transitioning" : "Idle",
                        "Interactable",
                        modalContainer.Interactable ? "Yes" : "No"
                    );

                    GUILayout.Space(10);

                    // Modal list
                    if (modalContainer.ModalCount > 0)
                    {
                        EditorGUILayout.LabelField("Active Modals:", EnhancedUIEditorStyles.BoldLabel);

                        // Would need access to internal _modals list
                        // For now, show count
                        EditorGUILayout.LabelField(
                            $"{modalContainer.ModalCount} modal(s) active",
                            EditorStyles.miniLabel
                        );
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No active modals", EditorStyles.centeredGreyMiniLabel);
                    }
                }
            }
        }

        protected override void DrawQuickActions()
        {
            var modalContainer = target as ModalContainer;
            if (modalContainer == null) return;

            bool showActions = EnhancedUIEditorStyles.DrawSectionHeader("⚡ Quick Actions", true);

            if (showActions)
            {
                GUILayout.Space(5);

                using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Pop Modal", EnhancedUIEditorStyles.SecondaryButton))
                        {
                            if (modalContainer.ModalCount > 0)
                            {
                                modalContainer.Pop(true);
                            }
                        }

                        if (GUILayout.Button("Pop All Modals", EnhancedUIEditorStyles.SecondaryButton))
                        {
                            if (EditorUtility.DisplayDialog(
                                "Pop All Modals",
                                "Are you sure you want to close all modals?",
                                "Yes",
                                "No"))
                            {
                                while (modalContainer.ModalCount > 0)
                                {
                                    modalContainer.Pop(false);
                                }
                            }
                        }
                    }

                    GUILayout.Space(5);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Open Control Center", EnhancedUIEditorStyles.SecondaryButton))
                        {
                            EnhancedUIControlCenter.ShowWindow();
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Enhanced editor for SheetContainer
    /// </summary>
    [CustomEditor(typeof(SheetContainer))]
    public class SheetContainerEditor : ContainerEditorEnhancedBase
    {
        protected override string ContainerIconName => "Grid.Default";
        protected override string ContainerTypeName => "Sheet Container";

        protected override void DrawRuntimeInformation()
        {
            var sheetContainer = target as SheetContainer;
            if (sheetContainer == null) return;

            bool showInfo = EnhancedUIEditorStyles.DrawSectionHeader("📑 Runtime State", true);

            if (showInfo)
            {
                GUILayout.Space(5);

                using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
                {
                    // Stats
                    var activeSheetName = sheetContainer.ActiveSheet != null
                        ? sheetContainer.ActiveSheet.name
                        : "None";

                    EnhancedUIEditorGUIUtility.DrawStatsRow(
                        "Sheet Count",
                        sheetContainer.SheetCount.ToString(),
                        "Active Sheet",
                        activeSheetName,
                        "Status",
                        sheetContainer.IsInTransition ? "Transitioning" : "Idle"
                    );

                    GUILayout.Space(10);

                    // Active sheet info
                    if (sheetContainer.ActiveSheet != null)
                    {
                        EditorGUILayout.LabelField("Active Sheet:", EnhancedUIEditorStyles.BoldLabel);

                        using (new EditorGUILayout.HorizontalScope(EnhancedUIEditorStyles.SectionBox))
                        {
                            EditorGUILayout.LabelField($"→ {sheetContainer.ActiveSheet.name}");

                            if (GUILayout.Button("Select", GUILayout.Width(60)))
                            {
                                Selection.activeGameObject = sheetContainer.ActiveSheet.gameObject;
                            }
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("No active sheet", EditorStyles.centeredGreyMiniLabel);
                    }
                }
            }
        }

        protected override void DrawQuickActions()
        {
            var sheetContainer = target as SheetContainer;
            if (sheetContainer == null) return;

            bool showActions = EnhancedUIEditorStyles.DrawSectionHeader("⚡ Quick Actions", true);

            if (showActions)
            {
                GUILayout.Space(5);

                using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Hide Active Sheet", EnhancedUIEditorStyles.SecondaryButton))
                        {
                            if (sheetContainer.ActiveSheet != null)
                            {
                                sheetContainer.Hide(true);
                            }
                        }

                        if (GUILayout.Button("Select Active Sheet", EnhancedUIEditorStyles.SecondaryButton))
                        {
                            if (sheetContainer.ActiveSheet != null)
                            {
                                Selection.activeGameObject = sheetContainer.ActiveSheet.gameObject;
                            }
                        }
                    }

                    GUILayout.Space(5);

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Open Control Center", EnhancedUIEditorStyles.SecondaryButton))
                        {
                            EnhancedUIControlCenter.ShowWindow();
                        }
                    }
                }
            }
        }
    }
}
