using UnityEngine;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Gameplay
{
    /// <summary>
    /// Presenter for Gameplay Screen
    /// Handles game logic simulation for demo purposes
    /// </summary>
    public class GameplayPresenter
    {
        private GameplayModel _model;
        private GameplayView _view;

        private float _gameTime;
        private float _nextResourceTime;
        private float _nextEnemyTime;
        private const float RESOURCE_INTERVAL = 5f; // Gain resources every 5 seconds
        private const float ENEMY_INTERVAL = 2f; // Enemy killed every 2 seconds (simulated)

        /// <summary>
        /// Constructor
        /// </summary>
        public GameplayPresenter(GameplayView view)
        {
            _view = view;
            _model = new GameplayModel();

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
            }

            // Load level ID
            _model.levelId = PlayerPrefs.GetString("CurrentLevelId", "1-1");

            // Reset timers
            _gameTime = 0f;
            _nextResourceTime = RESOURCE_INTERVAL;
            _nextEnemyTime = ENEMY_INTERVAL;

            // Start in playing state
            _model.currentState = GameplayState.Playing;

            // Initial view update
            UpdateView();

            Debug.Log($"[GameplayPresenter] Initialized for level {_model.levelId}");
        }

        /// <summary>
        /// Cleanup presenter
        /// </summary>
        public void Cleanup()
        {
            // Ensure time scale is restored
            Time.timeScale = 1f;

            Debug.Log("[GameplayPresenter] Cleanup complete");
        }

        /// <summary>
        /// Update game logic (called every frame)
        /// </summary>
        public void Update(float deltaTime)
        {
            if (_model.currentState != GameplayState.Playing)
                return;

            if (_model.isPaused)
                return;

            // Update timer
            _model.levelTimer += deltaTime;
            _gameTime += deltaTime;

            // Simulate resource generation
            if (_gameTime >= _nextResourceTime)
            {
                GainResources(10, 5, 2);
                _nextResourceTime = _gameTime + RESOURCE_INTERVAL;
            }

            // Simulate enemies being killed
            if (_gameTime >= _nextEnemyTime && _model.enemiesKilled < _model.enemiesTotal)
            {
                KillEnemy();
                _nextEnemyTime = _gameTime + ENEMY_INTERVAL;
            }

            // Check win/lose conditions
            CheckGameConditions();

            // Update view
            UpdateView();
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
        /// Handle pause button press
        /// </summary>
        public void OnPauseButtonPressed()
        {
            Debug.Log("[GameplayPresenter] Pause button pressed");
            _model.isPaused = true;
            _model.currentState = GameplayState.Paused;
            _view.ShowPauseMenu();
        }

        /// <summary>
        /// Handle resume button press
        /// </summary>
        public void OnResumeButtonPressed()
        {
            Debug.Log("[GameplayPresenter] Resume button pressed");
            _model.isPaused = false;
            _model.currentState = GameplayState.Playing;
            _view.HidePauseMenu();
        }

        /// <summary>
        /// Handle restart button press
        /// </summary>
        public void OnRestartButtonPressed()
        {
            Debug.Log("[GameplayPresenter] Restart button pressed");
            RestartLevel();
        }

        /// <summary>
        /// Handle quit button press
        /// </summary>
        public void OnQuitButtonPressed()
        {
            Debug.Log("[GameplayPresenter] Quit button pressed");
            QuitToLevelSelection();
        }

        /// <summary>
        /// Handle upgrade button press
        /// </summary>
        public void OnUpgradeButtonPressed(int slotId)
        {
            Debug.Log($"[GameplayPresenter] Upgrade button pressed for slot {slotId}");

            if (!_model.CanAffordUpgrade(slotId))
            {
                Debug.LogWarning($"[GameplayPresenter] Cannot afford upgrade for slot {slotId}");
                return;
            }

            PerformUpgrade(slotId);
        }

        /// <summary>
        /// Handle victory next button press
        /// </summary>
        public void OnVictoryNextButtonPressed()
        {
            Debug.Log("[GameplayPresenter] Victory Next button pressed");
            LoadNextLevel();
        }

        #endregion

        #region Game Logic

        /// <summary>
        /// Gain resources
        /// </summary>
        private void GainResources(int wood, int stone, int iron)
        {
            _model.wood += wood;
            _model.stone += stone;
            _model.iron += iron;

            Debug.Log($"[GameplayPresenter] Gained resources: +{wood}W, +{stone}S, +{iron}I");
        }

        /// <summary>
        /// Kill an enemy (simulated)
        /// </summary>
        private void KillEnemy()
        {
            _model.enemiesKilled++;
            _model.currentScore += 50; // 50 points per enemy

            Debug.Log($"[GameplayPresenter] Enemy killed: {_model.enemiesKilled}/{_model.enemiesTotal}");
        }

        /// <summary>
        /// Perform upgrade
        /// </summary>
        private void PerformUpgrade(int slotId)
        {
            var slot = _model.GetUpgradeSlot(slotId);
            if (slot == null) return;

            var cost = _model.GetUpgradeCost(slotId);

            // Deduct resources
            _model.wood -= cost.wood;
            _model.stone -= cost.stone;
            _model.iron -= cost.iron;

            // Upgrade
            slot.level++;

            Debug.Log($"[GameplayPresenter] Upgraded {slot.upgradeName} to level {slot.level}");

            // Update view
            UpdateView();
        }

        /// <summary>
        /// Check win/lose conditions
        /// </summary>
        private void CheckGameConditions()
        {
            // Victory: All enemies killed
            if (_model.enemiesKilled >= _model.enemiesTotal && _model.currentState == GameplayState.Playing)
            {
                OnVictory();
                return;
            }

            // Defeat: Time limit exceeded (example condition)
            if (_model.levelTimer >= 300f && _model.currentState == GameplayState.Playing) // 5 minutes
            {
                OnDefeat();
                return;
            }
        }

        /// <summary>
        /// Handle victory
        /// </summary>
        private void OnVictory()
        {
            Debug.Log("[GameplayPresenter] Victory!");

            _model.currentState = GameplayState.Victory;

            // Calculate stars
            int starsEarned = _model.GetStarsEarned();

            // Award rewards
            int goldReward = 100 * starsEarned;
            if (GameState.Instance != null)
            {
                GameState.Instance.AddCurrency(CurrencyType.Gold, goldReward);
                GameState.Instance.CompleteLevel(_model.levelId, starsEarned);
            }

            // Show victory screen
            _view.ShowVictory(_model.currentScore, starsEarned);

            Debug.Log($"[GameplayPresenter] Stars earned: {starsEarned}, Gold reward: {goldReward}");
        }

        /// <summary>
        /// Handle defeat
        /// </summary>
        private void OnDefeat()
        {
            Debug.Log("[GameplayPresenter] Defeat!");

            _model.currentState = GameplayState.Defeat;

            // Show defeat screen
            _view.ShowDefeat();
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Restart current level
        /// </summary>
        private void RestartLevel()
        {
            Debug.Log("[GameplayPresenter] Restarting level");

            // Deduct energy again
            if (GameState.Instance != null)
            {
                int energyCost = 10; // Should match LevelSelection
                bool success = GameState.Instance.SpendCurrency(CurrencyType.Energy, energyCost);

                if (!success)
                {
                    Debug.LogWarning("[GameplayPresenter] Insufficient energy to restart");
                    QuitToLevelSelection();
                    return;
                }
            }

            // Reset model
            _model = new GameplayModel();
            _model.levelId = PlayerPrefs.GetString("CurrentLevelId", "1-1");

            if (GameState.Instance != null)
            {
                _model.playerData = GameState.Instance.PlayerData;
            }

            // Reset timers
            _gameTime = 0f;
            _nextResourceTime = RESOURCE_INTERVAL;
            _nextEnemyTime = ENEMY_INTERVAL;

            // Reset view
            _view.HideAllPanels();
            Time.timeScale = 1f;
            UpdateView();

            Debug.Log("[GameplayPresenter] Level restarted");
        }

        /// <summary>
        /// Quit to level selection
        /// </summary>
        private void QuitToLevelSelection()
        {
            Debug.Log("[GameplayPresenter] Quitting to level selection");

            // Restore time scale
            Time.timeScale = 1f;

            NavigationManager.Instance?.GoBack();
        }

        /// <summary>
        /// Load next level
        /// </summary>
        private void LoadNextLevel()
        {
            Debug.Log("[GameplayPresenter] Loading next level");

            // Parse current level ID to get next level
            string currentLevelId = _model.levelId;
            string[] parts = currentLevelId.Split('-');

            if (parts.Length == 2 && int.TryParse(parts[0], out int chapter) && int.TryParse(parts[1], out int level))
            {
                // Next level in same chapter
                string nextLevelId = $"{chapter}-{level + 1}";

                // Check if next level exists and is unlocked
                if (GameState.Instance != null && GameState.Instance.PlayerData.IsLevelUnlocked(nextLevelId))
                {
                    // Load next level
                    PlayerPrefs.SetString("CurrentLevelId", nextLevelId);
                    PlayerPrefs.Save();

                    // Restart with new level
                    RestartLevel();
                    return;
                }
            }

            // No next level or not unlocked - go back to level selection
            QuitToLevelSelection();
        }

        #endregion
    }
}
