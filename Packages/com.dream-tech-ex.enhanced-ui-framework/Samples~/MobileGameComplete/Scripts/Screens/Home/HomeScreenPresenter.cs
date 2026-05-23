using UnityEngine;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Home
{
    /// <summary>
    /// Presenter for Home Screen - contains business logic for the main hub
    /// </summary>
    public class HomeScreenPresenter
    {
        private HomeScreenModel _model;
        private HomeScreenView _view;

        private const int ENERGY_COST_TO_PLAY = 10;

        /// <summary>
        /// Constructor
        /// </summary>
        public HomeScreenPresenter(HomeScreenView view)
        {
            _view = view;
            _model = new HomeScreenModel();

            Initialize();
        }

        /// <summary>
        /// Initialize presenter
        /// </summary>
        private void Initialize()
        {
            // Get player data from GameState
            if (GameState.Instance != null)
            {
                _model.playerData = GameState.Instance.PlayerData;
            }

            // Load screen data
            LoadHomeScreenData();

            // Subscribe to events
            SubscribeToEvents();

            // Initial view update
            UpdateView();
        }

        /// <summary>
        /// Cleanup presenter
        /// </summary>
        public void Cleanup()
        {
            UnsubscribeFromEvents();
            SaveHomeScreenData();
        }

        /// <summary>
        /// Subscribe to game state events
        /// </summary>
        private void SubscribeToEvents()
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.OnCurrencyChanged += OnCurrencyChanged;
                GameState.Instance.OnLevelCompleted += OnLevelCompleted;
            }
        }

        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (GameState.Instance != null)
            {
                GameState.Instance.OnCurrencyChanged -= OnCurrencyChanged;
                GameState.Instance.OnLevelCompleted -= OnLevelCompleted;
            }
        }

        /// <summary>
        /// Load home screen data
        /// </summary>
        private void LoadHomeScreenData()
        {
            // Check daily reward status
            CheckDailyReward();

            // Load notifications
            LoadNotifications();

            // Check active events
            CheckActiveEvents();

            // Update status message
            UpdateStatusMessage();

            Debug.Log("[HomePresenter] Home screen data loaded");
        }

        /// <summary>
        /// Save home screen data
        /// </summary>
        private void SaveHomeScreenData()
        {
            // Save any persistent data
            PlayerPrefs.SetString("LastHomeScreenVisit", System.DateTime.Now.ToString());
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Check if daily reward is available
        /// </summary>
        private void CheckDailyReward()
        {
            string lastClaimDate = PlayerPrefs.GetString("LastDailyRewardClaim", "");
            _model.hasDailyReward = string.IsNullOrEmpty(lastClaimDate) ||
                                    !IsSameDay(lastClaimDate, System.DateTime.Now.ToString());

            _model.dailyRewardDay = PlayerPrefs.GetInt("DailyRewardStreak", 1);
        }

        /// <summary>
        /// Load notification counts
        /// </summary>
        private void LoadNotifications()
        {
            // Example: Load from server or local storage
            _model.unreadMailCount = PlayerPrefs.GetInt("UnreadMailCount", 3);
            _model.unreadNotificationsCount = PlayerPrefs.GetInt("UnreadNotifications", 5);

            // Update side menu notifications
            _model.UpdateSideMenuNotification("mail", _model.unreadMailCount);
            _model.UpdateSideMenuNotification("events", PlayerPrefs.GetInt("ActiveEventsCount", 1));
        }

        /// <summary>
        /// Check for active events
        /// </summary>
        private void CheckActiveEvents()
        {
            // Example: Check if any events are running
            _model.hasActiveEvent = true; // For demo
            _model.activeEventName = "Summer Festival 2025";
            _model.activeEventProgress = PlayerPrefs.GetInt("EventProgress", 45);
            _model.activeEventMax = 100;
        }

        /// <summary>
        /// Update status message based on player state
        /// </summary>
        private void UpdateStatusMessage()
        {
            if (_model.playerData == null) return;

            int hour = System.DateTime.Now.Hour;

            if (hour < 12)
                _model.statusMessage = $"Good morning, {_model.playerData.playerName}!";
            else if (hour < 18)
                _model.statusMessage = $"Good afternoon, {_model.playerData.playerName}!";
            else
                _model.statusMessage = $"Good evening, {_model.playerData.playerName}!";

            // Add special messages
            if (_model.hasDailyReward)
            {
                _model.statusMessage += "\nDaily reward available!";
            }
        }

        /// <summary>
        /// Update the view with current model state
        /// </summary>
        private void UpdateView()
        {
            _view.UpdateView(_model);
        }

        #region Button Handlers

        /// <summary>
        /// Handle main play button press
        /// </summary>
        public void OnMainPlayButtonPressed()
        {
            Debug.Log("[HomePresenter] Main Play button pressed");

            // Check if player can play
            if (!_model.CanPlay())
            {
                Debug.LogWarning("[HomePresenter] Insufficient energy to play");
                _view.ShowInsufficientEnergyPopup();
                return;
            }

            // Check energy cost
            if (!GameState.Instance.CanAfford(CurrencyType.Energy, ENERGY_COST_TO_PLAY))
            {
                Debug.LogWarning("[HomePresenter] Not enough energy");
                _view.ShowInsufficientEnergyPopup();
                return;
            }

            // Navigate to level selection
            NavigateToLevelSelection();
        }

        /// <summary>
        /// Handle side menu button press
        /// </summary>
        public void OnSideMenuButtonPressed(string buttonId)
        {
            Debug.Log($"[HomePresenter] Side menu button pressed: {buttonId}");

            switch (buttonId)
            {
                case "mail":
                    OpenMail();
                    break;
                case "shop":
                    OpenShop();
                    break;
                case "events":
                    OpenEvents();
                    break;
                case "friends":
                    OpenFriends();
                    break;
                case "guild":
                    OpenGuild();
                    break;
                default:
                    Debug.LogWarning($"[HomePresenter] Unknown side menu button: {buttonId}");
                    break;
            }
        }

        /// <summary>
        /// Handle daily reward button press
        /// </summary>
        public void OnDailyRewardButtonPressed()
        {
            Debug.Log("[HomePresenter] Daily reward button pressed");

            if (!_model.hasDailyReward)
            {
                Debug.LogWarning("[HomePresenter] No daily reward available");
                return;
            }

            // Grant daily reward
            GrantDailyReward();

            // Update view
            _model.hasDailyReward = false;
            UpdateView();

            // Animate
            _view.AnimateDailyRewardCollection();
        }

        /// <summary>
        /// Handle event banner press
        /// </summary>
        public void OnEventBannerPressed()
        {
            Debug.Log("[HomePresenter] Event banner pressed");
            OpenEventDetails();
        }

        /// <summary>
        /// Handle news button press
        /// </summary>
        public void OnNewsButtonPressed()
        {
            Debug.Log("[HomePresenter] News button pressed");
            OpenNews();
        }

        /// <summary>
        /// Handle bottom navigation tab change
        /// </summary>
        public void OnBottomNavTabChanged(string tabId)
        {
            Debug.Log($"[HomePresenter] Bottom nav tab changed to: {tabId}");

            // Navigate to different screens based on tab
            switch (tabId)
            {
                case "home":
                    // Already on home
                    NavigationManager.Instance.GoToHome();
                    break;
                case "battle":
                    NavigateToBattle();
                    break;
                case "inventory":
                    NavigateToInventory();
                    break;
                case "shop":
                    OpenShop();
                    break;
                default:
                    Debug.LogWarning($"[HomePresenter] Unknown tab: {tabId}");
                    break;
            }
        }

        #endregion

        #region Navigation Methods

        private void NavigateToLevelSelection()
        {
            Debug.Log("[HomePresenter] Navigating to Level Selection");
            NavigationManager.Instance?.GoToLevelSelection();
        }

        private void NavigateToBattle()
        {
            Debug.Log("[HomePresenter] Navigating to Battle");
            NavigationManager.Instance?.GoToBattleArena();
        }

        private void NavigateToInventory()
        {
            Debug.Log("[HomePresenter] Navigating to Inventory");
            NavigationManager.Instance?.GoToInventory();
        }

        private void OpenMail()
        {
            Debug.Log("[HomePresenter] Opening Mail");
            // TODO: Open mail modal
            // ModalContainer.Instance.Push("MailModal");

            // Clear notification count
            _model.unreadMailCount = 0;
            _model.UpdateSideMenuNotification("mail", 0);
            UpdateView();
        }

        private void OpenShop()
        {
            Debug.Log("[HomePresenter] Opening Shop");
            NavigationManager.Instance?.GoToShop();
        }

        private void OpenEvents()
        {
            Debug.Log("[HomePresenter] Opening Events");
            // TODO: Open events screen
            // ModalContainer.Instance.Push("EventsModal");
        }

        private void OpenFriends()
        {
            Debug.Log("[HomePresenter] Opening Friends");
            // TODO: Open friends screen
            // ModalContainer.Instance.Push("FriendsModal");
        }

        private void OpenGuild()
        {
            Debug.Log("[HomePresenter] Opening Guild");
            // TODO: Open guild screen
            // PageContainer.Instance.Push("GuildPage");
        }

        private void OpenEventDetails()
        {
            Debug.Log("[HomePresenter] Opening Event Details");
            // TODO: Open event details modal
            // ModalContainer.Instance.Push("EventDetailsModal");
        }

        private void OpenNews()
        {
            Debug.Log("[HomePresenter] Opening News");
            // TODO: Open news modal
            // ModalContainer.Instance.Push("NewsModal");
        }

        #endregion

        #region Business Logic

        /// <summary>
        /// Grant daily reward to player
        /// </summary>
        private void GrantDailyReward()
        {
            int rewardGold = 100 * _model.dailyRewardDay;
            int rewardGems = 10 * _model.dailyRewardDay;

            // Grant rewards
            if (GameState.Instance != null)
            {
                GameState.Instance.AddCurrency(CurrencyType.Gold, rewardGold);
                GameState.Instance.AddCurrency(CurrencyType.Gems, rewardGems);
            }

            // Update streak
            _model.dailyRewardDay++;
            PlayerPrefs.SetInt("DailyRewardStreak", _model.dailyRewardDay);
            PlayerPrefs.SetString("LastDailyRewardClaim", System.DateTime.Now.ToString());
            PlayerPrefs.Save();

            Debug.Log($"[HomePresenter] Daily reward granted: {rewardGold} Gold, {rewardGems} Gems");
        }

        /// <summary>
        /// Check if two date strings represent the same day
        /// </summary>
        private bool IsSameDay(string date1Str, string date2Str)
        {
            try
            {
                System.DateTime date1 = System.DateTime.Parse(date1Str);
                System.DateTime date2 = System.DateTime.Parse(date2Str);

                return date1.Year == date2.Year &&
                       date1.Month == date2.Month &&
                       date1.Day == date2.Day;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handle currency changed event
        /// </summary>
        private void OnCurrencyChanged(CurrencyType type, int oldValue, int newValue)
        {
            Debug.Log($"[HomePresenter] Currency changed: {type} {oldValue} -> {newValue}");

            // Update view if energy changed (affects play button)
            if (type == CurrencyType.Energy)
            {
                UpdateView();
            }
        }

        /// <summary>
        /// Handle level completed event
        /// </summary>
        private void OnLevelCompleted(string levelId, int starsEarned)
        {
            Debug.Log($"[HomePresenter] Level completed: {levelId}, Stars: {starsEarned}");

            // Update status message to congratulate
            _model.statusMessage = $"Congratulations! Level {levelId} completed!";
            UpdateView();
        }

        #endregion
    }
}
