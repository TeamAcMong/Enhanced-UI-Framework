using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace EnhancedUI.Demo.Screens.Gameplay
{
    /// <summary>
    /// View for Gameplay Screen
    /// </summary>
    public class GameplayView : MonoBehaviour
    {
        [Header("3D Viewport")]
        [SerializeField] private RectTransform viewportContainer;
        [SerializeField] private Camera gameplayCamera;

        [Header("HUD - Top")]
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private Button pauseButton;

        [Header("Resources Display")]
        [SerializeField] private TextMeshProUGUI woodText;
        [SerializeField] private TextMeshProUGUI stoneText;
        [SerializeField] private TextMeshProUGUI ironText;
        [SerializeField] private Image woodIcon;
        [SerializeField] private Image stoneIcon;
        [SerializeField] private Image ironIcon;

        [Header("Upgrade Slots")]
        [SerializeField] private Transform upgradeSlotsContainer;
        [SerializeField] private List<UpgradeSlotView> upgradeSlots;

        [Header("Progress")]
        [SerializeField] private Image progressBar;
        [SerializeField] private TextMeshProUGUI progressText;

        [Header("Pause Menu")]
        [SerializeField] private GameObject pauseMenuPanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button quitButton;

        [Header("Victory/Defeat")]
        [SerializeField] private GameObject victoryPanel;
        [SerializeField] private GameObject defeatPanel;
        [SerializeField] private TextMeshProUGUI victoryScoreText;
        [SerializeField] private GameObject[] victoryStars; // 3 stars
        [SerializeField] private Button victoryNextButton;
        [SerializeField] private Button victoryRetryButton;
        [SerializeField] private Button defeatRetryButton;
        [SerializeField] private Button defeatQuitButton;

        private GameplayPresenter _presenter;

        private void Awake()
        {
            SetupButtonListeners();
            HideAllPanels();
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
            if (pauseButton != null)
                pauseButton.onClick.AddListener(OnPauseButtonClicked);

            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeButtonClicked);

            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartButtonClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitButtonClicked);

            if (victoryNextButton != null)
                victoryNextButton.onClick.AddListener(OnVictoryNextButtonClicked);

            if (victoryRetryButton != null)
                victoryRetryButton.onClick.AddListener(OnVictoryRetryButtonClicked);

            if (defeatRetryButton != null)
                defeatRetryButton.onClick.AddListener(OnDefeatRetryButtonClicked);

            if (defeatQuitButton != null)
                defeatQuitButton.onClick.AddListener(OnDefeatQuitButtonClicked);

            // Setup upgrade slot buttons
            foreach (var slot in upgradeSlots)
            {
                if (slot != null && slot.upgradeButton != null)
                {
                    int slotId = slot.slotId;
                    slot.upgradeButton.onClick.AddListener(() => OnUpgradeButtonClicked(slotId));
                }
            }
        }

        /// <summary>
        /// Cleanup button listeners
        /// </summary>
        private void CleanupButtonListeners()
        {
            if (pauseButton != null)
                pauseButton.onClick.RemoveListener(OnPauseButtonClicked);

            if (resumeButton != null)
                resumeButton.onClick.RemoveListener(OnResumeButtonClicked);

            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartButtonClicked);

            if (quitButton != null)
                quitButton.onClick.RemoveListener(OnQuitButtonClicked);

            if (victoryNextButton != null)
                victoryNextButton.onClick.RemoveListener(OnVictoryNextButtonClicked);

            if (victoryRetryButton != null)
                victoryRetryButton.onClick.RemoveListener(OnVictoryRetryButtonClicked);

            if (defeatRetryButton != null)
                defeatRetryButton.onClick.RemoveListener(OnDefeatRetryButtonClicked);

            if (defeatQuitButton != null)
                defeatQuitButton.onClick.RemoveListener(OnDefeatQuitButtonClicked);

            foreach (var slot in upgradeSlots)
            {
                if (slot != null && slot.upgradeButton != null)
                {
                    slot.upgradeButton.onClick.RemoveAllListeners();
                }
            }
        }

        /// <summary>
        /// Hide all panels
        /// </summary>
        public void HideAllPanels()
        {
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
            if (victoryPanel != null) victoryPanel.SetActive(false);
            if (defeatPanel != null) defeatPanel.SetActive(false);
        }

        /// <summary>
        /// Set the presenter
        /// </summary>
        public void SetPresenter(GameplayPresenter presenter)
        {
            _presenter = presenter;
        }

        /// <summary>
        /// Update the view with model data
        /// </summary>
        public void UpdateView(GameplayModel model)
        {
            // Update wave info
            if (waveText != null)
                waveText.text = $"Wave {model.waveNumber}/{model.totalWaves}";

            // Update timer
            if (timerText != null)
            {
                int minutes = Mathf.FloorToInt(model.levelTimer / 60f);
                int seconds = Mathf.FloorToInt(model.levelTimer % 60f);
                timerText.text = $"{minutes:00}:{seconds:00}";
            }

            // Update score
            if (scoreText != null)
                scoreText.text = $"Score: {model.currentScore}";

            // Update resources
            UpdateResources(model);

            // Update upgrade slots
            UpdateUpgradeSlots(model);

            // Update progress
            if (progressBar != null)
                progressBar.fillAmount = model.GetCompletionPercentage();

            if (progressText != null)
                progressText.text = $"{model.enemiesKilled}/{model.enemiesTotal}";
        }

        /// <summary>
        /// Update resources display
        /// </summary>
        private void UpdateResources(GameplayModel model)
        {
            if (woodText != null)
                woodText.text = model.wood.ToString();

            if (stoneText != null)
                stoneText.text = model.stone.ToString();

            if (ironText != null)
                ironText.text = model.iron.ToString();
        }

        /// <summary>
        /// Update upgrade slots
        /// </summary>
        private void UpdateUpgradeSlots(GameplayModel model)
        {
            foreach (var slotView in upgradeSlots)
            {
                if (slotView == null) continue;

                var slotData = model.GetUpgradeSlot(slotView.slotId);
                if (slotData == null) continue;

                // Update name
                if (slotView.upgradeNameText != null)
                    slotView.upgradeNameText.text = slotData.upgradeName;

                // Update level
                if (slotView.upgradeLevelText != null)
                    slotView.upgradeLevelText.text = $"Lv.{slotData.level}/{slotData.maxLevel}";

                // Update cost
                var cost = model.GetUpgradeCost(slotView.slotId);
                if (slotView.costText != null)
                    slotView.costText.text = $"{cost.wood}W {cost.stone}S";

                // Update button state
                bool canAfford = model.CanAffordUpgrade(slotView.slotId);
                bool maxed = slotData.level >= slotData.maxLevel;

                if (slotView.upgradeButton != null)
                {
                    slotView.upgradeButton.interactable = canAfford && !maxed && slotData.isUnlocked;
                }

                // Show/hide locked state
                if (slotView.lockedOverlay != null)
                    slotView.lockedOverlay.SetActive(!slotData.isUnlocked);
            }
        }

        /// <summary>
        /// Show pause menu
        /// </summary>
        public void ShowPauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(true);

            Time.timeScale = 0f; // Pause game
        }

        /// <summary>
        /// Hide pause menu
        /// </summary>
        public void HidePauseMenu()
        {
            if (pauseMenuPanel != null)
                pauseMenuPanel.SetActive(false);

            Time.timeScale = 1f; // Resume game
        }

        /// <summary>
        /// Show victory screen
        /// </summary>
        public void ShowVictory(int score, int starsEarned)
        {
            if (victoryPanel != null)
                victoryPanel.SetActive(true);

            if (victoryScoreText != null)
                victoryScoreText.text = $"Score: {score}";

            // Show stars
            if (victoryStars != null)
            {
                for (int i = 0; i < victoryStars.Length; i++)
                {
                    if (victoryStars[i] != null)
                        victoryStars[i].SetActive(i < starsEarned);
                }
            }

            Time.timeScale = 0f; // Pause game
        }

        /// <summary>
        /// Show defeat screen
        /// </summary>
        public void ShowDefeat()
        {
            if (defeatPanel != null)
                defeatPanel.SetActive(true);

            Time.timeScale = 0f; // Pause game
        }

        /// <summary>
        /// Show the view
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            HideAllPanels();
            Time.timeScale = 1f;
        }

        /// <summary>
        /// Hide the view
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            Time.timeScale = 1f; // Ensure time is restored
        }

        #region Button Handlers

        private void OnPauseButtonClicked()
        {
            Debug.Log("[GameplayView] Pause button clicked");
            _presenter?.OnPauseButtonPressed();
        }

        private void OnResumeButtonClicked()
        {
            Debug.Log("[GameplayView] Resume button clicked");
            _presenter?.OnResumeButtonPressed();
        }

        private void OnRestartButtonClicked()
        {
            Debug.Log("[GameplayView] Restart button clicked");
            _presenter?.OnRestartButtonPressed();
        }

        private void OnQuitButtonClicked()
        {
            Debug.Log("[GameplayView] Quit button clicked");
            _presenter?.OnQuitButtonPressed();
        }

        private void OnUpgradeButtonClicked(int slotId)
        {
            Debug.Log($"[GameplayView] Upgrade button clicked for slot {slotId}");
            _presenter?.OnUpgradeButtonPressed(slotId);
        }

        private void OnVictoryNextButtonClicked()
        {
            Debug.Log("[GameplayView] Victory Next button clicked");
            _presenter?.OnVictoryNextButtonPressed();
        }

        private void OnVictoryRetryButtonClicked()
        {
            Debug.Log("[GameplayView] Victory Retry button clicked");
            _presenter?.OnRestartButtonPressed();
        }

        private void OnDefeatRetryButtonClicked()
        {
            Debug.Log("[GameplayView] Defeat Retry button clicked");
            _presenter?.OnRestartButtonPressed();
        }

        private void OnDefeatQuitButtonClicked()
        {
            Debug.Log("[GameplayView] Defeat Quit button clicked");
            _presenter?.OnQuitButtonPressed();
        }

        #endregion
    }

    /// <summary>
    /// Upgrade slot view component
    /// </summary>
    [System.Serializable]
    public class UpgradeSlotView
    {
        public int slotId;
        public Button upgradeButton;
        public TextMeshProUGUI upgradeNameText;
        public TextMeshProUGUI upgradeLevelText;
        public TextMeshProUGUI costText;
        public GameObject lockedOverlay;
    }
}
