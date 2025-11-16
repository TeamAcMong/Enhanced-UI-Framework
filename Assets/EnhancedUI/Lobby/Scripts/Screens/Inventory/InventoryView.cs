using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EnhancedUI.Demo.Screens.Inventory
{
    /// <summary>
    /// Inventory View - Handles UI display and user input for inventory screen
    /// Displays items in a grid with filtering and sorting options
    /// </summary>
    public class InventoryView : MonoBehaviour
    {
        [Header("Filter Buttons")]
        [SerializeField] private Button allFilterButton;
        [SerializeField] private Button weaponsFilterButton;
        [SerializeField] private Button armorFilterButton;
        [SerializeField] private Button consumablesFilterButton;
        [SerializeField] private Button materialsFilterButton;
        [SerializeField] private Button specialFilterButton;

        [Header("Sort Dropdown")]
        [SerializeField] private TMP_Dropdown sortDropdown;

        [Header("Item Grid")]
        [SerializeField] private Transform itemGridContainer;
        [SerializeField] private GameObject itemCardPrefab;

        [Header("Item Detail Panel")]
        [SerializeField] private GameObject itemDetailPanel;
        [SerializeField] private TextMeshProUGUI detailNameText;
        [SerializeField] private TextMeshProUGUI detailDescriptionText;
        [SerializeField] private TextMeshProUGUI detailStatsText;
        [SerializeField] private Button equipButton;
        [SerializeField] private Button useButton;
        [SerializeField] private Button closeDetailButton;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI inventoryCountText;
        [SerializeField] private Button backButton;

        // Events
        public event Action<InventoryFilter> OnFilterChanged;
        public event Action<InventorySortMode> OnSortChanged;
        public event Action<InventoryItem> OnItemSelected;
        public event Action<InventoryItem> OnItemEquipped;
        public event Action<InventoryItem> OnItemUsed;
        public event Action OnBackPressed;

        private InventoryPresenter _presenter;
        private InventoryItem _selectedItem;

        /// <summary>
        /// Set the presenter for this view
        /// </summary>
        public void SetPresenter(InventoryPresenter presenter)
        {
            _presenter = presenter;
        }

        private void Awake()
        {
            // Setup filter button listeners
            if (allFilterButton != null)
                allFilterButton.onClick.AddListener(() => OnFilterChanged?.Invoke(InventoryFilter.All));

            if (weaponsFilterButton != null)
                weaponsFilterButton.onClick.AddListener(() => OnFilterChanged?.Invoke(InventoryFilter.Weapons));

            if (armorFilterButton != null)
                armorFilterButton.onClick.AddListener(() => OnFilterChanged?.Invoke(InventoryFilter.Armor));

            if (consumablesFilterButton != null)
                consumablesFilterButton.onClick.AddListener(() => OnFilterChanged?.Invoke(InventoryFilter.Consumables));

            if (materialsFilterButton != null)
                materialsFilterButton.onClick.AddListener(() => OnFilterChanged?.Invoke(InventoryFilter.Materials));

            if (specialFilterButton != null)
                specialFilterButton.onClick.AddListener(() => OnFilterChanged?.Invoke(InventoryFilter.Special));

            // Setup sort dropdown
            if (sortDropdown != null)
            {
                sortDropdown.ClearOptions();
                sortDropdown.AddOptions(new System.Collections.Generic.List<string>
                {
                    "Sort by Name",
                    "Sort by Rarity",
                    "Sort by Level",
                    "Sort by Quantity"
                });
                sortDropdown.onValueChanged.AddListener(index => OnSortChanged?.Invoke((InventorySortMode)index));
            }

            // Setup detail panel buttons
            if (equipButton != null)
                equipButton.onClick.AddListener(() => OnItemEquipped?.Invoke(_selectedItem));

            if (useButton != null)
                useButton.onClick.AddListener(() => OnItemUsed?.Invoke(_selectedItem));

            if (closeDetailButton != null)
                closeDetailButton.onClick.AddListener(() => HideItemDetail());

            // Setup back button
            if (backButton != null)
                backButton.onClick.AddListener(() => OnBackPressed?.Invoke());

            // Hide detail panel initially
            if (itemDetailPanel != null)
                itemDetailPanel.SetActive(false);
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (allFilterButton != null) allFilterButton.onClick.RemoveAllListeners();
            if (weaponsFilterButton != null) weaponsFilterButton.onClick.RemoveAllListeners();
            if (armorFilterButton != null) armorFilterButton.onClick.RemoveAllListeners();
            if (consumablesFilterButton != null) consumablesFilterButton.onClick.RemoveAllListeners();
            if (materialsFilterButton != null) materialsFilterButton.onClick.RemoveAllListeners();
            if (specialFilterButton != null) specialFilterButton.onClick.RemoveAllListeners();
            if (sortDropdown != null) sortDropdown.onValueChanged.RemoveAllListeners();
            if (equipButton != null) equipButton.onClick.RemoveAllListeners();
            if (useButton != null) useButton.onClick.RemoveAllListeners();
            if (closeDetailButton != null) closeDetailButton.onClick.RemoveAllListeners();
            if (backButton != null) backButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Show the view
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            Debug.Log("[InventoryView] Show");
        }

        /// <summary>
        /// Hide the view
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            Debug.Log("[InventoryView] Hide");
        }

        /// <summary>
        /// Display inventory items in grid
        /// </summary>
        public void DisplayItems(InventoryItem[] items)
        {
            if (itemGridContainer == null)
            {
                Debug.LogWarning("[InventoryView] Item grid container is not assigned");
                return;
            }

            // Clear existing items
            foreach (Transform child in itemGridContainer)
            {
                Destroy(child.gameObject);
            }

            // Create item cards
            foreach (var item in items)
            {
                CreateItemCard(item);
            }

            // Update inventory count
            if (inventoryCountText != null)
            {
                int totalItems = items.Length;
                int totalQuantity = 0;
                foreach (var item in items)
                {
                    totalQuantity += item.quantity;
                }
                inventoryCountText.text = $"Items: {totalItems} ({totalQuantity} total)";
            }

            Debug.Log($"[InventoryView] Displayed {items.Length} items");
        }

        /// <summary>
        /// Create UI card for a single item
        /// </summary>
        private void CreateItemCard(InventoryItem item)
        {
            if (itemCardPrefab == null)
            {
                Debug.LogWarning("[InventoryView] Item card prefab is not assigned");
                return;
            }

            GameObject cardObj = Instantiate(itemCardPrefab, itemGridContainer);

            // Find UI components
            var nameText = cardObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var quantityText = cardObj.transform.Find("QuantityText")?.GetComponent<TextMeshProUGUI>();
            var levelText = cardObj.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            var rarityBorder = cardObj.transform.Find("RarityBorder")?.GetComponent<Image>();
            var equippedIcon = cardObj.transform.Find("EquippedIcon")?.gameObject;
            var button = cardObj.GetComponent<Button>();

            // Set item data
            if (nameText != null)
                nameText.text = item.name;

            if (quantityText != null)
            {
                quantityText.text = item.quantity > 1 ? $"x{item.quantity}" : "";
                quantityText.gameObject.SetActive(item.quantity > 1);
            }

            if (levelText != null)
            {
                if (item.level > 1)
                {
                    levelText.text = $"Lv.{item.level}";
                    levelText.gameObject.SetActive(true);
                }
                else
                {
                    levelText.gameObject.SetActive(false);
                }
            }

            // Set rarity color
            if (rarityBorder != null)
            {
                if (ColorUtility.TryParseHtmlString(item.GetRarityColor(), out Color rarityColor))
                {
                    rarityBorder.color = rarityColor;
                }
            }

            // Show equipped icon
            if (equippedIcon != null)
                equippedIcon.SetActive(item.isEquipped);

            // Setup button click
            if (button != null)
            {
                button.onClick.AddListener(() => OnItemSelected?.Invoke(item));
            }
        }

        /// <summary>
        /// Show item detail panel
        /// </summary>
        public void ShowItemDetail(InventoryItem item, bool canEquip, bool canUse)
        {
            _selectedItem = item;

            if (itemDetailPanel != null)
                itemDetailPanel.SetActive(true);

            if (detailNameText != null)
            {
                detailNameText.text = item.name;
                // Apply rarity color
                if (ColorUtility.TryParseHtmlString(item.GetRarityColor(), out Color rarityColor))
                {
                    detailNameText.color = rarityColor;
                }
            }

            if (detailDescriptionText != null)
                detailDescriptionText.text = item.GetDescription();

            if (detailStatsText != null)
            {
                string stats = $"<b>Type:</b> {item.type}\n";
                stats += $"<b>Rarity:</b> {item.rarity}\n";
                stats += $"<b>Level:</b> {item.level}\n";
                stats += $"<b>Quantity:</b> {item.quantity}";
                detailStatsText.text = stats;
            }

            // Setup action buttons
            if (equipButton != null)
            {
                equipButton.gameObject.SetActive(canEquip);
                var buttonText = equipButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = item.isEquipped ? "Unequip" : "Equip";
                }
            }

            if (useButton != null)
            {
                useButton.gameObject.SetActive(canUse);
                useButton.interactable = item.quantity > 0;
            }

            Debug.Log($"[InventoryView] Showing detail for: {item.name}");
        }

        /// <summary>
        /// Hide item detail panel
        /// </summary>
        public void HideItemDetail()
        {
            if (itemDetailPanel != null)
                itemDetailPanel.SetActive(false);

            _selectedItem = null;
        }

        /// <summary>
        /// Show feedback message
        /// </summary>
        public void ShowFeedback(string message)
        {
            Debug.Log($"[InventoryView] Feedback: {message}");
            // In a real implementation, show a toast notification or popup
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Test item selection
        /// </summary>
        [ContextMenu("Test Select Item")]
        private void TestSelectItem()
        {
            var testItem = new InventoryItem
            {
                id = "test_sword",
                name = "Test Sword",
                type = ItemType.Weapon,
                rarity = ItemRarity.Rare,
                level = 5,
                quantity = 1,
                attack = 50
            };

            OnItemSelected?.Invoke(testItem);
        }
#endif
    }
}
