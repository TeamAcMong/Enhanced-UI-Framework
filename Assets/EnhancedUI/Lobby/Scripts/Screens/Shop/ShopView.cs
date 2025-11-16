using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EnhancedUI.Demo.Screens.Shop
{
    /// <summary>
    /// Shop View - Handles UI display and user input for shop screen
    /// Displays gem packages, gold packages, and special offers
    /// </summary>
    public class ShopView : MonoBehaviour
    {
        [Header("Tab Navigation")]
        [SerializeField] private Button gemsTabButton;
        [SerializeField] private Button goldTabButton;
        [SerializeField] private Button specialTabButton;
        [SerializeField] private GameObject gemsTabIndicator;
        [SerializeField] private GameObject goldTabIndicator;
        [SerializeField] private GameObject specialTabIndicator;

        [Header("Shop Item Grid")]
        [SerializeField] private Transform shopItemsContainer;
        [SerializeField] private GameObject shopItemPrefab;

        [Header("Player Info Display")]
        [SerializeField] private TextMeshProUGUI gemsText;
        [SerializeField] private TextMeshProUGUI goldText;

        [Header("Back Button")]
        [SerializeField] private Button backButton;

        [Header("Loading")]
        [SerializeField] private GameObject loadingIndicator;

        // Events
        public event Action<ShopTab> OnTabChanged;
        public event Action<ShopItem> OnItemPurchased;
        public event Action OnBackPressed;

        private ShopPresenter _presenter;

        /// <summary>
        /// Set the presenter for this view
        /// </summary>
        public void SetPresenter(ShopPresenter presenter)
        {
            _presenter = presenter;
        }

        private void Awake()
        {
            // Setup button listeners
            if (gemsTabButton != null)
                gemsTabButton.onClick.AddListener(() => OnTabChanged?.Invoke(ShopTab.Gems));

            if (goldTabButton != null)
                goldTabButton.onClick.AddListener(() => OnTabChanged?.Invoke(ShopTab.Gold));

            if (specialTabButton != null)
                specialTabButton.onClick.AddListener(() => OnTabChanged?.Invoke(ShopTab.Special));

            if (backButton != null)
                backButton.onClick.AddListener(() => OnBackPressed?.Invoke());
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (gemsTabButton != null)
                gemsTabButton.onClick.RemoveAllListeners();

            if (goldTabButton != null)
                goldTabButton.onClick.RemoveAllListeners();

            if (specialTabButton != null)
                specialTabButton.onClick.RemoveAllListeners();

            if (backButton != null)
                backButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Show the view
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            Debug.Log("[ShopView] Show");
        }

        /// <summary>
        /// Hide the view
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            Debug.Log("[ShopView] Hide");
        }

        /// <summary>
        /// Update tab indicators based on current tab
        /// </summary>
        public void UpdateTabIndicators(ShopTab currentTab)
        {
            if (gemsTabIndicator != null)
                gemsTabIndicator.SetActive(currentTab == ShopTab.Gems);

            if (goldTabIndicator != null)
                goldTabIndicator.SetActive(currentTab == ShopTab.Gold);

            if (specialTabIndicator != null)
                specialTabIndicator.SetActive(currentTab == ShopTab.Special);

            Debug.Log($"[ShopView] Tab changed to: {currentTab}");
        }

        /// <summary>
        /// Display shop items in the grid
        /// </summary>
        public void DisplayShopItems(ShopItem[] items, System.Func<ShopItem, bool> canAfford)
        {
            if (shopItemsContainer == null)
            {
                Debug.LogWarning("[ShopView] Shop items container is not assigned");
                return;
            }

            // Clear existing items
            foreach (Transform child in shopItemsContainer)
            {
                Destroy(child.gameObject);
            }

            // Create shop item UI for each item
            foreach (var item in items)
            {
                CreateShopItemUI(item, canAfford(item));
            }

            Debug.Log($"[ShopView] Displayed {items.Length} shop items");
        }

        /// <summary>
        /// Create UI for a single shop item
        /// </summary>
        private void CreateShopItemUI(ShopItem item, bool canAfford)
        {
            if (shopItemPrefab == null)
            {
                Debug.LogWarning("[ShopView] Shop item prefab is not assigned");
                return;
            }

            GameObject itemObj = Instantiate(shopItemPrefab, shopItemsContainer);

            // Find UI components (assuming specific child structure)
            var nameText = itemObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var descText = itemObj.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            var priceText = itemObj.transform.Find("PriceText")?.GetComponent<TextMeshProUGUI>();
            var buyButton = itemObj.transform.Find("BuyButton")?.GetComponent<Button>();
            var specialBadge = itemObj.transform.Find("SpecialBadge")?.gameObject;
            var timerText = itemObj.transform.Find("TimerText")?.GetComponent<TextMeshProUGUI>();

            // Set item data
            if (nameText != null)
                nameText.text = item.name;

            if (descText != null)
                descText.text = item.description;

            if (priceText != null)
            {
                if (item.realPrice > 0)
                {
                    priceText.text = $"${item.realPrice:F2}";
                }
                else if (item.gemsCost > 0)
                {
                    priceText.text = $"{item.gemsCost} Gems";
                }
            }

            // Show special badge if item is special
            if (specialBadge != null)
                specialBadge.SetActive(item.isSpecial);

            // Show timer for limited offers
            if (timerText != null)
            {
                if (item.timeLeft > 0)
                {
                    timerText.gameObject.SetActive(true);
                    timerText.text = FormatTimeLeft(item.timeLeft);
                }
                else
                {
                    timerText.gameObject.SetActive(false);
                }
            }

            // Setup buy button
            if (buyButton != null)
            {
                buyButton.interactable = canAfford;
                buyButton.onClick.AddListener(() => OnItemPurchased?.Invoke(item));

                // Change button appearance based on affordability
                var buttonText = buyButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null && !canAfford)
                {
                    buttonText.text = "Not Enough Gems";
                }
            }
        }

        /// <summary>
        /// Update player currency display
        /// </summary>
        public void UpdatePlayerCurrency(int gems, int gold)
        {
            if (gemsText != null)
                gemsText.text = gems.ToString("N0");

            if (goldText != null)
                goldText.text = gold.ToString("N0");
        }

        /// <summary>
        /// Show loading indicator
        /// </summary>
        public void ShowLoading(bool show)
        {
            if (loadingIndicator != null)
                loadingIndicator.SetActive(show);
        }

        /// <summary>
        /// Show purchase success feedback
        /// </summary>
        public void ShowPurchaseSuccess(ShopItem item)
        {
            Debug.Log($"[ShopView] Purchase successful: {item.name}");

            // In a real implementation, you would show an animation or popup here
            // For now, we'll just log it
        }

        /// <summary>
        /// Show purchase failed feedback
        /// </summary>
        public void ShowPurchaseFailed(string reason)
        {
            Debug.LogWarning($"[ShopView] Purchase failed: {reason}");

            // In a real implementation, you would show an error popup here
        }

        /// <summary>
        /// Format time left in seconds to readable string
        /// </summary>
        private string FormatTimeLeft(int seconds)
        {
            if (seconds < 60)
                return $"{seconds}s";
            else if (seconds < 3600)
                return $"{seconds / 60}m";
            else if (seconds < 86400)
                return $"{seconds / 3600}h";
            else
                return $"{seconds / 86400}d";
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Simulate purchase
        /// </summary>
        [ContextMenu("Test Purchase")]
        private void TestPurchase()
        {
            var testItem = new ShopItem
            {
                id = "test_item",
                name = "Test Item",
                description = "Test purchase",
                gemsAmount = 100,
                realPrice = 0.99f,
                isSpecial = true
            };

            OnItemPurchased?.Invoke(testItem);
        }
#endif
    }
}
