using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedUI.Demo.Models
{
    /// <summary>
    /// Player data model - stores all player-related information
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        [Header("Player Info")]
        public string playerName = "Player";
        public int playerLevel = 1;
        public int experience = 0;
        public int experienceToNextLevel = 100;
        public string avatarId = "avatar_default";

        [Header("Currencies")]
        public int energy = 100;
        public int maxEnergy = 100;
        public int gems = 500;
        public int gold = 10000;
        public int keys = 5;
        public int stars = 0;

        [Header("Progress")]
        public int currentChapter = 1;
        public int currentLevel = 1;
        public int totalStarsEarned = 0;
        public List<string> unlockedLevels = new List<string> { "1-1", "1-2", "1-3" };
        public List<string> completedLevels = new List<string>();

        [Header("Inventory")]
        public List<string> ownedItems = new List<string>();
        public Dictionary<string, int> itemCounts = new Dictionary<string, int>();

        [Header("Settings")]
        public bool musicEnabled = true;
        public bool sfxEnabled = true;
        public float masterVolume = 1.0f;
        public bool notificationsEnabled = true;

        /// <summary>
        /// Get currency value by type
        /// </summary>
        public int GetCurrency(CurrencyType type)
        {
            switch (type)
            {
                case CurrencyType.Energy:
                    return energy;
                case CurrencyType.Gems:
                    return gems;
                case CurrencyType.Gold:
                    return gold;
                case CurrencyType.Keys:
                    return keys;
                case CurrencyType.Stars:
                    return stars;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Set currency value by type
        /// </summary>
        public void SetCurrency(CurrencyType type, int value)
        {
            switch (type)
            {
                case CurrencyType.Energy:
                    energy = Mathf.Clamp(value, 0, maxEnergy);
                    break;
                case CurrencyType.Gems:
                    gems = Mathf.Max(0, value);
                    break;
                case CurrencyType.Gold:
                    gold = Mathf.Max(0, value);
                    break;
                case CurrencyType.Keys:
                    keys = Mathf.Max(0, value);
                    break;
                case CurrencyType.Stars:
                    stars = Mathf.Max(0, value);
                    break;
            }
        }

        /// <summary>
        /// Add currency
        /// </summary>
        public bool AddCurrency(CurrencyType type, int amount)
        {
            if (amount < 0) return false;

            int current = GetCurrency(type);
            SetCurrency(type, current + amount);
            return true;
        }

        /// <summary>
        /// Spend currency (returns true if successful)
        /// </summary>
        public bool SpendCurrency(CurrencyType type, int amount)
        {
            if (amount < 0) return false;

            int current = GetCurrency(type);
            if (current < amount) return false;

            SetCurrency(type, current - amount);
            return true;
        }

        /// <summary>
        /// Check if player can afford a cost
        /// </summary>
        public bool CanAfford(CurrencyType type, int amount)
        {
            return GetCurrency(type) >= amount;
        }

        /// <summary>
        /// Check if level is unlocked
        /// </summary>
        public bool IsLevelUnlocked(string levelId)
        {
            return unlockedLevels.Contains(levelId);
        }

        /// <summary>
        /// Check if level is completed
        /// </summary>
        public bool IsLevelCompleted(string levelId)
        {
            return completedLevels.Contains(levelId);
        }

        /// <summary>
        /// Unlock a level
        /// </summary>
        public void UnlockLevel(string levelId)
        {
            if (!unlockedLevels.Contains(levelId))
            {
                unlockedLevels.Add(levelId);
            }
        }

        /// <summary>
        /// Complete a level
        /// </summary>
        public void CompleteLevel(string levelId, int starsEarned)
        {
            if (!completedLevels.Contains(levelId))
            {
                completedLevels.Add(levelId);
            }

            totalStarsEarned += starsEarned;
        }
    }
}
