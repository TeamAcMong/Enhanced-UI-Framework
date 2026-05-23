using System;
using UnityEngine;

namespace EnhancedUI.Demo.Screens.Settings
{
    [Serializable]
    public class SettingsModel
    {
        // Audio settings
        public bool isMusicEnabled = true;
        public bool isSoundEffectsEnabled = true;
        public float musicVolume = 0.8f;
        public float soundEffectsVolume = 1.0f;

        // Notification settings
        public bool pushNotificationsEnabled = true;
        public bool energyNotificationsEnabled = true;
        public bool eventNotificationsEnabled = true;
        public bool friendNotificationsEnabled = true;

        // Gameplay settings
        public bool vibrationEnabled = true;
        public bool lowPerformanceModeEnabled = false;
        public string language = "English";

        // Account info
        public string playerId = "PLAYER-12345";
        public string playerName = "Unknown";
        public int accountLevel = 1;

        public SettingsModel()
        {
            LoadSettings();
        }

        /// <summary>
        /// Load settings from PlayerPrefs
        /// </summary>
        public void LoadSettings()
        {
            isMusicEnabled = PlayerPrefs.GetInt("Settings_MusicEnabled", 1) == 1;
            isSoundEffectsEnabled = PlayerPrefs.GetInt("Settings_SFXEnabled", 1) == 1;
            musicVolume = PlayerPrefs.GetFloat("Settings_MusicVolume", 0.8f);
            soundEffectsVolume = PlayerPrefs.GetFloat("Settings_SFXVolume", 1.0f);

            pushNotificationsEnabled = PlayerPrefs.GetInt("Settings_PushNotifications", 1) == 1;
            energyNotificationsEnabled = PlayerPrefs.GetInt("Settings_EnergyNotifications", 1) == 1;
            eventNotificationsEnabled = PlayerPrefs.GetInt("Settings_EventNotifications", 1) == 1;
            friendNotificationsEnabled = PlayerPrefs.GetInt("Settings_FriendNotifications", 1) == 1;

            vibrationEnabled = PlayerPrefs.GetInt("Settings_Vibration", 1) == 1;
            lowPerformanceModeEnabled = PlayerPrefs.GetInt("Settings_LowPerformance", 0) == 1;
            language = PlayerPrefs.GetString("Settings_Language", "English");

            Debug.Log("[SettingsModel] Settings loaded from PlayerPrefs");
        }

        /// <summary>
        /// Save settings to PlayerPrefs
        /// </summary>
        public void SaveSettings()
        {
            PlayerPrefs.SetInt("Settings_MusicEnabled", isMusicEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Settings_SFXEnabled", isSoundEffectsEnabled ? 1 : 0);
            PlayerPrefs.SetFloat("Settings_MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("Settings_SFXVolume", soundEffectsVolume);

            PlayerPrefs.SetInt("Settings_PushNotifications", pushNotificationsEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Settings_EnergyNotifications", energyNotificationsEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Settings_EventNotifications", eventNotificationsEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Settings_FriendNotifications", friendNotificationsEnabled ? 1 : 0);

            PlayerPrefs.SetInt("Settings_Vibration", vibrationEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Settings_LowPerformance", lowPerformanceModeEnabled ? 1 : 0);
            PlayerPrefs.SetString("Settings_Language", language);

            PlayerPrefs.Save();

            Debug.Log("[SettingsModel] Settings saved to PlayerPrefs");
        }

        /// <summary>
        /// Reset all settings to defaults
        /// </summary>
        public void ResetToDefaults()
        {
            isMusicEnabled = true;
            isSoundEffectsEnabled = true;
            musicVolume = 0.8f;
            soundEffectsVolume = 1.0f;

            pushNotificationsEnabled = true;
            energyNotificationsEnabled = true;
            eventNotificationsEnabled = true;
            friendNotificationsEnabled = true;

            vibrationEnabled = true;
            lowPerformanceModeEnabled = false;
            language = "English";

            SaveSettings();

            Debug.Log("[SettingsModel] Settings reset to defaults");
        }

        /// <summary>
        /// Load account info from game state
        /// </summary>
        public void LoadAccountInfo()
        {
            var gameState = Models.GameState.Instance;
            if (gameState != null && gameState.PlayerData != null)
            {
                playerName = gameState.PlayerData.playerName;
                accountLevel = gameState.PlayerData.playerLevel;
            }
        }
    }
}
