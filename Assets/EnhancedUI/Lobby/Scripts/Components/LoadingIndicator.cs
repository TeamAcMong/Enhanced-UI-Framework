using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EnhancedUI.Demo.Components
{
    /// <summary>
    /// Loading indicator component - shows spinning icon and optional message
    /// Used during screen transitions and async operations
    /// </summary>
    public class LoadingIndicator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform spinnerTransform;
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private Image backgroundOverlay;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Configuration")]
        [SerializeField] private float rotationSpeed = 360f; // Degrees per second
        [SerializeField] private string defaultMessage = "Loading...";
        [SerializeField] private bool blockInput = true;

        [Header("Fade Animation")]
        [SerializeField] private bool useFadeAnimation = true;
        [SerializeField] private float fadeDuration = 0.3f;

        private bool _isShowing = false;
        private bool _isFading = false;

        private void Awake()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            // Start hidden
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_isShowing && spinnerTransform != null)
            {
                // Rotate spinner
                float rotation = rotationSpeed * Time.deltaTime;
                spinnerTransform.Rotate(0, 0, -rotation);
            }
        }

        /// <summary>
        /// Show loading indicator
        /// </summary>
        public void Show(string message = null)
        {
            if (_isShowing) return;

            gameObject.SetActive(true);
            _isShowing = true;

            // Set message
            if (loadingText != null)
            {
                loadingText.text = string.IsNullOrEmpty(message) ? defaultMessage : message;
            }

            // Set input blocking
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = blockInput;
            }

            // Fade in
            if (useFadeAnimation)
            {
                StartCoroutine(FadeIn());
            }
            else
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }

            Debug.Log($"[LoadingIndicator] Showing: {loadingText?.text}");
        }

        /// <summary>
        /// Hide loading indicator
        /// </summary>
        public void Hide()
        {
            if (!_isShowing) return;

            _isShowing = false;

            // Fade out
            if (useFadeAnimation)
            {
                StartCoroutine(FadeOut());
            }
            else
            {
                gameObject.SetActive(false);
            }

            Debug.Log("[LoadingIndicator] Hidden");
        }

        /// <summary>
        /// Check if currently showing
        /// </summary>
        public bool IsShowing()
        {
            return _isShowing;
        }

        /// <summary>
        /// Update loading message
        /// </summary>
        public void SetMessage(string message)
        {
            if (loadingText != null)
            {
                loadingText.text = message;
            }
        }

        /// <summary>
        /// Fade in animation
        /// </summary>
        private System.Collections.IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;

            _isFading = true;
            float elapsed = 0f;
            canvasGroup.alpha = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f;
            _isFading = false;
        }

        /// <summary>
        /// Fade out animation
        /// </summary>
        private System.Collections.IEnumerator FadeOut()
        {
            if (canvasGroup == null)
            {
                gameObject.SetActive(false);
                yield break;
            }

            _isFading = true;
            float elapsed = 0f;
            canvasGroup.alpha = 1f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            _isFading = false;
        }

#if UNITY_EDITOR
        // Editor-only: Setup default references
        private void Reset()
        {
            spinnerTransform = transform.Find("Spinner")?.GetComponent<RectTransform>();
            loadingText = transform.Find("LoadingText")?.GetComponent<TextMeshProUGUI>();
            backgroundOverlay = GetComponent<Image>();
            canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
#endif
    }
}
