using UnityEngine;
using UnityScreen = UnityEngine.Screen;

namespace EnhancedUI.Demo.Utils
{
    /// <summary>
    /// Safe Area Adapter - automatically adjusts RectTransform to respect device safe area
    /// Essential for modern mobile devices with notches and rounded corners
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaAdapter : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private bool adaptOnEnable = true;
        [SerializeField] private bool adaptOnOrientationChange = true;
        [SerializeField] private bool logDebugInfo = false;

        [Header("Padding Override (optional)")]
        [SerializeField] private bool useCustomPadding = false;
        [SerializeField] private RectOffset customPadding = new RectOffset();

        [Header("Selective Adaptation")]
        [SerializeField] private bool adaptTop = true;
        [SerializeField] private bool adaptBottom = true;
        [SerializeField] private bool adaptLeft = true;
        [SerializeField] private bool adaptRight = true;

        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private ScreenOrientation _lastOrientation;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            if (adaptOnEnable)
            {
                ApplySafeArea();
            }
        }

        private void Update()
        {
            if (adaptOnOrientationChange)
            {
                // Check for orientation change or safe area change
                if (UnityScreen.safeArea != _lastSafeArea || UnityScreen.orientation != _lastOrientation)
                {
                    ApplySafeArea();
                }
            }
        }

        /// <summary>
        /// Apply safe area adjustments to RectTransform
        /// </summary>
        public void ApplySafeArea()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            Rect safeArea = UnityScreen.safeArea;
            _lastSafeArea = safeArea;
            _lastOrientation = UnityScreen.orientation;

            if (logDebugInfo)
            {
                Debug.Log($"[SafeAreaAdapter] Screen size: {UnityScreen.width}x{UnityScreen.height}");
                Debug.Log($"[SafeAreaAdapter] Safe area: {safeArea}");
                Debug.Log($"[SafeAreaAdapter] Orientation: {UnityScreen.orientation}");
            }

            // Get canvas size
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning("[SafeAreaAdapter] No Canvas found in parents");
                return;
            }

            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;

            // If canvas has zero size, use screen size
            if (canvasSize.x == 0 || canvasSize.y == 0)
            {
                canvasSize = new Vector2(UnityScreen.width, UnityScreen.height);
            }

            // Calculate anchor positions based on safe area
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;

            // Normalize to 0-1 range
            anchorMin.x /= UnityScreen.width;
            anchorMin.y /= UnityScreen.height;
            anchorMax.x /= UnityScreen.width;
            anchorMax.y /= UnityScreen.height;

            // Apply selective adaptation
            if (!adaptLeft)
                anchorMin.x = _rectTransform.anchorMin.x;
            if (!adaptBottom)
                anchorMin.y = _rectTransform.anchorMin.y;
            if (!adaptRight)
                anchorMax.x = _rectTransform.anchorMax.x;
            if (!adaptTop)
                anchorMax.y = _rectTransform.anchorMax.y;

            // Apply custom padding if enabled
            if (useCustomPadding)
            {
                Vector2 paddingMin = new Vector2(
                    customPadding.left / UnityScreen.width,
                    customPadding.bottom / UnityScreen.height
                );
                Vector2 paddingMax = new Vector2(
                    customPadding.right / UnityScreen.width,
                    customPadding.top / UnityScreen.height
                );

                anchorMin += paddingMin;
                anchorMax -= paddingMax;
            }

            // Apply to RectTransform
            _rectTransform.anchorMin = anchorMin;
            _rectTransform.anchorMax = anchorMax;

            // Reset position and size delta for clean adaptation
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;

            if (logDebugInfo)
            {
                Debug.Log($"[SafeAreaAdapter] Applied anchors: Min={anchorMin}, Max={anchorMax}");
            }
        }

        /// <summary>
        /// Reset to full screen (no safe area adaptation)
        /// </summary>
        public void ResetToFullScreen()
        {
            _rectTransform.anchorMin = Vector2.zero;
            _rectTransform.anchorMax = Vector2.one;
            _rectTransform.offsetMin = Vector2.zero;
            _rectTransform.offsetMax = Vector2.zero;
        }

        /// <summary>
        /// Set which edges should adapt to safe area
        /// </summary>
        public void SetAdaptationEdges(bool top, bool bottom, bool left, bool right)
        {
            adaptTop = top;
            adaptBottom = bottom;
            adaptLeft = left;
            adaptRight = right;

            ApplySafeArea();
        }

        /// <summary>
        /// Enable/disable safe area adaptation
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;

            if (enabled)
            {
                ApplySafeArea();
            }
            else
            {
                ResetToFullScreen();
            }
        }

#if UNITY_EDITOR
        // Editor-only: Apply safe area in edit mode for testing
        [ContextMenu("Apply Safe Area (Test)")]
        private void TestApplySafeArea()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            // Simulate common safe area scenarios
            SimulateSafeArea(SafeAreaPreset.iPhoneX);
        }

        [ContextMenu("Reset to Full Screen")]
        private void TestResetToFullScreen()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            ResetToFullScreen();
        }

        /// <summary>
        /// Simulate safe area for testing in editor
        /// </summary>
        private void SimulateSafeArea(SafeAreaPreset preset)
        {
            Rect simulatedSafeArea;

            switch (preset)
            {
                case SafeAreaPreset.iPhoneX:
                    // iPhone X in portrait: 1125 x 2436
                    // Safe area excludes notch (top ~132px) and home indicator (bottom ~102px)
                    simulatedSafeArea = new Rect(0, 102, 1125, 2436 - 102 - 132);
                    break;

                case SafeAreaPreset.iPadPro:
                    // iPad Pro has minimal safe area
                    simulatedSafeArea = new Rect(0, 20, 1668, 2388 - 20);
                    break;

                case SafeAreaPreset.AndroidNotch:
                    // Generic Android with notch
                    simulatedSafeArea = new Rect(0, 80, 1080, 2340 - 80 - 100);
                    break;

                default:
                    simulatedSafeArea = new Rect(0, 0, UnityScreen.width, UnityScreen.height);
                    break;
            }

            _lastSafeArea = simulatedSafeArea;
            Debug.Log($"[SafeAreaAdapter] Simulating {preset}: {simulatedSafeArea}");
        }

        private enum SafeAreaPreset
        {
            iPhoneX,
            iPadPro,
            AndroidNotch
        }
#endif
    }
}
