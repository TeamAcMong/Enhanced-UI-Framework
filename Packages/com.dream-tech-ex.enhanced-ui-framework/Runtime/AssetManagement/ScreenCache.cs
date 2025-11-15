using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.Utilities;

namespace EnhancedUI.AssetManagement
{
    /// <summary>
    /// LRU cache for screen instances
    /// </summary>
    public class ScreenCache
    {
        private class CacheEntry
        {
            public string Key;
            public GameObject Instance;
            public int AccessCount;
            public float LastAccessTime;
            public long EstimatedSizeBytes;
        }

        private readonly Dictionary<string, CacheEntry> _cache = new Dictionary<string, CacheEntry>();
        private readonly LinkedList<string> _lruList = new LinkedList<string>();
        private readonly long _maxSizeBytes;

        private long _currentSizeBytes;

        public ScreenCache(int maxSizeMB)
        {
            _maxSizeBytes = (long)maxSizeMB * 1024 * 1024;
        }

        /// <summary>
        /// Add or update a cached instance
        /// </summary>
        public void Add(string key, GameObject instance)
        {
            if (instance == null)
                return;

            // Estimate memory size (rough approximation)
            long size = EstimateMemorySize(instance);

            // Remove if already cached
            if (_cache.ContainsKey(key))
            {
                Remove(key);
            }

            // Ensure space
            while (_currentSizeBytes + size > _maxSizeBytes && _lruList.Count > 0)
            {
                var lruKey = _lruList.First.Value;
                Remove(lruKey);
                ScreenLogger.LogPerformance($"Evicted from cache: {lruKey} (memory budget)");
            }

            // Add to cache
            var entry = new CacheEntry
            {
                Key = key,
                Instance = instance,
                AccessCount = 1,
                LastAccessTime = Time.time,
                EstimatedSizeBytes = size
            };

            _cache[key] = entry;
            _lruList.AddLast(key);
            _currentSizeBytes += size;

            instance.SetActive(false);
            ScreenLogger.LogPerformance($"Cached screen: {key} ({FormatBytes(size)})");
        }

        /// <summary>
        /// Get a cached instance
        /// </summary>
        public GameObject Get(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                // Update LRU
                _lruList.Remove(key);
                _lruList.AddLast(key);

                // Update access info
                entry.AccessCount++;
                entry.LastAccessTime = Time.time;

                ScreenLogger.LogPerformance($"Cache hit: {key} (accessed {entry.AccessCount} times)");
                return entry.Instance;
            }

            return null;
        }

        /// <summary>
        /// Check if key is cached
        /// </summary>
        public bool Contains(string key)
        {
            return _cache.ContainsKey(key);
        }

        /// <summary>
        /// Remove from cache
        /// </summary>
        public bool Remove(string key)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.Instance != null)
                {
                    Object.Destroy(entry.Instance);
                }

                _currentSizeBytes -= entry.EstimatedSizeBytes;
                _cache.Remove(key);
                _lruList.Remove(key);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Clear all cached instances
        /// </summary>
        public void Clear()
        {
            foreach (var entry in _cache.Values)
            {
                if (entry.Instance != null)
                {
                    Object.Destroy(entry.Instance);
                }
            }

            _cache.Clear();
            _lruList.Clear();
            _currentSizeBytes = 0;

            ScreenLogger.LogPerformance("Screen cache cleared");
        }

        /// <summary>
        /// Get cache statistics
        /// </summary>
        public CacheStats GetStats()
        {
            return new CacheStats
            {
                Count = _cache.Count,
                CurrentSizeBytes = _currentSizeBytes,
                MaxSizeBytes = _maxSizeBytes,
                UtilizationPercent = _maxSizeBytes > 0 ? (float)_currentSizeBytes / _maxSizeBytes * 100f : 0f
            };
        }

        private long EstimateMemorySize(GameObject instance)
        {
            // Rough estimation based on component count and mesh renderers
            long size = 1024; // Base overhead

            var components = instance.GetComponentsInChildren<Component>(true);
            size += components.Length * 256; // ~256 bytes per component

            var renderers = instance.GetComponentsInChildren<UnityEngine.UI.Graphic>(true);
            foreach (var renderer in renderers)
            {
                if (renderer.mainTexture != null)
                {
                    size += renderer.mainTexture.width * renderer.mainTexture.height * 4; // RGBA
                }
            }

            return size;
        }

        private string FormatBytes(long bytes)
        {
            if (bytes < 1024) return $"{bytes} B";
            if (bytes < 1024 * 1024) return $"{bytes / 1024} KB";
            return $"{bytes / (1024 * 1024)} MB";
        }
    }

    public struct CacheStats
    {
        public int Count;
        public long CurrentSizeBytes;
        public long MaxSizeBytes;
        public float UtilizationPercent;
    }
}
