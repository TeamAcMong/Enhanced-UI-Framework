using System.Linq;
using UnityEngine;
using UnityEditor;
using EnhancedUI.Editor.Utilities;
using EnhancedUI.Utilities;

namespace EnhancedUI.Editor.Tools
{
    /// <summary>
    /// Performance analysis tool with real-time graphs and metrics
    /// </summary>
    public class PerformanceAnalyzerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private bool _showGraph = true;
        private bool _showMetrics = true;
        private bool _showBottlenecks = true;
        private int _graphTimeWindow = 5; // seconds

        // Throttled repaint
        private double _lastRepaintTime;
        private const double REPAINT_INTERVAL = 0.1; // 10 times per second

        [MenuItem("Tools/Enhanced UI/Analysis Tools/Performance Analyzer", false, 201)]
        public static void ShowWindow()
        {
            var window = GetWindow<PerformanceAnalyzerWindow>("Performance Analyzer");
            window.minSize = new Vector2(500, 500);
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

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Header
            DrawHeader(bridge);

            GUILayout.Space(10);

            // FPS Graph
            DrawFPSGraph(bridge);

            GUILayout.Space(10);

            // Metrics
            DrawMetrics(bridge);

            GUILayout.Space(10);

            // Bottlenecks
            DrawBottlenecks(bridge);

            GUILayout.Space(10);

            // Actions
            DrawActions(bridge);

            EditorGUILayout.EndScrollView();

            // Throttled repaint (10 times/sec instead of unlimited)
            if (EditorApplication.timeSinceStartup - _lastRepaintTime > REPAINT_INTERVAL)
            {
                Repaint();
                _lastRepaintTime = EditorApplication.timeSinceStartup;
            }
        }

        private void DrawNotPlayingMessage()
        {
            EnhancedUIEditorGUIUtility.DrawWindowHeader("Performance Analyzer", "Profiler.CPU");

            GUILayout.Space(20);

            EnhancedUIEditorGUIUtility.DrawMessageBox(
                "Performance Analyzer is only available in Play Mode.\n\nEnter Play Mode to analyze real-time performance metrics.",
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
                var icon = EditorGUIUtility.IconContent("Profiler.CPU");
                GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));

                GUILayout.Label("Performance Analyzer", EnhancedUIEditorStyles.HeaderLabel);

                GUILayout.FlexibleSpace();

                var metrics = bridge.GetRecentPerformanceMetrics(1f);

                if (metrics.SampleCount > 0)
                {
                    var fpsColor = metrics.AverageFPS >= 55f
                        ? EnhancedUIEditorStyles.SuccessColor
                        : (metrics.AverageFPS >= 40f ? EnhancedUIEditorStyles.WarningColor : EnhancedUIEditorStyles.ErrorColor);

                    EnhancedUIEditorStyles.DrawStatusBadge($"{metrics.AverageFPS:F1} FPS", fpsColor, 80);
                }
            }
        }

        private void DrawFPSGraph(EditorBridge bridge)
        {
            _showGraph = EnhancedUIEditorStyles.DrawSectionHeader("📈 FPS Graph", _showGraph);

            if (!_showGraph) return;

            GUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                // Time window selector
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Time Window:", GUILayout.Width(90));

                    if (GUILayout.Toggle(_graphTimeWindow == 5, "5s", EditorStyles.miniButton, GUILayout.Width(40)))
                        _graphTimeWindow = 5;
                    if (GUILayout.Toggle(_graphTimeWindow == 10, "10s", EditorStyles.miniButton, GUILayout.Width(40)))
                        _graphTimeWindow = 10;
                    if (GUILayout.Toggle(_graphTimeWindow == 30, "30s", EditorStyles.miniButton, GUILayout.Width(40)))
                        _graphTimeWindow = 30;

                    GUILayout.FlexibleSpace();
                }

                GUILayout.Space(5);

                // Draw FPS graph
                var cutoffTime = Time.realtimeSinceStartup - _graphTimeWindow;
                var recentSnapshots = bridge.PerformanceSnapshots
                    .Where(s => s.Timestamp >= cutoffTime)
                    .ToArray();

                if (recentSnapshots.Length > 0)
                {
                    var fpsValues = recentSnapshots.Select(s => s.FPS).ToArray();
                    EnhancedUIEditorGUIUtility.DrawGraph(
                        fpsValues,
                        0f,
                        75f,
                        EnhancedUIEditorStyles.SuccessColor,
                        150f
                    );

                    // Labels
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"0s", EditorStyles.miniLabel, GUILayout.Width(40));
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField($"{_graphTimeWindow}s", EditorStyles.miniLabel, GUILayout.Width(40));
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("No data yet...", EditorStyles.centeredGreyMiniLabel);
                }
            }
        }

        private void DrawMetrics(EditorBridge bridge)
        {
            _showMetrics = EnhancedUIEditorStyles.DrawSectionHeader("📊 Performance Metrics", _showMetrics);

            if (!_showMetrics) return;

            GUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                var metrics = bridge.GetRecentPerformanceMetrics(5f);

                if (metrics.SampleCount == 0)
                {
                    EditorGUILayout.LabelField("No metrics available yet...", EditorStyles.centeredGreyMiniLabel);
                    return;
                }

                // FPS Metrics
                EditorGUILayout.LabelField("Frame Rate:", EditorStyles.boldLabel);

                EnhancedUIEditorGUIUtility.DrawPerformanceMetric(
                    "Average FPS",
                    metrics.AverageFPS,
                    55f,
                    40f
                );

                EnhancedUIEditorGUIUtility.DrawPerformanceMetric(
                    "Min FPS",
                    metrics.MinFPS,
                    50f,
                    30f
                );

                EnhancedUIEditorGUIUtility.DrawPerformanceMetric(
                    "Max FPS",
                    metrics.MaxFPS,
                    60f,
                    55f
                );

                GUILayout.Space(10);

                // Frame Time Metrics
                EditorGUILayout.LabelField("Frame Time:", EditorStyles.boldLabel);

                EnhancedUIEditorGUIUtility.DrawPerformanceMetric(
                    "Average",
                    metrics.AverageFrameTime,
                    20f,
                    33f,
                    "ms"
                );

                EnhancedUIEditorGUIUtility.DrawPerformanceMetric(
                    "Min",
                    metrics.MinFrameTime,
                    16f,
                    20f,
                    "ms"
                );

                EnhancedUIEditorGUIUtility.DrawPerformanceMetric(
                    "Max",
                    metrics.MaxFrameTime,
                    25f,
                    40f,
                    "ms"
                );

                GUILayout.Space(10);

                // Memory
                EditorGUILayout.LabelField("Memory:", EditorStyles.boldLabel);

                EnhancedUIEditorGUIUtility.DrawLabeledField(
                    "Average Usage",
                    $"{metrics.AverageMemoryUsage:F2} MB"
                );

                EnhancedUIEditorGUIUtility.DrawLabeledField(
                    "Sample Count",
                    metrics.SampleCount.ToString()
                );
            }
        }

        private void DrawBottlenecks(EditorBridge bridge)
        {
            _showBottlenecks = EnhancedUIEditorStyles.DrawSectionHeader("⚠ Detected Issues", _showBottlenecks);

            if (!_showBottlenecks) return;

            GUILayout.Space(5);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                var metrics = bridge.GetRecentPerformanceMetrics(5f);

                if (metrics.SampleCount == 0)
                {
                    EditorGUILayout.LabelField("No data to analyze", EditorStyles.centeredGreyMiniLabel);
                    return;
                }

                bool hasIssues = false;

                // Check FPS issues
                if (metrics.MinFPS < 40f)
                {
                    hasIssues = true;
                    EnhancedUIEditorGUIUtility.DrawMessageBox(
                        $"⚠ Low FPS Detected: Minimum FPS dropped to {metrics.MinFPS:F1}. Consider optimizing transitions or reducing screen complexity.",
                        MessageType.Warning
                    );
                    GUILayout.Space(5);
                }

                // Check frame time spikes
                if (metrics.MaxFrameTime > 33f) // 30 FPS threshold
                {
                    hasIssues = true;
                    EnhancedUIEditorGUIUtility.DrawMessageBox(
                        $"⚠ Frame Time Spike: Maximum frame time was {metrics.MaxFrameTime:F1}ms. Check for heavy operations during transitions.",
                        MessageType.Warning
                    );
                    GUILayout.Space(5);
                }

                // Check transition performance
                var slowTransitions = bridge.TransitionHistory
                    .Where(t => t.IsCompleted && t.Duration > 1f)
                    .ToList();

                if (slowTransitions.Count > 0)
                {
                    hasIssues = true;
                    var slowest = slowTransitions.OrderByDescending(t => t.Duration).First();

                    EnhancedUIEditorGUIUtility.DrawMessageBox(
                        $"⚠ Slow Transitions: {slowTransitions.Count} transition(s) took longer than 1 second. Slowest: {slowest.FromScreen} → {slowest.ToScreen} ({slowest.Duration:F2}s)",
                        MessageType.Warning
                    );
                    GUILayout.Space(5);
                }

                // Check memory
                if (metrics.AverageMemoryUsage > 200f)
                {
                    hasIssues = true;
                    EnhancedUIEditorGUIUtility.DrawMessageBox(
                        $"ℹ High Memory Usage: Average memory usage is {metrics.AverageMemoryUsage:F2}MB. Consider implementing screen pooling or asset unloading.",
                        MessageType.Info
                    );
                }

                if (!hasIssues)
                {
                    EnhancedUIEditorGUIUtility.DrawMessageBox(
                        "✓ No performance issues detected. System is running smoothly!",
                        MessageType.Info
                    );
                }
            }
        }

        private void DrawActions(EditorBridge bridge)
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.LabelField("⚡ Actions", EnhancedUIEditorStyles.BoldLabel);

                GUILayout.Space(5);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Clear Data", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        bridge.Clear();
                    }

                    if (GUILayout.Button("Generate Report", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        GenerateReport(bridge);
                    }

                    if (GUILayout.Button("Open Settings", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        Selection.activeObject = EnhancedUISettings.Instance;
                    }
                }
            }
        }

        private void GenerateReport(EditorBridge bridge)
        {
            var metrics = bridge.GetRecentPerformanceMetrics(30f);

            if (metrics.SampleCount == 0)
            {
                EditorUtility.DisplayDialog(
                    "No Data",
                    "Not enough performance data to generate a report.",
                    "OK"
                );
                return;
            }

            var report = $@"Enhanced UI Framework - Performance Report
Generated: {System.DateTime.Now}

=== Frame Rate ===
Average FPS: {metrics.AverageFPS:F2}
Min FPS: {metrics.MinFPS:F2}
Max FPS: {metrics.MaxFPS:F2}

=== Frame Time ===
Average: {metrics.AverageFrameTime:F2}ms
Min: {metrics.MinFrameTime:F2}ms
Max: {metrics.MaxFrameTime:F2}ms

=== Memory ===
Average Usage: {metrics.AverageMemoryUsage:F2} MB

=== Transitions ===
Total Completed: {bridge.TransitionHistory.Count(t => t.IsCompleted)}
Average Duration: {(bridge.TransitionHistory.Any(t => t.IsCompleted) ? bridge.TransitionHistory.Where(t => t.IsCompleted).Average(t => t.Duration) : 0):F3}s

=== Sample Size ===
Samples Analyzed: {metrics.SampleCount}
Time Window: 30 seconds
";

            Debug.Log(report);

            EditorUtility.DisplayDialog(
                "Report Generated",
                "Performance report has been logged to console.\n\nCheck the Console window for details.",
                "OK"
            );
        }
    }
}
