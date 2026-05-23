using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using EnhancedUI.Demo.Components;

namespace EnhancedUI.Demo.Screens.Battle
{
    /// <summary>
    /// View for Battle Arena Screen
    /// </summary>
    public class BattleArenaView : MonoBehaviour
    {
        [Header("Common Components")]
        [SerializeField] private TopBar topBar;
        [SerializeField] private BottomNavigation bottomNavigation;
        [SerializeField] private Button backButton;

        [Header("Battle Selection")]
        [SerializeField] private Transform battleListContainer;
        [SerializeField] private Button battleButtonPrefab;
        [SerializeField] private TextMeshProUGUI battleNameText;
        [SerializeField] private TextMeshProUGUI battleDescriptionText;
        [SerializeField] private TextMeshProUGUI rewardsText;

        [Header("Cost Display")]
        [SerializeField] private TextMeshProUGUI costText;

        [Header("Play Button")]
        [SerializeField] private Button playButton;
        [SerializeField] private TextMeshProUGUI playButtonText;

        [Header("Leaderboard")]
        [SerializeField] private Transform leaderboardContainer;
        [SerializeField] private GameObject leaderboardEntryPrefab;
        [SerializeField] private Button viewFullLeaderboardButton;

        private BattleArenaPresenter _presenter;
        private List<Button> _battleButtons = new List<Button>();
        private List<GameObject> _leaderboardEntries = new List<GameObject>();

        private void Awake()
        {
            SetupButtonListeners();
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
            if (backButton != null)
                backButton.onClick.AddListener(OnBackButtonClicked);

            if (playButton != null)
                playButton.onClick.AddListener(OnPlayButtonClicked);

            if (viewFullLeaderboardButton != null)
                viewFullLeaderboardButton.onClick.AddListener(OnViewFullLeaderboardClicked);

            if (bottomNavigation != null)
                bottomNavigation.OnTabChanged += OnBottomNavTabChanged;
        }

        /// <summary>
        /// Cleanup button listeners
        /// </summary>
        private void CleanupButtonListeners()
        {
            if (backButton != null)
                backButton.onClick.RemoveListener(OnBackButtonClicked);

            if (playButton != null)
                playButton.onClick.RemoveListener(OnPlayButtonClicked);

            if (viewFullLeaderboardButton != null)
                viewFullLeaderboardButton.onClick.RemoveListener(OnViewFullLeaderboardClicked);

            if (bottomNavigation != null)
                bottomNavigation.OnTabChanged -= OnBottomNavTabChanged;

            // Cleanup dynamic buttons
            foreach (var button in _battleButtons)
            {
                if (button != null)
                    button.onClick.RemoveAllListeners();
            }
        }

        /// <summary>
        /// Set the presenter
        /// </summary>
        public void SetPresenter(BattleArenaPresenter presenter)
        {
            _presenter = presenter;
        }

        /// <summary>
        /// Update the view with model data
        /// </summary>
        public void UpdateView(BattleArenaModel model)
        {
            // Update selected battle info
            UpdateBattleInfo(model);

            // Update cost
            if (costText != null)
                costText.text = model.GetBattleCostText();

            // Update play button
            bool canAfford = model.CanAffordBattle();
            if (playButton != null)
                playButton.interactable = canAfford;

            if (playButtonText != null)
                playButtonText.text = canAfford ? "PLAY" : "Insufficient Resources";

            // Update top bar
            if (topBar != null)
                topBar.UpdatePlayerInfo();

            // Select battle tab in bottom navigation
            if (bottomNavigation != null)
                bottomNavigation.SelectTab("battle", false);
        }

        /// <summary>
        /// Update battle info display
        /// </summary>
        private void UpdateBattleInfo(BattleArenaModel model)
        {
            var battle = model.GetSelectedBattle();
            if (battle == null) return;

            if (battleNameText != null)
                battleNameText.text = $"{battle.battleId}. {battle.battleName}";

            if (battleDescriptionText != null)
                battleDescriptionText.text = battle.description;

            if (rewardsText != null)
                rewardsText.text = $"Rewards: {battle.rewards}";
        }

        /// <summary>
        /// Create battle selection buttons
        /// </summary>
        public void CreateBattleButtons(BattleArenaModel model)
        {
            if (battleListContainer == null || battleButtonPrefab == null)
            {
                Debug.LogWarning("[BattleArenaView] Battle list container or prefab is missing");
                return;
            }

            // Clear existing buttons
            foreach (var button in _battleButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            _battleButtons.Clear();

            // Create buttons for each battle type
            for (int i = 0; i < model.battleTypes.Count; i++)
            {
                var battle = model.battleTypes[i];
                var buttonObj = Instantiate(battleButtonPrefab, battleListContainer);
                var button = buttonObj.GetComponent<Button>();

                // Setup button text
                var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = $"{battle.battleId}. {battle.battleName}";

                // Setup interactability
                button.interactable = battle.isUnlocked;

                // Show lock icon if locked
                var lockIcon = buttonObj.transform.Find("LockIcon");
                if (lockIcon != null)
                    lockIcon.gameObject.SetActive(!battle.isUnlocked);

                // Add listener
                int index = i; // Capture for closure
                button.onClick.AddListener(() => OnBattleButtonClicked(index));

                _battleButtons.Add(button);
            }

            Debug.Log($"[BattleArenaView] Created {_battleButtons.Count} battle buttons");
        }

        /// <summary>
        /// Create leaderboard entries
        /// </summary>
        public void CreateLeaderboard(BattleArenaModel model)
        {
            if (leaderboardContainer == null || leaderboardEntryPrefab == null)
            {
                Debug.LogWarning("[BattleArenaView] Leaderboard container or prefab is missing");
                return;
            }

            // Clear existing entries
            foreach (var entry in _leaderboardEntries)
            {
                if (entry != null)
                    Destroy(entry);
            }
            _leaderboardEntries.Clear();

            // Create entries for top 5
            foreach (var entry in model.leaderboard)
            {
                var entryObj = Instantiate(leaderboardEntryPrefab, leaderboardContainer);

                // Setup entry text
                var rankText = entryObj.transform.Find("Rank")?.GetComponent<TextMeshProUGUI>();
                var nameText = entryObj.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                var scoreText = entryObj.transform.Find("Score")?.GetComponent<TextMeshProUGUI>();

                if (rankText != null)
                    rankText.text = $"#{entry.rank}";

                if (nameText != null)
                    nameText.text = entry.playerName;

                if (scoreText != null)
                    scoreText.text = entry.score.ToString("N0");

                _leaderboardEntries.Add(entryObj);
            }

            Debug.Log($"[BattleArenaView] Created {_leaderboardEntries.Count} leaderboard entries");
        }

        /// <summary>
        /// Highlight selected battle button
        /// </summary>
        public void HighlightBattleButton(int index)
        {
            for (int i = 0; i < _battleButtons.Count; i++)
            {
                var button = _battleButtons[i];
                if (button == null) continue;

                // Simple highlight: scale up selected button
                float scale = (i == index) ? 1.1f : 1.0f;
                button.transform.localScale = Vector3.one * scale;

                // Change color
                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = (i == index) ? new Color(1f, 1f, 0.5f) : Color.white;
                }
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
        }

        /// <summary>
        /// Hide the view
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        #region Button Handlers

        private void OnBackButtonClicked()
        {
            Debug.Log("[BattleArenaView] Back button clicked");
            _presenter?.OnBackButtonPressed();
        }

        private void OnBattleButtonClicked(int index)
        {
            Debug.Log($"[BattleArenaView] Battle button {index} clicked");
            _presenter?.OnBattleSelected(index);
        }

        private void OnPlayButtonClicked()
        {
            Debug.Log("[BattleArenaView] Play button clicked");
            _presenter?.OnPlayButtonPressed();
        }

        private void OnViewFullLeaderboardClicked()
        {
            Debug.Log("[BattleArenaView] View full leaderboard clicked");
            _presenter?.OnViewFullLeaderboardPressed();
        }

        private void OnBottomNavTabChanged(int index, string tabId)
        {
            Debug.Log($"[BattleArenaView] Bottom nav changed to: {tabId}");
            _presenter?.OnBottomNavTabChanged(tabId);
        }

        #endregion

#if UNITY_EDITOR
        private void Reset()
        {
            topBar = GetComponentInChildren<TopBar>();
            bottomNavigation = GetComponentInChildren<BottomNavigation>();
        }
#endif
    }
}
