using UnityEngine;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Battle
{
    /// <summary>
    /// Presenter for Battle Arena Screen
    /// </summary>
    public class BattleArenaPresenter
    {
        private BattleArenaModel _model;
        private BattleArenaView _view;

        /// <summary>
        /// Constructor
        /// </summary>
        public BattleArenaPresenter(BattleArenaView view)
        {
            _view = view;
            _model = new BattleArenaModel();

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

            // Load saved selection
            _model.selectedBattleIndex = PlayerPrefs.GetInt("SelectedBattleIndex", 0);

            // Subscribe to events
            SubscribeToEvents();

            // Create UI
            _view.CreateBattleButtons(_model);
            _view.CreateLeaderboard(_model);

            // Update view
            UpdateView();

            // Highlight selected battle
            _view.HighlightBattleButton(_model.selectedBattleIndex);

            Debug.Log("[BattleArenaPresenter] Initialized");
        }

        /// <summary>
        /// Cleanup presenter
        /// </summary>
        public void Cleanup()
        {
            UnsubscribeFromEvents();
            SaveSettings();
            Debug.Log("[BattleArenaPresenter] Cleanup complete");
        }

        /// <summary>
        /// Subscribe to game state events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.OnCurrencyChanged += OnCurrencyChanged;
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
            }
        }

        /// <summary>
        /// Save settings
        /// </summary>
        private void SaveSettings()
        {
            PlayerPrefs.SetInt("SelectedBattleIndex", _model.selectedBattleIndex);
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
            Debug.Log("[BattleArenaPresenter] Back button pressed");
            NavigateBack();
        }

        /// <summary>
        /// Handle battle selection
        /// </summary>
        public void OnBattleSelected(int index)
        {
            Debug.Log($"[BattleArenaPresenter] Battle {index} selected");

            if (index < 0 || index >= _model.battleTypes.Count)
            {
                Debug.LogWarning($"[BattleArenaPresenter] Invalid battle index: {index}");
                return;
            }

            var battle = _model.battleTypes[index];

            if (!battle.isUnlocked)
            {
                Debug.Log($"[BattleArenaPresenter] Battle {battle.battleName} is locked");
                ShowBattleLockedPopup(battle);
                return;
            }

            // Update selection
            _model.selectedBattleIndex = index;

            // Update view
            UpdateView();
            _view.HighlightBattleButton(index);
        }

        /// <summary>
        /// Handle play button press
        /// </summary>
        public void OnPlayButtonPressed()
        {
            var battle = _model.GetSelectedBattle();
            if (battle == null)
            {
                Debug.LogWarning("[BattleArenaPresenter] No battle selected");
                return;
            }

            Debug.Log($"[BattleArenaPresenter] Play button pressed for {battle.battleName}");

            // Check if player can afford
            if (!_model.CanAffordBattle())
            {
                Debug.LogWarning("[BattleArenaPresenter] Cannot afford battle");
                ShowInsufficientResourcesPopup(battle);
                return;
            }

            // Deduct costs
            bool success = DeductBattleCost(battle);
            if (!success)
            {
                Debug.LogError("[BattleArenaPresenter] Failed to deduct battle cost");
                ShowInsufficientResourcesPopup(battle);
                return;
            }

            // Start battle
            StartBattle(battle);
        }

        /// <summary>
        /// Handle view full leaderboard button press
        /// </summary>
        public void OnViewFullLeaderboardPressed()
        {
            Debug.Log("[BattleArenaPresenter] View full leaderboard pressed");
            OpenFullLeaderboard();
        }

        /// <summary>
        /// Handle bottom navigation tab change
        /// </summary>
        public void OnBottomNavTabChanged(string tabId)
        {
            Debug.Log($"[BattleArenaPresenter] Bottom nav tab changed to: {tabId}");

            // Navigate to different screens based on tab
            switch (tabId)
            {
                case "home":
                    NavigateToHome();
                    break;
                case "battle":
                    // Already on battle
                    break;
                case "inventory":
                    NavigateToInventory();
                    break;
                case "shop":
                    NavigateToShop();
                    break;
                default:
                    Debug.LogWarning($"[BattleArenaPresenter] Unknown tab: {tabId}");
                    break;
            }
        }

        #endregion

        #region Business Logic

        /// <summary>
        /// Deduct battle cost from player
        /// </summary>
        private bool DeductBattleCost(BattleTypeData battle)
        {
            if (GameState.Instance == null)
                return false;

            // Deduct energy
            if (battle.energyCost > 0)
            {
                bool success = GameState.Instance.SpendCurrency(CurrencyType.Energy, battle.energyCost);
                if (!success)
                    return false;
            }

            // Deduct tickets (not implemented in GameState yet)
            if (battle.ticketCost > 0)
            {
                // TODO: Implement ticket currency
                Debug.Log($"[BattleArenaPresenter] Would deduct {battle.ticketCost} tickets");
            }

            Debug.Log($"[BattleArenaPresenter] Deducted battle cost: {battle.energyCost} energy, {battle.ticketCost} tickets");
            return true;
        }

        #endregion

        #region Navigation

        /// <summary>
        /// Navigate back to home
        /// </summary>
        private void NavigateBack()
        {
            Debug.Log("[BattleArenaPresenter] Navigating back to Home");
            NavigationManager.Instance?.GoBack();
        }

        /// <summary>
        /// Start battle (navigate to gameplay)
        /// </summary>
        private void StartBattle(BattleTypeData battle)
        {
            Debug.Log($"[BattleArenaPresenter] Starting battle: {battle.battleName}");

            // Save battle ID for gameplay screen
            PlayerPrefs.SetString("CurrentBattleId", battle.battleId.ToString());
            PlayerPrefs.Save();

            // TODO: Implement navigation via PageContainer
            // For battle, we might use a different gameplay screen or pass battle type as parameter
            // PageContainer.Instance.Push("BattleGameplayPage", playAnimation: true);
        }

        /// <summary>
        /// Open full leaderboard
        /// </summary>
        private void OpenFullLeaderboard()
        {
            Debug.Log("[BattleArenaPresenter] Opening full leaderboard");
            // TODO: Open leaderboard modal
            // ModalContainer.Instance.Push("LeaderboardModal");
        }

        /// <summary>
        /// Navigate to home screen
        /// </summary>
        private void NavigateToHome()
        {
            Debug.Log("[BattleArenaPresenter] Navigating to Home");
            NavigationManager.Instance?.GoToHome();
        }

        /// <summary>
        /// Navigate to inventory screen
        /// </summary>
        private void NavigateToInventory()
        {
            Debug.Log("[BattleArenaPresenter] Navigating to Inventory");
            NavigationManager.Instance?.GoToInventory();
        }

        /// <summary>
        /// Navigate to shop screen
        /// </summary>
        private void NavigateToShop()
        {
            Debug.Log("[BattleArenaPresenter] Navigating to Shop");
            NavigationManager.Instance?.GoToShop();
        }

        #endregion

        #region Popups

        /// <summary>
        /// Show battle locked popup
        /// </summary>
        private void ShowBattleLockedPopup(BattleTypeData battle)
        {
            Debug.Log($"[BattleArenaPresenter] Battle {battle.battleName} is locked");
            // TODO: Show modal
            // ModalContainer.Instance.Push("BattleLockedModal", new { battleId = battle.battleId });
        }

        /// <summary>
        /// Show insufficient resources popup
        /// </summary>
        private void ShowInsufficientResourcesPopup(BattleTypeData battle)
        {
            Debug.Log("[BattleArenaPresenter] Insufficient resources");
            // TODO: Show modal
            // ModalContainer.Instance.Push("InsufficientResourcesModal", new { requiredEnergy = battle.energyCost, requiredTickets = battle.ticketCost });
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle currency changed event
        /// </summary>
        private void OnCurrencyChanged(CurrencyType type, int oldValue, int newValue)
        {
            // Update view if energy changed (affects play button)
            if (type == CurrencyType.Energy)
            {
                UpdateView();
            }
        }

        #endregion
    }
}
