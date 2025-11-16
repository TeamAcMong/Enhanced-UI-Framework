using System;
using System.Collections.Generic;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Home
{
    /// <summary>
    /// Model for Home Screen - contains all data for the main hub screen
    /// </summary>
    [Serializable]
    public class HomeScreenModel
    {
        // Player reference
        public PlayerData playerData;

        // UI State
        public string mainButtonText;
        public bool isMainButtonEnabled;
        public string statusMessage;

        // Side menu items
        public List<SideMenuItem> sideMenuItems;

        // Daily rewards
        public bool hasDailyReward;
        public int dailyRewardDay;

        // Notifications
        public int unreadMailCount;
        public int unreadNotificationsCount;

        // Events
        public bool hasActiveEvent;
        public string activeEventName;
        public int activeEventProgress;
        public int activeEventMax;

        public HomeScreenModel()
        {
            mainButtonText = "PLAY";
            isMainButtonEnabled = true;
            statusMessage = "Welcome back!";

            sideMenuItems = new List<SideMenuItem>
            {
                new SideMenuItem { id = "mail", displayName = "Mail", notificationCount = 0 },
                new SideMenuItem { id = "shop", displayName = "Shop", notificationCount = 0 },
                new SideMenuItem { id = "events", displayName = "Events", notificationCount = 1 },
                new SideMenuItem { id = "friends", displayName = "Friends", notificationCount = 0 },
                new SideMenuItem { id = "guild", displayName = "Guild", notificationCount = 0 }
            };

            hasDailyReward = false;
            dailyRewardDay = 1;
            unreadMailCount = 0;
            unreadNotificationsCount = 0;
            hasActiveEvent = false;
            activeEventName = "";
            activeEventProgress = 0;
            activeEventMax = 100;
        }

        /// <summary>
        /// Get side menu item by ID
        /// </summary>
        public SideMenuItem GetSideMenuItem(string id)
        {
            return sideMenuItems.Find(item => item.id == id);
        }

        /// <summary>
        /// Update side menu item notification count
        /// </summary>
        public void UpdateSideMenuNotification(string id, int count)
        {
            var item = GetSideMenuItem(id);
            if (item != null)
            {
                item.notificationCount = count;
            }
        }

        /// <summary>
        /// Check if player has enough energy to play
        /// </summary>
        public bool CanPlay()
        {
            return playerData != null && playerData.energy > 0;
        }

        /// <summary>
        /// Get total notification count
        /// </summary>
        public int GetTotalNotificationCount()
        {
            int total = unreadMailCount + unreadNotificationsCount;

            foreach (var item in sideMenuItems)
            {
                total += item.notificationCount;
            }

            return total;
        }
    }

    /// <summary>
    /// Side menu item data
    /// </summary>
    [Serializable]
    public class SideMenuItem
    {
        public string id;
        public string displayName;
        public int notificationCount;
        public bool isLocked;
    }
}
