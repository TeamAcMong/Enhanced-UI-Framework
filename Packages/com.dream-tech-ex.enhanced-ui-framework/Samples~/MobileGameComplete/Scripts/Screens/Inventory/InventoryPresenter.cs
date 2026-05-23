using System;
using UnityEngine;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Inventory
{
    /// <summary>
    /// Inventory Presenter - Business logic for inventory screen
    /// Handles filtering, sorting, item actions (equip/use)
    /// </summary>
    public class InventoryPresenter
    {
        private readonly InventoryModel _model;
        private readonly InventoryView _view;

        public InventoryPresenter(InventoryView view)
        {
            _view = view;
            _model = new InventoryModel();

            // Get player data from singleton
            _model.playerData = GameState.Instance.PlayerData;

            // Subscribe to view events
            _view.OnFilterChanged += HandleFilterChanged;
            _view.OnSortChanged += HandleSortChanged;
            _view.OnItemSelected += HandleItemSelected;
            _view.OnItemEquipped += HandleItemEquipped;
            _view.OnItemUsed += HandleItemUsed;
            _view.OnBackPressed += HandleBackPressed;

            // Initial setup
            Initialize();
        }

        /// <summary>
        /// Initialize presenter
        /// </summary>
        private void Initialize()
        {
            Debug.Log("[InventoryPresenter] Initialize");

            // Display initial items
            RefreshItemDisplay();
        }

        /// <summary>
        /// Cleanup presenter
        /// </summary>
        public void Cleanup()
        {
            Debug.Log("[InventoryPresenter] Cleanup");

            // Unsubscribe from view events
            _view.OnFilterChanged -= HandleFilterChanged;
            _view.OnSortChanged -= HandleSortChanged;
            _view.OnItemSelected -= HandleItemSelected;
            _view.OnItemEquipped -= HandleItemEquipped;
            _view.OnItemUsed -= HandleItemUsed;
            _view.OnBackPressed -= HandleBackPressed;
        }

        #region Event Handlers

        /// <summary>
        /// Handle filter change
        /// </summary>
        private void HandleFilterChanged(InventoryFilter newFilter)
        {
            Debug.Log($"[InventoryPresenter] Filter changed to: {newFilter}");

            _model.currentFilter = newFilter;
            RefreshItemDisplay();
        }

        /// <summary>
        /// Handle sort mode change
        /// </summary>
        private void HandleSortChanged(InventorySortMode newSortMode)
        {
            Debug.Log($"[InventoryPresenter] Sort mode changed to: {newSortMode}");

            _model.currentSortMode = newSortMode;
            RefreshItemDisplay();
        }

        /// <summary>
        /// Handle item selection
        /// </summary>
        private void HandleItemSelected(InventoryItem item)
        {
            Debug.Log($"[InventoryPresenter] Item selected: {item.name}");

            // Show item detail
            bool canEquip = _model.CanEquip(item);
            bool canUse = _model.CanUse(item);

            _view.ShowItemDetail(item, canEquip, canUse);
        }

        /// <summary>
        /// Handle item equip/unequip
        /// </summary>
        private void HandleItemEquipped(InventoryItem item)
        {
            if (item == null) return;

            Debug.Log($"[InventoryPresenter] Equip/Unequip item: {item.name}");

            if (!_model.CanEquip(item))
            {
                _view.ShowFeedback("This item cannot be equipped");
                return;
            }

            // Toggle equip state
            item.isEquipped = !item.isEquipped;

            // If equipping, unequip other items of the same type
            if (item.isEquipped)
            {
                foreach (var otherItem in _model.items)
                {
                    if (otherItem.id != item.id && otherItem.type == item.type && otherItem.isEquipped)
                    {
                        otherItem.isEquipped = false;
                    }
                }

                _view.ShowFeedback($"Equipped {item.name}");
            }
            else
            {
                _view.ShowFeedback($"Unequipped {item.name}");
            }

            // Refresh display
            RefreshItemDisplay();

            // Update detail panel
            _view.ShowItemDetail(item, _model.CanEquip(item), _model.CanUse(item));

            Debug.Log($"[InventoryPresenter] {item.name} is now {(item.isEquipped ? "equipped" : "unequipped")}");
        }

        /// <summary>
        /// Handle item use
        /// </summary>
        private void HandleItemUsed(InventoryItem item)
        {
            if (item == null) return;

            Debug.Log($"[InventoryPresenter] Use item: {item.name}");

            if (!_model.CanUse(item))
            {
                _view.ShowFeedback("This item cannot be used");
                return;
            }

            if (item.quantity <= 0)
            {
                _view.ShowFeedback("No more items to use");
                return;
            }

            // Use item based on type
            if (item.type == ItemType.Consumable)
            {
                UseConsumable(item);
            }
            else if (item.type == ItemType.Special)
            {
                UseSpecialItem(item);
            }

            // Decrease quantity
            item.quantity--;

            // If quantity is 0, remove from inventory (optional)
            if (item.quantity <= 0)
            {
                _model.items.Remove(item);
                _view.HideItemDetail();
            }
            else
            {
                // Update detail panel
                _view.ShowItemDetail(item, _model.CanEquip(item), _model.CanUse(item));
            }

            // Refresh display
            RefreshItemDisplay();
        }

        /// <summary>
        /// Handle back button press
        /// </summary>
        private void HandleBackPressed()
        {
            Debug.Log("[InventoryPresenter] Back button pressed");
            NavigationManager.Instance?.GoBack();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Refresh item display based on current filter and sort
        /// </summary>
        private void RefreshItemDisplay()
        {
            var filteredItems = _model.GetFilteredItems();
            _view.DisplayItems(filteredItems.ToArray());
        }

        /// <summary>
        /// Use a consumable item
        /// </summary>
        private void UseConsumable(InventoryItem item)
        {
            if (item.healAmount > 0)
            {
                // In a real game, this would heal the player
                Debug.Log($"[InventoryPresenter] Healed for {item.healAmount} HP");
                _view.ShowFeedback($"Restored {item.healAmount} HP");
            }
            else if (item.manaAmount > 0)
            {
                // In a real game, this would restore mana
                Debug.Log($"[InventoryPresenter] Restored {item.manaAmount} MP");
                _view.ShowFeedback($"Restored {item.manaAmount} MP");
            }
        }

        /// <summary>
        /// Use a special item
        /// </summary>
        private void UseSpecialItem(InventoryItem item)
        {
            Debug.Log($"[InventoryPresenter] Used special item: {item.name}");

            // In a real game, special items would have various effects
            if (item.name.Contains("Key"))
            {
                _view.ShowFeedback("Used key to unlock something!");
            }
            else if (item.name.Contains("Scroll"))
            {
                _view.ShowFeedback("Teleported to safety!");
            }
            else
            {
                _view.ShowFeedback($"Used {item.name}");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the inventory (reload data)
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[InventoryPresenter] Refresh");

            // Reload player data
            _model.playerData = GameState.Instance.PlayerData;

            // Refresh display
            RefreshItemDisplay();
        }

        /// <summary>
        /// Get current model state (for testing)
        /// </summary>
        public InventoryModel GetModel()
        {
            return _model;
        }

        #endregion
    }
}
