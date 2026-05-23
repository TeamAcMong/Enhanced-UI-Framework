using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EnhancedUI.Demo.Screens.Battle
{
    /// <summary>
    /// Battle Arena Screen - Battle type selection and leaderboard
    /// Inherits from Page and connects MVP pattern with Enhanced UI Framework
    /// </summary>
    public class BattleArenaScreen : Page
    {
        private BattleArenaView _view;
        private BattleArenaPresenter _presenter;

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[BattleArenaScreen] Initialize");

            // Get view component
            _view = GetComponent<BattleArenaView>();

            if (_view == null)
            {
                Debug.LogError("[BattleArenaScreen] BattleArenaView component not found! Please attach BattleArenaView to this GameObject.");
                return;
            }

            // Create presenter
            _presenter = new BattleArenaPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[BattleArenaScreen] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[BattleArenaScreen] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();
            _presenter = null;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[BattleArenaScreen] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[BattleArenaScreen] OnEnable");

            // Show view when screen becomes active
            _view?.Show();
        }

        private void OnDisable()
        {
            Debug.Log("[BattleArenaScreen] OnDisable");

            // Hide view when screen becomes inactive
            _view?.Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the battle arena screen
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[BattleArenaScreen] Refresh requested");

            // Recreate presenter to reload data
            if (_view != null && _presenter != null)
            {
                _presenter.Cleanup();
                _presenter = new BattleArenaPresenter(_view);
                _view.SetPresenter(_presenter);
            }
        }

        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Validate setup
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            // Check for required components
            var view = GetComponent<BattleArenaView>();
            if (view == null)
            {
                Debug.LogWarning($"[BattleArenaScreen] BattleArenaView component is missing on {gameObject.name}. " +
                                "Please add BattleArenaView component to this GameObject.", this);
            }

            // Check for RectTransform
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"[BattleArenaScreen] RectTransform is missing on {gameObject.name}. " +
                              "BattleArenaScreen must be a UI element with RectTransform.", this);
            }

            // Check for CanvasGroup
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning($"[BattleArenaScreen] CanvasGroup is missing on {gameObject.name}. " +
                                "CanvasGroup is required by Screen base class. Adding automatically.", this);
                gameObject.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Editor-only: Setup default components
        /// </summary>
        private void Reset()
        {
            // Add required components if missing
            if (GetComponent<BattleArenaView>() == null)
            {
                gameObject.AddComponent<BattleArenaView>();
            }

            if (GetComponent<CanvasGroup>() == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }

            Debug.Log("[BattleArenaScreen] Default components added");
        }
#endif
    }
}
