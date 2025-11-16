using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EnhancedUI.Demo.Screens.Gameplay
{
    /// <summary>
    /// Gameplay Screen - Active gameplay with simulated game logic
    /// Inherits from Page and connects MVP pattern with Enhanced UI Framework
    /// </summary>
    public class GameplayScreen : Page
    {
        private GameplayView _view;
        private GameplayPresenter _presenter;

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[GameplayScreen] Initialize");

            // Get view component
            _view = GetComponent<GameplayView>();

            if (_view == null)
            {
                Debug.LogError("[GameplayScreen] GameplayView component not found! Please attach GameplayView to this GameObject.");
                return;
            }

            // Create presenter
            _presenter = new GameplayPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[GameplayScreen] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[GameplayScreen] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();
            _presenter = null;

            // Ensure time scale is restored
            Time.timeScale = 1f;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[GameplayScreen] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[GameplayScreen] OnEnable");

            // Show view when screen becomes active
            _view?.Show();
        }

        private void OnDisable()
        {
            Debug.Log("[GameplayScreen] OnDisable");

            // Hide view when screen becomes inactive
            _view?.Hide();

            // Ensure time scale is restored
            Time.timeScale = 1f;
        }

        private void Update()
        {
            // Update game logic every frame
            if (_presenter != null && Application.isPlaying)
            {
                _presenter.Update(Time.deltaTime);
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
            var view = GetComponent<GameplayView>();
            if (view == null)
            {
                Debug.LogWarning($"[GameplayScreen] GameplayView component is missing on {gameObject.name}. " +
                                "Please add GameplayView component to this GameObject.", this);
            }

            // Check for RectTransform
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"[GameplayScreen] RectTransform is missing on {gameObject.name}. " +
                              "GameplayScreen must be a UI element with RectTransform.", this);
            }

            // Check for CanvasGroup
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning($"[GameplayScreen] CanvasGroup is missing on {gameObject.name}. " +
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
            if (GetComponent<GameplayView>() == null)
            {
                gameObject.AddComponent<GameplayView>();
            }

            if (GetComponent<CanvasGroup>() == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }

            Debug.Log("[GameplayScreen] Default components added");
        }
#endif
    }
}
