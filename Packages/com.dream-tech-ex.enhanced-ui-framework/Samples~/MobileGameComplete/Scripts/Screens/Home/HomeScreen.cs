using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EnhancedUI.Demo.Screens.Home
{
    /// <summary>
    /// Home Screen - Main hub screen of the game
    /// Inherits from Page and connects MVP pattern with Enhanced UI Framework
    /// </summary>
    public class HomeScreen : Page
    {
        private HomeScreenView _view;
        private HomeScreenPresenter _presenter;

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[HomeScreen] Initialize");

            // Get view component
            _view = GetComponent<HomeScreenView>();

            if (_view == null)
            {
                Debug.LogError("[HomeScreen] HomeScreenView component not found! Please attach HomeScreenView to this GameObject.");
                return;
            }

            // Create presenter
            _presenter = new HomeScreenPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[HomeScreen] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[HomeScreen] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();
            _presenter = null;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[HomeScreen] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[HomeScreen] OnEnable");

            // Show view when screen becomes active
            if (_view != null)
            {
                _view.Show();
                _view.PlayEnterAnimation();
            }
        }

        private void OnDisable()
        {
            Debug.Log("[HomeScreen] OnDisable");

            // Hide view when screen becomes inactive
            _view?.Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the home screen (useful when returning from other screens)
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[HomeScreen] Refresh requested");

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
                Debug.LogWarning($"[HomeScreen] HomeScreenView component is missing on {gameObject.name}. " +
                                "Please add HomeScreenView component to this GameObject.", this);
            }

            // Check for RectTransform
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"[HomeScreen] RectTransform is missing on {gameObject.name}. " +
                              "HomeScreen must be a UI element with RectTransform.", this);
            }

            // Check for CanvasGroup
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning($"[HomeScreen] CanvasGroup is missing on {gameObject.name}. " +
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
            if (GetComponent<HomeScreenView>() == null)
            {
                gameObject.AddComponent<HomeScreenView>();
            }

            if (GetComponent<CanvasGroup>() == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }

            Debug.Log("[HomeScreen] Default components added");
        }
#endif
    }
}
