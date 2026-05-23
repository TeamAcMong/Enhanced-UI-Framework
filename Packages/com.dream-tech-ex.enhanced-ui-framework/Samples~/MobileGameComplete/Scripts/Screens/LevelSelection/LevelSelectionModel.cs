using System;
using System.Collections.Generic;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.LevelSelection
{
    /// <summary>
    /// Model for Level Selection Screen
    /// </summary>
    [Serializable]
    public class LevelSelectionModel
    {
        // Player reference
        public PlayerData playerData;

        // Current selection
        public int selectedChapter;
        public string selectedLevelId;

        // Chapters
        public List<ChapterData> chapters;

        // UI State
        public bool isKingdomPassActive;
        public int kingdomPassProgress;
        public int kingdomPassMax;

        // Energy cost
        public int energyCostPerLevel;

        public LevelSelectionModel()
        {
            selectedChapter = 1;
            selectedLevelId = null;
            chapters = new List<ChapterData>();
            energyCostPerLevel = 10;
            isKingdomPassActive = true;
            kingdomPassProgress = 15;
            kingdomPassMax = 30;

            InitializeChapters();
        }

        /// <summary>
        /// Initialize chapter data
        /// </summary>
        private void InitializeChapters()
        {
            // Create demo chapters
            for (int i = 1; i <= 5; i++)
            {
                var chapter = new ChapterData
                {
                    chapterId = i,
                    chapterName = $"Chapter {i}",
                    isUnlocked = i <= 2, // First 2 chapters unlocked
                    levels = new List<LevelData>()
                };

                // Add levels to chapter (20 levels per chapter)
                for (int j = 1; j <= 20; j++)
                {
                    string levelId = $"{i}-{j}";
                    bool isUnlocked = false;

                    // First chapter: first 5 levels unlocked
                    if (i == 1 && j <= 5)
                        isUnlocked = true;
                    // Second chapter: first 3 levels unlocked
                    else if (i == 2 && j <= 3)
                        isUnlocked = true;

                    var level = new LevelData
                    {
                        levelId = levelId,
                        levelNumber = j,
                        chapterId = i,
                        isUnlocked = isUnlocked,
                        starsEarned = 0,
                        bestScore = 0
                    };

                    // Some completed levels
                    if (i == 1 && j <= 3)
                    {
                        level.starsEarned = UnityEngine.Random.Range(1, 4);
                        level.bestScore = UnityEngine.Random.Range(1000, 3000);
                    }

                    chapter.levels.Add(level);
                }

                chapters.Add(chapter);
            }
        }

        /// <summary>
        /// Get chapter by ID
        /// </summary>
        public ChapterData GetChapter(int chapterId)
        {
            return chapters.Find(c => c.chapterId == chapterId);
        }

        /// <summary>
        /// Get level by ID
        /// </summary>
        public LevelData GetLevel(string levelId)
        {
            foreach (var chapter in chapters)
            {
                var level = chapter.levels.Find(l => l.levelId == levelId);
                if (level != null)
                    return level;
            }
            return null;
        }

        /// <summary>
        /// Check if player can play selected level
        /// </summary>
        public bool CanPlayLevel(string levelId)
        {
            var level = GetLevel(levelId);
            if (level == null || !level.isUnlocked)
                return false;

            // Check energy
            return playerData != null && playerData.energy >= energyCostPerLevel;
        }

        /// <summary>
        /// Get total stars earned in chapter
        /// </summary>
        public int GetChapterStars(int chapterId)
        {
            var chapter = GetChapter(chapterId);
            if (chapter == null)
                return 0;

            int totalStars = 0;
            foreach (var level in chapter.levels)
            {
                totalStars += level.starsEarned;
            }

            return totalStars;
        }

        /// <summary>
        /// Get max possible stars in chapter
        /// </summary>
        public int GetChapterMaxStars(int chapterId)
        {
            var chapter = GetChapter(chapterId);
            return chapter != null ? chapter.levels.Count * 3 : 0;
        }

        /// <summary>
        /// Get chapter completion percentage
        /// </summary>
        public float GetChapterCompletion(int chapterId)
        {
            var chapter = GetChapter(chapterId);
            if (chapter == null)
                return 0f;

            int completedCount = 0;
            foreach (var level in chapter.levels)
            {
                if (level.starsEarned > 0)
                    completedCount++;
            }

            return (float)completedCount / chapter.levels.Count;
        }
    }

    /// <summary>
    /// Chapter data
    /// </summary>
    [Serializable]
    public class ChapterData
    {
        public int chapterId;
        public string chapterName;
        public bool isUnlocked;
        public List<LevelData> levels;
    }

    /// <summary>
    /// Level data
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public string levelId;
        public int levelNumber;
        public int chapterId;
        public bool isUnlocked;
        public int starsEarned; // 0-3
        public int bestScore;
    }
}
