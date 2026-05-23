using UnityEngine;
using Cysharp.Threading.Tasks;
using EnhancedUI;

namespace EnhancedUI.Demo.Screens.Shop
{
    /// <summary>
    /// Shop Sheet - Shop tab screen
    /// Inherits from Sheet (tab-based navigation) instead of Page
    /// Part of bottom tab navigation with horizontal swipe support
    /// </summary>
    public class ShopSheet : Sheet, ITabContent
    {
        private ShopView _view;
        private ShopPresenter _presenter;

        #region ITabContent Implementation

        /// <summary>
        /// Tab index for horizontal navigation
        /// Shop is the fourth tab (index 3)
        /// </summary>
        public int TabIndex => 3;

        #endregion

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[ShopSheet] Initialize");

            // Get view component
            _view = GetComponent<ShopView>();

            if (_view == null)
            {
                Debug.LogError("[ShopSheet] ShopView component not found!");
                return;
            }

            // Create presenter
            _presenter = new ShopPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[ShopSheet] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[ShopSheet] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();
            _presenter = null;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[ShopSheet] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[ShopSheet] OnEnable");

            if (_view != null)
            {
                _view.Show();
            }
        }

        private void OnDisable()
        {
            Debug.Log("[ShopSheet] OnDisable");
            _view?.Hide();
        }

        #endregion

        #region Public Methods

        public void Refresh()
        {
            Debug.Log("[ShopSheet] Refresh requested");

            if (_view != null && _presenter != null)
            {
                _presenter.Cleanup();
                _presenter = new ShopPresenter(_view);
                _view.SetPresenter(_presenter);
            }
        }

        #endregion
    }
}
