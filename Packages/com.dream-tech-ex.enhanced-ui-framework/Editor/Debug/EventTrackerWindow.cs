using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using EnhancedUI.Editor.Utilities;
using EnhancedUI.Utilities;

namespace EnhancedUI.Editor.Tools
{
    /// <summary>
    /// Real-time event tracking window for debugging
    /// </summary>
    public class EventTrackerWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private string _searchText = "";
        private int _selectedCategoryIndex = 0;
        private string[] _categories = { "All", "Lifecycle", "AssetLoading", "Transitions" };
        private int _selectedEventIndex = -1;
        private bool _autoScroll = true;
        private bool _showStackTraces = false;

        private const int MAX_VISIBLE_EVENTS = 100;

        // Throttled repaint
        private double _lastRepaintTime;
        private const double REPAINT_INTERVAL = 0.1; // 10 times per second

        [MenuItem("Tools/Enhanced UI/Debug Tools/Event Tracker", false, 100)]
        public static void ShowWindow()
        {
            var window = GetWindow<EventTrackerWindow>("Event Tracker");
            window.minSize = new Vector2(600, 400);
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
                EditorGUILayout.HelpBox("EditorBridge not available. Make sure it's initialized.", MessageType.Warning);
                return;
            }

            // Header
            DrawHeader();

            GUILayout.Space(5);

            // Toolbar
            DrawToolbar(bridge);

            GUILayout.Space(5);

            // Event list
            DrawEventList(bridge);

            GUILayout.Space(5);

            // Event details
            if (_selectedEventIndex >= 0 && _selectedEventIndex < bridge.EventHistory.Count)
            {
                DrawEventDetails(bridge.EventHistory[_selectedEventIndex]);
            }

            // Throttled repaint (10 times/sec instead of unlimited)
            if (EditorApplication.timeSinceStartup - _lastRepaintTime > REPAINT_INTERVAL)
            {
                Repaint();
                _lastRepaintTime = EditorApplication.timeSinceStartup;
            }
        }

        private void DrawNotPlayingMessage()
        {
            EnhancedUIEditorGUIUtility.DrawWindowHeader("Event Tracker", "console.infoicon");

            GUILayout.Space(20);

            EnhancedUIEditorGUIUtility.DrawMessageBox(
                "Event Tracker is only available in Play Mode.\n\nEnter Play Mode to track lifecycle events, transitions, and asset loading in real-time.",
                MessageType.Info
            );

            GUILayout.Space(10);

            if (GUILayout.Button("Enter Play Mode", EnhancedUIEditorStyles.PrimaryButton))
            {
                EditorApplication.isPlaying = true;
            }
        }

        private void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EnhancedUIEditorStyles.HeaderBox))
            {
                var icon = EditorGUIUtility.IconContent("console.infoicon");
                GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));

                GUILayout.Label("Event Tracker", EnhancedUIEditorStyles.HeaderLabel);

                GUILayout.FlexibleSpace();

                var bridge = EditorBridge.Instance;
                if (bridge != null)
                {
                    EnhancedUIEditorStyles.DrawStatusBadge($"{bridge.EventHistory.Count} Events", EnhancedUIEditorStyles.InfoColor, 100);
                }
            }
        }

        private void DrawToolbar(EditorBridge bridge)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                // Category filter
                GUILayout.Label("Filter:", GUILayout.Width(40));
                _selectedCategoryIndex = EditorGUILayout.Popup(_selectedCategoryIndex, _categories, EditorStyles.toolbarPopup, GUILayout.Width(120));

                GUILayout.Space(10);

                // Search
                _searchText = EnhancedUIEditorGUIUtility.DrawSearchField(_searchText);

                GUILayout.FlexibleSpace();

                // Options
                _autoScroll = GUILayout.Toggle(_autoScroll, "Auto-scroll", EditorStyles.toolbarButton, GUILayout.Width(80));

                GUILayout.Space(5);

                // Clear button
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    if (EditorUtility.DisplayDialog(
                        "Clear Events",
                        "Are you sure you want to clear all tracked events?",
                        "Yes",
                        "No"))
                    {
                        bridge.Clear();
                        _selectedEventIndex = -1;
                    }
                }

                // Export button
                if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    ExportEvents(bridge);
                }
            }
        }

        private void DrawEventList(EditorBridge bridge)
        {
            // Filter events
            var filteredEvents = FilterEvents(bridge.EventHistory);

            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                // Header row
                using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
                {
                    EditorGUILayout.LabelField("Time", EditorStyles.boldLabel, GUILayout.Width(60));
                    EditorGUILayout.LabelField("Event", EditorStyles.boldLabel, GUILayout.Width(150));
                    EditorGUILayout.LabelField("Container", EditorStyles.boldLabel, GUILayout.Width(120));
                    EditorGUILayout.LabelField("Screen", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("Duration", EditorStyles.boldLabel, GUILayout.Width(70));
                }

                // Event list with scroll
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(250));

                if (filteredEvents.Count == 0)
                {
                    GUILayout.Space(10);
                    EditorGUILayout.LabelField("No events to display", EditorStyles.centeredGreyMiniLabel);
                }
                else
                {
                    // Show only last N events
                    var eventsToShow = filteredEvents.TakeLast(MAX_VISIBLE_EVENTS).ToList();

                    for (int i = 0; i < eventsToShow.Count; i++)
                    {
                        var evt = eventsToShow[i];
                        // Find index in the original list
                        int globalIndex = -1;
                        for (int j = 0; j < bridge.EventHistory.Count; j++)
                        {
                            if (bridge.EventHistory[j].Timestamp == evt.Timestamp &&
                                bridge.EventHistory[j].EventName == evt.EventName)
                            {
                                globalIndex = j;
                                break;
                            }
                        }

                        var isSelected = globalIndex == _selectedEventIndex;

                        DrawEventRow(evt, globalIndex, isSelected);
                    }

                    // Auto-scroll to bottom
                    if (_autoScroll && Event.current.type == EventType.Repaint)
                    {
                        _scrollPosition.y = float.MaxValue;
                    }
                }

                EditorGUILayout.EndScrollView();

                // Stats footer
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(
                        $"Showing {Math.Min(filteredEvents.Count, MAX_VISIBLE_EVENTS)} of {filteredEvents.Count} events",
                        EditorStyles.miniLabel
                    );

                    GUILayout.FlexibleSpace();

                    if (filteredEvents.Count > MAX_VISIBLE_EVENTS)
                    {
                        EditorGUILayout.LabelField(
                            $"(Last {MAX_VISIBLE_EVENTS} shown)",
                            EditorStyles.miniLabel
                        );
                    }
                }
            }
        }

        private void DrawEventRow(EditorBridge.LifecycleEventRecord evt, int index, bool isSelected)
        {
            var originalColor = GUI.backgroundColor;

            if (isSelected)
            {
                GUI.backgroundColor = new Color(0.3f, 0.6f, 1f, 0.5f);
            }

            using (new EditorGUILayout.HorizontalScope(GUI.skin.box, GUILayout.Height(20)))
            {
                // Time
                EditorGUILayout.LabelField(FormatTime(evt.Timestamp), EditorStyles.miniLabel, GUILayout.Width(60));

                // Event name with color coding
                var eventColor = GetEventColor(evt.Category);
                var originalContentColor = GUI.contentColor;
                GUI.contentColor = eventColor;
                EditorGUILayout.LabelField(evt.EventName, EditorStyles.miniLabel, GUILayout.Width(150));
                GUI.contentColor = originalContentColor;

                // Container
                EditorGUILayout.LabelField(evt.ContainerName, EditorStyles.miniLabel, GUILayout.Width(120));

                // Screen
                EditorGUILayout.LabelField(evt.ScreenName, EditorStyles.miniLabel);

                // Duration
                if (evt.Duration > 0)
                {
                    EditorGUILayout.LabelField($"{evt.Duration:F3}s", EditorStyles.miniLabel, GUILayout.Width(70));
                }
                else
                {
                    EditorGUILayout.LabelField("-", EditorStyles.miniLabel, GUILayout.Width(70));
                }

                // Make row clickable
                var rect = GUILayoutUtility.GetLastRect();
                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
                {
                    _selectedEventIndex = index;
                    Event.current.Use();
                }
            }

            GUI.backgroundColor = originalColor;
        }

        private void DrawEventDetails(EditorBridge.LifecycleEventRecord evt)
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.LabelField("Event Details", EnhancedUIEditorStyles.BoldLabel);

                GUILayout.Space(5);

                EnhancedUIEditorGUIUtility.DrawLabeledField("Event Name", evt.EventName);
                EnhancedUIEditorGUIUtility.DrawLabeledField("Category", evt.Category);
                EnhancedUIEditorGUIUtility.DrawLabeledField("Container", evt.ContainerName);
                EnhancedUIEditorGUIUtility.DrawLabeledField("Screen", evt.ScreenName);
                EnhancedUIEditorGUIUtility.DrawLabeledField("Timestamp", FormatTime(evt.Timestamp));

                if (evt.Duration > 0)
                {
                    EnhancedUIEditorGUIUtility.DrawLabeledField("Duration", $"{evt.Duration:F3}s");
                }

                if (!string.IsNullOrEmpty(evt.AdditionalInfo))
                {
                    GUILayout.Space(5);
                    EditorGUILayout.LabelField("Additional Info:", EditorStyles.boldLabel);
                    EditorGUILayout.TextArea(evt.AdditionalInfo, EditorStyles.textArea, GUILayout.Height(40));
                }

                // Stack trace
                if (!string.IsNullOrEmpty(evt.StackTrace))
                {
                    GUILayout.Space(5);

                    _showStackTraces = EditorGUILayout.Foldout(_showStackTraces, "Stack Trace", true);

                    if (_showStackTraces)
                    {
                        EditorGUILayout.TextArea(evt.StackTrace, EditorStyles.textArea, GUILayout.Height(100));
                    }
                }
            }
        }

        private List<EditorBridge.LifecycleEventRecord> FilterEvents(IReadOnlyList<EditorBridge.LifecycleEventRecord> events)
        {
            var filtered = new List<EditorBridge.LifecycleEventRecord>(events);

            // Category filter
            if (_selectedCategoryIndex > 0)
            {
                var selectedCategory = _categories[_selectedCategoryIndex];
                filtered = filtered.Where(e => e.Category == selectedCategory).ToList();
            }

            // Search filter
            if (!string.IsNullOrEmpty(_searchText))
            {
                var searchLower = _searchText.ToLower();
                filtered = filtered.Where(e =>
                    e.EventName.ToLower().Contains(searchLower) ||
                    e.ContainerName.ToLower().Contains(searchLower) ||
                    e.ScreenName.ToLower().Contains(searchLower)
                ).ToList();
            }

            return filtered;
        }

        private void ExportEvents(EditorBridge bridge)
        {
            var path = EditorUtility.SaveFilePanel(
                "Export Events",
                "",
                "enhanced_ui_events.csv",
                "csv"
            );

            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                var sb = new StringBuilder();

                // Header
                sb.AppendLine("Timestamp,Event,Category,Container,Screen,Duration,AdditionalInfo");

                // Events
                var filteredEvents = FilterEvents(bridge.EventHistory);
                foreach (var evt in filteredEvents)
                {
                    sb.AppendLine($"{evt.Timestamp},{evt.EventName},{evt.Category},{evt.ContainerName},{evt.ScreenName},{evt.Duration},{evt.AdditionalInfo}");
                }

                System.IO.File.WriteAllText(path, sb.ToString());

                EditorUtility.DisplayDialog(
                    "Export Complete",
                    $"Exported {filteredEvents.Count} events to:\n{path}",
                    "OK"
                );
            }
            catch (Exception ex)
            {
                EditorUtility.DisplayDialog(
                    "Export Failed",
                    $"Failed to export events:\n{ex.Message}",
                    "OK"
                );
            }
        }

        private string FormatTime(float timestamp)
        {
            var timeSpan = TimeSpan.FromSeconds(timestamp);
            return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";
        }

        private Color GetEventColor(string category)
        {
            switch (category)
            {
                case "Lifecycle":
                    return EnhancedUIEditorStyles.InfoColor;
                case "AssetLoading":
                    return EnhancedUIEditorStyles.WarningColor;
                case "Transitions":
                    return new Color(0.7f, 0.4f, 0.9f); // Purple
                default:
                    return Color.white;
            }
        }
    }
}
