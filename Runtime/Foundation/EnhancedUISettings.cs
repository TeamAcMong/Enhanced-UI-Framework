using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedUI
{
    /// <summary>
    /// Global settings for Enhanced UI Framework.
    /// Create via Assets > Create > Enhanced UI > Settings
    /// </summary>
    [CreateAssetMenu(fileName = "EnhancedUISettings", menuName = "Enhanced UI/Settings", order = 1)]
    public class EnhancedUISettings : ScriptableObject
    {
        private static EnhancedUISettings _instance;

        /// <summary>
        /// Get the global settings instance. Creates default if not found.
        /// </summary>
        public static EnhancedUISettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<EnhancedUISettings>("EnhancedUISettings");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<EnhancedUISettings>();
                        Debug.LogWarning("[EnhancedUI] Settings not found. Using default settings. " +
                                       "Create one via Assets > Create > Enhanced UI > Settings");
                    }
                }
                return _instance;
            }
        }

        [Header("Asset Loading")]
        [Tooltip("Default asset loader type (Resources, Addressables, or custom)")]
        public AssetLoaderType assetLoaderType = AssetLoaderType.Addressables;

        [Tooltip("Enable preloading system for faster transitions")]
        public bool enablePreloading = true;

        [Header("Interaction Control")]
        [Tooltip("Allow user interaction during screen transitions")]
        public bool enableInteractionInTransition = false;

        [Tooltip("Control all containers' interaction, not just the transitioning one")]
        public bool controlInteractionOfAllContainers = true;

        [Header("Lifecycle")]
        [Tooltip("Call Cleanup() when screen GameObject is destroyed")]
        public bool callCleanupWhenDestroy = true;

        [Tooltip("Enable async lifecycle methods (requires UniTask or C# Task support)")]
        public bool enableAsyncLifecycle = true;

        [Header("Memory Management")]
        [Tooltip("Enable object pooling for frequently used screens")]
        public bool enableObjectPooling = true;

        [Tooltip("Enable smart caching with LRU policy")]
        public bool enableSmartCaching = true;

        [Tooltip("Maximum memory budget for cached screens in MB (0 = unlimited)")]
        public int memoryBudgetMB = 100;

        [Tooltip("Pool configuration per screen (key = screen resource key, value = pool size)")]
        public List<PoolConfig> poolConfigurations = new List<PoolConfig>();

        [Header("Mobile Features")]
        [Tooltip("Enable safe area adaptation for notch/home indicator")]
        public bool enableSafeArea = true;

        [Tooltip("Enable Android back button handling")]
        public bool enableBackButton = true;

        [Tooltip("Enable orientation management")]
        public bool enableOrientationManagement = false;

        [Header("Performance")]
        [Tooltip("Target frame rate during transitions (0 = no change)")]
        public int targetFrameRateDuringTransition = 0;

        [Tooltip("Enable automatic GC scheduling")]
        public bool enableAutoGC = false;

        [Tooltip("Reduce allocations during transitions")]
        public bool optimizeTransitionPerformance = true;

        [Header("Debug")]
        [Tooltip("Enable detailed logging")]
        public bool enableDebugLog = false;

        [Tooltip("Log lifecycle events")]
        public bool logLifecycleEvents = false;

        [Tooltip("Log transition events")]
        public bool logTransitionEvents = false;

        [Tooltip("Show performance warnings")]
        public bool showPerformanceWarnings = true;

        [Header("Default Transitions")]
        public TransitionSettings pageTransitions = new TransitionSettings
        {
            pushEnterDuration = 0.3f,
            pushExitDuration = 0.3f,
            popEnterDuration = 0.3f,
            popExitDuration = 0.3f
        };

        public TransitionSettings modalTransitions = new TransitionSettings
        {
            pushEnterDuration = 0.3f,
            pushExitDuration = 0.3f,
            popEnterDuration = 0.3f,
            popExitDuration = 0.3f
        };

        public TransitionSettings sheetTransitions = new TransitionSettings
        {
            pushEnterDuration = 0.3f,
            pushExitDuration = 0.3f,
            popEnterDuration = 0.3f,
            popExitDuration = 0.3f
        };

        /// <summary>
        /// Get pool size for a specific screen
        /// </summary>
        public int GetPoolSize(string resourceKey)
        {
            var config = poolConfigurations.Find(x => x.resourceKey == resourceKey);
            return config?.poolSize ?? 0;
        }

        /// <summary>
        /// Check if a screen should be pooled
        /// </summary>
        public bool ShouldPool(string resourceKey)
        {
            return enableObjectPooling && GetPoolSize(resourceKey) > 0;
        }
    }

    [Serializable]
    public class PoolConfig
    {
        public string resourceKey;
        public int poolSize = 3;
    }

    [Serializable]
    public class TransitionSettings
    {
        public float pushEnterDuration = 0.3f;
        public float pushExitDuration = 0.3f;
        public float popEnterDuration = 0.3f;
        public float popExitDuration = 0.3f;
    }

    public enum AssetLoaderType
    {
        Resources,
        Addressables,
        Custom
    }
}
