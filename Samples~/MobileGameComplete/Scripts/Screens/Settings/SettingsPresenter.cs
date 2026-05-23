using System;
using UnityEngine;

namespace EnhancedUI.Demo.Screens.Settings
{
    /// <summary>
    /// Settings Presenter - Business logic for settings modal
    /// Handles settings changes, saving/loading, and account actions
    /// </summary>
    public class SettingsPresenter
    {
        private readonly SettingsModel _model;
        private readonly SettingsView _view;

        public SettingsPresenter(SettingsView view)
        {
            _view = view;
            _model = new SettingsModel();

            // Load account info
            _model.LoadAccountInfo();

            // Subscribe to view events
            SubscribeToViewEvents();

            // Initial setup
            Initialize();
        }

        /// <summary>
        /// Initialize presenter
        /// </summary>
        private void Initialize()
        {
            Debug.Log("[SettingsPresenter] Initialize");

            // Update view with current settings
            _view.UpdateSettingsUI(_model);
        }

        /// <summary>
        /// Cleanup presenter
        /// </summary>
        public void Cleanup()
        {
            Debug.Log("[SettingsPresenter] Cleanup");

            // Save settings before cleanup
            _model.SaveSettings();

            // Unsubscribe from view events
            UnsubscribeFromViewEvents();
        }

        #region Event Subscription

        private void SubscribeToViewEvents()
        {
            _view.OnTabChanged += HandleTabChanged;
            _view.OnMusicToggled += HandleMusicToggled;
            _view.OnSoundEffectsToggled += HandleSoundEffectsToggled;
            _view.OnMusicVolumeChanged += HandleMusicVolumeChanged;
            _view.OnSoundEffectsVolumeChanged += HandleSoundEffectsVolumeChanged;
            _view.OnPushNotificationsToggled += HandlePushNotificationsToggled;
            _view.OnEnergyNotificationsToggled += HandleEnergyNotificationsToggled;
            _view.OnEventNotificationsToggled += HandleEventNotificationsToggled;
            _view.OnFriendNotificationsToggled += HandleFriendNotificationsToggled;
            _view.OnVibrationToggled += HandleVibrationToggled;
            _view.OnLowPerformanceModeToggled += HandleLowPerformanceModeToggled;
            _view.OnLanguageChanged += HandleLanguageChanged;
            _view.OnLinkAccountPressed += HandleLinkAccountPressed;
            _view.OnLogoutPressed += HandleLogoutPressed;
            _view.OnResetPressed += HandleResetPressed;
            _view.OnClosePressed += HandleClosePressed;
        }

        private void UnsubscribeFromViewEvents()
        {
            _view.OnTabChanged -= HandleTabChanged;
            _view.OnMusicToggled -= HandleMusicToggled;
            _view.OnSoundEffectsToggled -= HandleSoundEffectsToggled;
            _view.OnMusicVolumeChanged -= HandleMusicVolumeChanged;
            _view.OnSoundEffectsVolumeChanged -= HandleSoundEffectsVolumeChanged;
            _view.OnPushNotificationsToggled -= HandlePushNotificationsToggled;
            _view.OnEnergyNotificationsToggled -= HandleEnergyNotificationsToggled;
            _view.OnEventNotificationsToggled -= HandleEventNotificationsToggled;
            _view.OnFriendNotificationsToggled -= HandleFriendNotificationsToggled;
            _view.OnVibrationToggled -= HandleVibrationToggled;
            _view.OnLowPerformanceModeToggled -= HandleLowPerformanceModeToggled;
            _view.OnLanguageChanged -= HandleLanguageChanged;
            _view.OnLinkAccountPressed -= HandleLinkAccountPressed;
            _view.OnLogoutPressed -= HandleLogoutPressed;
            _view.OnResetPressed -= HandleResetPressed;
            _view.OnClosePressed -= HandleClosePressed;
        }

        #endregion

        #region Event Handlers

        private void HandleTabChanged(SettingsTab newTab)
        {
            Debug.Log($"[SettingsPresenter] Tab changed to: {newTab}");
            _view.ShowTab(newTab);
        }

        private void HandleMusicToggled(bool isEnabled)
        {
            Debug.Log($"[SettingsPresenter] Music toggled: {isEnabled}");
            _model.isMusicEnabled = isEnabled;
            _model.SaveSettings();

            // In a real implementation, you would control audio system here
            ApplyAudioSettings();
        }

        private void HandleSoundEffectsToggled(bool isEnabled)
        {
            Debug.Log($"[SettingsPresenter] Sound effects toggled: {isEnabled}");
            _model.isSoundEffectsEnabled = isEnabled;
            _model.SaveSettings();

            // In a real implementation, you would control audio system here
            ApplyAudioSettings();
        }

        private void HandleMusicVolumeChanged(float volume)
        {
            _model.musicVolume = volume;
            _view.UpdateVolumeTexts(_model.musicVolume, _model.soundEffectsVolume);
            _model.SaveSettings();

            // In a real implementation, you would control audio system here
            ApplyAudioSettings();
        }

        private void HandleSoundEffectsVolumeChanged(float volume)
        {
            _model.soundEffectsVolume = volume;
            _view.UpdateVolumeTexts(_model.musicVolume, _model.soundEffectsVolume);
            _model.SaveSettings();

            // In a real implementation, you would control audio system here
            ApplyAudioSettings();
        }

        private void HandlePushNotificationsToggled(bool isEnabled)
        {
            Debug.Log($"[SettingsPresenter] Push notifications toggled: {isEnabled}");
            _model.pushNotificationsEnabled = isEnabled;
            _model.SaveSettings();
        }

        private void HandleEnergyNotificationsToggled(bool isEnabled)
        {
            Debug.Log($"[SettingsPresenter] Energy notifications toggled: {isEnabled}");
            _model.energyNotificationsEnabled = isEnabled;
            _model.SaveSettings();
        }

        private void HandleEventNotificationsToggled(bool isEnabled)
        {
            Debug.Log($"[SettingsPresenter] Event notifications toggled: {isEnabled}");
            _model.eventNotificationsEnabled = isEnabled;
            _model.SaveSettings();
        }

        private void HandleFriendNotificationsToggled(bool isEnabled)
        {
            Debug.Log($"[SettingsPresenter] Friend notifications toggled: {isEnabled}");
            _model.friendNotificationsEnabled = isEnabled;
            _model.SaveSettings();
        }

        private void HandleVibrationToggled(bool isEnabled)
        {
            Debug.Log($"[SettingsPresenter] Vibration toggled: {isEnabled}");
            _model.vibrationEnabled = isEnabled;
            _model.SaveSettings();
        }

        private void HandleLowPerformanceModeToggled(bool isEnabled)
        {
            Debug.Log($"[SettingsPresenter] Low performance mode toggled: {isEnabled}");
            _model.lowPerformanceModeEnabled = isEnabled;
            _model.SaveSettings();

            // In a real implementation, you would adjust graphics quality here
            ApplyPerformanceSettings();
        }

        private void HandleLanguageChanged(string language)
        {
            Debug.Log($"[SettingsPresenter] Language changed to: {language}");
            _model.language = language;
            _model.SaveSettings();

            // In a real implementation, you would switch localization here
            _view.ShowConfirmation($"Language changed to {language}. Restart required.");
        }

        private void HandleLinkAccountPressed()
        {
            Debug.Log("[SettingsPresenter] Link account pressed");

            // In a real implementation, you would show account linking UI
            _view.ShowConfirmation("Account linking coming soon!");
        }

        private void HandleLogoutPressed()
        {
            Debug.Log("[SettingsPresenter] Logout pressed");

            // In a real implementation, you would show confirmation dialog and logout
            _view.ShowConfirmation("Logout functionality coming soon!");
        }

        private void HandleResetPressed()
        {
            Debug.Log("[SettingsPresenter] Reset settings pressed");

            // Reset to defaults
            _model.ResetToDefaults();

            // Update view
            _view.UpdateSettingsUI(_model);

            // Apply settings
            ApplyAudioSettings();
            ApplyPerformanceSettings();

            _view.ShowConfirmation("Settings reset to defaults");
        }

        private void HandleClosePressed()
        {
            Debug.Log("[SettingsPresenter] Close pressed");

            // Save settings
            _model.SaveSettings();

            NavigationManager.Instance?.CloseModal();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Apply audio settings to audio system
        /// </summary>
        private void ApplyAudioSettings()
        {
            // In a real implementation, you would control Unity's AudioListener or audio mixer
            // For example:
            // AudioListener.volume = _model.isMusicEnabled ? _model.musicVolume : 0f;

            Debug.Log($"[SettingsPresenter] Audio settings applied - Music: {_model.isMusicEnabled} ({_model.musicVolume}), SFX: {_model.isSoundEffectsEnabled} ({_model.soundEffectsVolume})");
        }

        /// <summary>
        /// Apply performance settings
        /// </summary>
        private void ApplyPerformanceSettings()
        {
            // In a real implementation, you would adjust Unity's Quality Settings
            // For example:
            // QualitySettings.SetQualityLevel(_model.lowPerformanceModeEnabled ? 0 : 2);

            Debug.Log($"[SettingsPresenter] Performance settings applied - Low Performance Mode: {_model.lowPerformanceModeEnabled}");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh settings (reload from PlayerPrefs)
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[SettingsPresenter] Refresh");

            _model.LoadSettings();
            _model.LoadAccountInfo();
            _view.UpdateSettingsUI(_model);
        }

        /// <summary>
        /// Get current model state (for testing)
        /// </summary>
        public SettingsModel GetModel()
        {
            return _model;
        }

        #endregion
    }
}
