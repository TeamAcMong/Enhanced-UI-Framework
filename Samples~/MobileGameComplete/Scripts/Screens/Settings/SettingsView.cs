using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EnhancedUI.Demo.Screens.Settings
{
    /// <summary>
    /// Settings View - Handles UI display and user input for settings modal
    /// Displays audio, notification, gameplay, and account settings
    /// </summary>
    public class SettingsView : MonoBehaviour
    {
        [Header("Tab Navigation")]
        [SerializeField] private Button audioTabButton;
        [SerializeField] private Button notificationsTabButton;
        [SerializeField] private Button gameplayTabButton;
        [SerializeField] private Button accountTabButton;

        [Header("Tab Panels")]
        [SerializeField] private GameObject audioPanel;
        [SerializeField] private GameObject notificationsPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject accountPanel;

        [Header("Audio Settings")]
        [SerializeField] private Toggle musicToggle;
        [SerializeField] private Toggle soundEffectsToggle;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider soundEffectsVolumeSlider;
        [SerializeField] private TextMeshProUGUI musicVolumeText;
        [SerializeField] private TextMeshProUGUI soundEffectsVolumeText;

        [Header("Notification Settings")]
        [SerializeField] private Toggle pushNotificationsToggle;
        [SerializeField] private Toggle energyNotificationsToggle;
        [SerializeField] private Toggle eventNotificationsToggle;
        [SerializeField] private Toggle friendNotificationsToggle;

        [Header("Gameplay Settings")]
        [SerializeField] private Toggle vibrationToggle;
        [SerializeField] private Toggle lowPerformanceModeToggle;
        [SerializeField] private TMP_Dropdown languageDropdown;

        [Header("Account Info")]
        [SerializeField] private TextMeshProUGUI playerIdText;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI accountLevelText;
        [SerializeField] private Button linkAccountButton;
        [SerializeField] private Button logoutButton;

        [Header("Action Buttons")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button closeButton;

        // Events
        public event Action<SettingsTab> OnTabChanged;
        public event Action<bool> OnMusicToggled;
        public event Action<bool> OnSoundEffectsToggled;
        public event Action<float> OnMusicVolumeChanged;
        public event Action<float> OnSoundEffectsVolumeChanged;
        public event Action<bool> OnPushNotificationsToggled;
        public event Action<bool> OnEnergyNotificationsToggled;
        public event Action<bool> OnEventNotificationsToggled;
        public event Action<bool> OnFriendNotificationsToggled;
        public event Action<bool> OnVibrationToggled;
        public event Action<bool> OnLowPerformanceModeToggled;
        public event Action<string> OnLanguageChanged;
        public event Action OnLinkAccountPressed;
        public event Action OnLogoutPressed;
        public event Action OnResetPressed;
        public event Action OnClosePressed;

        private SettingsPresenter _presenter;
        private SettingsTab _currentTab = SettingsTab.Audio;

        /// <summary>
        /// Set the presenter for this view
        /// </summary>
        public void SetPresenter(SettingsPresenter presenter)
        {
            _presenter = presenter;
        }

        private void Awake()
        {
            SetupListeners();
        }

        private void SetupListeners()
        {
            // Tab buttons
            if (audioTabButton != null)
                audioTabButton.onClick.AddListener(() => OnTabChanged?.Invoke(SettingsTab.Audio));

            if (notificationsTabButton != null)
                notificationsTabButton.onClick.AddListener(() => OnTabChanged?.Invoke(SettingsTab.Notifications));

            if (gameplayTabButton != null)
                gameplayTabButton.onClick.AddListener(() => OnTabChanged?.Invoke(SettingsTab.Gameplay));

            if (accountTabButton != null)
                accountTabButton.onClick.AddListener(() => OnTabChanged?.Invoke(SettingsTab.Account));

            // Audio settings
            if (musicToggle != null)
                musicToggle.onValueChanged.AddListener(value => OnMusicToggled?.Invoke(value));

            if (soundEffectsToggle != null)
                soundEffectsToggle.onValueChanged.AddListener(value => OnSoundEffectsToggled?.Invoke(value));

            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(value => OnMusicVolumeChanged?.Invoke(value));

            if (soundEffectsVolumeSlider != null)
                soundEffectsVolumeSlider.onValueChanged.AddListener(value => OnSoundEffectsVolumeChanged?.Invoke(value));

            // Notification settings
            if (pushNotificationsToggle != null)
                pushNotificationsToggle.onValueChanged.AddListener(value => OnPushNotificationsToggled?.Invoke(value));

            if (energyNotificationsToggle != null)
                energyNotificationsToggle.onValueChanged.AddListener(value => OnEnergyNotificationsToggled?.Invoke(value));

            if (eventNotificationsToggle != null)
                eventNotificationsToggle.onValueChanged.AddListener(value => OnEventNotificationsToggled?.Invoke(value));

            if (friendNotificationsToggle != null)
                friendNotificationsToggle.onValueChanged.AddListener(value => OnFriendNotificationsToggled?.Invoke(value));

            // Gameplay settings
            if (vibrationToggle != null)
                vibrationToggle.onValueChanged.AddListener(value => OnVibrationToggled?.Invoke(value));

            if (lowPerformanceModeToggle != null)
                lowPerformanceModeToggle.onValueChanged.AddListener(value => OnLowPerformanceModeToggled?.Invoke(value));

            if (languageDropdown != null)
            {
                languageDropdown.ClearOptions();
                languageDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "English",
                    "日本語",
                    "한국어",
                    "中文",
                    "Español",
                    "Français"
                });
                languageDropdown.onValueChanged.AddListener(index =>
                {
                    string language = languageDropdown.options[index].text;
                    OnLanguageChanged?.Invoke(language);
                });
            }

            // Account buttons
            if (linkAccountButton != null)
                linkAccountButton.onClick.AddListener(() => OnLinkAccountPressed?.Invoke());

            if (logoutButton != null)
                logoutButton.onClick.AddListener(() => OnLogoutPressed?.Invoke());

            // Action buttons
            if (resetButton != null)
                resetButton.onClick.AddListener(() => OnResetPressed?.Invoke());

            if (closeButton != null)
                closeButton.onClick.AddListener(() => OnClosePressed?.Invoke());
        }

        private void OnDestroy()
        {
            // Clean up all listeners
            if (audioTabButton != null) audioTabButton.onClick.RemoveAllListeners();
            if (notificationsTabButton != null) notificationsTabButton.onClick.RemoveAllListeners();
            if (gameplayTabButton != null) gameplayTabButton.onClick.RemoveAllListeners();
            if (accountTabButton != null) accountTabButton.onClick.RemoveAllListeners();

            if (musicToggle != null) musicToggle.onValueChanged.RemoveAllListeners();
            if (soundEffectsToggle != null) soundEffectsToggle.onValueChanged.RemoveAllListeners();
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveAllListeners();
            if (soundEffectsVolumeSlider != null) soundEffectsVolumeSlider.onValueChanged.RemoveAllListeners();

            if (pushNotificationsToggle != null) pushNotificationsToggle.onValueChanged.RemoveAllListeners();
            if (energyNotificationsToggle != null) energyNotificationsToggle.onValueChanged.RemoveAllListeners();
            if (eventNotificationsToggle != null) eventNotificationsToggle.onValueChanged.RemoveAllListeners();
            if (friendNotificationsToggle != null) friendNotificationsToggle.onValueChanged.RemoveAllListeners();

            if (vibrationToggle != null) vibrationToggle.onValueChanged.RemoveAllListeners();
            if (lowPerformanceModeToggle != null) lowPerformanceModeToggle.onValueChanged.RemoveAllListeners();
            if (languageDropdown != null) languageDropdown.onValueChanged.RemoveAllListeners();

            if (linkAccountButton != null) linkAccountButton.onClick.RemoveAllListeners();
            if (logoutButton != null) logoutButton.onClick.RemoveAllListeners();
            if (resetButton != null) resetButton.onClick.RemoveAllListeners();
            if (closeButton != null) closeButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Show the view
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            ShowTab(_currentTab);
            Debug.Log("[SettingsView] Show");
        }

        /// <summary>
        /// Hide the view
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            Debug.Log("[SettingsView] Hide");
        }

        /// <summary>
        /// Show specific settings tab
        /// </summary>
        public void ShowTab(SettingsTab tab)
        {
            _currentTab = tab;

            if (audioPanel != null) audioPanel.SetActive(tab == SettingsTab.Audio);
            if (notificationsPanel != null) notificationsPanel.SetActive(tab == SettingsTab.Notifications);
            if (gameplayPanel != null) gameplayPanel.SetActive(tab == SettingsTab.Gameplay);
            if (accountPanel != null) accountPanel.SetActive(tab == SettingsTab.Account);

            Debug.Log($"[SettingsView] Showing tab: {tab}");
        }

        /// <summary>
        /// Update UI with settings data (without triggering events)
        /// </summary>
        public void UpdateSettingsUI(SettingsModel model)
        {
            // Temporarily remove listeners to prevent triggering events
            RemoveAudioListeners();

            // Update audio settings
            if (musicToggle != null) musicToggle.isOn = model.isMusicEnabled;
            if (soundEffectsToggle != null) soundEffectsToggle.isOn = model.isSoundEffectsEnabled;
            if (musicVolumeSlider != null) musicVolumeSlider.value = model.musicVolume;
            if (soundEffectsVolumeSlider != null) soundEffectsVolumeSlider.value = model.soundEffectsVolume;

            UpdateVolumeTexts(model.musicVolume, model.soundEffectsVolume);

            // Re-add listeners
            AddAudioListeners();

            // Temporarily remove notification listeners
            RemoveNotificationListeners();

            // Update notification settings
            if (pushNotificationsToggle != null) pushNotificationsToggle.isOn = model.pushNotificationsEnabled;
            if (energyNotificationsToggle != null) energyNotificationsToggle.isOn = model.energyNotificationsEnabled;
            if (eventNotificationsToggle != null) eventNotificationsToggle.isOn = model.eventNotificationsEnabled;
            if (friendNotificationsToggle != null) friendNotificationsToggle.isOn = model.friendNotificationsEnabled;

            // Re-add listeners
            AddNotificationListeners();

            // Temporarily remove gameplay listeners
            RemoveGameplayListeners();

            // Update gameplay settings
            if (vibrationToggle != null) vibrationToggle.isOn = model.vibrationEnabled;
            if (lowPerformanceModeToggle != null) lowPerformanceModeToggle.isOn = model.lowPerformanceModeEnabled;
            if (languageDropdown != null)
            {
                int languageIndex = languageDropdown.options.FindIndex(option => option.text == model.language);
                if (languageIndex >= 0) languageDropdown.value = languageIndex;
            }

            // Re-add listeners
            AddGameplayListeners();

            // Update account info
            if (playerIdText != null) playerIdText.text = $"ID: {model.playerId}";
            if (playerNameText != null) playerNameText.text = model.playerName;
            if (accountLevelText != null) accountLevelText.text = $"Level {model.accountLevel}";

            Debug.Log("[SettingsView] Settings UI updated");
        }

        /// <summary>
        /// Update volume text displays
        /// </summary>
        public void UpdateVolumeTexts(float musicVolume, float sfxVolume)
        {
            if (musicVolumeText != null)
                musicVolumeText.text = $"{(int)(musicVolume * 100)}%";

            if (soundEffectsVolumeText != null)
                soundEffectsVolumeText.text = $"{(int)(sfxVolume * 100)}%";
        }

        /// <summary>
        /// Show confirmation feedback
        /// </summary>
        public void ShowConfirmation(string message)
        {
            Debug.Log($"[SettingsView] Confirmation: {message}");
            // In a real implementation, show a toast or popup
        }

        #region Listener Management

        private void RemoveAudioListeners()
        {
            if (musicToggle != null) musicToggle.onValueChanged.RemoveAllListeners();
            if (soundEffectsToggle != null) soundEffectsToggle.onValueChanged.RemoveAllListeners();
            if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveAllListeners();
            if (soundEffectsVolumeSlider != null) soundEffectsVolumeSlider.onValueChanged.RemoveAllListeners();
        }

        private void AddAudioListeners()
        {
            if (musicToggle != null)
                musicToggle.onValueChanged.AddListener(value => OnMusicToggled?.Invoke(value));
            if (soundEffectsToggle != null)
                soundEffectsToggle.onValueChanged.AddListener(value => OnSoundEffectsToggled?.Invoke(value));
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(value => OnMusicVolumeChanged?.Invoke(value));
            if (soundEffectsVolumeSlider != null)
                soundEffectsVolumeSlider.onValueChanged.AddListener(value => OnSoundEffectsVolumeChanged?.Invoke(value));
        }

        private void RemoveNotificationListeners()
        {
            if (pushNotificationsToggle != null) pushNotificationsToggle.onValueChanged.RemoveAllListeners();
            if (energyNotificationsToggle != null) energyNotificationsToggle.onValueChanged.RemoveAllListeners();
            if (eventNotificationsToggle != null) eventNotificationsToggle.onValueChanged.RemoveAllListeners();
            if (friendNotificationsToggle != null) friendNotificationsToggle.onValueChanged.RemoveAllListeners();
        }

        private void AddNotificationListeners()
        {
            if (pushNotificationsToggle != null)
                pushNotificationsToggle.onValueChanged.AddListener(value => OnPushNotificationsToggled?.Invoke(value));
            if (energyNotificationsToggle != null)
                energyNotificationsToggle.onValueChanged.AddListener(value => OnEnergyNotificationsToggled?.Invoke(value));
            if (eventNotificationsToggle != null)
                eventNotificationsToggle.onValueChanged.AddListener(value => OnEventNotificationsToggled?.Invoke(value));
            if (friendNotificationsToggle != null)
                friendNotificationsToggle.onValueChanged.AddListener(value => OnFriendNotificationsToggled?.Invoke(value));
        }

        private void RemoveGameplayListeners()
        {
            if (vibrationToggle != null) vibrationToggle.onValueChanged.RemoveAllListeners();
            if (lowPerformanceModeToggle != null) lowPerformanceModeToggle.onValueChanged.RemoveAllListeners();
            if (languageDropdown != null) languageDropdown.onValueChanged.RemoveAllListeners();
        }

        private void AddGameplayListeners()
        {
            if (vibrationToggle != null)
                vibrationToggle.onValueChanged.AddListener(value => OnVibrationToggled?.Invoke(value));
            if (lowPerformanceModeToggle != null)
                lowPerformanceModeToggle.onValueChanged.AddListener(value => OnLowPerformanceModeToggled?.Invoke(value));
            if (languageDropdown != null)
            {
                languageDropdown.onValueChanged.AddListener(index =>
                {
                    string language = languageDropdown.options[index].text;
                    OnLanguageChanged?.Invoke(language);
                });
            }
        }

        #endregion
    }

    public enum SettingsTab
    {
        Audio,
        Notifications,
        Gameplay,
        Account
    }
}
