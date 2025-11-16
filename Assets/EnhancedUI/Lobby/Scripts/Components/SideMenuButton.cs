using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace EnhancedUI.Demo.Components
{
    /// <summary>
    /// Side menu button component - appears on left/right side of screen
    /// Common in mobile games for quick access to features (mail, shop, events, etc.)
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class SideMenuButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private GameObject notificationBadge;
        [SerializeField] private TextMeshProUGUI notificationCountText;
        [SerializeField] private Button button;

        [Header("Configuration")]
        [SerializeField] private string buttonId;
        [SerializeField] private Sprite iconSprite;
        [SerializeField] private string labelTextContent;
        [SerializeField] private bool showLabel = true;

        [Header("Animation")]
        [SerializeField] private bool animateOnEnable = true;
        [SerializeField] private float slideInDuration = 0.3f;
        [SerializeField] private float slideInDelay = 0f;
        [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Position")]
        [SerializeField] private SidePosition sidePosition = SidePosition.Left;
        [SerializeField] private float slideDistance = 200f;

        private RectTransform _rectTransform;
        private Vector2 _originalPosition;
        private int _notificationCount = 0;
        private bool _hasSlideIn = false;

        // Events
        public event Action<string> OnButtonClicked;

        public enum SidePosition
        {
            Left,
            Right
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _originalPosition = _rectTransform.anchoredPosition;

            // Get button reference if not set
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            // Setup button listener
            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }

            // Initialize visuals
            UpdateVisuals();
        }

        private void OnEnable()
        {
            if (animateOnEnable && !_hasSlideIn)
            {
                SlideIn();
            }
        }

        private void OnDisable()
        {
            _hasSlideIn = false;
        }

        /// <summary>
        /// Update button visuals (icon, label, etc.)
        /// </summary>
        private void UpdateVisuals()
        {
            // Update icon
            if (iconImage != null && iconSprite != null)
            {
                iconImage.sprite = iconSprite;
            }

            // Update label
            if (labelText != null)
            {
                labelText.text = labelTextContent;
                labelText.gameObject.SetActive(showLabel);
            }

            // Update notification badge
            UpdateNotificationBadge();
        }

        /// <summary>
        /// Set notification count
        /// </summary>
        public void SetNotificationCount(int count)
        {
            _notificationCount = count;
            UpdateNotificationBadge();
        }

        /// <summary>
        /// Update notification badge visibility and count
        /// </summary>
        private void UpdateNotificationBadge()
        {
            bool hasNotifications = _notificationCount > 0;

            if (notificationBadge != null)
            {
                notificationBadge.SetActive(hasNotifications);
            }

            if (notificationCountText != null && hasNotifications)
            {
                notificationCountText.text = _notificationCount > 99 ? "99+" : _notificationCount.ToString();
            }
        }

        /// <summary>
        /// Slide in animation
        /// </summary>
        private void SlideIn()
        {
            _hasSlideIn = true;

            // Calculate start position (off-screen)
            Vector2 startPos = _originalPosition;
            float direction = sidePosition == SidePosition.Left ? -1f : 1f;
            startPos.x += direction * slideDistance;

            // Set initial position
            _rectTransform.anchoredPosition = startPos;

            // Start animation
            StartCoroutine(AnimateSlideIn(startPos, _originalPosition));
        }

        /// <summary>
        /// Animate slide in
        /// </summary>
        private System.Collections.IEnumerator AnimateSlideIn(Vector2 start, Vector2 end)
        {
            // Wait for delay
            if (slideInDelay > 0)
            {
                yield return new UnityEngine.WaitForSeconds(slideInDelay);
            }

            float elapsed = 0f;

            while (elapsed < slideInDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / slideInDuration);
                float curveValue = slideInCurve.Evaluate(progress);

                _rectTransform.anchoredPosition = Vector2.Lerp(start, end, curveValue);

                yield return null;
            }

            // Ensure final position
            _rectTransform.anchoredPosition = end;
        }

        /// <summary>
        /// Button click handler
        /// </summary>
        private void OnClick()
        {
            Debug.Log($"[SideMenuButton] Button clicked: {buttonId}");
            OnButtonClicked?.Invoke(buttonId);

            // Add click animation
            StartCoroutine(AnimateClick());
        }

        /// <summary>
        /// Animate button click (scale bounce)
        /// </summary>
        private System.Collections.IEnumerator AnimateClick()
        {
            Vector3 originalScale = transform.localScale;
            float duration = 0.1f;

            // Scale down
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale, originalScale * 0.9f, progress);
                yield return null;
            }

            // Scale back up with bounce
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float overshoot = 1.1f;
                float scale = Mathf.Lerp(0.9f, overshoot, progress);
                transform.localScale = originalScale * scale;
                yield return null;
            }

            // Return to original
            elapsed = 0f;
            while (elapsed < duration * 0.5f)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / (duration * 0.5f);
                transform.localScale = Vector3.Lerp(originalScale * 1.1f, originalScale, progress);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        /// <summary>
        /// Set button configuration
        /// </summary>
        public void SetConfiguration(string id, Sprite icon, string label, int notificationCount = 0)
        {
            buttonId = id;
            iconSprite = icon;
            labelTextContent = label;
            _notificationCount = notificationCount;

            UpdateVisuals();
        }

        /// <summary>
        /// Set slide in delay (useful when multiple buttons animate in sequence)
        /// </summary>
        public void SetSlideInDelay(float delay)
        {
            slideInDelay = delay;
        }

#if UNITY_EDITOR
        // Editor-only: Setup default references
        private void Reset()
        {
            button = GetComponent<Button>();
            iconImage = transform.Find("Icon")?.GetComponent<Image>();
            labelText = transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            notificationBadge = transform.Find("NotificationBadge")?.gameObject;
            notificationCountText = notificationBadge?.transform.Find("Count")?.GetComponent<TextMeshProUGUI>();
        }

        // Editor-only: Update visuals in edit mode
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            if (iconImage != null && iconSprite != null)
            {
                iconImage.sprite = iconSprite;
            }

            if (labelText != null)
            {
                labelText.text = labelTextContent;
            }
        }
#endif
    }
}
