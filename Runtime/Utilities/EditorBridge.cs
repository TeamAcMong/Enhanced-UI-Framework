using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedUI.Utilities
{
    /// <summary>
    /// Bridge between runtime and editor for debug/analysis tools
    /// </summary>
    public class EditorBridge : MonoBehaviour
    {
        private static EditorBridge _instance;
        private readonly List<LifecycleEventRecord> _eventHistory = new List<LifecycleEventRecord>();
        private readonly List<TransitionRecord> _transitionHistory = new List<TransitionRecord>();
        private readonly List<PerformanceSnapshot> _performanceSnapshots = new List<PerformanceSnapshot>();

        private const int MaxEventHistory = 1000;
        private const int MaxTransitionHistory = 100;
        private const int MaxPerformanceSnapshots = 300; // 5 minutes at 60fps

        // Performance monitoring optimization
        [SerializeField] private bool enablePerformanceMonitoring = false;
        private int _frameCount = 0;
        private const int MEMORY_SAMPLE_FREQUENCY = 60; // Sample memory once per second at 60fps
        private float _lastMemoryReading = 0f;

        public static EditorBridge Instance
        {
            get
            {
                if (_instance == null && Application.isPlaying)
                {
#if UNITY_EDITOR
                    var go = new GameObject("[Enhanced UI Editor Bridge]");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<EditorBridge>();
#endif
                }
                return _instance;
            }
        }

        public IReadOnlyList<LifecycleEventRecord> EventHistory => _eventHistory;
        public IReadOnlyList<TransitionRecord> TransitionHistory => _transitionHistory;
        public IReadOnlyList<PerformanceSnapshot> PerformanceSnapshots => _performanceSnapshots;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        private void Update()
        {
#if UNITY_EDITOR
            // Only record performance if monitoring is enabled
            if (enablePerformanceMonitoring)
            {
                RecordPerformanceSnapshot();
            }
#endif
        }

        /// <summary>
        /// Record a lifecycle event
        /// </summary>
        public void RecordLifecycleEvent(string eventName, string containerName, string screenName, string category = "Lifecycle")
        {
#if UNITY_EDITOR
            var record = new LifecycleEventRecord
            {
                Timestamp = Time.realtimeSinceStartup,
                EventName = eventName,
                ContainerName = containerName,
                ScreenName = screenName,
                Category = category,
                StackTrace = UnityEngine.StackTraceUtility.ExtractStackTrace()
            };

            _eventHistory.Add(record);

            // Trim history if too long
            if (_eventHistory.Count > MaxEventHistory)
            {
                _eventHistory.RemoveRange(0, _eventHistory.Count - MaxEventHistory);
            }
#endif
        }

        /// <summary>
        /// Record a transition start
        /// </summary>
        public void RecordTransitionStart(string containerName, string fromScreen, string toScreen, string transitionType)
        {
#if UNITY_EDITOR
            var record = new TransitionRecord
            {
                StartTime = Time.realtimeSinceStartup,
                ContainerName = containerName,
                FromScreen = fromScreen,
                ToScreen = toScreen,
                TransitionType = transitionType,
                IsCompleted = false
            };

            _transitionHistory.Add(record);

            // Trim history
            if (_transitionHistory.Count > MaxTransitionHistory)
            {
                _transitionHistory.RemoveRange(0, _transitionHistory.Count - MaxTransitionHistory);
            }
#endif
        }

        /// <summary>
        /// Record a transition completion
        /// </summary>
        public void RecordTransitionEnd(string containerName)
        {
#if UNITY_EDITOR
            // Find the most recent incomplete transition for this container
            for (int i = _transitionHistory.Count - 1; i >= 0; i--)
            {
                var record = _transitionHistory[i];
                if (record.ContainerName == containerName && !record.IsCompleted)
                {
                    record.EndTime = Time.realtimeSinceStartup;
                    record.Duration = record.EndTime - record.StartTime;
                    record.IsCompleted = true;
                    _transitionHistory[i] = record;
                    break;
                }
            }
#endif
        }

        /// <summary>
        /// Record an asset load event
        /// </summary>
        public void RecordAssetLoad(string assetKey, float loadTime, bool wasAsync, bool fromCache)
        {
#if UNITY_EDITOR
            var record = new LifecycleEventRecord
            {
                Timestamp = Time.realtimeSinceStartup,
                EventName = fromCache ? "Asset Loaded (Cached)" : "Asset Loaded",
                ContainerName = "AssetLoader",
                ScreenName = assetKey,
                Category = "AssetLoading",
                Duration = loadTime,
                AdditionalInfo = $"Async: {wasAsync}, LoadTime: {loadTime:F3}s"
            };

            _eventHistory.Add(record);
#endif
        }

        /// <summary>
        /// Record performance snapshot - optimized to reduce overhead
        /// </summary>
        private void RecordPerformanceSnapshot()
        {
            _frameCount++;

            // Sample memory less frequently (once per second instead of every frame)
            if (_frameCount % MEMORY_SAMPLE_FREQUENCY == 0)
            {
                _lastMemoryReading = (float)GC.GetTotalMemory(false) / (1024f * 1024f); // Convert to MB
            }

            var snapshot = new PerformanceSnapshot
            {
                Timestamp = Time.realtimeSinceStartup,
                FPS = 1f / Time.deltaTime,
                FrameTime = Time.deltaTime * 1000f, // Convert to ms
                MemoryUsage = _lastMemoryReading // Use cached memory reading
            };

            _performanceSnapshots.Add(snapshot);

            // Trim snapshots more efficiently - remove in batch when we exceed capacity
            if (_performanceSnapshots.Count > MaxPerformanceSnapshots + 60) // Allow some overflow before trimming
            {
                _performanceSnapshots.RemoveRange(0, 60); // Remove 60 items at once instead of one at a time
            }
        }

        /// <summary>
        /// Clear all recorded data
        /// </summary>
        public void Clear()
        {
            _eventHistory.Clear();
            _transitionHistory.Clear();
            _performanceSnapshots.Clear();
        }

        /// <summary>
        /// Clear old data beyond a time threshold
        /// </summary>
        public void ClearOldData(float maxAge)
        {
            float cutoffTime = Time.realtimeSinceStartup - maxAge;

            _eventHistory.RemoveAll(e => e.Timestamp < cutoffTime);
            _transitionHistory.RemoveAll(t => t.StartTime < cutoffTime);
            _performanceSnapshots.RemoveAll(p => p.Timestamp < cutoffTime);
        }

        /// <summary>
        /// Get events filtered by category
        /// </summary>
        public List<LifecycleEventRecord> GetEventsByCategory(string category)
        {
            return _eventHistory.FindAll(e => e.Category == category);
        }

        /// <summary>
        /// Get recent performance metrics
        /// </summary>
        public PerformanceMetrics GetRecentPerformanceMetrics(float timeWindow = 1f)
        {
            float cutoffTime = Time.realtimeSinceStartup - timeWindow;
            var recentSnapshots = _performanceSnapshots.FindAll(s => s.Timestamp >= cutoffTime);

            if (recentSnapshots.Count == 0)
            {
                return new PerformanceMetrics();
            }

            float sumFPS = 0, sumFrameTime = 0, sumMemory = 0;
            float minFPS = float.MaxValue, maxFPS = 0;
            float minFrameTime = float.MaxValue, maxFrameTime = 0;

            foreach (var snapshot in recentSnapshots)
            {
                sumFPS += snapshot.FPS;
                sumFrameTime += snapshot.FrameTime;
                sumMemory += snapshot.MemoryUsage;

                minFPS = Mathf.Min(minFPS, snapshot.FPS);
                maxFPS = Mathf.Max(maxFPS, snapshot.FPS);
                minFrameTime = Mathf.Min(minFrameTime, snapshot.FrameTime);
                maxFrameTime = Mathf.Max(maxFrameTime, snapshot.FrameTime);
            }

            int count = recentSnapshots.Count;
            return new PerformanceMetrics
            {
                AverageFPS = sumFPS / count,
                MinFPS = minFPS,
                MaxFPS = maxFPS,
                AverageFrameTime = sumFrameTime / count,
                MinFrameTime = minFrameTime,
                MaxFrameTime = maxFrameTime,
                AverageMemoryUsage = sumMemory / count,
                SampleCount = count
            };
        }

        /// <summary>
        /// Lifecycle event record
        /// </summary>
        [Serializable]
        public struct LifecycleEventRecord
        {
            public float Timestamp;
            public string EventName;
            public string ContainerName;
            public string ScreenName;
            public string Category;
            public float Duration;
            public string AdditionalInfo;
            public string StackTrace;
        }

        /// <summary>
        /// Transition record
        /// </summary>
        [Serializable]
        public struct TransitionRecord
        {
            public float StartTime;
            public float EndTime;
            public float Duration;
            public string ContainerName;
            public string FromScreen;
            public string ToScreen;
            public string TransitionType;
            public bool IsCompleted;
        }

        /// <summary>
        /// Performance snapshot
        /// </summary>
        [Serializable]
        public struct PerformanceSnapshot
        {
            public float Timestamp;
            public float FPS;
            public float FrameTime;
            public float MemoryUsage;
        }

        /// <summary>
        /// Performance metrics summary
        /// </summary>
        [Serializable]
        public struct PerformanceMetrics
        {
            public float AverageFPS;
            public float MinFPS;
            public float MaxFPS;
            public float AverageFrameTime;
            public float MinFrameTime;
            public float MaxFrameTime;
            public float AverageMemoryUsage;
            public int SampleCount;
        }
    }
}
