using UnityEngine;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.LevelSelection
{
    /// <summary>
    /// Presenter for Level Selection Screen
    /// </summary>
    public class LevelSelectionPresenter
    {
        private LevelSelectionModel _model;
        private LevelSelectionView _view;

        /// <summary>
        /// Constructor
        /// </summary>
        public LevelSelectionPresenter(LevelSelectionView view)
        {
            _view = view;
            _model = new LevelSelectionModel();

            Initialize();
        }

        /// <summary>
        /// Initialize presenter
        /// </summary>
        private void Initialize()
        {
            // Get player data from GameState
            if (GameState.Instance != null)
            {
                _model.playerData = GameState.Instance.PlayerData;
                LoadLevelProgress();
            }

            // Subscribe to events
            SubscribeToEvents();

            // Create UI
            _view.CreateChapterButtons(_model);
            _view.CreateLevelGrid(_model);

            // Update view
            UpdateView();

            // Highlight default chapter
            _view.HighlightChapterButton(_model.selectedChapter);

            Debug.Log("[LevelSelectionPresenter] Initialized");
        }

        /// <summary>
        /// Cleanup presenter
        /// </summary>
        public void Cleanup()
        {
            UnsubscribeFromEvents();
            SaveLevelProgress();
            Debug.Log("[LevelSelectionPresenter] Cleanup complete");
        }

        /// <summary>
        /// Subscribe to game state events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.OnCurrencyChanged += OnCurrencyChanged;
                GameState.Instance.OnLevelCompleted += OnLevelCompleted;
                GameState.Instance.OnLevelUnlocked += OnLevelUnlocked;
            }
        }

        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.OnCurrencyChanged -= OnCurrencyChanged;
                GameState.Instance.OnLevelCompleted -= OnLevelCompleted;
                GameState.Instance.OnLevelUnlocked -= OnLevelUnlocked;
            }
        }

        /// <summary>
        /// Load level progress from GameState
        /// </summary>
        private void LoadLevelProgress()
        {
            if (GameState.Instance == null) return;

            // Sync unlocked levels
            foreach (var chapter in _model.chapters)
            {
                foreach (var level in chapter.levels)
                {
                    bool isUnlocked = GameState.Instance.PlayerData.IsLevelUnlocked(level.levelId);
                    level.isUnlocked = isUnlocked;

                    // Load stars earned
                    bool isCompleted = GameState.Instance.PlayerData.IsLevelCompleted(level.levelId);
                    if (isCompleted)
                    {
                        // Load stars from saved data (for demo, use random)
                        string key = $"Level_{level.levelId}_Stars";
                        level.starsEarned = PlayerPrefs.GetInt(key, UnityEngine.Random.Range(1, 4));
                    }
                }
            }

            Debug.Log("[LevelSelectionPresenter] Level progress loaded");
        }

        /// <summary>
        /// Save level progress
        /// </summary>
        private void SaveLevelProgress()
        {
            // Save selected chapter
            PlayerPrefs.SetInt("SelectedChapter", _model.selectedChapter);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Update the view
        /// </summary>
        private void UpdateView()
        {
            _view.UpdateView(_model);
        }

        #region Button Handlers

        /// <summary>
        /// Handle back button press
        /// </summary>
        public void OnBackButtonPressed()
        {
            Debug.Log("[LevelSelectionPresenter] Back button pressed");
            NavigateBack();
        }

        /// <summary>
        /// Handle chapter selection
        /// </summary>
        public void OnChapterSelected(int chapterId)
        {
            Debug.Log($"[LevelSelectionPresenter] Chapter {chapterId} selected");

            var chapter = _model.GetChapter(chapterId);
            if (chapter == null)
            {
                Debug.LogWarning($"[LevelSelectionPresenter] Chapter {chapterId} not found");
                return;
            }

            if (!chapter.isUnlocked)
            {
                Debug.Log($"[LevelSelectionPresenter] Chapter {chapterId} is locked");
                ShowChapterLockedPopup(chapterId);
                return;
            }

            // Update selected chapter
            _model.selectedChapter = chapterId;
            _model.selectedLevelId = null; // Clear level selection

            // Update view
            _view.CreateLevelGrid(_model);
            _view.UpdateView(_model);
            _view.HighlightChapterButton(chapterId);
            _view.ShowLevelSelection(); // Hide play button
        }

        /// <summary>
        /// Handle level selection
        /// </summary>
        public void OnLevelSelected(string levelId)
        {
            Debug.Log($"[LevelSelectionPresenter] Level {levelId} selected");

            var level = _model.GetLevel(levelId);
            if (level == null)
            {
                Debug.LogWarning($"[LevelSelectionPresenter] Level {levelId} not found");
                return;
            }

            if (!level.isUnlocked)
            {
                Debug.Log($"[LevelSelectionPresenter] Level {levelId} is locked");
                ShowLevelLockedPopup(levelId);
                return;
            }

            // Update selected level
            _model.selectedLevelId = levelId;

            // Check if player can play
            bool canPlay = _model.CanPlayLevel(levelId);

            // Show play button
            _view.ShowPlayButton(levelId, _model.energyCostPerLevel, canPlay);
        }

        /// <summary>
        /// Handle play button press
        /// </summary>
        public void OnPlayButtonPressed()
        {
            if (string.IsNullOrEmpty(_model.selectedLevelId))
            {
                Debug.LogWarning("[LevelSelectionPresenter] No level selected");
                return;
            }

            Debug.Log($"[LevelSelectionPresenter] Play button pressed for level {_model.selectedLevelId}");

            // Validate
            if (!_model.CanPlayLevel(_model.selectedLevelId))
            {
                Debug.LogWarning("[LevelSelectionPresenter] Cannot play level - insufficient energy or locked");
                ShowInsufficientEnergyPopup();
                return;
            }

            // Deduct energy
            bool success = GameState.Instance.SpendCurrency(CurrencyType.Energy, _model.energyCostPerLevel);
            if (!success)
            {
                Debug.LogError("[LevelSelectionPresenter] Failed to deduct energy");
                ShowInsufficientEnergyPopup();
                return;
            }

            // Start level
            StartLevel(_model.selectedLevelId);
        }

        /// <summary>
        /// Handle Kingdom Pass button press
        /// </summary>
        public void OnKingdomPassButtonPressed()
        {
            Debug.Log("[LevelSelectionPresenter] Kingdom Pass button pressed");
            OpenKingdomPassDetails();
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Navigate back to home screen
        /// </summary>
        private void NavigateBack()
        {
            Debug.Log("[LevelSelectionPresenter] Navigating back to Home");
            NavigationManager.Instance?.GoBack();
        }

        /// <summary>
        /// Start level (navigate to gameplay screen)
        /// </summary>
        private void StartLevel(string levelId)
        {
            Debug.Log($"[LevelSelectionPresenter] Starting level {levelId}");

            // Save level ID for gameplay screen
            PlayerPrefs.SetString("CurrentLevelId", levelId);
            PlayerPrefs.Save();

            NavigationManager.Instance?.GoToGameplay();
        }

        /// <summary>
        /// Open Kingdom Pass details
        /// </summary>
        private void OpenKingdomPassDetails()
        {
            Debug.Log("[LevelSelectionPresenter] Opening Kingdom Pass details");
            // TODO: Implement modal
            // ModalContainer.Instance.Push("KingdomPassModal");
        }

        #endregion

        #region Popups

        /// <summary>
        /// Show chapter locked popup
        /// </summary>
        private void ShowChapterLockedPopup(int chapterId)
        {
            Debug.Log($"[LevelSelectionPresenter] Chapter {chapterId} is locked");
            // TODO: Show modal
            // ModalContainer.Instance.Push("ChapterLockedModal", new { chapterId = chapterId });
        }

        /// <summary>
        /// Show level locked popup
        /// </summary>
        private void ShowLevelLockedPopup(string levelId)
        {
            Debug.Log($"[LevelSelectionPresenter] Level {levelId} is locked");
            // TODO: Show modal
            // ModalContainer.Instance.Push("LevelLockedModal", new { levelId = levelId });
        }

        /// <summary>
        /// Show insufficient energy popup
        /// </summary>
        private void ShowInsufficientEnergyPopup()
        {
            Debug.Log("[LevelSelectionPresenter] Insufficient energy");
            // TODO: Show modal
            // ModalContainer.Instance.Push("InsufficientEnergyModal");
        }

        #endregion

        #region Business Logic

        /// <summary>
        /// Unlock next level in sequence
        /// </summary>
        private void UnlockNextLevel(string completedLevelId)
        {
            var completedLevel = _model.GetLevel(completedLevelId);
            if (completedLevel == null) return;

            var chapter = _model.GetChapter(completedLevel.chapterId);
            if (chapter == null) return;

            // Find next level in chapter
            int currentIndex = chapter.levels.FindIndex(l => l.levelId == completedLevelId);
            if (currentIndex >= 0 && currentIndex < chapter.levels.Count - 1)
            {
                var nextLevel = chapter.levels[currentIndex + 1];
                if (!nextLevel.isUnlocked)
                {
                    nextLevel.isUnlocked = true;
                    GameState.Instance.UnlockLevel(nextLevel.levelId);
                    Debug.Log($"[LevelSelectionPresenter] Unlocked next level: {nextLevel.levelId}");
                }
            }
            else if (currentIndex == chapter.levels.Count - 1)
            {
                // Last level in chapter - unlock next chapter
                if (completedLevel.chapterId < _model.chapters.Count)
                {
                    var nextChapter = _model.GetChapter(completedLevel.chapterId + 1);
                    if (nextChapter != null && !nextChapter.isUnlocked)
                    {
                        nextChapter.isUnlocked = true;
                        Debug.Log($"[LevelSelectionPresenter] Unlocked next chapter: {nextChapter.chapterId}");
                    }
                }
            }
        }

        /// <summary>
        /// Update Kingdom Pass progress
        /// </summary>
        private void UpdateKingdomPassProgress(int starsEarned)
        {
            if (!_model.isKingdomPassActive) return;

            _model.kingdomPassProgress += starsEarned;
            _model.kingdomPassProgress = Mathf.Min(_model.kingdomPassProgress, _model.kingdomPassMax);

            Debug.Log($"[LevelSelectionPresenter] Kingdom Pass progress: {_model.kingdomPassProgress}/{_model.kingdomPassMax}");

            UpdateView();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle currency changed event
        /// </summary>
        private void OnCurrencyChanged(CurrencyType type, int oldValue, int newValue)
        {
            // Update view if energy changed
            if (type == CurrencyType.Energy && !string.IsNullOrEmpty(_model.selectedLevelId))
            {
                bool canPlay = _model.CanPlayLevel(_model.selectedLevelId);
                _view.ShowPlayButton(_model.selectedLevelId, _model.energyCostPerLevel, canPlay);
            }
        }

        /// <summary>
        /// Handle level completed event
        /// </summary>
        private void OnLevelCompleted(string levelId, int starsEarned)
        {
            Debug.Log($"[LevelSelectionPresenter] Level {levelId} completed with {starsEarned} stars");

            var level = _model.GetLevel(levelId);
            if (level != null)
            {
                // Update stars
                level.starsEarned = Mathf.Max(level.starsEarned, starsEarned);

                // Save stars
                string key = $"Level_{levelId}_Stars";
                PlayerPrefs.SetInt(key, level.starsEarned);
                PlayerPrefs.Save();

                // Unlock next level
                UnlockNextLevel(levelId);

                // Update Kingdom Pass
                UpdateKingdomPassProgress(starsEarned);

                // Refresh view
                _view.CreateLevelGrid(_model);
                UpdateView();
            }
        }

        /// <summary>
        /// Handle level unlocked event
        /// </summary>
        private void OnLevelUnlocked(string levelId)
        {
            Debug.Log($"[LevelSelectionPresenter] Level {levelId} unlocked");

            var level = _model.GetLevel(levelId);
            if (level != null)
            {
                level.isUnlocked = true;

                // Refresh view
                _view.CreateLevelGrid(_model);
                UpdateView();
            }
        }

        #endregion
    }
}
