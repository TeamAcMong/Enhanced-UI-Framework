using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace EnhancedUI.Demo.Components
{
    /// <summary>
    /// Level button component for level selection screen
    /// Shows level number, lock state, completion stars, etc.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class LevelButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI levelNumberText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject[] stars; // 3 stars for completion rating
        [SerializeField] private Image progressFill; // Optional progress indicator

        [Header("Visual States")]
        [SerializeField] private Sprite unlockedSprite;
        [SerializeField] private Sprite lockedSprite;
        [SerializeField] private Sprite completedSprite;
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        [SerializeField] private Color completedColor = new Color(1f, 0.9f, 0.4f);

        [Header("Configuration")]
        [SerializeField] private string levelId;
        [SerializeField] private int levelNumber;
        [SerializeField] private bool isLocked = true;
        [SerializeField] private int starsEarned = 0;

        // Events
        public event Action<string, int> OnLevelButtonClicked; // levelId, levelNumber

        public string LevelId => levelId;
        public int LevelNumber => levelNumber;
        public bool IsLocked => isLocked;
        public int StarsEarned => starsEarned;

        private void Awake()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (button != null)
            {
                button.onClick.AddListener(OnClick);
            }

            UpdateVisuals();
        }

        /// <summary>
        /// Setup level button
        /// </summary>
        public void Setup(string id, int number, bool locked, int stars = 0)
        {
            levelId = id;
            levelNumber = number;
            isLocked = locked;
            starsEarned = Mathf.Clamp(stars, 0, 3);

            UpdateVisuals();
        }

        /// <summary>
        /// Unlock this level
        /// </summary>
        public void Unlock()
        {
            if (isLocked)
            {
                isLocked = false;
                UpdateVisuals();

                // Play unlock animation
                StartCoroutine(AnimateUnlock());
            }
        }

        /// <summary>
        /// Set stars earned (0-3)
        /// </summary>
        public void SetStarsEarned(int stars)
        {
            starsEarned = Mathf.Clamp(stars, 0, 3);
            UpdateStars();
        }

        /// <summary>
        /// Update button visuals based on state
        /// </summary>
        private void UpdateVisuals()
        {
            // Update level number text
            if (levelNumberText != null)
            {
                levelNumberText.text = levelNumber.ToString();
                levelNumberText.gameObject.SetActive(!isLocked);
            }

            // Update lock icon
            if (lockIcon != null)
            {
                lockIcon.SetActive(isLocked);
            }

            // Update background
            if (backgroundImage != null)
            {
                if (isLocked && lockedSprite != null)
                {
                    backgroundImage.sprite = lockedSprite;
                    backgroundImage.color = lockedColor;
                }
                else if (starsEarned > 0 && completedSprite != null)
                {
                    backgroundImage.sprite = completedSprite;
                    backgroundImage.color = completedColor;
                }
                else if (unlockedSprite != null)
                {
                    backgroundImage.sprite = unlockedSprite;
                    backgroundImage.color = unlockedColor;
                }
            }

            // Update stars
            UpdateStars();

            // Update button interactability
            if (button != null)
            {
                button.interactable = !isLocked;
            }
        }

        /// <summary>
        /// Update star display
        /// </summary>
        private void UpdateStars()
        {
            if (stars == null || stars.Length == 0) return;

            for (int i = 0; i < stars.Length; i++)
            {
                if (stars[i] != null)
                {
                    // Show stars only if level is completed (not locked)
                    bool showStar = !isLocked && i < starsEarned;
                    stars[i].SetActive(showStar);
                }
            }
        }

        /// <summary>
        /// Set progress (0-1) for partially completed levels
        /// </summary>
        public void SetProgress(float progress)
        {
            if (progressFill != null)
            {
                progressFill.fillAmount = Mathf.Clamp01(progress);
                progressFill.gameObject.SetActive(progress > 0f && progress < 1f && !isLocked);
            }
        }

        /// <summary>
        /// Button click handler
        /// </summary>
        private void OnClick()
        {
            if (isLocked)
            {
                Debug.Log($"[LevelButton] Level {levelNumber} is locked");
                // Play "locked" sound/animation
                StartCoroutine(AnimateShake());
                return;
            }

            Debug.Log($"[LevelButton] Level {levelNumber} clicked");
            OnLevelButtonClicked?.Invoke(levelId, levelNumber);

            // Play click animation
            StartCoroutine(AnimateClick());
        }

        /// <summary>
        /// Animate unlock effect
        /// </summary>
        private System.Collections.IEnumerator AnimateUnlock()
        {
            // Scale up and fade in effect
            Vector3 originalScale = transform.localScale;
            transform.localScale = originalScale * 0.5f;

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Ease out elastic
                float scale = Mathf.Lerp(0.5f, 1.2f, progress);
                transform.localScale = originalScale * scale;

                yield return null;
            }

            // Bounce back to normal
            elapsed = 0f;
            duration = 0.2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float scale = Mathf.Lerp(1.2f, 1f, progress);
                transform.localScale = originalScale * scale;

                yield return null;
            }

            transform.localScale = originalScale;
        }

        /// <summary>
        /// Animate click (scale bounce)
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

            // Scale up
            elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                transform.localScale = Vector3.Lerp(originalScale * 0.9f, originalScale, progress);
                yield return null;
            }

            transform.localScale = originalScale;
        }

        /// <summary>
        /// Animate shake (when clicking locked level)
        /// </summary>
        private System.Collections.IEnumerator AnimateShake()
        {
            Vector3 originalPos = transform.localPosition;
            float duration = 0.3f;
            float magnitude = 10f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;

                // Diminishing shake
                float currentMagnitude = magnitude * (1f - progress);
                float x = originalPos.x + UnityEngine.Random.Range(-currentMagnitude, currentMagnitude);

                transform.localPosition = new Vector3(x, originalPos.y, originalPos.z);

                yield return null;
            }

            transform.localPosition = originalPos;
        }

#if UNITY_EDITOR
        // Editor-only: Setup default references
        private void Reset()
        {
            button = GetComponent<Button>();
            levelNumberText = transform.Find("LevelNumber")?.GetComponent<TextMeshProUGUI>();
            backgroundImage = GetComponent<Image>();
            lockIcon = transform.Find("LockIcon")?.gameObject;

            // Find stars
            Transform starsParent = transform.Find("Stars");
            if (starsParent != null)
            {
                stars = new GameObject[3];
                for (int i = 0; i < 3; i++)
                {
                    Transform star = starsParent.Find($"Star{i + 1}");
                    if (star != null)
                    {
                        stars[i] = star.gameObject;
                    }
                }
            }

            progressFill = transform.Find("ProgressBar/Fill")?.GetComponent<Image>();
        }

        // Editor-only: Update visuals in edit mode
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            // Update visuals in editor for preview
            if (levelNumberText != null)
            {
                levelNumberText.text = levelNumber.ToString();
            }
        }
#endif
    }
}
