using System;
using UnityEngine;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Shop
{
    /// <summary>
    /// Shop Presenter - Business logic for shop screen
    /// Handles purchases, tab switching, and currency updates
    /// </summary>
    public class ShopPresenter
    {
        private readonly ShopModel _model;
        private readonly ShopView _view;

        public ShopPresenter(ShopView view)
        {
            _view = view;
            _model = new ShopModel();

            // Get player data from singleton
            _model.playerData = GameState.Instance.PlayerData;

            // Subscribe to view events
            _view.OnTabChanged += HandleTabChanged;
            _view.OnItemPurchased += HandleItemPurchased;
            _view.OnBackPressed += HandleBackPressed;

            // Subscribe to game state events
            GameState.Instance.OnCurrencyChanged += HandleCurrencyChanged;

            // Initial setup
            Initialize();
        }

        /// <summary>
        /// Initialize presenter
        /// </summary>
        private void Initialize()
        {
            Debug.Log("[ShopPresenter] Initialize");

            // Show initial tab
            UpdateCurrentTab(_model.currentTab);

            // Update player currency display
            UpdatePlayerCurrencyDisplay();
        }

        /// <summary>
        /// Cleanup presenter
        /// </summary>
        public void Cleanup()
        {
            Debug.Log("[ShopPresenter] Cleanup");

            // Unsubscribe from view events
            _view.OnTabChanged -= HandleTabChanged;
            _view.OnItemPurchased -= HandleItemPurchased;
            _view.OnBackPressed -= HandleBackPressed;

            // Unsubscribe from game state events
            if (GameState.Instance != null)
            {
                GameState.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handle tab change
        /// </summary>
        private void HandleTabChanged(ShopTab newTab)
        {
            Debug.Log($"[ShopPresenter] Tab changed to: {newTab}");

            _model.currentTab = newTab;
            UpdateCurrentTab(newTab);
        }

        /// <summary>
        /// Handle item purchase attempt
        /// </summary>
        private void HandleItemPurchased(ShopItem item)
        {
            Debug.Log($"[ShopPresenter] Attempting to purchase: {item.name}");

            _view.ShowLoading(true);

            // Simulate purchase delay (in real app, this would be IAP or server call)
            // For now, we'll process immediately
            ProcessPurchase(item);

            _view.ShowLoading(false);
        }

        /// <summary>
        /// Handle back button press
        /// </summary>
        private void HandleBackPressed()
        {
            Debug.Log("[ShopPresenter] Back button pressed");
            NavigationManager.Instance?.GoBack();
        }

        /// <summary>
        /// Handle currency change from game state
        /// </summary>
        private void HandleCurrencyChanged(CurrencyType type, int oldValue, int newValue)
        {
            Debug.Log($"[ShopPresenter] Currency changed: {type} {oldValue} -> {newValue}");

            // Update display
            UpdatePlayerCurrencyDisplay();

            // Refresh shop items to update affordability
            UpdateCurrentTab(_model.currentTab);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update current tab display
        /// </summary>
        private void UpdateCurrentTab(ShopTab tab)
        {
            // Update tab indicators
            _view.UpdateTabIndicators(tab);

            // Get items for current tab
            var items = _model.GetCurrentTabItems();

            // Display items
            _view.DisplayShopItems(items.ToArray(), item => _model.CanAfford(item));
        }

        /// <summary>
        /// Update player currency display
        /// </summary>
        private void UpdatePlayerCurrencyDisplay()
        {
            if (_model.playerData != null)
            {
                _view.UpdatePlayerCurrency(_model.playerData.gems, _model.playerData.gold);
            }
        }

        /// <summary>
        /// Process a purchase
        /// </summary>
        private void ProcessPurchase(ShopItem item)
        {
            // Check if player can afford
            if (!_model.CanAfford(item))
            {
                _view.ShowPurchaseFailed("Not enough gems");
                return;
            }

            // Process purchase based on payment type
            if (item.realPrice > 0)
            {
                // Real money purchase
                ProcessRealMoneyPurchase(item);
            }
            else if (item.gemsCost > 0)
            {
                // Gem purchase
                ProcessGemPurchase(item);
            }
        }

        /// <summary>
        /// Process real money purchase (simulated)
        /// </summary>
        private void ProcessRealMoneyPurchase(ShopItem item)
        {
            Debug.Log($"[ShopPresenter] Processing real money purchase: ${item.realPrice}");

            // In a real app, you would initiate IAP here
            // For demo purposes, we'll simulate a successful purchase

            // Add purchased items to player
            if (item.gemsAmount > 0)
            {
                GameState.Instance.AddCurrency(CurrencyType.Gems, item.gemsAmount);
            }

            if (item.goldAmount > 0)
            {
                GameState.Instance.AddCurrency(CurrencyType.Gold, item.goldAmount);
            }

            if (item.keysAmount > 0)
            {
                GameState.Instance.AddCurrency(CurrencyType.Keys, item.keysAmount);
            }

            // Show success feedback
            _view.ShowPurchaseSuccess(item);

            Debug.Log($"[ShopPresenter] Purchase successful: {item.name}");
        }

        /// <summary>
        /// Process gem purchase
        /// </summary>
        private void ProcessGemPurchase(ShopItem item)
        {
            Debug.Log($"[ShopPresenter] Processing gem purchase: {item.gemsCost} gems");

            // Check if player has enough gems
            if (_model.playerData.gems < item.gemsCost)
            {
                _view.ShowPurchaseFailed("Not enough gems");
                return;
            }

            // Deduct gems
            GameState.Instance.SpendCurrency(CurrencyType.Gems, item.gemsCost);

            // Add purchased gold
            if (item.goldAmount > 0)
            {
                GameState.Instance.AddCurrency(CurrencyType.Gold, item.goldAmount);
            }

            // Show success feedback
            _view.ShowPurchaseSuccess(item);

            Debug.Log($"[ShopPresenter] Purchase successful: {item.name}");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the shop (reload data)
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[ShopPresenter] Refresh");

            // Reload player data
            _model.playerData = GameState.Instance.PlayerData;

            // Update display
            UpdateCurrentTab(_model.currentTab);
            UpdatePlayerCurrencyDisplay();
        }

        /// <summary>
        /// Get current model state (for testing)
        /// </summary>
        public ShopModel GetModel()
        {
            return _model;
        }

        #endregion
    }
}
