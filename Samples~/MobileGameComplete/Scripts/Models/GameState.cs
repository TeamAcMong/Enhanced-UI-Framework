using System;
using UnityEngine;

namespace EnhancedUI.Demo.Models
{
    /// <summary>
    /// Centralized game state manager - Singleton pattern
    /// Manages player data and provides events for state changes
    /// </summary>
    public class GameState : MonoBehaviour
    {
        private static GameState _instance;
        public static GameState Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[Game State]");
                    _instance = go.AddComponent<GameState>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        [SerializeField] private PlayerData _playerData = new PlayerData();

        public PlayerData PlayerData => _playerData;

        // Events
        public event Action<CurrencyType, int, int> OnCurrencyChanged; // type, oldValue, newValue
        public event Action<string> OnLevelUnlocked; // levelId
        public event Action<string, int> OnLevelCompleted; // levelId, starsEarned
        public event Action<int, int> OnPlayerLevelUp; // oldLevel, newLevel

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeDefaultData();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initialize default player data for demo
        /// </summary>
        private void InitializeDefaultData()
        {
            _playerData.playerName = "Demo Player";
            _playerData.energy = 100;
            _playerData.maxEnergy = 100;
            _playerData.gems = 500;
            _playerData.gold = 10000;
            _playerData.keys = 5;
            _playerData.playerLevel = 1;

            // Unlock first few levels
            _playerData.unlockedLevels.Clear();
            for (int i = 1; i <= 5; i++)
            {
                _playerData.unlockedLevels.Add($"1-{i}");
            }

            Debug.Log("[Game State] Initialized with default demo data");
        }

        /// <summary>
        /// Get currency value
        /// </summary>
        public int GetCurrency(CurrencyType type)
        {
            return _playerData.GetCurrency(type);
        }

        /// <summary>
        /// Add currency with event notification
        /// </summary>
        public void AddCurrency(CurrencyType type, int amount)
        {
            int oldValue = _playerData.GetCurrency(type);
            _playerData.AddCurrency(type, amount);
            int newValue = _playerData.GetCurrency(type);

            if (oldValue != newValue)
            {
                OnCurrencyChanged?.Invoke(type, oldValue, newValue);
            }
        }

        /// <summary>
        /// Spend currency with event notification
        /// </summary>
        public bool SpendCurrency(CurrencyType type, int amount)
        {
            int oldValue = _playerData.GetCurrency(type);
            bool success = _playerData.SpendCurrency(type, amount);

            if (success)
            {
                int newValue = _playerData.GetCurrency(type);
                OnCurrencyChanged?.Invoke(type, oldValue, newValue);
            }

            return success;
        }

        /// <summary>
        /// Check if player can afford a cost
        /// </summary>
        public bool CanAfford(CurrencyType type, int amount)
        {
            return _playerData.CanAfford(type, amount);
        }

        /// <summary>
        /// Unlock level with event notification
        /// </summary>
        public void UnlockLevel(string levelId)
        {
            if (!_playerData.IsLevelUnlocked(levelId))
            {
                _playerData.UnlockLevel(levelId);
                OnLevelUnlocked?.Invoke(levelId);
            }
        }

        /// <summary>
        /// Complete level with event notification
        /// </summary>
        public void CompleteLevel(string levelId, int starsEarned)
        {
            _playerData.CompleteLevel(levelId, starsEarned);
            OnLevelCompleted?.Invoke(levelId, starsEarned);
        }

        /// <summary>
        /// Add experience and check for level up
        /// </summary>
        public void AddExperience(int amount)
        {
            _playerData.experience += amount;

            while (_playerData.experience >= _playerData.experienceToNextLevel)
            {
                _playerData.experience -= _playerData.experienceToNextLevel;
                int oldLevel = _playerData.playerLevel;
                _playerData.playerLevel++;
                _playerData.experienceToNextLevel = CalculateExpForNextLevel(_playerData.playerLevel);

                OnPlayerLevelUp?.Invoke(oldLevel, _playerData.playerLevel);
            }
        }

        /// <summary>
        /// Calculate experience required for next level
        /// </summary>
        private int CalculateExpForNextLevel(int currentLevel)
        {
            return 100 + (currentLevel - 1) * 50; // Simple formula: 100, 150, 200, etc.
        }

        /// <summary>
        /// Save player data (stub for demo - could integrate with PlayerPrefs or save system)
        /// </summary>
        public void SavePlayerData()
        {
            // TODO: Implement save system
            Debug.Log("[Game State] Player data saved (stub)");
        }

        /// <summary>
        /// Load player data (stub for demo)
        /// </summary>
        public void LoadPlayerData()
        {
            // TODO: Implement load system
            Debug.Log("[Game State] Player data loaded (stub)");
        }

        /// <summary>
        /// Reset player data to defaults (useful for testing)
        /// </summary>
        public void ResetToDefaults()
        {
            _playerData = new PlayerData();
            InitializeDefaultData();
            Debug.Log("[Game State] Player data reset to defaults");
        }
    }
}
