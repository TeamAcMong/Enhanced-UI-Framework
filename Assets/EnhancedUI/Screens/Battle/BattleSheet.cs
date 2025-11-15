using UnityEngine;
using Cysharp.Threading.Tasks;
using EnhancedUI;

namespace EnhancedUI.Demo.Screens.Battle
{
    /// <summary>
    /// Battle Sheet - Battle arena tab screen
    /// Inherits from Sheet (tab-based navigation) instead of Page
    /// Part of bottom tab navigation with horizontal swipe support
    /// </summary>
    public class BattleSheet : Sheet, ITabContent
    {
        private BattleArenaView _view;
        private BattleArenaPresenter _presenter;

        #region ITabContent Implementation

        /// <summary>
        /// Tab index for horizontal navigation
        /// Battle is the second tab (index 1)
        /// </summary>
        public int TabIndex => 1;

        #endregion

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[BattleSheet] Initialize");

            // Get view component
            _view = GetComponent<BattleArenaView>();

            if (_view == null)
            {
                Debug.LogError("[BattleSheet] BattleArenaView component not found!");
                return;
            }

            // Create presenter
            _presenter = new BattleArenaPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[BattleSheet] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[BattleSheet] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();
            _presenter = null;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[BattleSheet] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[BattleSheet] OnEnable");

            if (_view != null)
            {
                _view.Show();
            }
        }

        private void OnDisable()
        {
            Debug.Log("[BattleSheet] OnDisable");
            _view?.Hide();
        }

        #endregion

        #region Public Methods

        public void Refresh()
        {
            Debug.Log("[BattleSheet] Refresh requested");

            if (_view != null && _presenter != null)
            {
                _presenter.Cleanup();
                _presenter = new BattleArenaPresenter(_view);
                _view.SetPresenter(_presenter);
            }
        }

        #endregion
    }
}
