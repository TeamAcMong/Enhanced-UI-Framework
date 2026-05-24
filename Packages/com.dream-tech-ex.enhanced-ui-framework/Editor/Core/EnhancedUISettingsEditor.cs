using System;
using UnityEngine;
using UnityEditor;
using EnhancedUI.Editor.Utilities;
using EnhancedUI.Editor.Tools;

namespace EnhancedUI.Editor.Core
{
    /// <summary>
    /// Custom inspector for <see cref="EnhancedUISettings"/>.
    /// Covers every serialized field, grouped into colored, persistent foldout sections,
    /// with contextual warnings and quick-action presets.
    /// </summary>
    [CustomEditor(typeof(EnhancedUISettings))]
    public class EnhancedUISettingsEditor : UnityEditor.Editor
    {
        private const string PrefPrefix = "EnhancedUI.SettingsEditor.";

        // Foldout state
        private bool _showAssetLoading;
        private bool _showLifecycle;
        private bool _showMemory;
        private bool _showInteraction;
        private bool _showMobile;
        private bool _showPerformance;
        private bool _showTransitions;
        private bool _showDebug;
        private int _transitionTab;

        // Cached SerializedProperties (all 20 fields + 3 transition structs)
        private SerializedProperty _assetLoaderType;
        private SerializedProperty _enablePreloading;
        private SerializedProperty _enableInteractionInTransition;
        private SerializedProperty _controlInteractionOfAllContainers;
        private SerializedProperty _callCleanupWhenDestroy;
        private SerializedProperty _enableAsyncLifecycle;
        private SerializedProperty _enableObjectPooling;
        private SerializedProperty _enableSmartCaching;
        private SerializedProperty _memoryBudgetMB;
        private SerializedProperty _poolConfigurations;
        private SerializedProperty _enableSafeArea;
        private SerializedProperty _enableBackButton;
        private SerializedProperty _enableOrientationManagement;
        private SerializedProperty _targetFrameRateDuringTransition;
        private SerializedProperty _enableAutoGC;
        private SerializedProperty _optimizeTransitionPerformance;
        private SerializedProperty _enableDebugLog;
        private SerializedProperty _logLifecycleEvents;
        private SerializedProperty _logTransitionEvents;
        private SerializedProperty _showPerformanceWarnings;
        private SerializedProperty _pageTransitions;
        private SerializedProperty _modalTransitions;
        private SerializedProperty _sheetTransitions;

        private static readonly string[] TransitionTabs = { "Page", "Modal", "Sheet" };
        private static readonly GUIContent[] LoaderOptions =
        {
            new GUIContent("Resources", "Load from any Resources/ folder via Resources.Load. Zero setup, included in build."),
            new GUIContent("Addressables", "Load via Addressables. Each prefab must be registered with the bare key as its Address."),
            new GUIContent("Custom", "Provide your own IAssetLoader at runtime (e.g. asset bundles, remote CDN)."),
        };

        private void OnEnable()
        {
            CacheProperties();
            LoadFoldoutState();
        }

        private void OnDisable()
        {
            SaveFoldoutState();
        }

        private void CacheProperties()
        {
            _assetLoaderType = serializedObject.FindProperty("assetLoaderType");
            _enablePreloading = serializedObject.FindProperty("enablePreloading");
            _enableInteractionInTransition = serializedObject.FindProperty("enableInteractionInTransition");
            _controlInteractionOfAllContainers = serializedObject.FindProperty("controlInteractionOfAllContainers");
            _callCleanupWhenDestroy = serializedObject.FindProperty("callCleanupWhenDestroy");
            _enableAsyncLifecycle = serializedObject.FindProperty("enableAsyncLifecycle");
            _enableObjectPooling = serializedObject.FindProperty("enableObjectPooling");
            _enableSmartCaching = serializedObject.FindProperty("enableSmartCaching");
            _memoryBudgetMB = serializedObject.FindProperty("memoryBudgetMB");
            _poolConfigurations = serializedObject.FindProperty("poolConfigurations");
            _enableSafeArea = serializedObject.FindProperty("enableSafeArea");
            _enableBackButton = serializedObject.FindProperty("enableBackButton");
            _enableOrientationManagement = serializedObject.FindProperty("enableOrientationManagement");
            _targetFrameRateDuringTransition = serializedObject.FindProperty("targetFrameRateDuringTransition");
            _enableAutoGC = serializedObject.FindProperty("enableAutoGC");
            _optimizeTransitionPerformance = serializedObject.FindProperty("optimizeTransitionPerformance");
            _enableDebugLog = serializedObject.FindProperty("enableDebugLog");
            _logLifecycleEvents = serializedObject.FindProperty("logLifecycleEvents");
            _logTransitionEvents = serializedObject.FindProperty("logTransitionEvents");
            _showPerformanceWarnings = serializedObject.FindProperty("showPerformanceWarnings");
            _pageTransitions = serializedObject.FindProperty("pageTransitions");
            _modalTransitions = serializedObject.FindProperty("modalTransitions");
            _sheetTransitions = serializedObject.FindProperty("sheetTransitions");
        }

        private void LoadFoldoutState()
        {
            _showAssetLoading = EditorPrefs.GetBool(PrefPrefix + "AssetLoading", true);
            _showLifecycle = EditorPrefs.GetBool(PrefPrefix + "Lifecycle", true);
            _showMemory = EditorPrefs.GetBool(PrefPrefix + "Memory", true);
            _showInteraction = EditorPrefs.GetBool(PrefPrefix + "Interaction", false);
            _showMobile = EditorPrefs.GetBool(PrefPrefix + "Mobile", true);
            _showPerformance = EditorPrefs.GetBool(PrefPrefix + "Performance", false);
            _showTransitions = EditorPrefs.GetBool(PrefPrefix + "Transitions", false);
            _showDebug = EditorPrefs.GetBool(PrefPrefix + "Debug", false);
            _transitionTab = EditorPrefs.GetInt(PrefPrefix + "TransitionTab", 0);
        }

        private void SaveFoldoutState()
        {
            EditorPrefs.SetBool(PrefPrefix + "AssetLoading", _showAssetLoading);
            EditorPrefs.SetBool(PrefPrefix + "Lifecycle", _showLifecycle);
            EditorPrefs.SetBool(PrefPrefix + "Memory", _showMemory);
            EditorPrefs.SetBool(PrefPrefix + "Interaction", _showInteraction);
            EditorPrefs.SetBool(PrefPrefix + "Mobile", _showMobile);
            EditorPrefs.SetBool(PrefPrefix + "Performance", _showPerformance);
            EditorPrefs.SetBool(PrefPrefix + "Transitions", _showTransitions);
            EditorPrefs.SetBool(PrefPrefix + "Debug", _showDebug);
            EditorPrefs.SetInt(PrefPrefix + "TransitionTab", _transitionTab);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();

            GUILayout.Space(10);

            _showAssetLoading = Section("🚀 Asset Loading",
                "How prefabs are resolved at runtime.",
                EnhancedUIEditorStyles.InfoColor, _showAssetLoading, DrawAssetLoading);

            _showLifecycle = Section("♻️ Lifecycle",
                "Screen lifecycle and async behavior.",
                new Color(0.62f, 0.42f, 0.92f), _showLifecycle, DrawLifecycle);

            _showMemory = Section("🧠 Memory & Pooling",
                "Caching, pooling and memory budget.",
                EnhancedUIEditorStyles.SuccessColor, _showMemory, DrawMemory);

            _showInteraction = Section("👆 Interaction",
                "Input control during screen transitions.",
                new Color(0.95f, 0.55f, 0.20f), _showInteraction, DrawInteraction);

            _showMobile = Section("📱 Mobile Features",
                "Safe area, back button and orientation handling.",
                new Color(0.20f, 0.72f, 0.95f), _showMobile, DrawMobile);

            _showPerformance = Section("⚡ Performance",
                "Frame rate and runtime optimizations.",
                EnhancedUIEditorStyles.WarningColor, _showPerformance, DrawPerformance);

            _showTransitions = Section("🎬 Default Transitions",
                "Push/pop durations per container type.",
                new Color(0.86f, 0.38f, 0.86f), _showTransitions, DrawTransitions);

            _showDebug = Section("🐞 Debug & Logging",
                "Verbose logs and runtime warnings.",
                new Color(0.55f, 0.55f, 0.55f), _showDebug, DrawDebug);

            GUILayout.Space(12);
            DrawActions();
            GUILayout.Space(4);

            serializedObject.ApplyModifiedProperties();
        }

        // ─── Sections ──────────────────────────────────────────────────────

        private void DrawHeader()
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.HeaderBox))
            {
                // Row 1 — icon + title + subtitle
                using (new EditorGUILayout.HorizontalScope())
                {
                    var icon = EditorGUIUtility.IconContent("Settings");
                    GUILayout.Label(icon, GUILayout.Width(28), GUILayout.Height(28));

                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField("Enhanced UI Settings", EnhancedUIEditorStyles.HeaderLabel);
                        EditorGUILayout.LabelField("Global configuration for Enhanced UI Framework",
                            EditorStyles.miniLabel);
                    }
                }

                GUILayout.Space(4);

                // Row 2 — status pills, auto-sized, left-aligned
                using (new EditorGUILayout.HorizontalScope())
                {
                    var loaderName = LoaderOptions[Mathf.Clamp(_assetLoaderType.enumValueIndex, 0, 2)].text;
                    DrawPill(loaderName, EnhancedUIEditorStyles.InfoColor);

                    if (_enableObjectPooling.boolValue)
                        DrawPill("Pool", EnhancedUIEditorStyles.SuccessColor);

                    if (_enableAsyncLifecycle.boolValue)
                        DrawPill("Async", new Color(0.62f, 0.42f, 0.92f));

                    if (_enableSmartCaching.boolValue)
                        DrawPill("Cache", new Color(0.20f, 0.72f, 0.95f));

                    GUILayout.FlexibleSpace();
                }
            }
        }

        private static GUIStyle _pillStyle;
        private static GUIStyle PillStyle
        {
            get
            {
                if (_pillStyle == null)
                {
                    _pillStyle = new GUIStyle(GUI.skin.box)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 10,
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset(10, 10, 3, 3),
                        margin = new RectOffset(0, 6, 0, 0),
                        fixedHeight = 20,
                        wordWrap = false,
                        clipping = TextClipping.Overflow,
                    };
                }
                return _pillStyle;
            }
        }

        private static void DrawPill(string text, Color color)
        {
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = color;
            var content = new GUIContent(text);
            // Buffer width by +6 so multi-line wrap never triggers when CalcSize is conservative.
            var width = PillStyle.CalcSize(content).x + 6;
            GUILayout.Label(content, PillStyle, GUILayout.Width(width), GUILayout.Height(20));
            GUI.backgroundColor = prev;
        }

        private void DrawAssetLoading()
        {
            EditorGUILayout.LabelField("Loader", EnhancedUIEditorStyles.BoldLabel);

            // Segmented loader picker
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                int selected = GUILayout.Toolbar(_assetLoaderType.enumValueIndex, LoaderOptions,
                    GUILayout.Height(26));
                if (check.changed) _assetLoaderType.enumValueIndex = selected;
            }

            EditorGUILayout.LabelField(LoaderOptions[_assetLoaderType.enumValueIndex].tooltip,
                EditorStyles.wordWrappedMiniLabel);

            GUILayout.Space(4);

            // Contextual hints / warnings
            switch ((AssetLoaderType)_assetLoaderType.enumValueIndex)
            {
                case AssetLoaderType.Resources:
                    EditorGUILayout.HelpBox(
                        "Place prefabs in a Resources/ folder. Push key matches the relative path (without .prefab).",
                        MessageType.Info);
                    break;

                case AssetLoaderType.Addressables:
#if EUI_ADDRESSABLES_SUPPORT
                    EditorGUILayout.HelpBox(
                        "Each prefab must be marked Addressable. Push key must equal the entry's Address.",
                        MessageType.Info);
#else
                    EditorGUILayout.HelpBox(
                        "Addressables package not detected. Install com.unity.addressables (1.17.4+) or pick another loader.",
                        MessageType.Warning);
#endif
                    break;

                case AssetLoaderType.Custom:
                    EditorGUILayout.HelpBox(
                        "Register your loader via AssetLoaderProvider.SetCustomLoader(IAssetLoader) before pushing any screen.",
                        MessageType.Warning);
                    break;
            }

            GUILayout.Space(6);
            EditorGUILayout.PropertyField(_enablePreloading, new GUIContent(
                "Enable Preloading",
                "Allow IPreloadable.Preload() to be invoked ahead of Push for faster transitions."));
        }

        private void DrawLifecycle()
        {
            EditorGUILayout.PropertyField(_enableAsyncLifecycle, new GUIContent(
                "Enable Async Lifecycle",
                "Use UniTask / Task-based Initialize / Cleanup hooks. Disable for coroutine-only projects."));

            EditorGUILayout.PropertyField(_callCleanupWhenDestroy, new GUIContent(
                "Cleanup On Destroy",
                "Invoke Cleanup() automatically when a screen GameObject is destroyed (recommended)."));

            if (!_callCleanupWhenDestroy.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Disabling auto-cleanup may leak event subscriptions if you forget to call Cleanup() manually.",
                    MessageType.Warning);
            }
        }

        private void DrawMemory()
        {
            // Object Pooling
            EditorGUILayout.PropertyField(_enableObjectPooling, new GUIContent(
                "Enable Object Pooling",
                "Reuse instantiated screens instead of destroying them."));

            using (new EditorGUI.DisabledScope(!_enableObjectPooling.boolValue))
            {
                EditorGUILayout.PropertyField(_poolConfigurations, new GUIContent(
                    "Pool Configurations",
                    "Pool size per resource key. Only screens listed here are pooled."), true);

                if (_enableObjectPooling.boolValue && _poolConfigurations.arraySize == 0)
                {
                    EditorGUILayout.HelpBox(
                        "Tip: Add entries for screens shown/hidden frequently (modals, tooltips, list items).",
                        MessageType.Info);
                }
            }

            EnhancedUIEditorStyles.DrawSeparator();

            // Smart Caching
            EditorGUILayout.PropertyField(_enableSmartCaching, new GUIContent(
                "Enable Smart Caching",
                "Cache asset handles with LRU eviction under the memory budget."));

            using (new EditorGUI.DisabledScope(!_enableSmartCaching.boolValue))
            {
                EditorGUILayout.PropertyField(_memoryBudgetMB, new GUIContent(
                    "Memory Budget (MB)",
                    "Max memory cached screens can use. 0 = unlimited."));

                GUILayout.Space(2);

                var budget = _memoryBudgetMB.intValue;
                var t = budget <= 0 ? 1f : Mathf.InverseLerp(0, 256, budget);
                var color = budget == 0
                    ? EnhancedUIEditorStyles.InfoColor
                    : budget < 20 ? EnhancedUIEditorStyles.ErrorColor
                    : budget < 50 ? EnhancedUIEditorStyles.WarningColor
                    : EnhancedUIEditorStyles.SuccessColor;

                using (EnhancedUIEditorStyles.ColorScope(color))
                {
                    EnhancedUIEditorStyles.DrawProgressBar(t, budget == 0 ? "Unlimited" : budget + " MB");
                }

                if (budget > 0 && budget < 10)
                    EditorGUILayout.HelpBox("Below 10 MB you may evict cached screens almost every Push.", MessageType.Warning);
            }
        }

        private void DrawInteraction()
        {
            EditorGUILayout.PropertyField(_enableInteractionInTransition, new GUIContent(
                "Allow Input During Transitions",
                "Let users tap UI while a Push/Pop animation is playing."));

            if (_enableInteractionInTransition.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Spam-tapping during animations can produce unexpected navigation. Use with care.",
                    MessageType.Warning);
            }

            GUILayout.Space(4);

            EditorGUILayout.PropertyField(_controlInteractionOfAllContainers, new GUIContent(
                "Lock All Containers Together",
                "When ON, any container's transition blocks input on every container (global lock)."));
        }

        private void DrawMobile()
        {
            EditorGUILayout.PropertyField(_enableSafeArea, new GUIContent(
                "Enable Safe Area",
                "Auto-adapt RectTransforms tagged with SafeAreaAdapter to device notch / home indicator."));

            EditorGUILayout.PropertyField(_enableBackButton, new GUIContent(
                "Enable Back Button",
                "Globally intercept Android back / Escape and route to the top screen."));

            EditorGUILayout.PropertyField(_enableOrientationManagement, new GUIContent(
                "Enable Orientation Management",
                "Track Screen.orientation changes and dispatch IOnOrientationChanged events."));
        }

        private void DrawPerformance()
        {
            EditorGUILayout.PropertyField(_targetFrameRateDuringTransition, new GUIContent(
                "Target FPS During Transition",
                "Temporarily clamp Application.targetFrameRate while a transition runs (0 = no change)."));

            if (_targetFrameRateDuringTransition.intValue > 0 && _targetFrameRateDuringTransition.intValue < 30)
            {
                EditorGUILayout.HelpBox(
                    "Below 30 FPS the transition itself will visibly stutter.",
                    MessageType.Warning);
            }

            EditorGUILayout.PropertyField(_optimizeTransitionPerformance, new GUIContent(
                "Optimize Transition Allocations",
                "Reduce per-frame allocations during transitions (recommended for mobile)."));

            EditorGUILayout.PropertyField(_enableAutoGC, new GUIContent(
                "Auto GC After Transition",
                "Schedule GC.Collect after heavy transitions. Off by default — only enable for huge screens."));

            if (_enableAutoGC.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Forced GC stalls the main thread. Only enable on platforms with no incremental GC.",
                    MessageType.Warning);
            }
        }

        private void DrawTransitions()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                _transitionTab = GUILayout.Toolbar(_transitionTab, TransitionTabs,
                    GUILayout.Height(22), GUILayout.MinWidth(220));
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(4);

            SerializedProperty group;
            switch (_transitionTab)
            {
                case 1: group = _modalTransitions; break;
                case 2: group = _sheetTransitions; break;
                default: group = _pageTransitions; break;
            }

            DrawTransitionGroup(group);
        }

        private void DrawTransitionGroup(SerializedProperty group)
        {
            var pushEnter = group.FindPropertyRelative("pushEnterDuration");
            var pushExit = group.FindPropertyRelative("pushExitDuration");
            var popEnter = group.FindPropertyRelative("popEnterDuration");
            var popExit = group.FindPropertyRelative("popExitDuration");

            EditorGUILayout.Slider(pushEnter, 0f, 2f, new GUIContent("Push — Enter"));
            EditorGUILayout.Slider(pushExit, 0f, 2f, new GUIContent("Push — Exit"));
            EditorGUILayout.Slider(popEnter, 0f, 2f, new GUIContent("Pop — Enter"));
            EditorGUILayout.Slider(popExit, 0f, 2f, new GUIContent("Pop — Exit"));

            GUILayout.Space(6);

            EditorGUILayout.LabelField("Presets", EditorStyles.miniBoldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Instant (0s)", EnhancedUIEditorStyles.SecondaryButton))
                    SetTransitionDurations(group, 0f);
                if (GUILayout.Button("Fast (0.15s)", EnhancedUIEditorStyles.SecondaryButton))
                    SetTransitionDurations(group, 0.15f);
                if (GUILayout.Button("Normal (0.3s)", EnhancedUIEditorStyles.SecondaryButton))
                    SetTransitionDurations(group, 0.30f);
                if (GUILayout.Button("Slow (0.5s)", EnhancedUIEditorStyles.SecondaryButton))
                    SetTransitionDurations(group, 0.50f);
            }
        }

        private static void SetTransitionDurations(SerializedProperty group, float v)
        {
            group.FindPropertyRelative("pushEnterDuration").floatValue = v;
            group.FindPropertyRelative("pushExitDuration").floatValue = v;
            group.FindPropertyRelative("popEnterDuration").floatValue = v;
            group.FindPropertyRelative("popExitDuration").floatValue = v;
        }

        private void DrawDebug()
        {
            EditorGUILayout.PropertyField(_enableDebugLog, new GUIContent(
                "Enable Debug Logging",
                "Master switch for framework logs."));

            using (new EditorGUI.DisabledScope(!_enableDebugLog.boolValue))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(_logLifecycleEvents, new GUIContent(
                    "Log Lifecycle Events",
                    "Initialize / WillEnter / DidExit / Cleanup."));
                EditorGUILayout.PropertyField(_logTransitionEvents, new GUIContent(
                    "Log Transition Events",
                    "Push / Pop start & complete."));
                EditorGUI.indentLevel--;
            }

            GUILayout.Space(4);
            EditorGUILayout.PropertyField(_showPerformanceWarnings, new GUIContent(
                "Show Performance Warnings",
                "Yellow-box warnings in the Console when frame budget is exceeded."));
        }

        private void DrawActions()
        {
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.LabelField("⚡ Quick Actions", EnhancedUIEditorStyles.BoldLabel);

                GUILayout.Space(4);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Reset to Defaults", EnhancedUIEditorStyles.SecondaryButton))
                    {
                        if (EditorUtility.DisplayDialog(
                            "Reset Settings",
                            "Reset every field on this asset to default values?",
                            "Reset", "Cancel"))
                        {
                            ResetToDefaults();
                        }
                    }

                    if (GUILayout.Button("Open Control Center", EnhancedUIEditorStyles.SecondaryButton))
                        EnhancedUIControlCenter.ShowWindow();
                }

                GUILayout.Space(4);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Run Health Check", EnhancedUIEditorStyles.SecondaryButton))
                        HealthCheckWindow.ShowWindow();

                    if (GUILayout.Button("Documentation", EnhancedUIEditorStyles.SecondaryButton))
                        Application.OpenURL("https://github.com/TeamAcMong/Enhanced-UI-Framework");
                }
            }
        }

        // ─── Helpers ───────────────────────────────────────────────────────

        // Local, generously-padded section card. Cached lazily.
        private static GUIStyle _sectionCardStyle;
        private static GUIStyle SectionCardStyle
        {
            get
            {
                if (_sectionCardStyle == null)
                {
                    _sectionCardStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        // Extra left padding so content never sits under the colored stripe.
                        padding = new RectOffset(14, 12, 10, 10),
                        margin = new RectOffset(2, 2, 4, 4),
                    };
                }
                return _sectionCardStyle;
            }
        }

        /// <summary>
        /// Draws a section as a card: colored left stripe + foldable header bar + padded body.
        /// Sections are visually separated by a generous gap so they don't blur together.
        /// </summary>
        private bool Section(string title, string subtitle, Color stripe, bool open, Action body)
        {
            GUILayout.Space(2);

            // Capture the card's outer rect so we can paint the full-height stripe AFTER layout is finalized.
            var cardRect = EditorGUILayout.BeginVertical(SectionCardStyle);

            // ── Header bar ──────────────────────────────────────────────
            // Note: header sits INSIDE the card's padding, so we draw bg / underline relative to padded rect.
            var headerRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbarButton,
                GUILayout.Height(24), GUILayout.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                // Subtle header background tint
                var headerBg = EditorGUIUtility.isProSkin
                    ? new Color(1f, 1f, 1f, 0.04f)
                    : new Color(0f, 0f, 0f, 0.06f);
                EditorGUI.DrawRect(headerRect, headerBg);

                // Colored underline accent (only when open)
                if (open)
                {
                    var underline = new Rect(headerRect.x, headerRect.yMax - 1, headerRect.width, 1);
                    EditorGUI.DrawRect(underline, new Color(stripe.r, stripe.g, stripe.b, 0.55f));
                }
            }

            // Foldout label sits a bit indented for breathing room from the stripe
            var foldRect = new Rect(headerRect.x + 4, headerRect.y + 3, headerRect.width - 8, headerRect.height - 4);
            open = EditorGUI.Foldout(foldRect, open, title, true, EditorStyles.boldLabel);

            // ── Body ────────────────────────────────────────────────────
            if (open)
            {
                GUILayout.Space(8);

                if (!string.IsNullOrEmpty(subtitle))
                {
                    EditorGUILayout.LabelField(subtitle, EditorStyles.wordWrappedMiniLabel);
                    GUILayout.Space(6);
                }

                body?.Invoke();

                GUILayout.Space(6);
            }

            EditorGUILayout.EndVertical();

            // ── Left stripe ─────────────────────────────────────────────
            // Position INSIDE the helpBox's left border (offset by 2px) so it reads as part of the card,
            // not as a stray line outside it. Width 4 keeps it visually present without crowding content.
            if (Event.current.type == EventType.Repaint)
            {
                var stripeX = cardRect.x + 3;            // skip helpBox border
                var stripeY = cardRect.y + 2;            // align with card top inset
                var stripeH = cardRect.height - 4;       // align with card bottom inset
                EditorGUI.DrawRect(new Rect(stripeX, stripeY, 3, stripeH), stripe);
            }

            // Generous gap between sections — prevents the eye from running them together
            GUILayout.Space(10);
            return open;
        }

        private void ResetToDefaults()
        {
            var settings = target as EnhancedUISettings;
            if (settings == null) return;

            Undo.RecordObject(settings, "Reset Enhanced UI Settings");

            settings.assetLoaderType = AssetLoaderType.Addressables;
            settings.enablePreloading = true;
            settings.enableInteractionInTransition = false;
            settings.controlInteractionOfAllContainers = true;
            settings.callCleanupWhenDestroy = true;
            settings.enableAsyncLifecycle = true;
            settings.enableObjectPooling = true;
            settings.enableSmartCaching = true;
            settings.memoryBudgetMB = 100;
            settings.poolConfigurations = new System.Collections.Generic.List<PoolConfig>();
            settings.enableSafeArea = true;
            settings.enableBackButton = true;
            settings.enableOrientationManagement = false;
            settings.targetFrameRateDuringTransition = 0;
            settings.enableAutoGC = false;
            settings.optimizeTransitionPerformance = true;
            settings.enableDebugLog = false;
            settings.logLifecycleEvents = false;
            settings.logTransitionEvents = false;
            settings.showPerformanceWarnings = true;
            settings.pageTransitions = new TransitionSettings();
            settings.modalTransitions = new TransitionSettings();
            settings.sheetTransitions = new TransitionSettings();

            EditorUtility.SetDirty(settings);
            serializedObject.Update();
        }
    }
}
