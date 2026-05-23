using System;
using System.Collections.Generic;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.RPGStage
{
    [Serializable]
    public class RPGStageModel
    {
        public PlayerData playerData;
        public List<CharacterData> availableCharacters;
        public List<CharacterData> selectedParty;
        public BossData currentBoss;
        public StageInfo currentStage;
        public int maxPartySize = 4;

        public RPGStageModel()
        {
            InitializeCharacters();
            InitializeBoss();
            InitializeStage();
            selectedParty = new List<CharacterData>();
        }

        private void InitializeCharacters()
        {
            availableCharacters = new List<CharacterData>
            {
                new CharacterData { id = "hero_knight", name = "Sir Valor", role = CharacterRole.Tank, level = 15, hp = 2500, maxHp = 2500, attack = 180, defense = 220, isUnlocked = true },
                new CharacterData { id = "hero_mage", name = "Arcana", role = CharacterRole.Mage, level = 14, hp = 1200, maxHp = 1200, attack = 350, defense = 80, isUnlocked = true },
                new CharacterData { id = "hero_archer", name = "Swift Arrow", role = CharacterRole.DPS, level = 13, hp = 1500, maxHp = 1500, attack = 280, defense = 120, isUnlocked = true },
                new CharacterData { id = "hero_healer", name = "Divine Light", role = CharacterRole.Support, level = 14, hp = 1300, maxHp = 1300, attack = 100, defense = 100, isUnlocked = true },
                new CharacterData { id = "hero_assassin", name = "Shadow Strike", role = CharacterRole.DPS, level = 12, hp = 1400, maxHp = 1400, attack = 320, defense = 90, isUnlocked = false },
                new CharacterData { id = "hero_paladin", name = "Holy Guardian", role = CharacterRole.Tank, level = 10, hp = 2800, maxHp = 2800, attack = 150, defense = 250, isUnlocked = false }
            };
        }

        private void InitializeBoss()
        {
            currentBoss = new BossData
            {
                id = "boss_dragon",
                name = "Crimson Dragon",
                level = 18,
                hp = 50000,
                maxHp = 50000,
                attack = 450,
                defense = 200,
                abilities = new List<string> { "Fire Breath", "Wing Strike", "Rage Mode" },
                weaknesses = new List<CharacterRole> { CharacterRole.Mage },
                resistances = new List<CharacterRole> { CharacterRole.Tank }
            };
        }

        private void InitializeStage()
        {
            currentStage = new StageInfo
            {
                stageNumber = 5,
                stageName = "Dragon's Lair",
                difficulty = "Hard",
                recommendedLevel = 15,
                energyCost = 20,
                rewards = new StageRewards
                {
                    gold = 5000,
                    experience = 1200,
                    lootItems = new List<string> { "Dragon Scale", "Magic Crystal", "Rare Weapon Box" }
                }
            };
        }

        public bool CanAddToParty(CharacterData character)
        {
            return selectedParty.Count < maxPartySize &&
                   character.isUnlocked &&
                   !selectedParty.Contains(character);
        }

        public bool CanRemoveFromParty(CharacterData character)
        {
            return selectedParty.Contains(character);
        }

        public bool CanStartBattle()
        {
            return selectedParty.Count > 0 &&
                   playerData != null &&
                   playerData.energy >= currentStage.energyCost;
        }

        public bool IsPartyFull()
        {
            return selectedParty.Count >= maxPartySize;
        }
    }

    [Serializable]
    public class CharacterData
    {
        public string id;
        public string name;
        public CharacterRole role;
        public int level;
        public int hp;
        public int maxHp;
        public int attack;
        public int defense;
        public bool isUnlocked;

        public string GetRoleColor()
        {
            return role switch
            {
                CharacterRole.Tank => "#4A90E2",      // Blue
                CharacterRole.DPS => "#E74C3C",       // Red
                CharacterRole.Mage => "#9B59B6",      // Purple
                CharacterRole.Support => "#2ECC71",   // Green
                _ => "#FFFFFF"
            };
        }

        public string GetRoleIcon()
        {
            return role switch
            {
                CharacterRole.Tank => "🛡️",
                CharacterRole.DPS => "⚔️",
                CharacterRole.Mage => "✨",
                CharacterRole.Support => "❤️",
                _ => "?"
            };
        }
    }

    [Serializable]
    public class BossData
    {
        public string id;
        public string name;
        public int level;
        public int hp;
        public int maxHp;
        public int attack;
        public int defense;
        public List<string> abilities;
        public List<CharacterRole> weaknesses;
        public List<CharacterRole> resistances;

        public float GetHpPercentage()
        {
            return maxHp > 0 ? (float)hp / maxHp : 0f;
        }
    }

    [Serializable]
    public class StageInfo
    {
        public int stageNumber;
        public string stageName;
        public string difficulty;
        public int recommendedLevel;
        public int energyCost;
        public StageRewards rewards;
    }

    [Serializable]
    public class StageRewards
    {
        public int gold;
        public int experience;
        public List<string> lootItems;
    }

    public enum CharacterRole
    {
        Tank,
        DPS,
        Mage,
        Support
    }
}
