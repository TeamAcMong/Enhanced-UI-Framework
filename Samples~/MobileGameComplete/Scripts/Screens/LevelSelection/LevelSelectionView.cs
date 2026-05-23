using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using EnhancedUI.Demo.Components;

namespace EnhancedUI.Demo.Screens.LevelSelection
{
    /// <summary>
    /// View for Level Selection Screen
    /// </summary>
    public class LevelSelectionView : MonoBehaviour
    {
        [Header("Common Components")]
        [SerializeField] private TopBar topBar;
        [SerializeField] private Button backButton;

        [Header("Chapter Selection")]
        [SerializeField] private Transform chapterButtonsContainer;
        [SerializeField] private Button chapterButtonPrefab;
        [SerializeField] private TextMeshProUGUI chapterNameText;
        [SerializeField] private TextMeshProUGUI chapterProgressText;
        [SerializeField] private Image chapterProgressBar;

        [Header("Level Grid")]
        [SerializeField] private ScrollRect levelScrollView;
        [SerializeField] private GridLayoutGroup levelGridLayout;
        [SerializeField] private Transform levelGridContainer;
        [SerializeField] private LevelButton levelButtonPrefab;

        [Header("Kingdom Pass Banner")]
        [SerializeField] private GameObject kingdomPassBanner;
        [SerializeField] private TextMeshProUGUI kingdomPassProgressText;
        [SerializeField] private Image kingdomPassProgressBar;
        [SerializeField] private Button kingdomPassButton;

        [Header("Play Button")]
        [SerializeField] private GameObject playButtonPanel;
        [SerializeField] private Button playButton;
        [SerializeField] private TextMeshProUGUI playButtonText;
        [SerializeField] private TextMeshProUGUI energyCostText;

        private LevelSelectionPresenter _presenter;
        private List<Button> _chapterButtons = new List<Button>();
        private List<LevelButton> _levelButtons = new List<LevelButton>();
        private int _currentDisplayedChapter = -1;

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

            if (kingdomPassButton != null)
                kingdomPassButton.onClick.AddListener(OnKingdomPassButtonClicked);
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

            if (kingdomPassButton != null)
                kingdomPassButton.onClick.RemoveListener(OnKingdomPassButtonClicked);

            // Cleanup dynamic buttons
            foreach (var button in _chapterButtons)
            {
                if (button != null)
                    button.onClick.RemoveAllListeners();
            }

            foreach (var levelButton in _levelButtons)
            {
                if (levelButton != null)
                    levelButton.OnLevelButtonClicked -= OnLevelButtonClicked;
            }
        }

        /// <summary>
        /// Set the presenter
        /// </summary>
        public void SetPresenter(LevelSelectionPresenter presenter)
        {
            _presenter = presenter;
        }

        /// <summary>
        /// Update the view with model data
        /// </summary>
        public void UpdateView(LevelSelectionModel model)
        {
            // Update chapter display
            UpdateChapterInfo(model);

            // Update Kingdom Pass banner
            UpdateKingdomPass(model);

            // Update top bar
            if (topBar != null)
                topBar.UpdatePlayerInfo();

            // Hide play button panel initially
            if (playButtonPanel != null)
                playButtonPanel.SetActive(false);
        }

        /// <summary>
        /// Update chapter information display
        /// </summary>
        private void UpdateChapterInfo(LevelSelectionModel model)
        {
            var chapter = model.GetChapter(model.selectedChapter);
            if (chapter == null) return;

            // Update chapter name
            if (chapterNameText != null)
                chapterNameText.text = chapter.chapterName;

            // Update chapter progress
            int starsEarned = model.GetChapterStars(model.selectedChapter);
            int maxStars = model.GetChapterMaxStars(model.selectedChapter);

            if (chapterProgressText != null)
                chapterProgressText.text = $"{starsEarned}/{maxStars} Stars";

            if (chapterProgressBar != null)
            {
                float progress = maxStars > 0 ? (float)starsEarned / maxStars : 0f;
                chapterProgressBar.fillAmount = progress;
            }
        }

        /// <summary>
        /// Update Kingdom Pass display
        /// </summary>
        private void UpdateKingdomPass(LevelSelectionModel model)
        {
            if (kingdomPassBanner != null)
                kingdomPassBanner.SetActive(model.isKingdomPassActive);

            if (model.isKingdomPassActive)
            {
                if (kingdomPassProgressText != null)
                    kingdomPassProgressText.text = $"{model.kingdomPassProgress}/{model.kingdomPassMax}";

                if (kingdomPassProgressBar != null)
                {
                    float progress = model.kingdomPassMax > 0
                        ? (float)model.kingdomPassProgress / model.kingdomPassMax
                        : 0f;
                    kingdomPassProgressBar.fillAmount = progress;
                }
            }
        }

        /// <summary>
        /// Create chapter selection buttons
        /// </summary>
        public void CreateChapterButtons(LevelSelectionModel model)
        {
            if (chapterButtonsContainer == null || chapterButtonPrefab == null)
            {
                Debug.LogWarning("[LevelSelectionView] Chapter buttons container or prefab is missing");
                return;
            }

            // Clear existing buttons
            foreach (var button in _chapterButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            _chapterButtons.Clear();

            // Create buttons for each chapter
            foreach (var chapter in model.chapters)
            {
                var buttonObj = Instantiate(chapterButtonPrefab, chapterButtonsContainer);
                var button = buttonObj.GetComponent<Button>();

                // Setup button
                var buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                    buttonText.text = $"{chapter.chapterId}";

                // Setup interactability
                button.interactable = chapter.isUnlocked;

                // Add listener
                int chapterId = chapter.chapterId; // Capture for closure
                button.onClick.AddListener(() => OnChapterButtonClicked(chapterId));

                _chapterButtons.Add(button);
            }

            Debug.Log($"[LevelSelectionView] Created {_chapterButtons.Count} chapter buttons");
        }

        /// <summary>
        /// Create level grid for selected chapter
        /// </summary>
        public void CreateLevelGrid(LevelSelectionModel model)
        {
            if (levelGridContainer == null || levelButtonPrefab == null)
            {
                Debug.LogWarning("[LevelSelectionView] Level grid container or prefab is missing");
                return;
            }

            var chapter = model.GetChapter(model.selectedChapter);
            if (chapter == null) return;

            // Check if we need to recreate
            if (_currentDisplayedChapter == model.selectedChapter && _levelButtons.Count > 0)
            {
                // Just update existing buttons
                UpdateLevelButtons(model);
                return;
            }

            // Clear existing buttons
            foreach (var levelButton in _levelButtons)
            {
                if (levelButton != null)
                {
                    levelButton.OnLevelButtonClicked -= OnLevelButtonClicked;
                    Destroy(levelButton.gameObject);
                }
            }
            _levelButtons.Clear();

            // Create buttons for each level
            foreach (var level in chapter.levels)
            {
                var buttonObj = Instantiate(levelButtonPrefab, levelGridContainer);
                var levelButton = buttonObj.GetComponent<LevelButton>();

                if (levelButton != null)
                {
                    // Setup level button
                    levelButton.Setup(level.levelId, level.levelNumber, !level.isUnlocked, level.starsEarned);

                    // Add listener
                    levelButton.OnLevelButtonClicked += OnLevelButtonClicked;

                    _levelButtons.Add(levelButton);
                }
            }

            _currentDisplayedChapter = model.selectedChapter;

            // Scroll to top
            if (levelScrollView != null)
                levelScrollView.verticalNormalizedPosition = 1f;

            Debug.Log($"[LevelSelectionView] Created {_levelButtons.Count} level buttons for chapter {model.selectedChapter}");
        }

        /// <summary>
        /// Update existing level buttons
        /// </summary>
        private void UpdateLevelButtons(LevelSelectionModel model)
        {
            var chapter = model.GetChapter(model.selectedChapter);
            if (chapter == null) return;

            for (int i = 0; i < _levelButtons.Count && i < chapter.levels.Count; i++)
            {
                var levelButton = _levelButtons[i];
                var levelData = chapter.levels[i];

                if (levelButton != null)
                {
                    levelButton.Setup(levelData.levelId, levelData.levelNumber, !levelData.isUnlocked, levelData.starsEarned);
                }
            }
        }

        /// <summary>
        /// Show level selection (deselect all levels)
        /// </summary>
        public void ShowLevelSelection()
        {
            if (playButtonPanel != null)
                playButtonPanel.SetActive(false);
        }

        /// <summary>
        /// Show play button for selected level
        /// </summary>
        public void ShowPlayButton(string levelId, int energyCost, bool canPlay)
        {
            if (playButtonPanel != null)
                playButtonPanel.SetActive(true);

            if (playButtonText != null)
                playButtonText.text = $"Play Level";

            if (energyCostText != null)
                energyCostText.text = $"-{energyCost} Energy";

            if (playButton != null)
                playButton.interactable = canPlay;
        }

        /// <summary>
        /// Highlight selected chapter button
        /// </summary>
        public void HighlightChapterButton(int chapterId)
        {
            for (int i = 0; i < _chapterButtons.Count; i++)
            {
                var button = _chapterButtons[i];
                if (button == null) continue;

                // Simple highlight: scale up selected button
                int buttonChapterId = i + 1;
                float scale = (buttonChapterId == chapterId) ? 1.2f : 1.0f;
                button.transform.localScale = Vector3.one * scale;
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
            Debug.Log("[LevelSelectionView] Back button clicked");
            _presenter?.OnBackButtonPressed();
        }

        private void OnChapterButtonClicked(int chapterId)
        {
            Debug.Log($"[LevelSelectionView] Chapter {chapterId} button clicked");
            _presenter?.OnChapterSelected(chapterId);
        }

        private void OnLevelButtonClicked(string levelId, int levelNumber)
        {
            Debug.Log($"[LevelSelectionView] Level {levelId} button clicked");
            _presenter?.OnLevelSelected(levelId);
        }

        private void OnPlayButtonClicked()
        {
            Debug.Log("[LevelSelectionView] Play button clicked");
            _presenter?.OnPlayButtonPressed();
        }

        private void OnKingdomPassButtonClicked()
        {
            Debug.Log("[LevelSelectionView] Kingdom Pass button clicked");
            _presenter?.OnKingdomPassButtonPressed();
        }

        #endregion

#if UNITY_EDITOR
        private void Reset()
        {
            topBar = GetComponentInChildren<TopBar>();
        }
#endif
    }
}
