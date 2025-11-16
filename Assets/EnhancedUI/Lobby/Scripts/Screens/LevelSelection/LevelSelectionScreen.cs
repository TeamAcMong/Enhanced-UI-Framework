using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EnhancedUI.Demo.Screens.LevelSelection
{
    /// <summary>
    /// Level Selection Screen - Shows chapters and levels for player to choose
    /// Inherits from Page and connects MVP pattern with Enhanced UI Framework
    /// </summary>
    public class LevelSelectionScreen : Page
    {
        private LevelSelectionView _view;
        private LevelSelectionPresenter _presenter;

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[LevelSelectionScreen] Initialize");

            // Get view component
            _view = GetComponent<LevelSelectionView>();

            if (_view == null)
            {
                Debug.LogError("[LevelSelectionScreen] LevelSelectionView component not found! Please attach LevelSelectionView to this GameObject.");
                return;
            }

            // Create presenter
            _presenter = new LevelSelectionPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[LevelSelectionScreen] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[LevelSelectionScreen] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();
            _presenter = null;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[LevelSelectionScreen] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[LevelSelectionScreen] OnEnable");

            // Show view when screen becomes active
            _view?.Show();
        }

        private void OnDisable()
        {
            Debug.Log("[LevelSelectionScreen] OnDisable");

            // Hide view when screen becomes inactive
            _view?.Hide();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the level selection screen (useful when returning after completing a level)
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[LevelSelectionScreen] Refresh requested");

            // Recreate presenter to reload data
            if (_view != null && _presenter != null)
            {
                _presenter.Cleanup();
                _presenter = new LevelSelectionPresenter(_view);
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
            var view = GetComponent<LevelSelectionView>();
            if (view == null)
            {
                Debug.LogWarning($"[LevelSelectionScreen] LevelSelectionView component is missing on {gameObject.name}. " +
                                "Please add LevelSelectionView component to this GameObject.", this);
            }

            // Check for RectTransform
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"[LevelSelectionScreen] RectTransform is missing on {gameObject.name}. " +
                              "LevelSelectionScreen must be a UI element with RectTransform.", this);
            }

            // Check for CanvasGroup
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning($"[LevelSelectionScreen] CanvasGroup is missing on {gameObject.name}. " +
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
            if (GetComponent<LevelSelectionView>() == null)
            {
                gameObject.AddComponent<LevelSelectionView>();
            }

            if (GetComponent<CanvasGroup>() == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }

            Debug.Log("[LevelSelectionScreen] Default components added");
        }
#endif
    }
}
