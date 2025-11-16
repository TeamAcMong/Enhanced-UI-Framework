using System;
using System.Collections.Generic;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Shop
{
    [Serializable]
    public class ShopModel
    {
        public PlayerData playerData;
        public List<ShopItem> gemPackages;
        public List<ShopItem> goldPackages;
        public List<ShopItem> specialOffers;
        public ShopTab currentTab;

        public ShopModel()
        {
            currentTab = ShopTab.Gems;
            InitializeShopItems();
        }

        private void InitializeShopItems()
        {
            gemPackages = new List<ShopItem>
            {
                new ShopItem { id = "gems_small", name = "Gem Pouch", description = "100 Gems", gemsAmount = 100, realPrice = 0.99f, isSpecial = false },
                new ShopItem { id = "gems_medium", name = "Gem Bag", description = "500 Gems", gemsAmount = 500, realPrice = 4.99f, isSpecial = false },
                new ShopItem { id = "gems_large", name = "Gem Chest", description = "1200 Gems + 200 Bonus", gemsAmount = 1400, realPrice = 9.99f, isSpecial = true },
                new ShopItem { id = "gems_huge", name = "Gem Vault", description = "5000 Gems + 1000 Bonus", gemsAmount = 6000, realPrice = 49.99f, isSpecial = true }
            };

            goldPackages = new List<ShopItem>
            {
                new ShopItem { id = "gold_small", name = "Gold Pouch", description = "10K Gold", goldAmount = 10000, gemsCost = 100, isSpecial = false },
                new ShopItem { id = "gold_medium", name = "Gold Bag", description = "50K Gold", goldAmount = 50000, gemsCost = 400, isSpecial = false },
                new ShopItem { id = "gold_large", name = "Gold Chest", description = "100K Gold + 20K Bonus", goldAmount = 120000, gemsCost = 700, isSpecial = true }
            };

            specialOffers = new List<ShopItem>
            {
                new ShopItem { id = "starter_pack", name = "Starter Pack", description = "500 Gems + 10K Gold + 5 Keys", gemsAmount = 500, goldAmount = 10000, keysAmount = 5, realPrice = 4.99f, isSpecial = true, timeLeft = 86400 },
                new ShopItem { id = "daily_deal", name = "Daily Deal", description = "200 Gems + 5K Gold", gemsAmount = 200, goldAmount = 5000, realPrice = 1.99f, isSpecial = true, timeLeft = 7200 },
                new ShopItem { id = "mega_pack", name = "Mega Pack", description = "10K Gems + 100K Gold", gemsAmount = 10000, goldAmount = 100000, realPrice = 99.99f, isSpecial = true }
            };
        }

        public List<ShopItem> GetCurrentTabItems()
        {
            return currentTab switch
            {
                ShopTab.Gems => gemPackages,
                ShopTab.Gold => goldPackages,
                ShopTab.Special => specialOffers,
                _ => gemPackages
            };
        }

        public bool CanAfford(ShopItem item)
        {
            if (item.realPrice > 0) return true; // Real money always available (for demo)
            return playerData != null && playerData.gems >= item.gemsCost;
        }
    }

    [Serializable]
    public class ShopItem
    {
        public string id;
        public string name;
        public string description;
        public int gemsAmount;
        public int goldAmount;
        public int keysAmount;
        public float realPrice; // USD
        public int gemsCost; // For items bought with gems
        public bool isSpecial;
        public int timeLeft; // Seconds remaining for limited offers
    }

    public enum ShopTab
    {
        Gems,
        Gold,
        Special
    }
}
