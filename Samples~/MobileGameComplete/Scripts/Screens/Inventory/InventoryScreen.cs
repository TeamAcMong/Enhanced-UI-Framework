using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EnhancedUI.Demo.Screens.Inventory
{
    /// <summary>
    /// Inventory Screen - Player inventory with items, sorting, and filtering
    /// Inherits from Page and connects MVP pattern with Enhanced UI Framework
    /// </summary>
    public class InventoryScreen : Page
    {
        private InventoryView _view;
        private InventoryPresenter _presenter;

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[InventoryScreen] Initialize");

            // Get view component
            _view = GetComponent<InventoryView>();

            if (_view == null)
            {
                Debug.LogError("[InventoryScreen] InventoryView component not found! Please attach InventoryView to this GameObject.");
                return;
            }

            // Create presenter
            _presenter = new InventoryPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[InventoryScreen] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[InventoryScreen] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();
            _presenter = null;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[InventoryScreen] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[InventoryScreen] OnEnable");

            // Show view when screen becomes active
            _view?.Show();
        }

        private void OnDisable()
        {
            Debug.Log("[InventoryScreen] OnDisable");

            // Hide view when screen becomes inactive
            _view?.Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the inventory screen
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[InventoryScreen] Refresh requested");

            // Recreate presenter to reload data
            if (_view != null && _presenter != null)
            {
                _presenter.Cleanup();
                _presenter = new InventoryPresenter(_view);
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
            var view = GetComponent<InventoryView>();
            if (view == null)
            {
                Debug.LogWarning($"[InventoryScreen] InventoryView component is missing on {gameObject.name}. " +
                                "Please add InventoryView component to this GameObject.", this);
            }

            // Check for RectTransform
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"[InventoryScreen] RectTransform is missing on {gameObject.name}. " +
                              "InventoryScreen must be a UI element with RectTransform.", this);
            }

            // Check for CanvasGroup
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning($"[InventoryScreen] CanvasGroup is missing on {gameObject.name}. " +
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
            if (GetComponent<InventoryView>() == null)
            {
                gameObject.AddComponent<InventoryView>();
            }

            if (GetComponent<CanvasGroup>() == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }

            Debug.Log("[InventoryScreen] Default components added");
        }
#endif
    }
}
