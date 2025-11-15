using UnityEngine;
using System.Collections;
using EnhancedUI.Demo.Screens.HomeContainer;

namespace EnhancedUI.Demo
{
    /// <summary>
    /// Bootstrap script to initialize demo and load first screen
    /// Attach to a GameObject in the scene
    ///
    /// Setup Instructions:
    /// 1. Create empty GameObject in scene named "DemoBootstrap"
    /// 2. Add this script as component
    /// 3. Assign PageContainer and ModalContainer references
    /// 4. Press Play - HomeContainerPage will load automatically with tab sheets
    /// </summary>
    public class DemoBootstrap : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private PageContainer pageContainer;
        [SerializeField] private ModalContainer modalContainer;

        [Header("Settings")]
        [SerializeField] private bool autoLoadHomeScreen = true;
        [SerializeField] private bool initializeGameState = true;
        [SerializeField] private bool loadAsync = false;

        private void Awake()
        {
            Debug.Log("[DemoBootstrap] Awake - Validating setup...");

            // Validate references
            if (pageContainer == null)
            {
                Debug.LogError("[DemoBootstrap] PageContainer is not assigned! Please assign in Inspector.");
            }

            if (modalContainer == null)
            {
                Debug.LogWarning("[DemoBootstrap] ModalContainer is not assigned. Modals will not work.");
            }
        }

        private IEnumerator Start()
        {
            Debug.Log("[DemoBootstrap] Starting demo initialization...");

            // Wait one frame for all Awake methods to complete
            yield return null;

            // Initialize game state singleton
            if (initializeGameState)
            {
                InitializeGameState();
            }

            // Initialize Navigation Manager
            InitializeNavigationManager();

            // Load home screen automatically
            if (autoLoadHomeScreen)
            {
                LoadHomeScreen();
            }

            Debug.Log("[DemoBootstrap] Demo initialization complete!");
        }

        /// <summary>
        /// Initialize game state singleton with default data
        /// </summary>
        private void InitializeGameState()
        {
            Debug.Log("[DemoBootstrap] Initializing game state...");

            // Access singleton - it will auto-initialize on first access
            var gameState = Models.GameState.Instance;

            if (gameState != null && gameState.PlayerData != null)
            {
                Debug.Log($"[DemoBootstrap] Game state initialized successfully!");
                Debug.Log($"  Player: {gameState.PlayerData.playerName}");
                Debug.Log($"  Level: {gameState.PlayerData.playerLevel}");
                Debug.Log($"  Gold: {gameState.PlayerData.gold:N0}");
                Debug.Log($"  Gems: {gameState.PlayerData.gems:N0}");
                Debug.Log($"  Energy: {gameState.PlayerData.energy}/{gameState.PlayerData.maxEnergy}");
            }
            else
            {
                Debug.LogError("[DemoBootstrap] Failed to initialize game state!");
            }
        }

        /// <summary>
        /// Initialize Navigation Manager
        /// </summary>
        private void InitializeNavigationManager()
        {
            // Navigation Manager will auto-initialize as singleton
            var navManager = NavigationManager.Instance;

            if (navManager != null)
            {
                Debug.Log("[DemoBootstrap] Navigation Manager initialized");
            }
            else
            {
                Debug.LogError("[DemoBootstrap] Failed to initialize Navigation Manager!");
            }
        }

        /// <summary>
        /// Load home container page as the initial screen
        /// This will load the main container with 4 tab sheets (Home, Battle, Inventory, Shop)
        /// </summary>
        private void LoadHomeScreen()
        {
            if (pageContainer == null)
            {
                Debug.LogError("[DemoBootstrap] Cannot load HomeContainerPage - PageContainer not assigned!");
                return;
            }

            Debug.Log("[DemoBootstrap] Loading HomeContainerPage with tab sheets...");

            try
            {
                var options = new WindowOptions
                {
                    LoadAsync = loadAsync,
                    Stack = true,
                    PlayAnimation = false, // No animation for first screen
                    OnLoaded = screen =>
                    {
                        Debug.Log("[DemoBootstrap] HomeContainerPage loaded successfully!");
                        Debug.Log("[DemoBootstrap] Tab sheets (Home, Battle, Inventory, Shop) are now available for horizontal swipe navigation");
                    }/*,
                    OnLoadFailed = (resourceKey, exception) =>
                    {
                        Debug.LogError($"[DemoBootstrap] Failed to load HomeContainerPage: {exception.Message}");
                        Debug.LogException(exception);
                    }*/
                };

                pageContainer.Push<HomeContainerPage>(ScreenKeys.HomeContainer, options);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[DemoBootstrap] Failed to push HomeScreen: {e.Message}");
                Debug.LogException(e);
            }
        }

        #if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Validate setup
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            // Check for PageContainer
            if (pageContainer == null)
            {
                pageContainer = FindObjectOfType<PageContainer>();
                if (pageContainer != null)
                {
                    Debug.Log("[DemoBootstrap] Auto-assigned PageContainer");
                }
            }

            // Check for ModalContainer
            if (modalContainer == null)
            {
                modalContainer = FindObjectOfType<ModalContainer>();
                if (modalContainer != null)
                {
                    Debug.Log("[DemoBootstrap] Auto-assigned ModalContainer");
                }
            }
        }

        /// <summary>
        /// Editor-only: Quick setup menu
        /// </summary>
        [ContextMenu("Auto-Find Containers")]
        private void AutoFindContainers()
        {
            pageContainer = FindObjectOfType<PageContainer>();
            modalContainer = FindObjectOfType<ModalContainer>();

            if (pageContainer != null)
                Debug.Log("[DemoBootstrap] Found and assigned PageContainer");
            else
                Debug.LogWarning("[DemoBootstrap] PageContainer not found in scene");

            if (modalContainer != null)
                Debug.Log("[DemoBootstrap] Found and assigned ModalContainer");
            else
                Debug.LogWarning("[DemoBootstrap] ModalContainer not found in scene");
        }

        [ContextMenu("Print Setup Status")]
        private void PrintSetupStatus()
        {
            Debug.Log("=== DemoBootstrap Setup Status ===");
            Debug.Log($"PageContainer: {(pageContainer != null ? "✓" : "✗")}");
            Debug.Log($"ModalContainer: {(modalContainer != null ? "✓" : "✗")}");
            Debug.Log($"Auto Load Home: {autoLoadHomeScreen}");
            Debug.Log($"Initialize GameState: {initializeGameState}");
            Debug.Log($"Load Async: {loadAsync}");
            Debug.Log("==================================");
        }
        #endif
    }
}
