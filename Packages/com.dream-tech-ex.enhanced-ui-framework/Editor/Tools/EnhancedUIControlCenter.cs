using System.Linq;
using UnityEngine;
using UnityEditor;
using EnhancedUI.Editor.Utilities;
using EnhancedUI.Utilities;

namespace EnhancedUI.Editor.Tools
{
    /// <summary>
    /// Main control center dashboard for Enhanced UI Framework
    /// </summary>
    public class EnhancedUIControlCenter : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showContainers = true;
        private bool _showRecentEvents = true;
        private bool _showPerformance = true;

        // Throttled repaint
        private double _lastRepaintTime;
        private const double REPAINT_INTERVAL = 0.1; // 10 times per second

        // Cached FindObjectsOfType results
        private PageContainer[] _cachedPageContainers;
        private ModalContainer[] _cachedModalContainers;
        private SheetContainer[] _cachedSheetContainers;
        private float _lastContainerScanTime;
        private const float CONTAINER_SCAN_INTERVAL = 1f; // Update once per second

        [MenuItem("Tools/Enhanced UI/Control Center", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<EnhancedUIControlCenter>("Enhanced UI Control Center");
            window.minSize = new Vector2(400, 600);
            window.Show();
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Header
            DrawHeader();

            GUILayout.Space(10);

            // Dashboard stats
            DrawDashboardStats();

            GUILayout.Space(10);

            // Quick actions
            DrawQuickActions();

            GUILayout.Space(10);

            // Active containers
            DrawActiveContainers();

            GUILayout.Space(10);

            // Recent events
            if (Application.isPlaying)
            {
                DrawRecentEvents();
                GUILayout.Space(10);
                DrawPerformanceOverview();
            }

            GUILayout.Space(10);

            // Tools section
            DrawToolsSection();

            GUILayout.Space(10);

            // Footer
            DrawFooter();

            EditorGUILayout.EndScrollView();

            // Throttled repaint in play mode (10 times/sec instead of unlimited)
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
            EnhancedUIEditorGUIUtility.DrawWindowHeader("Enhanced UI Control Center", "d_UnityEditor.GameView");

            using (new EditorGUILayout.HorizontalScope(EnhancedUIEditorStyles.StatusBox))
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField("v2.0.0", EditorStyles.miniLabel);
                GUILayout.FlexibleSpace();
            }
        }

        private void UpdateContainerCache()
        {
            // Cache FindObjectsOfType results - update only once per second
            if (Time.realtimeSinceStartup - _lastContainerScanTime > CONTAINER_SCAN_INTERVAL)
            {
                _cachedPageContainers = FindObjectsOfType<PageContainer>();
                _cachedModalContainers = FindObjectsOfType<ModalContainer>();
                _cachedSheetContainers = FindObjectsOfType<SheetContainer>();
                _lastContainerScanTime = Time.realtimeSinceStartup;
            }
        }

        private void DrawDashboardStats()
        {
            if (!Application.isPlaying)
            {
                EnhancedUIEditorGUIUtility.DrawMessageBox(
                    "Enter Play Mode to see system status and real-time information.",
                    MessageType.Info
                );
                return;
            }

            // Use cached container references instead of scanning scene every frame
            UpdateContainerCache();
            var pageContainers = _cachedPageContainers ?? new PageContainer[0];
            var modalContainers = _cachedModalContainers ?? new ModalContainer[0];
            var sheetContainers = _cachedSheetContainers ?? new SheetContainer[0];

            int totalScreens = 0;
            foreach (var pc in pageContainers) totalScreens += pc.PageCount;
            foreach (var mc in modalContainers) totalScreens += mc.ModalCount;
            foreach (var sc in sheetContainers) totalScreens += sc.ActiveSheet != null ? 1 : 0;

            // Performance
            var bridge = EditorBridge.Instance;
            var metrics = bridge != null ? bridge.GetRecentPerformanceMetrics(1f) : new EditorBridge.PerformanceMetrics();
            var fpsText = metrics.SampleCount > 0 ? $"{metrics.AverageFPS:F1} FPS" : "N/A";

            // Draw stats row
            EnhancedUIEditorGUIUtility.DrawStatsRow(
                "Containers",
                (pageContainers.Length + modalContainers.Length + sheetContainers.Length).ToString(),
                "Active Screens",
                totalScreens.ToString(),
                "Performance",
                fpsText
            );
        }

        private void DrawQuickActions()
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.LabelField("⚡ Quick Actions", EnhancedUIEditorStyles.BoldLabel);

                GUILayout.Space(5);

                // Row 1
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Setup Wizard", EnhancedUIEditorStyles.PrimaryButton))
                    {
                        SetupWizard.ShowWindow();
                    }

                    if (GUILayout.Button("Screen Generator", EnhancedUIEditorStyles.PrimaryButton))
                    {
                        ScreenGenerator.ShowWindow();
                    }
                }

                GUILayout.Space(5);

                // Row 2
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Health Check", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        HealthCheckWindow.ShowWindow();
                    }

                    if (GUILayout.Button("Event Tracker", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        if (!Application.isPlaying)
                        {
                            EditorUtility.DisplayDialog(
                                "Event Tracker",
                                "Event Tracker is only available in Play Mode.",
                                "OK"
                            );
                        }
                        else
                        {
                            EventTrackerWindow.ShowWindow();
                        }
                    }
                }
            }
        }

        private void DrawActiveContainers()
        {
            _showContainers = EnhancedUIEditorStyles.DrawSectionHeader("📦 Active Containers", _showContainers);

            if (!_showContainers) return;

            GUILayout.Space(5);

            if (!Application.isPlaying)
            {
                EnhancedUIEditorGUIUtility.DrawMessageBox(
                    "Enter Play Mode to see active containers.",
                    MessageType.Info
                );
                return;
            }

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                // Use cached container references instead of scanning scene every frame
                UpdateContainerCache();

                // Page Containers
                var pageContainers = _cachedPageContainers ?? new PageContainer[0];
                if (pageContainers.Length > 0)
                {
                    EditorGUILayout.LabelField("📄 Page Containers:", EditorStyles.boldLabel);

                    foreach (var container in pageContainers)
                    {
                        DrawContainerItem(container, container.PageCount, container.CurrentPage?.name);
                    }

                    GUILayout.Space(5);
                }

                // Modal Containers
                var modalContainers = _cachedModalContainers ?? new ModalContainer[0];
                if (modalContainers.Length > 0)
                {
                    EditorGUILayout.LabelField("🔲 Modal Containers:", EditorStyles.boldLabel);

                    foreach (var container in modalContainers)
                    {
                        DrawContainerItem(container, container.ModalCount, $"{container.ModalCount} modal(s)");
                    }

                    GUILayout.Space(5);
                }

                // Sheet Containers
                var sheetContainers = _cachedSheetContainers ?? new SheetContainer[0];
                if (sheetContainers.Length > 0)
                {
                    EditorGUILayout.LabelField("📑 Sheet Containers:", EditorStyles.boldLabel);

                    foreach (var container in sheetContainers)
                    {
                        DrawContainerItem(container, container.SheetCount, container.ActiveSheet?.name);
                    }
                }

                if (pageContainers.Length == 0 && modalContainers.Length == 0 && sheetContainers.Length == 0)
                {
                    EditorGUILayout.LabelField("No containers found in scene", EditorStyles.centeredGreyMiniLabel);
                }
            }
        }

        private void DrawContainerItem(IUIContainer container, int count, string detail)
        {
            using (new EditorGUILayout.HorizontalScope(GUI.skin.box))
            {
                // Status dot
                var color = container.IsInTransition
                    ? EnhancedUIEditorStyles.TransitionColor
                    : (count > 0 ? EnhancedUIEditorStyles.SuccessColor : EnhancedUIEditorStyles.DisabledColor);

                var originalColor = GUI.backgroundColor;
                GUI.backgroundColor = color;
                GUILayout.Box("", GUILayout.Width(10), GUILayout.Height(20));
                GUI.backgroundColor = originalColor;

                // Name and info
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(container.Name, EditorStyles.boldLabel);
                    EditorGUILayout.LabelField($"{count} item(s) • {detail ?? "None"}", EditorStyles.miniLabel);
                }

                GUILayout.FlexibleSpace();

                // View button
                if (GUILayout.Button("View", GUILayout.Width(50)))
                {
                    Selection.activeObject = container as Object;
                }
            }
        }

        private void DrawRecentEvents()
        {
            var bridge = EditorBridge.Instance;
            if (bridge == null) return;

            _showRecentEvents = EnhancedUIEditorStyles.DrawSectionHeader("📋 Recent Events", _showRecentEvents);

            if (!_showRecentEvents) return;

            GUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                var recentEvents = bridge.EventHistory.TakeLast(5).Reverse().ToList();

                if (recentEvents.Count > 0)
                {
                    foreach (var evt in recentEvents)
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(
                                FormatTime(evt.Timestamp),
                                GUILayout.Width(60)
                            );

                            EditorGUILayout.LabelField(
                                evt.EventName,
                                GUILayout.Width(150)
                            );

                            EditorGUILayout.LabelField(
                                evt.ScreenName,
                                EditorStyles.miniLabel
                            );
                        }
                    }

                    GUILayout.Space(5);

                    if (GUILayout.Button("Open Event Tracker", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        EventTrackerWindow.ShowWindow();
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No events recorded yet", EditorStyles.centeredGreyMiniLabel);
                }
            }
        }

        private void DrawPerformanceOverview()
        {
            var bridge = EditorBridge.Instance;
            if (bridge == null) return;

            _showPerformance = EnhancedUIEditorStyles.DrawSectionHeader("📊 Performance Overview", _showPerformance);

            if (!_showPerformance) return;

            GUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                var metrics = bridge.GetRecentPerformanceMetrics(1f);

                if (metrics.SampleCount > 0)
                {
                    EnhancedUIEditorGUIUtility.DrawPerformanceMetric(
                        "Average FPS",
                        metrics.AverageFPS,
                        55f,
                        40f
                    );

                    EnhancedUIEditorGUIUtility.DrawPerformanceMetric(
                        "Frame Time",
                        metrics.AverageFrameTime,
                        20f,
                        33f,
                        "ms"
                    );

                    EnhancedUIEditorGUIUtility.DrawLabeledField(
                        "Memory Usage",
                        $"{metrics.AverageMemoryUsage:F2} MB"
                    );

                    GUILayout.Space(5);

                    EnhancedUIEditorGUIUtility.DrawLabeledField(
                        "Total Transitions",
                        bridge.TransitionHistory.Count(t => t.IsCompleted).ToString()
                    );
                }
                else
                {
                    EditorGUILayout.LabelField("No performance data yet", EditorStyles.centeredGreyMiniLabel);
                }
            }
        }

        private void DrawToolsSection()
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.LabelField("🛠 Debug & Analysis Tools", EnhancedUIEditorStyles.BoldLabel);

                GUILayout.Space(5);

                // Debug tools
                EditorGUILayout.LabelField("Debug Tools:", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Event Tracker", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        if (Application.isPlaying)
                            EventTrackerWindow.ShowWindow();
                        else
                            ShowPlayModeOnlyDialog("Event Tracker");
                    }

                    if (GUILayout.Button("Transition Debugger", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        if (Application.isPlaying)
                            TransitionDebuggerWindow.ShowWindow();
                        else
                            ShowPlayModeOnlyDialog("Transition Debugger");
                    }
                }

                GUILayout.Space(5);

                // Analysis tools
                EditorGUILayout.LabelField("Analysis Tools:", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Health Check", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        HealthCheckWindow.ShowWindow();
                    }

                    if (GUILayout.Button("Performance Analyzer", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        if (Application.isPlaying)
                            PerformanceAnalyzerWindow.ShowWindow();
                        else
                            ShowPlayModeOnlyDialog("Performance Analyzer");
                    }
                }
            }
        }

        private void DrawFooter()
        {
            EnhancedUIEditorStyles.DrawSeparator();

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Documentation", EditorStyles.miniButton))
                {
                    Application.OpenURL("https://github.com/yourstudio/enhanced-ui-framework");
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Settings", EditorStyles.miniButton))
                {
                    Selection.activeObject = EnhancedUISettings.Instance;
                }

                if (GUILayout.Button("Refresh", EditorStyles.miniButton))
                {
                    Repaint();
                }
            }
        }

        private string FormatTime(float timestamp)
        {
            var timeSpan = System.TimeSpan.FromSeconds(timestamp);
            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }

        private void ShowPlayModeOnlyDialog(string toolName)
        {
            EditorUtility.DisplayDialog(
                toolName,
                $"{toolName} is only available in Play Mode.\n\nPlease enter Play Mode to use this tool.",
                "OK"
            );
        }
    }
}
