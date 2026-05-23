using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EnhancedUI.Demo.Screens.Shop
{
    /// <summary>
    /// Shop Screen - Shop with gem/gold packages and special offers
    /// Inherits from Page and connects MVP pattern with Enhanced UI Framework
    /// </summary>
    public class ShopScreen : Page
    {
        private ShopView _view;
        private ShopPresenter _presenter;

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[ShopScreen] Initialize");

            // Get view component
            _view = GetComponent<ShopView>();

            if (_view == null)
            {
                Debug.LogError("[ShopScreen] ShopView component not found! Please attach ShopView to this GameObject.");
                return;
            }

            // Create presenter
            _presenter = new ShopPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[ShopScreen] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[ShopScreen] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();
            _presenter = null;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[ShopScreen] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[ShopScreen] OnEnable");

            // Show view when screen becomes active
            _view?.Show();
        }

        private void OnDisable()
        {
            Debug.Log("[ShopScreen] OnDisable");

            // Hide view when screen becomes inactive
            _view?.Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the shop screen
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[ShopScreen] Refresh requested");

            // Recreate presenter to reload data
            if (_view != null && _presenter != null)
            {
                _presenter.Cleanup();
                _presenter = new ShopPresenter(_view);
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
            var view = GetComponent<ShopView>();
            if (view == null)
            {
                Debug.LogWarning($"[ShopScreen] ShopView component is missing on {gameObject.name}. " +
                                "Please add ShopView component to this GameObject.", this);
            }

            // Check for RectTransform
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"[ShopScreen] RectTransform is missing on {gameObject.name}. " +
                              "ShopScreen must be a UI element with RectTransform.", this);
            }

            // Check for CanvasGroup
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning($"[ShopScreen] CanvasGroup is missing on {gameObject.name}. " +
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
            if (GetComponent<ShopView>() == null)
            {
                gameObject.AddComponent<ShopView>();
            }

            if (GetComponent<CanvasGroup>() == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }

            Debug.Log("[ShopScreen] Default components added");
        }
#endif
    }
}
