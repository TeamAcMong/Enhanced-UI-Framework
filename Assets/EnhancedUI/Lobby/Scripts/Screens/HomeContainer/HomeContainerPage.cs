using UnityEngine;
using Cysharp.Threading.Tasks;
using EnhancedUI;

namespace EnhancedUI.Demo.Screens.HomeContainer
{
    /// <summary>
    /// Home Container Page - Contains SheetContainer for bottom tab navigation
    /// Manages 4 tab sheets: Home, Battle, Inventory, Shop
    /// Supports horizontal swipe navigation between tabs
    /// </summary>
    public class HomeContainerPage : Page
    {
        [Header("Tab Container")]
        [SerializeField] private SheetContainer tabContainer;

        [Header("Settings")]
        [SerializeField] private string initialTabSheetId = "HomeSheet";
        [SerializeField] private bool playInitialAnimation = false;

        #region Enhanced UI Lifecycle

        public override async UniTask Initialize()
        {
            Debug.Log("[HomeContainerPage] Initialize");

            if (tabContainer == null)
            {
                Debug.LogError("[HomeContainerPage] SheetContainer is not assigned!");
                return;
            }

            // Register all tab sheets (preload them)
            await RegisterAllTabs();

            // Show initial tab
            await tabContainer.Show(initialTabSheetId, playInitialAnimation);

            // Base initialization
            await base.Initialize();

            Debug.Log("[HomeContainerPage] Initialization complete");
        }

        public override async UniTask Cleanup()
        {
            Debug.Log("[HomeContainerPage] Cleanup");

            // Base cleanup
            await base.Cleanup();

            Debug.Log("[HomeContainerPage] Cleanup complete");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Register all tab sheets with the container
        /// </summary>
        private async UniTask RegisterAllTabs()
        {
            Debug.Log("[HomeContainerPage] Registering all tab sheets...");

            // Register Home tab
            var homeOptions = new WindowOptions
            {
                OnLoaded = screen => Debug.Log($"[HomeContainerPage] Home sheet registered")
            };
            await tabContainer.Register<Sheet>(ScreenKeys.HomeSheet, homeOptions).Task;

            // Register Battle tab
            var battleOptions = new WindowOptions
            {
                OnLoaded = screen => Debug.Log($"[HomeContainerPage] Battle sheet registered")
            };
            await tabContainer.Register<Sheet>(ScreenKeys.BattleSheet, battleOptions).Task;

            // Register Inventory tab
            var inventoryOptions = new WindowOptions
            {
                OnLoaded = screen => Debug.Log($"[HomeContainerPage] Inventory sheet registered")
            };
            await tabContainer.Register<Sheet>(ScreenKeys.InventorySheet, inventoryOptions).Task;

            // Register Shop tab
            var shopOptions = new WindowOptions
            {
                OnLoaded = screen => Debug.Log($"[HomeContainerPage] Shop sheet registered")
            };
            await tabContainer.Register<Sheet>(ScreenKeys.ShopSheet, shopOptions).Task;

            Debug.Log("[HomeContainerPage] All tab sheets registered");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Switch to specific tab by ID
        /// </summary>
        public async UniTask SwitchToTab(string tabSheetId, bool playAnimation = true)
        {
            if (tabContainer == null)
            {
                Debug.LogError("[HomeContainerPage] SheetContainer is not assigned!");
                return;
            }

            if (tabContainer.IsInTransition)
            {
                Debug.LogWarning("[HomeContainerPage] Sheet container is in transition, ignoring switch request");
                return;
            }

            if (tabContainer.ActiveSheet != null && tabContainer.ActiveSheet.Identifier == tabSheetId)
            {
                Debug.Log($"[HomeContainerPage] Already on tab: {tabSheetId}");
                return;
            }

            Debug.Log($"[HomeContainerPage] Switching to tab: {tabSheetId}");
            await tabContainer.Show(tabSheetId, playAnimation).Task;
        }

        /// <summary>
        /// Get the currently active tab sheet ID
        /// </summary>
        public string GetActiveTabId()
        {
            return tabContainer?.ActiveSheet?.Identifier;
        }

        /// <summary>
        /// Check if container is currently transitioning
        /// </summary>
        public bool IsInTransition()
        {
            return tabContainer != null && tabContainer.IsInTransition;
        }

        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Validate setup
        /// </summary>
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            // Check for SheetContainer
            if (tabContainer == null)
            {
                tabContainer = GetComponentInChildren<SheetContainer>();
                if (tabContainer != null)
                {
                    Debug.Log("[HomeContainerPage] Auto-found SheetContainer");
                }
                else
                {
                    Debug.LogWarning("[HomeContainerPage] SheetContainer not found! Please assign or add to hierarchy.");
                }
            }
        }

        [ContextMenu("Print Tab Status")]
        private void PrintTabStatus()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[HomeContainerPage] This only works in Play Mode");
                return;
            }

            Debug.Log("=== Home Container Tab Status ===");
            Debug.Log($"Active Tab: {tabContainer?.ActiveSheet?.Identifier ?? "None"}");
            Debug.Log($"Is In Transition: {IsInTransition()}");
            Debug.Log($"Sheet Count: {tabContainer?.SheetCount ?? 0}");
            Debug.Log("=================================");
        }
#endif
    }
}
