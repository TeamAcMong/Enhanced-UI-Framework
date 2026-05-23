using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EnhancedUI.Demo.Screens.Settings
{
    /// <summary>
    /// Settings Modal - Settings dialog with audio, notifications, gameplay, and account tabs
    /// Inherits from Modal and connects MVP pattern with Enhanced UI Framework
    /// Note: This uses Modal base class instead of Page since it's a popup overlay
    /// </summary>
    public class SettingsModal : Modal
    {
        private SettingsView _view;
        private SettingsPresenter _presenter;

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[SettingsModal] Initialize");

            // Get view component
            _view = GetComponent<SettingsView>();

            if (_view == null)
            {
                Debug.LogError("[SettingsModal] SettingsView component not found! Please attach SettingsView to this GameObject.");
                return;
            }

            // Create presenter
            _presenter = new SettingsPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[SettingsModal] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[SettingsModal] Cleanup");

            // Cleanup presenter (this will save settings)
            _presenter?.Cleanup();
            _presenter = null;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[SettingsModal] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[SettingsModal] OnEnable");

            // Show view when modal becomes active
            _view?.Show();
        }

        private void OnDisable()
        {
            Debug.Log("[SettingsModal] OnDisable");

            // Hide view when modal becomes inactive
            _view?.Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the settings modal
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[SettingsModal] Refresh requested");

            // Recreate presenter to reload data
            if (_view != null && _presenter != null)
            {
                _presenter.Cleanup();
                _presenter = new SettingsPresenter(_view);
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
            var view = GetComponent<SettingsView>();
            if (view == null)
            {
                Debug.LogWarning($"[SettingsModal] SettingsView component is missing on {gameObject.name}. " +
                                "Please add SettingsView component to this GameObject.", this);
            }

            // Check for RectTransform
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"[SettingsModal] RectTransform is missing on {gameObject.name}. " +
                              "SettingsModal must be a UI element with RectTransform.", this);
            }

            // Check for CanvasGroup
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning($"[SettingsModal] CanvasGroup is missing on {gameObject.name}. " +
                                "CanvasGroup is required by Modal base class. Adding automatically.", this);
                gameObject.AddComponent<CanvasGroup>();
            }
        }

        /// <summary>
        /// Editor-only: Setup default components
        /// </summary>
        private void Reset()
        {
            // Add required components if missing
            if (GetComponent<SettingsView>() == null)
            {
                gameObject.AddComponent<SettingsView>();
            }

            if (GetComponent<CanvasGroup>() == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }

            Debug.Log("[SettingsModal] Default components added");
        }
#endif
    }
}
