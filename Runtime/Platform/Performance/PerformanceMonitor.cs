using UnityEngine;
using EnhancedUI.Utilities;

namespace EnhancedUI.Platform.Performance
{
    /// <summary>
    /// Simple performance monitor for FPS and memory tracking
    /// </summary>
    public class PerformanceMonitor : MonoBehaviour
    {
        private static PerformanceMonitor _instance;

        private float _deltaTime;
        private int _frameCount;
        private float _fps;
        private float _updateInterval = 0.5f;
        private float _timeSinceLastUpdate;

        public static PerformanceMonitor Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[Performance Monitor]");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<PerformanceMonitor>();
                }
                return _instance;
            }
        }

        public float CurrentFPS => _fps;
        public long CurrentMemoryMB => System.GC.GetTotalMemory(false) / (1024 * 1024);

        private void Update()
        {
            _frameCount++;
            _deltaTime += Time.unscaledDeltaTime;
            _timeSinceLastUpdate += Time.unscaledDeltaTime;

            if (_timeSinceLastUpdate >= _updateInterval)
            {
                _fps = _frameCount / _deltaTime;
                _frameCount = 0;
                _deltaTime = 0f;
                _timeSinceLastUpdate = 0f;

                CheckPerformance();
            }
        }

        private void CheckPerformance()
        {
            if (!EnhancedUISettings.Instance.showPerformanceWarnings)
                return;

            // Warn on low FPS
            if (_fps < 30f)
            {
                ScreenLogger.LogPerformance($"Low FPS detected: {_fps:F1} FPS");
            }

            // Warn on high memory
            long memoryMB = CurrentMemoryMB;
            long budgetMB = EnhancedUISettings.Instance.memoryBudgetMB;

            if (budgetMB > 0 && memoryMB > budgetMB)
            {
                ScreenLogger.LogPerformance($"Memory over budget: {memoryMB}MB / {budgetMB}MB");
            }
        }

        /// <summary>
        /// Set the update interval for FPS calculation
        /// </summary>
        public void SetUpdateInterval(float interval)
        {
            _updateInterval = Mathf.Max(0.1f, interval);
        }

        /// <summary>
        /// Log current performance stats
        /// </summary>
        public void LogStats()
        {
            ScreenLogger.Log(ScreenLogger.LogCategory.Performance,
                $"FPS: {_fps:F1}, Memory: {CurrentMemoryMB}MB");
        }
    }
}
