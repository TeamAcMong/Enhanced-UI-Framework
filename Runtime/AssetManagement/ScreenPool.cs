using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.Utilities;

namespace EnhancedUI.AssetManagement
{
    /// <summary>
    /// Object pool for frequently used screens
    /// </summary>
    public class ScreenPool
    {
        private readonly Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
        private readonly Dictionary<string, HashSet<GameObject>> _activeObjects = new Dictionary<string, HashSet<GameObject>>();
        private readonly Transform _poolRoot;

        public ScreenPool(Transform poolRoot = null)
        {
            if (poolRoot == null)
            {
                var rootGO = new GameObject("[Screen Pool]");
                Object.DontDestroyOnLoad(rootGO);
                _poolRoot = rootGO.transform;
            }
            else
            {
                _poolRoot = poolRoot;
            }
        }

        /// <summary>
        /// Get a pooled instance or create new
        /// </summary>
        public GameObject Get(string key, GameObject prefab, Transform parent = null)
        {
            if (!_pools.ContainsKey(key))
            {
                _pools[key] = new Queue<GameObject>();
                _activeObjects[key] = new HashSet<GameObject>();
            }

            GameObject instance;
            if (_pools[key].Count > 0)
            {
                instance = _pools[key].Dequeue();
                instance.SetActive(true);
                ScreenLogger.LogPerformance($"Reused pooled screen: {key}");
            }
            else
            {
                instance = Object.Instantiate(prefab);
                instance.name = $"{prefab.name} (Pooled)";
                ScreenLogger.LogPerformance($"Created new pooled screen: {key}");
            }

            if (parent != null)
            {
                instance.transform.SetParent(parent, false);
            }

            _activeObjects[key].Add(instance);
            return instance;
        }

        /// <summary>
        /// Return an instance to the pool
        /// </summary>
        public void Return(string key, GameObject instance)
        {
            if (instance == null)
                return;

            if (!_pools.ContainsKey(key))
            {
                Object.Destroy(instance);
                return;
            }

            _activeObjects[key].Remove(instance);
            instance.SetActive(false);
            instance.transform.SetParent(_poolRoot, false);
            _pools[key].Enqueue(instance);

            ScreenLogger.LogPerformance($"Returned screen to pool: {key}");
        }

        /// <summary>
        /// Preload instances into the pool
        /// </summary>
        public void Preload(string key, GameObject prefab, int count)
        {
            if (!_pools.ContainsKey(key))
            {
                _pools[key] = new Queue<GameObject>();
                _activeObjects[key] = new HashSet<GameObject>();
            }

            for (int i = 0; i < count; i++)
            {
                var instance = Object.Instantiate(prefab, _poolRoot);
                instance.name = $"{prefab.name} (Pooled)";
                instance.SetActive(false);
                _pools[key].Enqueue(instance);
            }

            ScreenLogger.LogPerformance($"Preloaded {count} instances of {key}");
        }

        /// <summary>
        /// Clear a specific pool
        /// </summary>
        public void Clear(string key)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                while (pool.Count > 0)
                {
                    var instance = pool.Dequeue();
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                }
            }

            if (_activeObjects.TryGetValue(key, out var activeSet))
            {
                foreach (var instance in activeSet)
                {
                    if (instance != null)
                    {
                        Object.Destroy(instance);
                    }
                }
                activeSet.Clear();
            }

            _pools.Remove(key);
            _activeObjects.Remove(key);
        }

        /// <summary>
        /// Clear all pools
        /// </summary>
        public void ClearAll()
        {
            foreach (var key in new List<string>(_pools.Keys))
            {
                Clear(key);
            }
        }

        /// <summary>
        /// Get pool statistics
        /// </summary>
        public PoolStats GetStats(string key)
        {
            return new PoolStats
            {
                Key = key,
                PooledCount = _pools.ContainsKey(key) ? _pools[key].Count : 0,
                ActiveCount = _activeObjects.ContainsKey(key) ? _activeObjects[key].Count : 0
            };
        }
    }

    public struct PoolStats
    {
        public string Key;
        public int PooledCount;
        public int ActiveCount;
        public int TotalCount => PooledCount + ActiveCount;
    }
}
