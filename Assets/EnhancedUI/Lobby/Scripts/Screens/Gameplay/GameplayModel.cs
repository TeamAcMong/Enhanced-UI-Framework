using System;
using System.Collections.Generic;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Gameplay
{
    /// <summary>
    /// Model for Gameplay Screen
    /// </summary>
    [Serializable]
    public class GameplayModel
    {
        // Level info
        public string levelId;
        public int waveNumber;
        public int totalWaves;
        public float levelTimer;

        // Player data reference
        public PlayerData playerData;

        // Resources
        public int wood;
        public int stone;
        public int iron;

        // Upgrades
        public List<UpgradeSlotData> upgradeSlots;

        // Game state
        public GameplayState currentState;
        public bool isPaused;

        // Victory conditions
        public int enemiesKilled;
        public int enemiesTotal;
        public int targetScore;
        public int currentScore;

        public GameplayModel()
        {
            levelId = "";
            waveNumber = 1;
            totalWaves = 5;
            levelTimer = 0f;

            wood = 100;
            stone = 50;
            iron = 25;

            upgradeSlots = new List<UpgradeSlotData>
            {
                new UpgradeSlotData { slotId = 1, upgradeName = "Tower", level = 1, maxLevel = 5, isUnlocked = true },
                new UpgradeSlotData { slotId = 2, upgradeName = "Wall", level = 1, maxLevel = 5, isUnlocked = true },
                new UpgradeSlotData { slotId = 3, upgradeName = "Trap", level = 0, maxLevel = 5, isUnlocked = false }
            };

            currentState = GameplayState.Playing;
            isPaused = false;

            enemiesKilled = 0;
            enemiesTotal = 20;
            targetScore = 1000;
            currentScore = 0;
        }

        /// <summary>
        /// Check if player can afford upgrade
        /// </summary>
        public bool CanAffordUpgrade(int slotId)
        {
            var slot = GetUpgradeSlot(slotId);
            if (slot == null || slot.level >= slot.maxLevel)
                return false;

            // Simple cost calculation
            int woodCost = 50 * (slot.level + 1);
            int stoneCost = 25 * (slot.level + 1);

            return wood >= woodCost && stone >= stoneCost;
        }

        /// <summary>
        /// Get upgrade slot by ID
        /// </summary>
        public UpgradeSlotData GetUpgradeSlot(int slotId)
        {
            return upgradeSlots.Find(s => s.slotId == slotId);
        }

        /// <summary>
        /// Get upgrade cost
        /// </summary>
        public UpgradeCost GetUpgradeCost(int slotId)
        {
            var slot = GetUpgradeSlot(slotId);
            if (slot == null)
                return new UpgradeCost();

            return new UpgradeCost
            {
                wood = 50 * (slot.level + 1),
                stone = 25 * (slot.level + 1),
                iron = 10 * (slot.level + 1)
            };
        }

        /// <summary>
        /// Check if level is complete
        /// </summary>
        public bool IsLevelComplete()
        {
            return currentState == GameplayState.Victory || currentState == GameplayState.Defeat;
        }

        /// <summary>
        /// Get completion percentage
        /// </summary>
        public float GetCompletionPercentage()
        {
            if (enemiesTotal <= 0)
                return 0f;

            return (float)enemiesKilled / enemiesTotal;
        }

        /// <summary>
        /// Get stars earned (0-3)
        /// </summary>
        public int GetStarsEarned()
        {
            if (currentState != GameplayState.Victory)
                return 0;

            // Calculate stars based on score
            float scorePercentage = targetScore > 0 ? (float)currentScore / targetScore : 0f;

            if (scorePercentage >= 1.0f)
                return 3;
            else if (scorePercentage >= 0.7f)
                return 2;
            else if (scorePercentage >= 0.4f)
                return 1;
            else
                return 0;
        }
    }

    /// <summary>
    /// Upgrade slot data
    /// </summary>
    [Serializable]
    public class UpgradeSlotData
    {
        public int slotId;
        public string upgradeName;
        public int level;
        public int maxLevel;
        public bool isUnlocked;
    }

    /// <summary>
    /// Upgrade cost
    /// </summary>
    public struct UpgradeCost
    {
        public int wood;
        public int stone;
        public int iron;
    }

    /// <summary>
    /// Gameplay state
    /// </summary>
    public enum GameplayState
    {
        Loading,
        Playing,
        Paused,
        Victory,
        Defeat
    }
}
