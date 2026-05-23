using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using EnhancedUI.Demo.Components;

namespace EnhancedUI.Demo.Screens.Home
{
    /// <summary>
    /// View for Home Screen - handles UI display and user input for the main hub
    /// </summary>
    public class HomeScreenView : MonoBehaviour
    {
        [Header("Common Components")]
        [SerializeField] private TopBar topBar;
        [SerializeField] private BottomNavigation bottomNavigation;

        [Header("Main Content")]
        [SerializeField] private Button mainPlayButton;
        [SerializeField] private TextMeshProUGUI mainButtonText;
        [SerializeField] private TextMeshProUGUI statusMessageText;
        [SerializeField] private Image playerAvatarLarge; // Large avatar display in center

        [Header("Side Menu Buttons")]
        [SerializeField] private SideMenuButton mailButton;
        [SerializeField] private SideMenuButton shopButton;
        [SerializeField] private SideMenuButton eventsButton;
        [SerializeField] private SideMenuButton friendsButton;
        [SerializeField] private SideMenuButton guildButton;

        [Header("Daily Reward")]
        [SerializeField] private GameObject dailyRewardBanner;
        [SerializeField] private TextMeshProUGUI dailyRewardDayText;
        [SerializeField] private Button dailyRewardButton;

        [Header("Event Banner")]
        [SerializeField] private GameObject eventBanner;
        [SerializeField] private TextMeshProUGUI eventNameText;
        [SerializeField] private Image eventProgressBar;
        [SerializeField] private TextMeshProUGUI eventProgressText;
        [SerializeField] private Button eventBannerButton;

        [Header("News/Announcement")]
        [SerializeField] private GameObject newsBanner;
        [SerializeField] private TextMeshProUGUI newsText;
        [SerializeField] private Button newsButton;

        private HomeScreenPresenter _presenter;
        private Dictionary<string, SideMenuButton> _sideMenuButtons;

        private void Awake()
        {
            SetupButtonListeners();
            SetupSideMenuButtons();
        }

        private void OnDestroy()
        {
            CleanupButtonListeners();
        }

        /// <summary>
        /// Setup button listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            if (mainPlayButton != null)
                mainPlayButton.onClick.AddListener(OnMainPlayButtonClicked);

            if (dailyRewardButton != null)
                dailyRewardButton.onClick.AddListener(OnDailyRewardButtonClicked);

            if (eventBannerButton != null)
                eventBannerButton.onClick.AddListener(OnEventBannerClicked);

            if (newsButton != null)
                newsButton.onClick.AddListener(OnNewsButtonClicked);

            if (bottomNavigation != null)
                bottomNavigation.OnTabChanged += OnBottomNavTabChanged;
        }

        /// <summary>
        /// Cleanup button listeners
        /// </summary>
        private void CleanupButtonListeners()
        {
            if (mainPlayButton != null)
                mainPlayButton.onClick.RemoveListener(OnMainPlayButtonClicked);

            if (dailyRewardButton != null)
                dailyRewardButton.onClick.RemoveListener(OnDailyRewardButtonClicked);

            if (eventBannerButton != null)
                eventBannerButton.onClick.RemoveListener(OnEventBannerClicked);

            if (newsButton != null)
                newsButton.onClick.RemoveListener(OnNewsButtonClicked);

            if (bottomNavigation != null)
                bottomNavigation.OnTabChanged -= OnBottomNavTabChanged;
        }

        /// <summary>
        /// Setup side menu buttons
        /// </summary>
        private void SetupSideMenuButtons()
        {
            _sideMenuButtons = new Dictionary<string, SideMenuButton>();

            if (mailButton != null)
            {
                _sideMenuButtons["mail"] = mailButton;
                mailButton.OnButtonClicked += OnSideMenuButtonClicked;
                mailButton.SetSlideInDelay(0.1f);
            }

            if (shopButton != null)
            {
                _sideMenuButtons["shop"] = shopButton;
                shopButton.OnButtonClicked += OnSideMenuButtonClicked;
                shopButton.SetSlideInDelay(0.2f);
            }

            if (eventsButton != null)
            {
                _sideMenuButtons["events"] = eventsButton;
                eventsButton.OnButtonClicked += OnSideMenuButtonClicked;
                eventsButton.SetSlideInDelay(0.3f);
            }

            if (friendsButton != null)
            {
                _sideMenuButtons["friends"] = friendsButton;
                friendsButton.OnButtonClicked += OnSideMenuButtonClicked;
                friendsButton.SetSlideInDelay(0.4f);
            }

            if (guildButton != null)
            {
                _sideMenuButtons["guild"] = guildButton;
                guildButton.OnButtonClicked += OnSideMenuButtonClicked;
                guildButton.SetSlideInDelay(0.5f);
            }
        }

        /// <summary>
        /// Set the presenter
        /// </summary>
        public void SetPresenter(HomeScreenPresenter presenter)
        {
            _presenter = presenter;
        }

        /// <summary>
        /// Update the view with model data
        /// </summary>
        public void UpdateView(HomeScreenModel model)
        {
            // Update main button
            if (mainButtonText != null)
                mainButtonText.text = model.mainButtonText;

            if (mainPlayButton != null)
                mainPlayButton.interactable = model.isMainButtonEnabled && model.CanPlay();

            // Update status message
            if (statusMessageText != null)
                statusMessageText.text = model.statusMessage;

            // Update side menu buttons
            UpdateSideMenuButtons(model);

            // Update daily reward
            UpdateDailyReward(model);

            // Update event banner
            UpdateEventBanner(model);

            // Update top bar notification count
            if (topBar != null)
                topBar.SetNotificationCount(model.GetTotalNotificationCount());
        }

        /// <summary>
        /// Update side menu buttons with model data
        /// </summary>
        private void UpdateSideMenuButtons(HomeScreenModel model)
        {
            foreach (var menuItem in model.sideMenuItems)
            {
                if (_sideMenuButtons.TryGetValue(menuItem.id, out var button))
                {
                    button.SetNotificationCount(menuItem.notificationCount);
                }
            }
        }

        /// <summary>
        /// Update daily reward display
        /// </summary>
        private void UpdateDailyReward(HomeScreenModel model)
        {
            if (dailyRewardBanner != null)
                dailyRewardBanner.SetActive(model.hasDailyReward);

            if (dailyRewardDayText != null && model.hasDailyReward)
                dailyRewardDayText.text = $"Day {model.dailyRewardDay}";
        }

        /// <summary>
        /// Update event banner
        /// </summary>
        private void UpdateEventBanner(HomeScreenModel model)
        {
            if (eventBanner != null)
                eventBanner.SetActive(model.hasActiveEvent);

            if (model.hasActiveEvent)
            {
                if (eventNameText != null)
                    eventNameText.text = model.activeEventName;

                if (eventProgressBar != null)
                {
                    float progress = (float)model.activeEventProgress / model.activeEventMax;
                    eventProgressBar.fillAmount = progress;
                }

                if (eventProgressText != null)
                    eventProgressText.text = $"{model.activeEventProgress}/{model.activeEventMax}";
            }
        }

        /// <summary>
        /// Show the view
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);

            // Update top bar
            if (topBar != null)
                topBar.UpdatePlayerInfo();

            // Select home tab in bottom navigation
            if (bottomNavigation != null)
                bottomNavigation.SelectTab("home", false);
        }

        /// <summary>
        /// Hide the view
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Play enter animation
        /// </summary>
        public void PlayEnterAnimation()
        {
            // Side menu buttons will auto-animate on enable
            // Add any additional animations here
        }

        /// <summary>
        /// Main play button clicked
        /// </summary>
        private void OnMainPlayButtonClicked()
        {
            Debug.Log("[HomeScreenView] Main Play button clicked");
            _presenter?.OnMainPlayButtonPressed();
        }

        /// <summary>
        /// Side menu button clicked
        /// </summary>
        private void OnSideMenuButtonClicked(string buttonId)
        {
            Debug.Log($"[HomeScreenView] Side menu button clicked: {buttonId}");
            _presenter?.OnSideMenuButtonPressed(buttonId);
        }

        /// <summary>
        /// Daily reward button clicked
        /// </summary>
        private void OnDailyRewardButtonClicked()
        {
            Debug.Log("[HomeScreenView] Daily reward button clicked");
            _presenter?.OnDailyRewardButtonPressed();
        }

        /// <summary>
        /// Event banner clicked
        /// </summary>
        private void OnEventBannerClicked()
        {
            Debug.Log("[HomeScreenView] Event banner clicked");
            _presenter?.OnEventBannerPressed();
        }

        /// <summary>
        /// News button clicked
        /// </summary>
        private void OnNewsButtonClicked()
        {
            Debug.Log("[HomeScreenView] News button clicked");
            _presenter?.OnNewsButtonPressed();
        }

        /// <summary>
        /// Bottom navigation tab changed
        /// </summary>
        private void OnBottomNavTabChanged(int index, string tabId)
        {
            Debug.Log($"[HomeScreenView] Bottom nav changed to: {tabId}");
            _presenter?.OnBottomNavTabChanged(tabId);
        }

        /// <summary>
        /// Show insufficient energy popup
        /// </summary>
        public void ShowInsufficientEnergyPopup()
        {
            Debug.Log("[HomeScreenView] Showing insufficient energy popup");
            // TODO: Implement popup via ModalContainer
            // ModalContainer.Instance.Push("InsufficientEnergyModal");
        }

        /// <summary>
        /// Animate daily reward collection
        /// </summary>
        public void AnimateDailyRewardCollection()
        {
            // TODO: Implement collection animation
            if (dailyRewardBanner != null)
            {
                dailyRewardBanner.SetActive(false);
            }
        }

#if UNITY_EDITOR
        // Editor-only: Setup default references
        private void Reset()
        {
            topBar = GetComponentInChildren<TopBar>();
            bottomNavigation = GetComponentInChildren<BottomNavigation>();
        }
#endif
    }
}
