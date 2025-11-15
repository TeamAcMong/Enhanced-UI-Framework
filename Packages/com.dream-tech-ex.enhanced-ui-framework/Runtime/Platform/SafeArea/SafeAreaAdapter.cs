using UnityEngine;

namespace EnhancedUI.Platform.SafeArea
{
    /// <summary>
    /// Automatically adapts RectTransform to device safe area (notch, home indicator)
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaAdapter : MonoBehaviour
    {
        [SerializeField] private bool adaptOnAwake = true;
        [SerializeField] private bool adaptLeft = true;
        [SerializeField] private bool adaptRight = true;
        [SerializeField] private bool adaptTop = true;
        [SerializeField] private bool adaptBottom = true;

        private RectTransform _rectTransform;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();

            if (adaptOnAwake)
            {
                Apply();
            }
        }

        private void Update()
        {
            // Check if safe area or screen size changed
            if (_lastSafeArea != UnityEngine.Screen.safeArea ||
                _lastScreenSize.x != UnityEngine.Screen.width ||
                _lastScreenSize.y != UnityEngine.Screen.height)
            {
                Apply();
            }
        }

        /// <summary>
        /// Apply safe area adjustments
        /// </summary>
        public void Apply()
        {
            if (_rectTransform == null)
            {
                _rectTransform = GetComponent<RectTransform>();
            }

            if (!EnhancedUISettings.Instance.enableSafeArea)
            {
                return;
            }

            var safeArea = UnityEngine.Screen.safeArea;
            var screenSize = new Vector2(UnityEngine.Screen.width, UnityEngine.Screen.height);

            // Calculate anchors
            var anchorMin = safeArea.position;
            var anchorMax = safeArea.position + safeArea.size;

            // Normalize to 0-1
            anchorMin.x /= screenSize.x;
            anchorMin.y /= screenSize.y;
            anchorMax.x /= screenSize.x;
            anchorMax.y /= screenSize.y;

            // Apply selectively based on settings
            if (adaptLeft)
                _rectTransform.anchorMin = new Vector2(anchorMin.x, _rectTransform.anchorMin.y);

            if (adaptBottom)
                _rectTransform.anchorMin = new Vector2(_rectTransform.anchorMin.x, anchorMin.y);

            if (adaptRight)
                _rectTransform.anchorMax = new Vector2(anchorMax.x, _rectTransform.anchorMax.y);

            if (adaptTop)
                _rectTransform.anchorMax = new Vector2(_rectTransform.anchorMax.x, anchorMax.y);

            _lastSafeArea = safeArea;
            _lastScreenSize = new Vector2Int(UnityEngine.Screen.width, UnityEngine.Screen.height);
        }

        /// <summary>
        /// Reset to full screen
        /// </summary>
        public void Reset()
        {
            if (_rectTransform == null)
                return;

            _rectTransform.anchorMin = Vector2.zero;
            _rectTransform.anchorMax = Vector2.one;
        }

#if UNITY_EDITOR
        [ContextMenu("Apply Safe Area")]
        private void ApplyInEditor()
        {
            Apply();
        }

        [ContextMenu("Reset Safe Area")]
        private void ResetInEditor()
        {
            Reset();
        }
#endif
    }
}
