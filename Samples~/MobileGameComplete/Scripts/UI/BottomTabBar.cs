using UnityEngine;
using UnityEngine.UI;
using EnhancedUI;
using EnhancedUI.Demo.Input;
using System.Collections.Generic;

namespace EnhancedUI.Demo.UI
{
    /// <summary>
    /// Bottom tab bar component for tab navigation
    /// Handles button clicks and visual state updates
    /// Wire this up to your bottom navigation UI
    /// </summary>
    public class BottomTabBar : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SheetContainer sheetContainer;

        [Tooltip("Optional pager. When assigned, button clicks route through " +
                 "the pager's JumpToTab so click navigation uses the same " +
                 "snap visuals as a swipe.")]
        [SerializeField] private SheetSwipePager swipePager;

        [Header("Tab Buttons")]
        [SerializeField] private Button homeButton;
        [SerializeField] private Button battleButton;
        [SerializeField] private Button inventoryButton;
        [SerializeField] private Button shopButton;

        [Header("Visual States (Optional)")]
        [SerializeField] private GameObject homeActiveIndicator;
        [SerializeField] private GameObject battleActiveIndicator;
        [SerializeField] private GameObject inventoryActiveIndicator;
        [SerializeField] private GameObject shopActiveIndicator;

        [Header("Settings")]
        [SerializeField] private bool updateVisualStates = true;

        private Dictionary<string, Button> _sheetIdToButton;
        private Dictionary<string, GameObject> _sheetIdToIndicator;
        private string _currentSheetId;

        private void Start()
        {
            InitializeButtonMappings();
            SubscribeToButtons();

            // Update initial visual state
            if (sheetContainer != null && updateVisualStates && sheetContainer.ActiveSheet != null)
            {
                UpdateVisualState(sheetContainer.ActiveSheet.Identifier);
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromButtons();
        }

        #region Initialization

        private void InitializeButtonMappings()
        {
            // Map sheet IDs to buttons
            _sheetIdToButton = new Dictionary<string, Button>
            {
                { ScreenKeys.HomeSheet, homeButton },
                { ScreenKeys.BattleSheet, battleButton },
                { ScreenKeys.InventorySheet, inventoryButton },
                { ScreenKeys.ShopSheet, shopButton }
            };

            // Map sheet IDs to indicators (if provided)
            _sheetIdToIndicator = new Dictionary<string, GameObject>
            {
                { ScreenKeys.HomeSheet, homeActiveIndicator },
                { ScreenKeys.BattleSheet, battleActiveIndicator },
                { ScreenKeys.InventorySheet, inventoryActiveIndicator },
                { ScreenKeys.ShopSheet, shopActiveIndicator }
            };
        }

        private void SubscribeToButtons()
        {
            if (homeButton != null)
                homeButton.onClick.AddListener(() => OnTabButtonPressed(ScreenKeys.HomeSheet));

            if (battleButton != null)
                battleButton.onClick.AddListener(() => OnTabButtonPressed(ScreenKeys.BattleSheet));

            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(() => OnTabButtonPressed(ScreenKeys.InventorySheet));

            if (shopButton != null)
                shopButton.onClick.AddListener(() => OnTabButtonPressed(ScreenKeys.ShopSheet));
        }

        private void UnsubscribeFromButtons()
        {
            if (homeButton != null)
                homeButton.onClick.RemoveAllListeners();

            if (battleButton != null)
                battleButton.onClick.RemoveAllListeners();

            if (inventoryButton != null)
                inventoryButton.onClick.RemoveAllListeners();

            if (shopButton != null)
                shopButton.onClick.RemoveAllListeners();
        }

        #endregion

        #region Button Handlers

        private void OnTabButtonPressed(string sheetId)
        {
            if (sheetContainer == null)
            {
                Debug.LogError("[BottomTabBar] SheetContainer is not assigned!");
                return;
            }

            if (sheetContainer.IsInTransition)
            {
                Debug.Log("[BottomTabBar] Sheet container is in transition, ignoring button press");
                return;
            }

            if (sheetContainer.ActiveSheet != null
                && sheetContainer.ActiveSheet.Identifier == sheetId)
            {
                Debug.Log($"[BottomTabBar] Already on tab: {sheetId}");
                return;
            }

            // Prefer pager when present so click uses the same snap visuals
            // as a finger swipe. Falls back to framework's transition system
            // when no pager is wired (legacy / standalone tab bar usage).
            if (swipePager != null)
            {
                Debug.Log($"[BottomTabBar] Routing to pager: {sheetId}");
                swipePager.JumpToTab(sheetId, animate: true);
            }
            else
            {
                Debug.Log($"[BottomTabBar] Switching to tab via container: {sheetId}");
                sheetContainer.Show(sheetId, playAnimation: true);
            }

            // Update visual state
            if (updateVisualStates)
            {
                UpdateVisualState(sheetId);
            }
        }

        #endregion

        #region Visual State Management

        /// <summary>
        /// Update visual state to show which tab is active
        /// </summary>
        private void UpdateVisualState(string activeSheetId)
        {
            _currentSheetId = activeSheetId;

            // Update button interactable states
            foreach (var kvp in _sheetIdToButton)
            {
                if (kvp.Value != null)
                {
                    bool isActive = kvp.Key == activeSheetId;
                    // You can customize button appearance here
                    // For example: change color, scale, etc.
                    // kvp.Value.interactable = !isActive; // Uncomment to disable active button
                }
            }

            // Update indicator visibility
            foreach (var kvp in _sheetIdToIndicator)
            {
                if (kvp.Value != null)
                {
                    bool isActive = kvp.Key == activeSheetId;
                    kvp.Value.SetActive(isActive);
                }
            }

            Debug.Log($"[BottomTabBar] Visual state updated for: {activeSheetId}");
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Manually set which tab is visually active (without switching)
        /// Useful when tabs are changed programmatically
        /// </summary>
        public void SetActiveTab(string sheetId)
        {
            if (updateVisualStates)
            {
                UpdateVisualState(sheetId);
            }
        }

        /// <summary>
        /// Get the currently active tab sheet ID
        /// </summary>
        public string GetActiveTab()
        {
            return _currentSheetId;
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (Application.isPlaying) return;

            // Auto-find SheetContainer if not assigned
            if (sheetContainer == null)
            {
                sheetContainer = GetComponentInParent<SheetContainer>()
                    ?? GetComponentInChildren<SheetContainer>();
                if (sheetContainer != null)
                {
                    Debug.Log("[BottomTabBar] Auto-found SheetContainer");
                }
            }

            // Auto-find SheetSwipePager (optional — click falls back to
            // SheetContainer.Show when no pager is wired).
            if (swipePager == null)
            {
                swipePager = GetComponentInParent<SheetSwipePager>()
                    ?? GetComponentInChildren<SheetSwipePager>();
                if (swipePager != null)
                {
                    Debug.Log("[BottomTabBar] Auto-found SheetSwipePager");
                }
            }
        }

        [ContextMenu("Print Button Status")]
        private void PrintButtonStatus()
        {
            Debug.Log("=== Bottom Tab Bar Status ===");
            Debug.Log($"Home Button: {(homeButton != null ? "✓" : "✗")}");
            Debug.Log($"Battle Button: {(battleButton != null ? "✓" : "✗")}");
            Debug.Log($"Inventory Button: {(inventoryButton != null ? "✓" : "✗")}");
            Debug.Log($"Shop Button: {(shopButton != null ? "✓" : "✗")}");
            Debug.Log($"SheetContainer: {(sheetContainer != null ? "✓" : "✗")}");

            if (Application.isPlaying && sheetContainer != null)
            {
                Debug.Log($"Active Sheet: {sheetContainer.ActiveSheet?.Identifier ?? "None"}");
            }

            Debug.Log("============================");
        }
#endif
    }
}
