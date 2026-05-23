using UnityEngine;
using EnhancedUI;
using Cysharp.Threading.Tasks;
using EnhancedUI.Demo.Screens.HomeContainer;
using EnhancedUI.Demo.Screens.LevelSelection;
using EnhancedUI.Demo.Screens.Gameplay;
using EnhancedUI.Demo.Screens.RPGStage;
using EnhancedUI.Demo.Screens.Settings;

namespace EnhancedUI.Demo
{
    /// <summary>
    /// Centralized navigation manager for the demo
    /// Singleton pattern for easy access from any screen
    ///
    /// Usage:
    /// - NavigationManager.Instance.GoToShop();
    /// - NavigationManager.Instance.ShowSettings();
    /// - NavigationManager.Instance.GoBack();
    ///
    /// Setup Instructions:
    /// 1. Attach to GameObject in scene named "NavigationManager"
    /// 2. Assign PageContainer and ModalContainer references in Inspector
    /// 3. Call navigation methods from anywhere in code
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        #region Singleton

        private static NavigationManager _instance;
        public static NavigationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<NavigationManager>();

                    if (_instance == null)
                    {
                        Debug.LogWarning("[NavigationManager] No instance found in scene. Creating new one.");
                        var go = new GameObject("NavigationManager");
                        _instance = go.AddComponent<NavigationManager>();
                    }
                }
                return _instance;
            }
        }

        #endregion

        [Header("References")]
        [SerializeField] private PageContainer pageContainer;
        [SerializeField] private ModalContainer modalContainer;

        [Header("Settings")]
        [SerializeField] private bool enableTransitions = true;
        [SerializeField] private bool enableDebugLogs = true;
        [SerializeField] private bool loadAsync = false;
        [SerializeField] private bool stackScreens = true;

        private void Awake()
        {
            // Singleton pattern
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                Debug.Log("[NavigationManager] Initialized as singleton");
            }
            else if (_instance != this)
            {
                Debug.LogWarning("[NavigationManager] Duplicate instance detected. Destroying...");
                Destroy(gameObject);
                return;
            }

            // Validate references
            ValidateSetup();
        }

        private void ValidateSetup()
        {
            if (pageContainer == null)
            {
                pageContainer = FindObjectOfType<PageContainer>();
                if (pageContainer == null)
                {
                    Debug.LogError("[NavigationManager] PageContainer not found! Please assign or add to scene.");
                }
                else
                {
                    Debug.Log("[NavigationManager] Auto-found PageContainer");
                }
            }

            if (modalContainer == null)
            {
                modalContainer = FindObjectOfType<ModalContainer>();
                if (modalContainer == null)
                {
                    Debug.LogWarning("[NavigationManager] ModalContainer not found. Modals will not work.");
                }
                else
                {
                    Debug.Log("[NavigationManager] Auto-found ModalContainer");
                }
            }
        }

        #region Navigation Methods

        // Reference to current HomeContainerPage (cached for tab switching)
        private HomeContainerPage _cachedHomeContainer;

        /// <summary>
        /// Navigate to Home Container (main page with tab sheets)
        /// This is the main entry point - loads the container with 4 tabs
        /// </summary>
        public void GoToHome()
        {
            PushScreen<HomeContainerPage>(ScreenKeys.HomeContainer);
        }

        /// <summary>
        /// Switch to Home tab (within HomeContainer)
        /// Uses SheetContainer instead of PageContainer
        /// </summary>
        public async void SwitchToHomeTab()
        {
            var homeContainer = GetHomeContainerPage();
            if (homeContainer != null)
            {
                await homeContainer.SwitchToTab(ScreenKeys.HomeSheet, enableTransitions);
            }
            else
            {
                Debug.LogWarning("[NavigationManager] HomeContainer not active, navigating to it first");
                GoToHome();
            }
        }

        /// <summary>
        /// Switch to Battle tab (within HomeContainer)
        /// Uses SheetContainer instead of PageContainer
        /// </summary>
        public async void SwitchToBattleTab()
        {
            var homeContainer = GetHomeContainerPage();
            if (homeContainer != null)
            {
                await homeContainer.SwitchToTab(ScreenKeys.BattleSheet, enableTransitions);
            }
            else
            {
                Debug.LogWarning("[NavigationManager] HomeContainer not active, navigating to it first");
                GoToHome();
            }
        }

        /// <summary>
        /// Switch to Inventory tab (within HomeContainer)
        /// Uses SheetContainer instead of PageContainer
        /// </summary>
        public async void SwitchToInventoryTab()
        {
            var homeContainer = GetHomeContainerPage();
            if (homeContainer != null)
            {
                await homeContainer.SwitchToTab(ScreenKeys.InventorySheet, enableTransitions);
            }
            else
            {
                Debug.LogWarning("[NavigationManager] HomeContainer not active, navigating to it first");
                GoToHome();
            }
        }

        /// <summary>
        /// Switch to Shop tab (within HomeContainer)
        /// Uses SheetContainer instead of PageContainer
        /// </summary>
        public async void SwitchToShopTab()
        {
            var homeContainer = GetHomeContainerPage();
            if (homeContainer != null)
            {
                await homeContainer.SwitchToTab(ScreenKeys.ShopSheet, enableTransitions);
            }
            else
            {
                Debug.LogWarning("[NavigationManager] HomeContainer not active, navigating to it first");
                GoToHome();
            }
        }

        /// <summary>
        /// Legacy method - redirects to tab switching
        /// </summary>
        public void GoToBattleArena()
        {
            SwitchToBattleTab();
        }

        /// <summary>
        /// Legacy method - redirects to tab switching
        /// </summary>
        public void GoToShop()
        {
            SwitchToShopTab();
        }

        /// <summary>
        /// Legacy method - redirects to tab switching
        /// </summary>
        public void GoToInventory()
        {
            SwitchToInventoryTab();
        }

        /// <summary>
        /// Navigate to Level Selection screen (Push navigation)
        /// </summary>
        public void GoToLevelSelection()
        {
            PushScreen<LevelSelectionScreen>(ScreenKeys.LevelSelection);
        }

        /// <summary>
        /// Navigate to Gameplay screen with level data (Push navigation)
        /// </summary>
        public void GoToGameplay(int levelId = 1, int chapterId = 1)
        {
            if (enableDebugLogs)
                Debug.Log($"[NavigationManager] Loading Gameplay - Chapter {chapterId}, Level {levelId}");

            // TODO: Pass level data via WindowOptions.onLoaded callback
            PushScreen<GameplayScreen>(ScreenKeys.Gameplay);
        }

        /// <summary>
        /// Navigate to RPG Stage screen (landscape)
        /// </summary>
        public void GoToRPGStage()
        {
            PushScreen<RPGStageScreen>(ScreenKeys.RPGStage);
        }

        /// <summary>
        /// Show Settings modal
        /// </summary>
        public void ShowSettings()
        {
            PushModal<SettingsModal>(ScreenKeys.Settings, withBackdrop: true);
        }

        /// <summary>
        /// Go back to previous screen
        /// </summary>
        public void GoBack()
        {
            if (enableDebugLogs)
                Debug.Log("[NavigationManager] Going back to previous screen");

            if (pageContainer == null)
            {
                Debug.LogError("[NavigationManager] PageContainer not assigned!");
                return;
            }

            if (pageContainer.PageCount > 1)
            {
                pageContainer.Pop(enableTransitions);
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log("[NavigationManager] Cannot go back - only 1 page in stack");
            }
        }

        /// <summary>
        /// Close top modal (if any)
        /// </summary>
        public void CloseModal()
        {
            if (enableDebugLogs)
                Debug.Log("[NavigationManager] Closing top modal");

            if (modalContainer == null)
            {
                Debug.LogError("[NavigationManager] ModalContainer not assigned!");
                return;
            }

            if (modalContainer.ModalCount > 0)
            {
                modalContainer.Pop(enableTransitions);
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log("[NavigationManager] No modals to close");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Push a screen using PageContainer
        /// </summary>
        private void PushScreen<T>(string resourceKey) where T : Page
        {
            if (enableDebugLogs)
                Debug.Log($"[NavigationManager] Pushing screen: {resourceKey}");

            if (pageContainer == null)
            {
                Debug.LogError("[NavigationManager] Cannot push screen - PageContainer not assigned!");
                return;
            }

            var options = new WindowOptions
            {
                LoadAsync = loadAsync,
                Stack = stackScreens,
                PlayAnimation = enableTransitions,
                OnLoaded = screen =>
                {
                    if (enableDebugLogs)
                        Debug.Log($"[NavigationManager] Screen loaded: {typeof(T).Name}");
                }
            };

            pageContainer.Push<T>(resourceKey, options);
        }

        /// <summary>
        /// Push a modal using ModalContainer
        /// </summary>
        private void PushModal<T>(string resourceKey, bool withBackdrop = true) where T : Modal
        {
            if (enableDebugLogs)
                Debug.Log($"[NavigationManager] Pushing modal: {resourceKey}");

            if (modalContainer == null)
            {
                Debug.LogError("[NavigationManager] Cannot push modal - ModalContainer not assigned!");
                return;
            }

            var options = new WindowOptions
            {
                LoadAsync = loadAsync,
                // Backdrop = withBackdrop,
                PlayAnimation = enableTransitions,
                OnLoaded = modal =>
                {
                    if (enableDebugLogs)
                        Debug.Log($"[NavigationManager] Modal loaded: {typeof(T).Name}");
                }
            };

            modalContainer.Push<T>(resourceKey, options);
        }

        /// <summary>
        /// Get the currently active HomeContainerPage from PageContainer
        /// Returns null if HomeContainerPage is not the active page
        /// </summary>
        private HomeContainerPage GetHomeContainerPage()
        {
            if (pageContainer == null || pageContainer.PageCount == 0)
                return null;

            // Check if cached instance is still valid and is the current page
            if (_cachedHomeContainer != null && pageContainer.CurrentPage == _cachedHomeContainer)
                return _cachedHomeContainer;

            // Check if current page is HomeContainerPage
            if (pageContainer.CurrentPage is HomeContainerPage homeContainer)
            {
                _cachedHomeContainer = homeContainer;
                return homeContainer;
            }

            return null;
        }

        #endregion

        #region Public Utilities

        /// <summary>
        /// Check if navigation is ready
        /// </summary>
        public bool IsReady()
        {
            return pageContainer != null;
        }

        /// <summary>
        /// Get current page count in stack
        /// </summary>
        public int GetPageStackCount()
        {
            return pageContainer != null ? pageContainer.PageCount : 0;
        }

        /// <summary>
        /// Get current modal count
        /// </summary>
        public int GetModalStackCount()
        {
            return modalContainer != null ? modalContainer.ModalCount : 0;
        }

        /// <summary>
        /// Clear all pages from stack
        /// </summary>
        public void ClearPageStack()
        {
            if (pageContainer != null && pageContainer.PageCount > 0)
            {
                // Pop all except first one
                while (pageContainer.PageCount > 1)
                {
                    pageContainer.Pop(false); // No animation for bulk clear
                }
                if (enableDebugLogs)
                    Debug.Log("[NavigationManager] Page stack cleared");
            }
        }

        /// <summary>
        /// Close all modals
        /// </summary>
        public void CloseAllModals()
        {
            if (modalContainer != null && modalContainer.ModalCount > 0)
            {
                while (modalContainer.ModalCount > 0)
                {
                    modalContainer.Pop(false); // No animation for bulk clear
                }
                if (enableDebugLogs)
                    Debug.Log("[NavigationManager] All modals closed");
            }
        }

        #endregion

        #if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Auto-find references
        /// </summary>
        [ContextMenu("Auto-Find References")]
        private void AutoFindReferences()
        {
            pageContainer = FindObjectOfType<PageContainer>();
            modalContainer = FindObjectOfType<ModalContainer>();

            Debug.Log($"[NavigationManager] Auto-find complete:");
            Debug.Log($"  PageContainer: {(pageContainer != null ? "✓" : "✗")}");
            Debug.Log($"  ModalContainer: {(modalContainer != null ? "✓" : "✗")}");
        }

        [ContextMenu("Print Navigation Status")]
        private void PrintNavigationStatus()
        {
            Debug.Log("=== Navigation Status ===");
            Debug.Log($"Ready: {IsReady()}");
            Debug.Log($"Page Stack Count: {GetPageStackCount()}");
            Debug.Log($"Modal Stack Count: {GetModalStackCount()}");
            Debug.Log($"Transitions Enabled: {enableTransitions}");
            Debug.Log($"Debug Logs Enabled: {enableDebugLogs}");
            Debug.Log($"Load Async: {loadAsync}");
            Debug.Log($"Stack Screens: {stackScreens}");
            Debug.Log("========================");
        }

        /// <summary>
        /// Editor-only: Test navigation
        /// </summary>
        [ContextMenu("Test - Go To Shop")]
        private void TestGoToShop()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[NavigationManager] This test only works in Play Mode");
                return;
            }
            GoToShop();
        }

        [ContextMenu("Test - Show Settings")]
        private void TestShowSettings()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[NavigationManager] This test only works in Play Mode");
                return;
            }
            ShowSettings();
        }

        [ContextMenu("Test - Go Back")]
        private void TestGoBack()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[NavigationManager] This test only works in Play Mode");
                return;
            }
            GoBack();
        }
        #endif
    }
}
