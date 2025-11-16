using UnityEngine;
using Cysharp.Threading.Tasks;
using EnhancedUI;

namespace EnhancedUI.Demo.Screens.Home
{
    /// <summary>
    /// Home Sheet - Main hub tab screen
    /// Inherits from Sheet (tab-based navigation) instead of Page
    /// Part of bottom tab navigation with horizontal swipe support
    /// </summary>
    public class HomeSheet : Sheet, ITabContent
    {
        private HomeScreenView _view;
        private HomeScreenPresenter _presenter;

        #region ITabContent Implementation

        /// <summary>
        /// Tab index for horizontal navigation (0 = leftmost)
        /// Home is the first tab (index 0)
        /// </summary>
        public int TabIndex => 0;

        #endregion

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[HomeSheet] Initialize");

            // Get view component
            _view = GetComponent<HomeScreenView>();

            if (_view == null)
            {
                Debug.LogError("[HomeSheet] HomeScreenView component not found! Please attach HomeScreenView to this GameObject.");
                return;
            }

            // Create presenter
            _presenter = new HomeScreenPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[HomeSheet] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[HomeSheet] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();
            _presenter = null;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[HomeSheet] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[HomeSheet] OnEnable");

            // Show view when sheet becomes active
            if (_view != null)
            {
                _view.Show();
                _view.PlayEnterAnimation();
            }
        }

        private void OnDisable()
        {
            Debug.Log("[HomeSheet] OnDisable");

            // Hide view when sheet becomes inactive
            _view?.Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the home sheet (useful when returning to this tab)
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[HomeSheet] Refresh requested");

            // Recreate presenter to reload data
            if (_view != null && _presenter != null)
            {
                _presenter.Cleanup();
                _presenter = new HomeScreenPresenter(_view);
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
            var view = GetComponent<HomeScreenView>();
            if (view == null)
            {
                Debug.LogWarning($"[HomeSheet] HomeScreenView component is missing on {gameObject.name}. " +
                                "Please add HomeScreenView component to this GameObject.", this);
            }

            // Check for RectTransform
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"[HomeSheet] RectTransform is missing on {gameObject.name}. " +
                              "HomeSheet must be a UI element with RectTransform.", this);
            }

            // Check for CanvasGroup
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning($"[HomeSheet] CanvasGroup is missing on {gameObject.name}. " +
                                "CanvasGroup is required by Sheet base class. Adding automatically.", this);
                gameObject.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Editor-only: Setup default components
        /// </summary>
        private void Reset()
        {
            // Add required components if missing
            if (GetComponent<HomeScreenView>() == null)
            {
                gameObject.AddComponent<HomeScreenView>();
            }

            if (GetComponent<CanvasGroup>() == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }

            Debug.Log("[HomeSheet] Default components added");
        }
#endif
    }
}
