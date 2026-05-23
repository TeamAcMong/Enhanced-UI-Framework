using UnityEngine;
using Cysharp.Threading.Tasks;

namespace EnhancedUI.Demo.Screens.RPGStage
{
    /// <summary>
    /// RPG Stage Screen - RPG-style stage with character selection and boss preview
    /// Designed for landscape orientation with horizontal layout
    /// Inherits from Page and connects MVP pattern with Enhanced UI Framework
    /// </summary>
    public class RPGStageScreen : Page
    {
        private RPGStageView _view;
        private RPGStagePresenter _presenter;

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[RPGStageScreen] Initialize");

            // Get view component
            _view = GetComponent<RPGStageView>();

            if (_view == null)
            {
                Debug.LogError("[RPGStageScreen] RPGStageView component not found! Please attach RPGStageView to this GameObject.");
                return;
            }

            // Create presenter
            _presenter = new RPGStagePresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            await base.Initialize();

            Debug.Log("[RPGStageScreen] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[RPGStageScreen] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();
            _presenter = null;

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[RPGStageScreen] Cleanup complete");
        }

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            Debug.Log("[RPGStageScreen] OnEnable");

            // Show view when screen becomes active
            _view?.Show();

            // Force landscape orientation for this screen
            SetLandscapeOrientation();
        }

        private void OnDisable()
        {
            Debug.Log("[RPGStageScreen] OnDisable");

            // Hide view when screen becomes inactive
            _view?.Hide();

            // Restore auto-rotation when leaving this screen
            RestoreAutoRotation();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the RPG stage screen
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[RPGStageScreen] Refresh requested");

            // Recreate presenter to reload data
            if (_view != null && _presenter != null)
            {
                _presenter.Cleanup();
                _presenter = new RPGStagePresenter(_view);
                _view.SetPresenter(_presenter);
            }
        }

        /// <summary>
        /// Get current party statistics
        /// </summary>
        public string GetPartyStats()
        {
            return _presenter?.GetPartyStats() ?? "No data";
        }

        #endregion

        #region Orientation Management

        /// <summary>
        /// Force landscape orientation for this screen
        /// </summary>
        private void SetLandscapeOrientation()
        {
            #if !UNITY_EDITOR
            // Only force orientation on actual devices, not in editor
            UnityEngine.Screen.orientation = ScreenOrientation.LandscapeLeft;
            UnityEngine.Screen.autorotateToPortrait = false;
            UnityEngine.Screen.autorotateToPortraitUpsideDown = false;
            UnityEngine.Screen.autorotateToLandscapeLeft = true;
            UnityEngine.Screen.autorotateToLandscapeRight = true;
            #endif

            Debug.Log("[RPGStageScreen] Set landscape orientation");
        }

        /// <summary>
        /// Restore auto-rotation when leaving screen
        /// </summary>
        private void RestoreAutoRotation()
        {
            #if !UNITY_EDITOR
            // Restore default orientation settings
            UnityEngine.Screen.orientation = ScreenOrientation.AutoRotation;
            UnityEngine.Screen.autorotateToPortrait = true;
            UnityEngine.Screen.autorotateToPortraitUpsideDown = false;
            UnityEngine.Screen.autorotateToLandscapeLeft = true;
            UnityEngine.Screen.autorotateToLandscapeRight = true;
            #endif

            Debug.Log("[RPGStageScreen] Restored auto-rotation");
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
            var view = GetComponent<RPGStageView>();
            if (view == null)
            {
                Debug.LogWarning($"[RPGStageScreen] RPGStageView component is missing on {gameObject.name}. " +
                                "Please add RPGStageView component to this GameObject.", this);
            }

            // Check for RectTransform
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                Debug.LogError($"[RPGStageScreen] RectTransform is missing on {gameObject.name}. " +
                              "RPGStageScreen must be a UI element with RectTransform.", this);
            }

            // Check for CanvasGroup
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning($"[RPGStageScreen] CanvasGroup is missing on {gameObject.name}. " +
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
            if (GetComponent<RPGStageView>() == null)
            {
                gameObject.AddComponent<RPGStageView>();
            }

            if (GetComponent<CanvasGroup>() == null)
            {
                gameObject.AddComponent<CanvasGroup>();
            }

            Debug.Log("[RPGStageScreen] Default components added");
        }

        /// <summary>
        /// Editor-only: Test party selection
        /// </summary>
        [ContextMenu("Test Auto-Select Party")]
        private void TestAutoSelectParty()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[RPGStageScreen] This test only works in Play Mode");
                return;
            }

            if (_presenter != null)
            {
                Debug.Log("[RPGStageScreen] Testing auto-select...");
                Debug.Log(_presenter.GetPartyStats());
            }
        }
#endif
    }
}
