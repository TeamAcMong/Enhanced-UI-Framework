using UnityEngine;
using UnityEditor;

namespace EnhancedUI.Editor.Utilities
{
    /// <summary>
    /// Shared styles and colors for Enhanced UI Framework editor
    /// </summary>
    public static class EnhancedUIEditorStyles
    {
        // Colors
        public static readonly Color SuccessColor = new Color(0.3f, 0.8f, 0.3f);
        public static readonly Color WarningColor = new Color(1f, 0.75f, 0.05f);
        public static readonly Color ErrorColor = new Color(0.95f, 0.25f, 0.25f);
        public static readonly Color InfoColor = new Color(0.13f, 0.59f, 0.95f);
        public static readonly Color DisabledColor = new Color(0.62f, 0.62f, 0.62f);
        public static readonly Color ActiveColor = new Color(0.3f, 0.8f, 0.3f);
        public static readonly Color TransitionColor = new Color(1f, 0.75f, 0.05f);

        // Box styles
        private static GUIStyle _headerBoxStyle;
        private static GUIStyle _sectionBoxStyle;
        private static GUIStyle _infoBoxStyle;
        private static GUIStyle _warningBoxStyle;
        private static GUIStyle _errorBoxStyle;
        private static GUIStyle _statusBoxStyle;

        // Text styles
        private static GUIStyle _headerLabelStyle;
        private static GUIStyle _boldLabelStyle;
        private static GUIStyle _centeredLabelStyle;
        private static GUIStyle _statusLabelStyle;
        private static GUIStyle _metricLabelStyle;

        // Button styles
        private static GUIStyle _primaryButtonStyle;
        private static GUIStyle _secondaryButtonStyle;
        private static GUIStyle _iconButtonStyle;

        public static GUIStyle HeaderBox
        {
            get
            {
                if (_headerBoxStyle == null)
                {
                    _headerBoxStyle = new GUIStyle(GUI.skin.box)
                    {
                        padding = new RectOffset(10, 10, 10, 10),
                        margin = new RectOffset(0, 0, 5, 5),
                        fontStyle = FontStyle.Bold
                    };
                }
                return _headerBoxStyle;
            }
        }

        public static GUIStyle SectionBox
        {
            get
            {
                if (_sectionBoxStyle == null)
                {
                    _sectionBoxStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(8, 8, 8, 8),
                        margin = new RectOffset(0, 0, 5, 5)
                    };
                }
                return _sectionBoxStyle;
            }
        }

        public static GUIStyle InfoBox
        {
            get
            {
                if (_infoBoxStyle == null)
                {
                    _infoBoxStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(10, 10, 10, 10),
                        margin = new RectOffset(0, 0, 3, 3),
                        normal = { textColor = InfoColor },
                        fontSize = 11
                    };
                }
                return _infoBoxStyle;
            }
        }

        public static GUIStyle WarningBox
        {
            get
            {
                if (_warningBoxStyle == null)
                {
                    _warningBoxStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(10, 10, 10, 10),
                        margin = new RectOffset(0, 0, 3, 3),
                        normal = { textColor = WarningColor },
                        fontSize = 11
                    };
                }
                return _warningBoxStyle;
            }
        }

        public static GUIStyle ErrorBox
        {
            get
            {
                if (_errorBoxStyle == null)
                {
                    _errorBoxStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(10, 10, 10, 10),
                        margin = new RectOffset(0, 0, 3, 3),
                        normal = { textColor = ErrorColor },
                        fontSize = 11
                    };
                }
                return _errorBoxStyle;
            }
        }

        public static GUIStyle StatusBox
        {
            get
            {
                if (_statusBoxStyle == null)
                {
                    _statusBoxStyle = new GUIStyle(GUI.skin.box)
                    {
                        padding = new RectOffset(10, 10, 8, 8),
                        margin = new RectOffset(2, 2, 2, 2),
                        alignment = TextAnchor.MiddleCenter
                    };
                }
                return _statusBoxStyle;
            }
        }

        public static GUIStyle HeaderLabel
        {
            get
            {
                if (_headerLabelStyle == null)
                {
                    _headerLabelStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 14,
                        margin = new RectOffset(5, 5, 5, 5)
                    };
                }
                return _headerLabelStyle;
            }
        }

        public static GUIStyle BoldLabel
        {
            get
            {
                if (_boldLabelStyle == null)
                {
                    _boldLabelStyle = new GUIStyle(EditorStyles.boldLabel);
                }
                return _boldLabelStyle;
            }
        }

        public static GUIStyle CenteredLabel
        {
            get
            {
                if (_centeredLabelStyle == null)
                {
                    _centeredLabelStyle = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }
                return _centeredLabelStyle;
            }
        }

        public static GUIStyle StatusLabel
        {
            get
            {
                if (_statusLabelStyle == null)
                {
                    _statusLabelStyle = new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold
                    };
                }
                return _statusLabelStyle;
            }
        }

        public static GUIStyle MetricLabel
        {
            get
            {
                if (_metricLabelStyle == null)
                {
                    _metricLabelStyle = new GUIStyle(EditorStyles.label)
                    {
                        fontSize = 11,
                        padding = new RectOffset(2, 2, 2, 2)
                    };
                }
                return _metricLabelStyle;
            }
        }

        public static GUIStyle PrimaryButton
        {
            get
            {
                if (_primaryButtonStyle == null)
                {
                    _primaryButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 30,
                        fontSize = 12
                    };
                }
                return _primaryButtonStyle;
            }
        }

        public static GUIStyle SecondaryButton
        {
            get
            {
                if (_secondaryButtonStyle == null)
                {
                    _secondaryButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fixedHeight = 25,
                        fontSize = 11
                    };
                }
                return _secondaryButtonStyle;
            }
        }

        public static GUIStyle IconButton
        {
            get
            {
                if (_iconButtonStyle == null)
                {
                    _iconButtonStyle = new GUIStyle(GUI.skin.button)
                    {
                        fixedHeight = 28,
                        fixedWidth = 28,
                        padding = new RectOffset(4, 4, 4, 4)
                    };
                }
                return _iconButtonStyle;
            }
        }

        /// <summary>
        /// Draw a colored status badge
        /// </summary>
        public static void DrawStatusBadge(string label, Color color, float width = 80)
        {
            var originalColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUILayout.Label(label, StatusBox, GUILayout.Width(width), GUILayout.Height(20));
            GUI.backgroundColor = originalColor;
        }

        /// <summary>
        /// Draw a section header with foldout
        /// </summary>
        public static bool DrawSectionHeader(string title, bool foldout, GUIContent icon = null)
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, HeaderBox, GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                HeaderBox.Draw(rect, false, false, false, false);
            }

            var labelRect = new Rect(rect.x + 20, rect.y, rect.width - 20, rect.height);

            if (icon != null)
            {
                var iconRect = new Rect(rect.x + 5, rect.y + 2, 16, 16);
                GUI.Label(iconRect, icon);
            }

            var newFoldout = EditorGUI.Foldout(labelRect, foldout, title, true, EditorStyles.boldLabel);

            return newFoldout;
        }

        /// <summary>
        /// Draw a separator line
        /// </summary>
        public static void DrawSeparator(int height = 1, int padding = 5)
        {
            GUILayout.Space(padding);
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(height));
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
            GUILayout.Space(padding);
        }

        /// <summary>
        /// Draw a progress bar
        /// </summary>
        public static void DrawProgressBar(float progress, string label = "")
        {
            var rect = GUILayoutUtility.GetRect(18, 18, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(rect, progress, label);
        }

        /// <summary>
        /// Begin a colored scope
        /// </summary>
        public static System.IDisposable ColorScope(Color color)
        {
            return new ColorScopeImpl(color);
        }

        private class ColorScopeImpl : System.IDisposable
        {
            private Color _originalColor;

            public ColorScopeImpl(Color color)
            {
                _originalColor = GUI.backgroundColor;
                GUI.backgroundColor = color;
            }

            public void Dispose()
            {
                GUI.backgroundColor = _originalColor;
            }
        }
    }
}
