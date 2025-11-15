using System;
using UnityEngine;

namespace EnhancedUI.Utilities
{
    /// <summary>
    /// Structured logger for Enhanced UI Framework
    /// </summary>
    public static class ScreenLogger
    {
        public enum LogCategory
        {
            Lifecycle,
            Transition,
            AssetLoading,
            Performance,
            Error,
            Warning,
            Info
        }

        private static EnhancedUISettings Settings => EnhancedUISettings.Instance;

        public static void Log(LogCategory category, string message, UnityEngine.Object context = null)
        {
            if (!Settings.enableDebugLog)
                return;

            switch (category)
            {
                case LogCategory.Lifecycle when Settings.logLifecycleEvents:
                case LogCategory.Transition when Settings.logTransitionEvents:
                case LogCategory.Info:
                    Debug.Log($"[EnhancedUI:{category}] {message}", context);
                    break;
            }
        }

        public static void LogWarning(string message, UnityEngine.Object context = null)
        {
            if (Settings.showPerformanceWarnings || Settings.enableDebugLog)
            {
                Debug.LogWarning($"[EnhancedUI:Warning] {message}", context);
            }
        }

        public static void LogError(string message, UnityEngine.Object context = null)
        {
            Debug.LogError($"[EnhancedUI:Error] {message}", context);
        }

        public static void LogException(Exception exception, UnityEngine.Object context = null)
        {
            Debug.LogException(exception, context);
        }

        public static void LogLifecycle(string screenName, string eventName, UnityEngine.Object context = null)
        {
            Log(LogCategory.Lifecycle, $"{screenName} - {eventName}", context);
        }

        public static void LogTransition(string from, string to, string transitionType, UnityEngine.Object context = null)
        {
            Log(LogCategory.Transition, $"{transitionType}: {from} -> {to}", context);
        }

        public static void LogAssetLoading(string resourceKey, bool isAsync, UnityEngine.Object context = null)
        {
            Log(LogCategory.AssetLoading, $"Loading {resourceKey} ({(isAsync ? "Async" : "Sync")})", context);
        }

        public static void LogPerformance(string message, UnityEngine.Object context = null)
        {
            if (Settings.showPerformanceWarnings)
            {
                Log(LogCategory.Performance, message, context);
            }
        }
    }
}
