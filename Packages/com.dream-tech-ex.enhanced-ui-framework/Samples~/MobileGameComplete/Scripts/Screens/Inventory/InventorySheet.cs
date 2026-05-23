using UnityEngine;
using Cysharp.Threading.Tasks;
using EnhancedUI;

namespace EnhancedUI.Demo.Screens.Inventory
{
    /// <summary>
    /// Inventory Sheet - Inventory tab screen
    /// Inherits from Sheet (tab-based navigation) instead of Page
    /// Part of bottom tab navigation with horizontal swipe support
    /// </summary>
    public class InventorySheet : Sheet, ITabContent
    {
        private InventoryView _view;
        private InventoryPresenter _presenter;

        #region ITabContent Implementation

        /// <summary>
        /// Tab index for horizontal navigation
        /// Inventory is the third tab (index 2)
        /// </summary>
        public int TabIndex => 2;

        #endregion

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[InventorySheet] Initialize");

            // Get view component
            _view = GetComponent<InventoryView>();

            if (_view == null)
            {
                Debug.LogError("[InventorySheet] InventoryView component not found!");
                return;
            }

            // Create presenter
            _presenter = new InventoryPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[InventorySheet] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[InventorySheet] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();
            _presenter = null;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[InventorySheet] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[InventorySheet] OnEnable");

            if (_view != null)
            {
                _view.Show();
            }
        }

        private void OnDisable()
        {
            Debug.Log("[InventorySheet] OnDisable");
            _view?.Hide();
        }

        #endregion

        #region Public Methods

        public void Refresh()
        {
            Debug.Log("[InventorySheet] Refresh requested");

            if (_view != null && _presenter != null)
            {
                _presenter.Cleanup();
                _presenter = new InventoryPresenter(_view);
                _view.SetPresenter(_presenter);
            }
        }

        #endregion
    }
}
