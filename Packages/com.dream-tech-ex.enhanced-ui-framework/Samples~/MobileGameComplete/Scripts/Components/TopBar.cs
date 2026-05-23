using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Components
{
    /// <summary>
    /// Top bar component - displays player avatar, level, and currencies
    /// Common component used across most screens
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class TopBar : MonoBehaviour
    {
        [Header("Player Info References")]
        [SerializeField] private Image avatarImage;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerLevelText;
        [SerializeField] private Button avatarButton; // Opens profile/settings
        [SerializeField] private Image levelProgressFill; // Optional level progress bar

        [Header("Currency Displays")]
        [SerializeField] private CurrencyDisplay energyDisplay;
        [SerializeField] private CurrencyDisplay gemsDisplay;
        [SerializeField] private CurrencyDisplay goldDisplay;

        [Header("Additional Buttons")]
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button notificationButton;
        [SerializeField] private GameObject notificationBadge; // Red dot indicator

        [Header("Configuration")]
        [SerializeField] private bool showPlayerName = true;
        [SerializeField] private bool showPlayerLevel = true;
        [SerializeField] private bool showLevelProgress = false;

        private int _unreadNotifications = 0;

        private void Awake()
        {
            // Setup button listeners
            if (avatarButton != null)
                avatarButton.onClick.AddListener(OnAvatarClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (notificationButton != null)
                notificationButton.onClick.AddListener(OnNotificationClicked);

            // Initialize currency displays
            SetupCurrencyDisplays();
        }

        private void OnEnable()
        {
            // Subscribe to game state events
            if (GameState.Instance != null)
            {
                GameState.Instance.OnPlayerLevelUp += OnPlayerLevelUp;
            }

            // Update display
            UpdatePlayerInfo();
            UpdateNotificationBadge();
        }

        private void OnDisable()
        {
            // Unsubscribe from events
            if (GameState.Instance != null)
            {
                GameState.Instance.OnPlayerLevelUp -= OnPlayerLevelUp;
            }
        }

        /// <summary>
        /// Setup currency displays with correct types
        /// </summary>
        private void SetupCurrencyDisplays()
        {
            if (energyDisplay != null)
                energyDisplay.SetCurrencyType(CurrencyType.Energy);

            if (gemsDisplay != null)
                gemsDisplay.SetCurrencyType(CurrencyType.Gems);

            if (goldDisplay != null)
                goldDisplay.SetCurrencyType(CurrencyType.Gold);
        }

        /// <summary>
        /// Update player information display
        /// </summary>
        public void UpdatePlayerInfo()
        {
            if (GameState.Instance == null) return;

            PlayerData playerData = GameState.Instance.PlayerData;

            // Update player name
            if (playerNameText != null && showPlayerName)
            {
                playerNameText.text = playerData.playerName;
                playerNameText.gameObject.SetActive(true);
            }
            else if (playerNameText != null)
            {
                playerNameText.gameObject.SetActive(false);
            }

            // Update player level
            if (playerLevelText != null && showPlayerLevel)
            {
                playerLevelText.text = $"Lv.{playerData.playerLevel}";
                playerLevelText.gameObject.SetActive(true);
            }
            else if (playerLevelText != null)
            {
                playerLevelText.gameObject.SetActive(false);
            }

            // Update level progress bar
            if (levelProgressFill != null && showLevelProgress)
            {
                float progress = (float)playerData.experience / playerData.experienceToNextLevel;
                levelProgressFill.fillAmount = progress;
                levelProgressFill.gameObject.SetActive(true);
            }
            else if (levelProgressFill != null)
            {
                levelProgressFill.gameObject.SetActive(false);
            }

            // Update avatar
            UpdateAvatar(playerData.avatarId);

            // Update currency displays
            if (energyDisplay != null) energyDisplay.UpdateDisplay(false);
            if (gemsDisplay != null) gemsDisplay.UpdateDisplay(false);
            if (goldDisplay != null) goldDisplay.UpdateDisplay(false);
        }

        /// <summary>
        /// Update avatar image
        /// </summary>
        private void UpdateAvatar(string avatarId)
        {
            if (avatarImage == null) return;

            // Load avatar sprite from Resources
            string avatarPath = $"Sprites/Avatars/{avatarId}";
            Sprite avatarSprite = Resources.Load<Sprite>(avatarPath);

            if (avatarSprite != null)
            {
                avatarImage.sprite = avatarSprite;
            }
            else
            {
                // Use default avatar
                Debug.LogWarning($"[TopBar] Avatar not found at path: {avatarPath}, using default");
            }
        }

        /// <summary>
        /// Set notification count and show/hide badge
        /// </summary>
        public void SetNotificationCount(int count)
        {
            _unreadNotifications = count;
            UpdateNotificationBadge();
        }

        /// <summary>
        /// Update notification badge visibility
        /// </summary>
        private void UpdateNotificationBadge()
        {
            if (notificationBadge != null)
            {
                notificationBadge.SetActive(_unreadNotifications > 0);
            }
        }

        /// <summary>
        /// Show specific currency displays
        /// </summary>
        public void SetVisibleCurrencies(bool showEnergy, bool showGems, bool showGold)
        {
            if (energyDisplay != null)
                energyDisplay.gameObject.SetActive(showEnergy);

            if (gemsDisplay != null)
                gemsDisplay.gameObject.SetActive(showGems);

            if (goldDisplay != null)
                goldDisplay.gameObject.SetActive(showGold);
        }

        /// <summary>
        /// Player level up event handler
        /// </summary>
        private void OnPlayerLevelUp(int oldLevel, int newLevel)
        {
            UpdatePlayerInfo();

            // TODO: Show level up effect/animation
            Debug.Log($"[TopBar] Player leveled up: {oldLevel} -> {newLevel}");
        }

        /// <summary>
        /// Avatar button clicked - open profile or settings
        /// </summary>
        private void OnAvatarClicked()
        {
            Debug.Log("[TopBar] Avatar clicked");
            // TODO: Open profile screen
            // Example: ModalContainer.Instance.Push("ProfileModal");
        }

        /// <summary>
        /// Settings button clicked
        /// </summary>
        private void OnSettingsClicked()
        {
            Debug.Log("[TopBar] Settings clicked");
            // TODO: Open settings screen
            // Example: ModalContainer.Instance.Push("SettingsModal");
        }

        /// <summary>
        /// Notification button clicked
        /// </summary>
        private void OnNotificationClicked()
        {
            Debug.Log("[TopBar] Notifications clicked");
            // TODO: Open notifications screen
            // Example: ModalContainer.Instance.Push("NotificationsModal");

            // Clear notification badge (for demo)
            SetNotificationCount(0);
        }

#if UNITY_EDITOR
        // Editor-only: Setup default references
        private void Reset()
        {
            // Try to find child components
            avatarImage = transform.Find("Avatar/AvatarImage")?.GetComponent<Image>();
            playerNameText = transform.Find("PlayerInfo/NameText")?.GetComponent<TextMeshProUGUI>();
            playerLevelText = transform.Find("PlayerInfo/LevelText")?.GetComponent<TextMeshProUGUI>();
            avatarButton = transform.Find("Avatar")?.GetComponent<Button>();
            levelProgressFill = transform.Find("PlayerInfo/ProgressBar/Fill")?.GetComponent<Image>();

            // Find currency displays
            Transform currenciesParent = transform.Find("Currencies");
            if (currenciesParent != null)
            {
                energyDisplay = currenciesParent.Find("EnergyDisplay")?.GetComponent<CurrencyDisplay>();
                gemsDisplay = currenciesParent.Find("GemsDisplay")?.GetComponent<CurrencyDisplay>();
                goldDisplay = currenciesParent.Find("GoldDisplay")?.GetComponent<CurrencyDisplay>();
            }

            settingsButton = transform.Find("SettingsButton")?.GetComponent<Button>();
            notificationButton = transform.Find("NotificationButton")?.GetComponent<Button>();
            notificationBadge = transform.Find("NotificationButton/Badge")?.gameObject;
        }

        // Editor-only: Validate references
        private void OnValidate()
        {
            if (avatarImage == null)
                Debug.LogWarning($"[TopBar] Avatar Image reference is missing on {gameObject.name}", this);
        }
#endif
    }
}
