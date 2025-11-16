using System;
using System.Collections.Generic;
using System.Linq;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Inventory
{
    [Serializable]
    public class InventoryModel
    {
        public PlayerData playerData;
        public List<InventoryItem> items;
        public InventoryFilter currentFilter;
        public InventorySortMode currentSortMode;

        public InventoryModel()
        {
            currentFilter = InventoryFilter.All;
            currentSortMode = InventorySortMode.Name;
            InitializeInventory();
        }

        private void InitializeInventory()
        {
            items = new List<InventoryItem>
            {
                // Weapons
                new InventoryItem { id = "sword_iron", name = "Iron Sword", type = ItemType.Weapon, rarity = ItemRarity.Common, level = 1, quantity = 1, isEquipped = true, attack = 15 },
                new InventoryItem { id = "sword_steel", name = "Steel Sword", type = ItemType.Weapon, rarity = ItemRarity.Uncommon, level = 5, quantity = 1, isEquipped = false, attack = 35 },
                new InventoryItem { id = "bow_wooden", name = "Wooden Bow", type = ItemType.Weapon, rarity = ItemRarity.Common, level = 1, quantity = 2, isEquipped = false, attack = 12 },
                new InventoryItem { id = "staff_fire", name = "Fire Staff", type = ItemType.Weapon, rarity = ItemRarity.Rare, level = 8, quantity = 1, isEquipped = false, attack = 50, magicPower = 30 },

                // Armor
                new InventoryItem { id = "armor_leather", name = "Leather Armor", type = ItemType.Armor, rarity = ItemRarity.Common, level = 1, quantity = 1, isEquipped = true, defense = 10 },
                new InventoryItem { id = "armor_chain", name = "Chain Mail", type = ItemType.Armor, rarity = ItemRarity.Uncommon, level = 4, quantity = 1, isEquipped = false, defense = 25 },
                new InventoryItem { id = "armor_plate", name = "Plate Armor", type = ItemType.Armor, rarity = ItemRarity.Rare, level = 10, quantity = 1, isEquipped = false, defense = 50 },

                // Consumables
                new InventoryItem { id = "potion_health_small", name = "Small Health Potion", type = ItemType.Consumable, rarity = ItemRarity.Common, level = 1, quantity = 15, healAmount = 50 },
                new InventoryItem { id = "potion_health_large", name = "Large Health Potion", type = ItemType.Consumable, rarity = ItemRarity.Uncommon, level = 1, quantity = 5, healAmount = 150 },
                new InventoryItem { id = "potion_mana", name = "Mana Potion", type = ItemType.Consumable, rarity = ItemRarity.Common, level = 1, quantity = 8, manaAmount = 100 },

                // Materials
                new InventoryItem { id = "mat_wood", name = "Wood Planks", type = ItemType.Material, rarity = ItemRarity.Common, level = 1, quantity = 50 },
                new InventoryItem { id = "mat_iron", name = "Iron Ore", type = ItemType.Material, rarity = ItemRarity.Common, level = 1, quantity = 30 },
                new InventoryItem { id = "mat_crystal", name = "Magic Crystal", type = ItemType.Material, rarity = ItemRarity.Rare, level = 1, quantity = 5 },

                // Special
                new InventoryItem { id = "special_key_gold", name = "Golden Key", type = ItemType.Special, rarity = ItemRarity.Epic, level = 1, quantity = 3 },
                new InventoryItem { id = "special_scroll", name = "Teleport Scroll", type = ItemType.Special, rarity = ItemRarity.Uncommon, level = 1, quantity = 2 }
            };
        }

        public List<InventoryItem> GetFilteredItems()
        {
            var filtered = items.AsEnumerable();

            // Apply filter
            if (currentFilter != InventoryFilter.All)
            {
                filtered = filtered.Where(item => GetItemFilterType(item.type) == currentFilter);
            }

            // Apply sorting
            filtered = currentSortMode switch
            {
                InventorySortMode.Name => filtered.OrderBy(i => i.name),
                InventorySortMode.Rarity => filtered.OrderByDescending(i => i.rarity),
                InventorySortMode.Level => filtered.OrderByDescending(i => i.level),
                InventorySortMode.Quantity => filtered.OrderByDescending(i => i.quantity),
                _ => filtered.OrderBy(i => i.name)
            };

            return filtered.ToList();
        }

        private InventoryFilter GetItemFilterType(ItemType type)
        {
            return type switch
            {
                ItemType.Weapon => InventoryFilter.Weapons,
                ItemType.Armor => InventoryFilter.Armor,
                ItemType.Consumable => InventoryFilter.Consumables,
                ItemType.Material => InventoryFilter.Materials,
                ItemType.Special => InventoryFilter.Special,
                _ => InventoryFilter.All
            };
        }

        public InventoryItem GetItemById(string id)
        {
            return items.FirstOrDefault(i => i.id == id);
        }

        public bool CanEquip(InventoryItem item)
        {
            return item.type == ItemType.Weapon || item.type == ItemType.Armor;
        }

        public bool CanUse(InventoryItem item)
        {
            return item.type == ItemType.Consumable || item.type == ItemType.Special;
        }
    }

    [Serializable]
    public class InventoryItem
    {
        public string id;
        public string name;
        public ItemType type;
        public ItemRarity rarity;
        public int level;
        public int quantity;
        public bool isEquipped;

        // Stats (for weapons/armor)
        public int attack;
        public int defense;
        public int magicPower;

        // Consumable effects
        public int healAmount;
        public int manaAmount;

        public string GetRarityColor()
        {
            return rarity switch
            {
                ItemRarity.Common => "#FFFFFF",      // White
                ItemRarity.Uncommon => "#00FF00",    // Green
                ItemRarity.Rare => "#0099FF",        // Blue
                ItemRarity.Epic => "#9933FF",        // Purple
                ItemRarity.Legendary => "#FF9900",   // Orange
                _ => "#FFFFFF"
            };
        }

        public string GetDescription()
        {
            if (type == ItemType.Weapon)
            {
                var desc = $"Attack: +{attack}";
                if (magicPower > 0) desc += $"\nMagic Power: +{magicPower}";
                return desc;
            }
            else if (type == ItemType.Armor)
            {
                return $"Defense: +{defense}";
            }
            else if (type == ItemType.Consumable)
            {
                if (healAmount > 0) return $"Restores {healAmount} HP";
                if (manaAmount > 0) return $"Restores {manaAmount} MP";
            }

            return "A mysterious item...";
        }
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Material,
        Special
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum InventoryFilter
    {
        All,
        Weapons,
        Armor,
        Consumables,
        Materials,
        Special
    }

    public enum InventorySortMode
    {
        Name,
        Rarity,
        Level,
        Quantity
    }
}
