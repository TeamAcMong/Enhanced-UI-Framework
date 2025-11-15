using System;
using UnityEngine;
using UnityScreen = UnityEngine.Screen;

namespace EnhancedUI.Demo.Utils
{
    /// <summary>
    /// Orientation manager - handles screen orientation changes and notifications
    /// Useful for games that support both portrait and landscape modes
    /// </summary>
    public class OrientationManager : MonoBehaviour
    {
        private static OrientationManager _instance;
        public static OrientationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[Orientation Manager]");
                    _instance = go.AddComponent<OrientationManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [Header("Configuration")]
        [SerializeField] private bool autoManageOrientation = true;
        [SerializeField] private ScreenOrientation defaultOrientation = ScreenOrientation.Portrait;
        [SerializeField] private bool allowAutoRotation = true;

        [Header("Allowed Orientations")]
        [SerializeField] private bool allowPortrait = true;
        [SerializeField] private bool allowPortraitUpsideDown = false;
        [SerializeField] private bool allowLandscapeLeft = true;
        [SerializeField] private bool allowLandscapeRight = true;

        private ScreenOrientation _currentOrientation;
        private float _checkInterval = 0.5f; // Check orientation every 0.5 seconds
        private float _lastCheckTime;

        // Events
        public event Action<ScreenOrientation, ScreenOrientation> OnOrientationChanged; // old, new
        public event Action OnOrientationToPortrait;
        public event Action OnOrientationToLandscape;

        public ScreenOrientation CurrentOrientation => _currentOrientation;
        public bool IsPortrait => _currentOrientation == ScreenOrientation.Portrait || _currentOrientation == ScreenOrientation.PortraitUpsideDown;
        public bool IsLandscape => _currentOrientation == ScreenOrientation.LandscapeLeft || _currentOrientation == ScreenOrientation.LandscapeRight;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Initialize()
        {
            _currentOrientation = UnityScreen.orientation;

            if (autoManageOrientation)
            {
                ApplyOrientationSettings();
            }

            Debug.Log($"[OrientationManager] Initialized with orientation: {_currentOrientation}");
        }

        private void Update()
        {
            // Check for orientation changes periodically
            if (Time.realtimeSinceStartup - _lastCheckTime >= _checkInterval)
            {
                _lastCheckTime = Time.realtimeSinceStartup;
                CheckOrientationChange();
            }
        }

        /// <summary>
        /// Check if orientation has changed and notify listeners
        /// </summary>
        private void CheckOrientationChange()
        {
            ScreenOrientation newOrientation = UnityScreen.orientation;

            if (newOrientation != _currentOrientation)
            {
                ScreenOrientation oldOrientation = _currentOrientation;
                _currentOrientation = newOrientation;

                Debug.Log($"[OrientationManager] Orientation changed: {oldOrientation} -> {newOrientation}");

                // Notify listeners
                OnOrientationChanged?.Invoke(oldOrientation, newOrientation);

                // Specific orientation events
                bool wasPortrait = oldOrientation == ScreenOrientation.Portrait || oldOrientation == ScreenOrientation.PortraitUpsideDown;
                bool isNowPortrait = IsPortrait;

                if (!wasPortrait && isNowPortrait)
                {
                    OnOrientationToPortrait?.Invoke();
                }
                else if (wasPortrait && !isNowPortrait)
                {
                    OnOrientationToLandscape?.Invoke();
                }
            }
        }

        /// <summary>
        /// Apply orientation settings to Unity's Screen API
        /// </summary>
        private void ApplyOrientationSettings()
        {
            if (allowAutoRotation)
            {
                UnityScreen.autorotateToPortrait = allowPortrait;
                UnityScreen.autorotateToPortraitUpsideDown = allowPortraitUpsideDown;
                UnityScreen.autorotateToLandscapeLeft = allowLandscapeLeft;
                UnityScreen.autorotateToLandscapeRight = allowLandscapeRight;
                UnityScreen.orientation = ScreenOrientation.AutoRotation;
            }
            else
            {
                UnityScreen.orientation = defaultOrientation;
            }

            Debug.Log($"[OrientationManager] Applied orientation settings - Auto: {allowAutoRotation}");
        }

        /// <summary>
        /// Set orientation to portrait
        /// </summary>
        public void SetPortrait()
        {
            UnityScreen.orientation = ScreenOrientation.Portrait;
            Debug.Log("[OrientationManager] Set orientation to Portrait");
        }

        /// <summary>
        /// Set orientation to landscape
        /// </summary>
        public void SetLandscape()
        {
            UnityScreen.orientation = ScreenOrientation.LandscapeLeft;
            Debug.Log("[OrientationManager] Set orientation to Landscape");
        }

        /// <summary>
        /// Enable auto-rotation
        /// </summary>
        public void EnableAutoRotation(bool portrait, bool landscape)
        {
            allowAutoRotation = true;
            allowPortrait = portrait;
            allowPortraitUpsideDown = portrait;
            allowLandscapeLeft = landscape;
            allowLandscapeRight = landscape;

            ApplyOrientationSettings();
        }

        /// <summary>
        /// Lock orientation to specific mode
        /// </summary>
        public void LockOrientation(ScreenOrientation orientation)
        {
            allowAutoRotation = false;
            defaultOrientation = orientation;
            UnityScreen.orientation = orientation;

            Debug.Log($"[OrientationManager] Locked orientation to {orientation}");
        }

        /// <summary>
        /// Unlock orientation (enable auto-rotation)
        /// </summary>
        public void UnlockOrientation()
        {
            allowAutoRotation = true;
            ApplyOrientationSettings();

            Debug.Log("[OrientationManager] Unlocked orientation");
        }

        /// <summary>
        /// Check if specific orientation is currently active
        /// </summary>
        public bool IsCurrentOrientation(ScreenOrientation orientation)
        {
            return _currentOrientation == orientation;
        }

        /// <summary>
        /// Get screen aspect ratio
        /// </summary>
        public float GetAspectRatio()
        {
            return (float)UnityScreen.width / UnityScreen.height;
        }

        /// <summary>
        /// Check if screen is in portrait aspect (taller than wide)
        /// </summary>
        public bool IsPortraitAspect()
        {
            return UnityScreen.height > UnityScreen.width;
        }

        /// <summary>
        /// Check if screen is in landscape aspect (wider than tall)
        /// </summary>
        public bool IsLandscapeAspect()
        {
            return UnityScreen.width > UnityScreen.height;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Simulate orientation change (for testing in editor)
        /// </summary>
        [ContextMenu("Simulate Portrait")]
        private void SimulatePortrait()
        {
            ScreenOrientation oldOrientation = _currentOrientation;
            _currentOrientation = ScreenOrientation.Portrait;
            OnOrientationChanged?.Invoke(oldOrientation, _currentOrientation);
            OnOrientationToPortrait?.Invoke();
            Debug.Log("[OrientationManager] Simulated Portrait orientation");
        }

        [ContextMenu("Simulate Landscape")]
        private void SimulateLandscape()
        {
            ScreenOrientation oldOrientation = _currentOrientation;
            _currentOrientation = ScreenOrientation.LandscapeLeft;
            OnOrientationChanged?.Invoke(oldOrientation, _currentOrientation);
            OnOrientationToLandscape?.Invoke();
            Debug.Log("[OrientationManager] Simulated Landscape orientation");
        }
#endif
    }
}
