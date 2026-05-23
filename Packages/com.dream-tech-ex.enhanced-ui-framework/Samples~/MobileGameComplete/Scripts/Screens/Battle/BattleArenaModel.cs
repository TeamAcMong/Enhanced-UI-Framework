using System;
using System.Collections.Generic;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Battle
{
    /// <summary>
    /// Model for Battle Arena Screen
    /// </summary>
    [Serializable]
    public class BattleArenaModel
    {
        // Player data reference
        public PlayerData playerData;

        // Battle types
        public List<BattleTypeData> battleTypes;

        // Selected battle
        public int selectedBattleIndex;

        // Leaderboard (top 5)
        public List<LeaderboardEntry> leaderboard;

        public BattleArenaModel()
        {
            selectedBattleIndex = 0;

            battleTypes = new List<BattleTypeData>
            {
                new BattleTypeData
                {
                    battleId = 1,
                    battleName = "Parking",
                    description = "Defend the parking lot from invaders",
                    energyCost = 20,
                    ticketCost = 0,
                    isUnlocked = true,
                    rewards = "100 Gold + 10 Gems"
                },
                new BattleTypeData
                {
                    battleId = 2,
                    battleName = "Boss Battle",
                    description = "Take on the mighty boss",
                    energyCost = 30,
                    ticketCost = 1,
                    isUnlocked = true,
                    rewards = "500 Gold + 50 Gems"
                },
                new BattleTypeData
                {
                    battleId = 3,
                    battleName = "Survival",
                    description = "How long can you last?",
                    energyCost = 25,
                    ticketCost = 0,
                    isUnlocked = true,
                    rewards = "200 Gold + 20 Gems"
                },
                new BattleTypeData
                {
                    battleId = 4,
                    battleName = "Tournament",
                    description = "Compete for the ultimate prize",
                    energyCost = 0,
                    ticketCost = 3,
                    isUnlocked = false,
                    rewards = "1000 Gold + 100 Gems"
                }
            };

            leaderboard = new List<LeaderboardEntry>
            {
                new LeaderboardEntry { rank = 1, playerName = "ProGamer123", score = 15000 },
                new LeaderboardEntry { rank = 2, playerName = "BattleMaster", score = 12500 },
                new LeaderboardEntry { rank = 3, playerName = "EliteWarrior", score = 10000 },
                new LeaderboardEntry { rank = 4, playerName = "SkillPlayer", score = 8500 },
                new LeaderboardEntry { rank = 5, playerName = "TopFighter", score = 7000 }
            };
        }

        /// <summary>
        /// Get selected battle type
        /// </summary>
        public BattleTypeData GetSelectedBattle()
        {
            if (selectedBattleIndex >= 0 && selectedBattleIndex < battleTypes.Count)
                return battleTypes[selectedBattleIndex];

            return null;
        }

        /// <summary>
        /// Check if player can afford selected battle
        /// </summary>
        public bool CanAffordBattle()
        {
            var battle = GetSelectedBattle();
            if (battle == null || !battle.isUnlocked)
                return false;

            bool hasEnergy = playerData.energy >= battle.energyCost;
            bool hasTickets = true; // Tickets not implemented in PlayerData yet

            return hasEnergy && hasTickets;
        }

        /// <summary>
        /// Get battle cost text
        /// </summary>
        public string GetBattleCostText()
        {
            var battle = GetSelectedBattle();
            if (battle == null)
                return "";

            List<string> costs = new List<string>();

            if (battle.energyCost > 0)
                costs.Add($"{battle.energyCost} Energy");

            if (battle.ticketCost > 0)
                costs.Add($"{battle.ticketCost} Tickets");

            return string.Join(" + ", costs);
        }
    }

    /// <summary>
    /// Battle type data
    /// </summary>
    [Serializable]
    public class BattleTypeData
    {
        public int battleId;
        public string battleName;
        public string description;
        public int energyCost;
        public int ticketCost;
        public bool isUnlocked;
        public string rewards;
    }

    /// <summary>
    /// Leaderboard entry
    /// </summary>
    [Serializable]
    public class LeaderboardEntry
    {
        public int rank;
        public string playerName;
        public int score;
    }
}
