using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace EnhancedUI.Demo.Screens.RPGStage
{
    /// <summary>
    /// RPG Stage View - Handles UI display for RPG-style stage with character selection and boss preview
    /// Designed for landscape orientation with horizontal layout
    /// </summary>
    public class RPGStageView : MonoBehaviour
    {
        [Header("Stage Info")]
        [SerializeField] private TextMeshProUGUI stageNumberText;
        [SerializeField] private TextMeshProUGUI stageNameText;
        [SerializeField] private TextMeshProUGUI difficultyText;
        [SerializeField] private TextMeshProUGUI recommendedLevelText;
        [SerializeField] private TextMeshProUGUI energyCostText;

        [Header("Boss Display")]
        [SerializeField] private GameObject bossPanel;
        [SerializeField] private TextMeshProUGUI bossNameText;
        [SerializeField] private TextMeshProUGUI bossLevelText;
        [SerializeField] private Slider bossHpSlider;
        [SerializeField] private TextMeshProUGUI bossHpText;
        [SerializeField] private TextMeshProUGUI bossStatsText;
        [SerializeField] private Transform bossAbilitiesContainer;
        [SerializeField] private GameObject abilityTextPrefab;

        [Header("Character Roster")]
        [SerializeField] private Transform characterRosterContainer;
        [SerializeField] private GameObject characterCardPrefab;

        [Header("Selected Party")]
        [SerializeField] private Transform partyContainer;
        [SerializeField] private TextMeshProUGUI partyCountText;
        [SerializeField] private GameObject[] partySlots;

        [Header("Rewards Preview")]
        [SerializeField] private TextMeshProUGUI rewardsGoldText;
        [SerializeField] private TextMeshProUGUI rewardsExpText;
        [SerializeField] private Transform rewardsLootContainer;

        [Header("Action Buttons")]
        [SerializeField] private Button startBattleButton;
        [SerializeField] private Button autoSelectButton;
        [SerializeField] private Button clearPartyButton;
        [SerializeField] private Button backButton;

        // Events
        public event Action<CharacterData> OnCharacterSelected;
        public event Action<CharacterData> OnCharacterRemoved;
        public event Action OnAutoSelectPressed;
        public event Action OnClearPartyPressed;
        public event Action OnStartBattlePressed;
        public event Action OnBackPressed;

        private RPGStagePresenter _presenter;

        /// <summary>
        /// Set the presenter for this view
        /// </summary>
        public void SetPresenter(RPGStagePresenter presenter)
        {
            _presenter = presenter;
        }

        private void Awake()
        {
            // Setup button listeners
            if (startBattleButton != null)
                startBattleButton.onClick.AddListener(() => OnStartBattlePressed?.Invoke());

            if (autoSelectButton != null)
                autoSelectButton.onClick.AddListener(() => OnAutoSelectPressed?.Invoke());

            if (clearPartyButton != null)
                clearPartyButton.onClick.AddListener(() => OnClearPartyPressed?.Invoke());

            if (backButton != null)
                backButton.onClick.AddListener(() => OnBackPressed?.Invoke());
        }

        private void OnDestroy()
        {
            // Clean up listeners
            if (startBattleButton != null) startBattleButton.onClick.RemoveAllListeners();
            if (autoSelectButton != null) autoSelectButton.onClick.RemoveAllListeners();
            if (clearPartyButton != null) clearPartyButton.onClick.RemoveAllListeners();
            if (backButton != null) backButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Show the view
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            Debug.Log("[RPGStageView] Show");
        }

        /// <summary>
        /// Hide the view
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
            Debug.Log("[RPGStageView] Hide");
        }

        /// <summary>
        /// Display stage information
        /// </summary>
        public void DisplayStageInfo(StageInfo stage)
        {
            if (stageNumberText != null)
                stageNumberText.text = $"Stage {stage.stageNumber}";

            if (stageNameText != null)
                stageNameText.text = stage.stageName;

            if (difficultyText != null)
            {
                difficultyText.text = stage.difficulty;
                // Color code difficulty
                difficultyText.color = stage.difficulty switch
                {
                    "Easy" => Color.green,
                    "Normal" => Color.yellow,
                    "Hard" => new Color(1f, 0.5f, 0f), // Orange
                    "Nightmare" => Color.red,
                    _ => Color.white
                };
            }

            if (recommendedLevelText != null)
                recommendedLevelText.text = $"Lv. {stage.recommendedLevel}+";

            if (energyCostText != null)
                energyCostText.text = $"⚡ {stage.energyCost}";

            Debug.Log($"[RPGStageView] Stage info displayed: {stage.stageName}");
        }

        /// <summary>
        /// Display boss information
        /// </summary>
        public void DisplayBossInfo(BossData boss)
        {
            if (bossNameText != null)
                bossNameText.text = boss.name;

            if (bossLevelText != null)
                bossLevelText.text = $"Lv. {boss.level}";

            if (bossHpSlider != null)
                bossHpSlider.value = boss.GetHpPercentage();

            if (bossHpText != null)
                bossHpText.text = $"{boss.hp:N0} / {boss.maxHp:N0}";

            if (bossStatsText != null)
                bossStatsText.text = $"ATK: {boss.attack} | DEF: {boss.defense}";

            // Display abilities
            if (bossAbilitiesContainer != null && abilityTextPrefab != null)
            {
                foreach (Transform child in bossAbilitiesContainer)
                {
                    Destroy(child.gameObject);
                }

                foreach (var ability in boss.abilities)
                {
                    var abilityObj = Instantiate(abilityTextPrefab, bossAbilitiesContainer);
                    var abilityText = abilityObj.GetComponent<TextMeshProUGUI>();
                    if (abilityText != null)
                    {
                        abilityText.text = $"• {ability}";
                    }
                }
            }

            Debug.Log($"[RPGStageView] Boss info displayed: {boss.name}");
        }

        /// <summary>
        /// Display available characters
        /// </summary>
        public void DisplayCharacterRoster(CharacterData[] characters)
        {
            if (characterRosterContainer == null || characterCardPrefab == null)
            {
                Debug.LogWarning("[RPGStageView] Character roster container or prefab not assigned");
                return;
            }

            // Clear existing
            foreach (Transform child in characterRosterContainer)
            {
                Destroy(child.gameObject);
            }

            // Create character cards
            foreach (var character in characters)
            {
                CreateCharacterCard(character, characterRosterContainer, true);
            }

            Debug.Log($"[RPGStageView] Displayed {characters.Length} characters");
        }

        /// <summary>
        /// Display selected party
        /// </summary>
        public void DisplaySelectedParty(CharacterData[] party, int maxPartySize)
        {
            // Update party count
            if (partyCountText != null)
                partyCountText.text = $"Party ({party.Length}/{maxPartySize})";

            // Clear party container
            if (partyContainer != null)
            {
                foreach (Transform child in partyContainer)
                {
                    Destroy(child.gameObject);
                }

                // Add selected characters
                foreach (var character in party)
                {
                    CreateCharacterCard(character, partyContainer, false);
                }
            }

            // Update party slots visual (if using fixed slots)
            if (partySlots != null)
            {
                for (int i = 0; i < partySlots.Length; i++)
                {
                    if (partySlots[i] != null)
                    {
                        partySlots[i].SetActive(i < party.Length);
                    }
                }
            }

            Debug.Log($"[RPGStageView] Party updated: {party.Length}/{maxPartySize}");
        }

        /// <summary>
        /// Create a character card UI
        /// </summary>
        private void CreateCharacterCard(CharacterData character, Transform container, bool isRoster)
        {
            GameObject cardObj = Instantiate(characterCardPrefab, container);

            // Find UI components
            var nameText = cardObj.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var levelText = cardObj.transform.Find("LevelText")?.GetComponent<TextMeshProUGUI>();
            var roleText = cardObj.transform.Find("RoleText")?.GetComponent<TextMeshProUGUI>();
            var hpText = cardObj.transform.Find("HPText")?.GetComponent<TextMeshProUGUI>();
            var statsText = cardObj.transform.Find("StatsText")?.GetComponent<TextMeshProUGUI>();
            var button = cardObj.GetComponent<Button>();
            var lockIcon = cardObj.transform.Find("LockIcon")?.gameObject;

            // Set data
            if (nameText != null)
                nameText.text = character.name;

            if (levelText != null)
                levelText.text = $"Lv.{character.level}";

            if (roleText != null)
            {
                roleText.text = $"{character.GetRoleIcon()} {character.role}";
                if (ColorUtility.TryParseHtmlString(character.GetRoleColor(), out Color roleColor))
                {
                    roleText.color = roleColor;
                }
            }

            if (hpText != null)
                hpText.text = $"HP: {character.hp}/{character.maxHp}";

            if (statsText != null)
                statsText.text = $"ATK: {character.attack} | DEF: {character.defense}";

            // Show lock icon for locked characters
            if (lockIcon != null)
                lockIcon.SetActive(!character.isUnlocked);

            // Setup button
            if (button != null)
            {
                button.interactable = character.isUnlocked;

                if (isRoster)
                {
                    button.onClick.AddListener(() => OnCharacterSelected?.Invoke(character));
                }
                else
                {
                    button.onClick.AddListener(() => OnCharacterRemoved?.Invoke(character));
                }
            }
        }

        /// <summary>
        /// Display rewards preview
        /// </summary>
        public void DisplayRewards(StageRewards rewards)
        {
            if (rewardsGoldText != null)
                rewardsGoldText.text = $"💰 {rewards.gold:N0}";

            if (rewardsExpText != null)
                rewardsExpText.text = $"⭐ {rewards.experience:N0} EXP";

            if (rewardsLootContainer != null && abilityTextPrefab != null)
            {
                foreach (Transform child in rewardsLootContainer)
                {
                    Destroy(child.gameObject);
                }

                foreach (var item in rewards.lootItems)
                {
                    var itemObj = Instantiate(abilityTextPrefab, rewardsLootContainer);
                    var itemText = itemObj.GetComponent<TextMeshProUGUI>();
                    if (itemText != null)
                    {
                        itemText.text = $"📦 {item}";
                    }
                }
            }

            Debug.Log("[RPGStageView] Rewards displayed");
        }

        /// <summary>
        /// Update start battle button state
        /// </summary>
        public void UpdateStartBattleButton(bool canStart, string reason = "")
        {
            if (startBattleButton != null)
            {
                startBattleButton.interactable = canStart;

                var buttonText = startBattleButton.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = canStart ? "START BATTLE" : (string.IsNullOrEmpty(reason) ? "Cannot Start" : reason);
                }
            }
        }

        /// <summary>
        /// Show feedback message
        /// </summary>
        public void ShowFeedback(string message)
        {
            Debug.Log($"[RPGStageView] Feedback: {message}");
            // In a real implementation, show a toast notification
        }
    }
}
