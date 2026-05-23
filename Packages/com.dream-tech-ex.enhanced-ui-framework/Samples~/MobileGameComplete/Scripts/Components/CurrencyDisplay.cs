using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Components
{
    /// <summary>
    /// Reusable currency display component - shows icon + value
    /// Automatically updates when currency changes in GameState
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class CurrencyDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI valueText;
        [SerializeField] private Button addButton; // Optional '+' button for premium currencies

        [Header("Configuration")]
        [SerializeField] private CurrencyType currencyType = CurrencyType.Gold;
        [SerializeField] private bool showAddButton = false;
        [SerializeField] private bool useCompactFormat = false; // 1000 -> 1K
        [SerializeField] private bool animateOnChange = true;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color insufficientColor = new Color(1f, 0.3f, 0.3f); // Red when insufficient
        [SerializeField] private float animationDuration = 0.3f;

        private int _currentDisplayValue;
        private int _targetValue;
        private float _animationTimer;
        private bool _isAnimating;

        private void Awake()
        {
            // Setup add button
            if (addButton != null)
            {
                addButton.gameObject.SetActive(showAddButton);
                addButton.onClick.AddListener(OnAddButtonClicked);
            }
        }

        private void OnEnable()
        {
            // Subscribe to currency changes
            if (GameState.Instance != null)
            {
                GameState.Instance.OnCurrencyChanged += OnCurrencyChanged;
            }

            // Initialize display
            UpdateDisplay(false);
        }

        private void OnDisable()
        {
            // Unsubscribe from currency changes
            if (GameState.Instance != null)
            {
                GameState.Instance.OnCurrencyChanged -= OnCurrencyChanged;
            }
        }

        private void Update()
        {
            if (_isAnimating)
            {
                _animationTimer += Time.deltaTime;
                float progress = Mathf.Clamp01(_animationTimer / animationDuration);

                // Lerp the display value
                int startValue = _currentDisplayValue;
                _currentDisplayValue = Mathf.RoundToInt(Mathf.Lerp(startValue, _targetValue, progress));

                // Update text
                UpdateValueText();

                // Check if animation complete
                if (progress >= 1f)
                {
                    _isAnimating = false;
                    _currentDisplayValue = _targetValue;
                    UpdateValueText();
                }
            }
        }

        /// <summary>
        /// Set currency type to display
        /// </summary>
        public void SetCurrencyType(CurrencyType type)
        {
            currencyType = type;
            UpdateDisplay(false);
        }

        /// <summary>
        /// Update display with current value from GameState
        /// </summary>
        public void UpdateDisplay(bool animate = true)
        {
            if (GameState.Instance == null) return;

            int newValue = GameState.Instance.GetCurrency(currencyType);

            if (animate && animateOnChange && _currentDisplayValue != newValue)
            {
                // Start animation
                _targetValue = newValue;
                _animationTimer = 0f;
                _isAnimating = true;
            }
            else
            {
                // Immediate update
                _currentDisplayValue = newValue;
                _targetValue = newValue;
                UpdateValueText();
            }

            // Update icon (if needed)
            UpdateIcon();
        }

        /// <summary>
        /// Set color to indicate insufficient currency
        /// </summary>
        public void SetInsufficientState(bool insufficient)
        {
            if (valueText != null)
            {
                valueText.color = insufficient ? insufficientColor : normalColor;
            }
        }

        /// <summary>
        /// Update the value text
        /// </summary>
        private void UpdateValueText()
        {
            if (valueText == null) return;

            string formattedValue = useCompactFormat
                ? FormatCompact(_currentDisplayValue)
                : _currentDisplayValue.ToString("N0");

            // Special formatting for energy (show current/max)
            if (currencyType == CurrencyType.Energy && GameState.Instance != null)
            {
                int maxEnergy = GameState.Instance.PlayerData.maxEnergy;
                formattedValue = $"{_currentDisplayValue}/{maxEnergy}";
            }

            valueText.text = formattedValue;
        }

        /// <summary>
        /// Update the icon sprite based on currency type
        /// </summary>
        private void UpdateIcon()
        {
            if (iconImage == null) return;

            // Load icon from Resources (you'll need to create these sprites)
            string iconPath = $"Sprites/Currency/{currencyType}Icon";
            Sprite icon = Resources.Load<Sprite>(iconPath);

            if (icon != null)
            {
                iconImage.sprite = icon;
            }
            else
            {
                Debug.LogWarning($"[CurrencyDisplay] Icon not found at path: {iconPath}");
            }
        }

        /// <summary>
        /// Format large numbers compactly (1000 -> 1K, 1000000 -> 1M)
        /// </summary>
        private string FormatCompact(int value)
        {
            if (value >= 1000000)
                return (value / 1000000f).ToString("0.#") + "M";
            else if (value >= 1000)
                return (value / 1000f).ToString("0.#") + "K";
            else
                return value.ToString();
        }

        /// <summary>
        /// Currency changed event handler
        /// </summary>
        private void OnCurrencyChanged(CurrencyType type, int oldValue, int newValue)
        {
            if (type == currencyType)
            {
                UpdateDisplay(true);
            }
        }

        /// <summary>
        /// Add button clicked handler (for premium currency purchases)
        /// </summary>
        private void OnAddButtonClicked()
        {
            Debug.Log($"[CurrencyDisplay] Add button clicked for {currencyType}");
            // TODO: Open shop/purchase screen
            // Example: ModalContainer.Instance.Push("ShopModal", new ShopArgs { selectedCurrency = currencyType });
        }

#if UNITY_EDITOR
        // Editor-only: Setup default references
        private void Reset()
        {
            // Try to find child components
            iconImage = transform.Find("Icon")?.GetComponent<Image>();
            valueText = transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
            addButton = transform.Find("AddButton")?.GetComponent<Button>();
        }

        // Editor-only: Validate references
        private void OnValidate()
        {
            if (iconImage == null)
                Debug.LogWarning($"[CurrencyDisplay] Icon Image reference is missing on {gameObject.name}", this);
            if (valueText == null)
                Debug.LogWarning($"[CurrencyDisplay] Value Text reference is missing on {gameObject.name}", this);
        }
#endif
    }
}
