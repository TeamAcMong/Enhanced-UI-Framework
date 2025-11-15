using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EnhancedUI.Editor.Utilities
{
    /// <summary>
    /// Utility methods for Enhanced UI Framework editor GUI
    /// </summary>
    public static class EnhancedUIEditorGUIUtility
    {
        /// <summary>
        /// Draw a stat box with label and value
        /// </summary>
        public static void DrawStatBox(string label, string value, Color? color = null)
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.StatusBox))
            {
                if (color.HasValue)
                {
                    var originalColor = GUI.contentColor;
                    GUI.contentColor = color.Value;
                    EditorGUILayout.LabelField(value, EnhancedUIEditorStyles.HeaderLabel);
                    GUI.contentColor = originalColor;
                }
                else
                {
                    EditorGUILayout.LabelField(value, EnhancedUIEditorStyles.HeaderLabel);
                }

                EditorGUILayout.LabelField(label, EnhancedUIEditorStyles.CenteredLabel);
            }
        }

        /// <summary>
        /// Draw a dashboard stat row (3 stats side by side)
        /// </summary>
        public static void DrawStatsRow(string label1, string value1, string label2, string value2, string label3, string value3)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawStatBox(label1, value1);
                GUILayout.Space(5);
                DrawStatBox(label2, value2);
                GUILayout.Space(5);
                DrawStatBox(label3, value3);
            }
        }

        /// <summary>
        /// Draw an info/warning/error message box
        /// </summary>
        public static void DrawMessageBox(string message, MessageType type)
        {
            GUIStyle style;
            switch (type)
            {
                case MessageType.Info:
                    style = EnhancedUIEditorStyles.InfoBox;
                    break;
                case MessageType.Warning:
                    style = EnhancedUIEditorStyles.WarningBox;
                    break;
                case MessageType.Error:
                    style = EnhancedUIEditorStyles.ErrorBox;
                    break;
                default:
                    style = EditorStyles.helpBox;
                    break;
            }

            EditorGUILayout.LabelField(message, style);
        }

        /// <summary>
        /// Draw a button row with multiple buttons
        /// </summary>
        public static int DrawButtonRow(params string[] buttons)
        {
            int clickedIndex = -1;

            using (new EditorGUILayout.HorizontalScope())
            {
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (GUILayout.Button(buttons[i], EnhancedUIEditorStyles.SecondaryButton))
                    {
                        clickedIndex = i;
                    }

                    if (i < buttons.Length - 1)
                    {
                        GUILayout.Space(5);
                    }
                }
            }

            return clickedIndex;
        }

        /// <summary>
        /// Draw a labeled field with optional tooltip
        /// </summary>
        public static void DrawLabeledField(string label, string value, string tooltip = null)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (string.IsNullOrEmpty(tooltip))
                {
                    EditorGUILayout.LabelField(label, GUILayout.Width(150));
                }
                else
                {
                    EditorGUILayout.LabelField(new GUIContent(label, tooltip), GUILayout.Width(150));
                }

                EditorGUILayout.LabelField(value, EnhancedUIEditorStyles.BoldLabel);
            }
        }

        /// <summary>
        /// Draw a list of items with optional action buttons
        /// </summary>
        public static void DrawList<T>(IList<T> list, Func<T, string> labelFunc, Action<T> onSelect = null, Action<T> onRemove = null)
        {
            if (list == null || list.Count == 0)
            {
                EditorGUILayout.LabelField("No items", EditorStyles.centeredGreyMiniLabel);
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];

                using (new EditorGUILayout.HorizontalScope(EnhancedUIEditorStyles.SectionBox))
                {
                    EditorGUILayout.LabelField(labelFunc(item));

                    if (onSelect != null && GUILayout.Button("View", GUILayout.Width(50)))
                    {
                        onSelect(item);
                    }

                    if (onRemove != null && GUILayout.Button("×", GUILayout.Width(25)))
                    {
                        onRemove(item);
                    }
                }
            }
        }

        /// <summary>
        /// Draw a toolbar with tabs
        /// </summary>
        public static int DrawToolbar(int selected, params string[] tabs)
        {
            return GUILayout.Toolbar(selected, tabs, GUILayout.Height(25));
        }

        /// <summary>
        /// Draw a search field
        /// </summary>
        public static string DrawSearchField(string searchText, string placeholder = "Search...")
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Label("🔍", GUILayout.Width(20));
                var newText = EditorGUILayout.TextField(searchText, EditorStyles.toolbarSearchField);

                if (GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(20)))
                {
                    newText = "";
                    GUI.FocusControl(null);
                }

                return newText;
            }
        }

        /// <summary>
        /// Draw a performance metric with color based on value
        /// </summary>
        public static void DrawPerformanceMetric(string label, float value, float goodThreshold, float badThreshold, string unit = "")
        {
            Color color;
            if (value <= goodThreshold)
                color = EnhancedUIEditorStyles.SuccessColor;
            else if (value <= badThreshold)
                color = EnhancedUIEditorStyles.WarningColor;
            else
                color = EnhancedUIEditorStyles.ErrorColor;

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(150));

                var originalColor = GUI.contentColor;
                GUI.contentColor = color;
                EditorGUILayout.LabelField($"{value:F2}{unit}", EnhancedUIEditorStyles.BoldLabel);
                GUI.contentColor = originalColor;
            }
        }

        /// <summary>
        /// Draw a status indicator dot
        /// </summary>
        public static void DrawStatusDot(bool active, float size = 10f)
        {
            var color = active ? EnhancedUIEditorStyles.ActiveColor : EnhancedUIEditorStyles.DisabledColor;
            var rect = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
            EditorGUI.DrawRect(rect, color);
        }

        /// <summary>
        /// Draw a simple graph
        /// </summary>
        public static void DrawGraph(float[] values, float min, float max, Color color, float height = 100)
        {
            if (values == null || values.Length == 0)
                return;

            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height), GUILayout.ExpandWidth(true));

            // Background
            EditorGUI.DrawRect(rect, new Color(0.1f, 0.1f, 0.1f, 0.5f));

            // Draw grid lines
            for (int i = 0; i <= 4; i++)
            {
                float y = rect.y + (rect.height * i / 4f);
                var lineRect = new Rect(rect.x, y, rect.width, 1);
                EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 0.3f));
            }

            // Draw graph
            if (values.Length > 1)
            {
                float range = max - min;
                if (range <= 0) range = 1;

                float stepX = rect.width / (values.Length - 1);

                for (int i = 0; i < values.Length - 1; i++)
                {
                    float x1 = rect.x + i * stepX;
                    float y1 = rect.y + rect.height - ((values[i] - min) / range * rect.height);
                    float x2 = rect.x + (i + 1) * stepX;
                    float y2 = rect.y + rect.height - ((values[i + 1] - min) / range * rect.height);

                    Drawing.DrawLine(new Vector2(x1, y1), new Vector2(x2, y2), color, 2f);
                }
            }
        }

        /// <summary>
        /// Helper class for drawing lines
        /// </summary>
        private static class Drawing
        {
            private static Texture2D _texture;
            private static GUIStyle _style;

            private static void Initialize()
            {
                if (_texture == null)
                {
                    _texture = new Texture2D(1, 1);
                    _texture.SetPixel(0, 0, Color.white);
                    _texture.Apply();
                }

                if (_style == null)
                {
                    _style = new GUIStyle();
                }
            }

            public static void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f)
            {
                Initialize();

                var originalColor = GUI.color;
                GUI.color = color;

                float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
                float length = Vector2.Distance(start, end);

                GUIUtility.RotateAroundPivot(angle, start);
                GUI.DrawTexture(new Rect(start.x, start.y - thickness / 2, length, thickness), _texture);
                GUIUtility.RotateAroundPivot(-angle, start);

                GUI.color = originalColor;
            }
        }

        /// <summary>
        /// Draw a header with icon
        /// </summary>
        public static void DrawWindowHeader(string title, string iconName = "")
        {
            using (new EditorGUILayout.HorizontalScope(EnhancedUIEditorStyles.HeaderBox))
            {
                if (!string.IsNullOrEmpty(iconName))
                {
                    var icon = EditorGUIUtility.IconContent(iconName);
                    GUILayout.Label(icon, GUILayout.Width(20), GUILayout.Height(20));
                }

                GUILayout.Label(title, EnhancedUIEditorStyles.HeaderLabel);
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(5);
        }

        /// <summary>
        /// Begin a foldout section
        /// </summary>
        public static bool BeginFoldoutSection(string title, bool foldout, out bool changed)
        {
            changed = false;
            var newFoldout = EnhancedUIEditorStyles.DrawSectionHeader(title, foldout);
            changed = newFoldout != foldout;
            return newFoldout;
        }

        /// <summary>
        /// Draw a horizontal line separator
        /// </summary>
        public static void DrawSeparator()
        {
            EnhancedUIEditorStyles.DrawSeparator();
        }
    }
}
