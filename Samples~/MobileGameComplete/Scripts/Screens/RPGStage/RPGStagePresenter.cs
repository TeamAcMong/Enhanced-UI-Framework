using System;
using System.Linq;
using UnityEngine;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.RPGStage
{
    /// <summary>
    /// RPG Stage Presenter - Business logic for RPG stage screen
    /// Handles character selection, party management, and battle initiation
    /// </summary>
    public class RPGStagePresenter
    {
        private readonly RPGStageModel _model;
        private readonly RPGStageView _view;

        public RPGStagePresenter(RPGStageView view)
        {
            _view = view;
            _model = new RPGStageModel();

            // Get player data from singleton
            _model.playerData = GameState.Instance.PlayerData;

            // Subscribe to view events
            _view.OnCharacterSelected += HandleCharacterSelected;
            _view.OnCharacterRemoved += HandleCharacterRemoved;
            _view.OnAutoSelectPressed += HandleAutoSelect;
            _view.OnClearPartyPressed += HandleClearParty;
            _view.OnStartBattlePressed += HandleStartBattle;
            _view.OnBackPressed += HandleBackPressed;

            // Initial setup
            Initialize();
        }

        /// <summary>
        /// Initialize presenter
        /// </summary>
        private void Initialize()
        {
            Debug.Log("[RPGStagePresenter] Initialize");

            // Display stage info
            _view.DisplayStageInfo(_model.currentStage);

            // Display boss info
            _view.DisplayBossInfo(_model.currentBoss);

            // Display rewards
            _view.DisplayRewards(_model.currentStage.rewards);

            // Display available characters
            UpdateCharacterRosterDisplay();

            // Display empty party
            UpdatePartyDisplay();

            // Update battle button state
            UpdateBattleButtonState();
        }

        /// <summary>
        /// Cleanup presenter
        /// </summary>
        public void Cleanup()
        {
            Debug.Log("[RPGStagePresenter] Cleanup");

            // Unsubscribe from view events
            _view.OnCharacterSelected -= HandleCharacterSelected;
            _view.OnCharacterRemoved -= HandleCharacterRemoved;
            _view.OnAutoSelectPressed -= HandleAutoSelect;
            _view.OnClearPartyPressed -= HandleClearParty;
            _view.OnStartBattlePressed -= HandleStartBattle;
            _view.OnBackPressed -= HandleBackPressed;
        }

        #region Event Handlers

        /// <summary>
        /// Handle character selection from roster
        /// </summary>
        private void HandleCharacterSelected(CharacterData character)
        {
            Debug.Log($"[RPGStagePresenter] Character selected: {character.name}");

            if (!_model.CanAddToParty(character))
            {
                if (!character.isUnlocked)
                {
                    _view.ShowFeedback($"{character.name} is locked!");
                }
                else if (_model.IsPartyFull())
                {
                    _view.ShowFeedback("Party is full! Remove a character first.");
                }
                else if (_model.selectedParty.Contains(character))
                {
                    _view.ShowFeedback($"{character.name} is already in the party!");
                }
                return;
            }

            // Add to party
            _model.selectedParty.Add(character);
            _view.ShowFeedback($"{character.name} added to party!");

            // Update displays
            UpdatePartyDisplay();
            UpdateBattleButtonState();

            Debug.Log($"[RPGStagePresenter] Party size: {_model.selectedParty.Count}/{_model.maxPartySize}");
        }

        /// <summary>
        /// Handle character removal from party
        /// </summary>
        private void HandleCharacterRemoved(CharacterData character)
        {
            Debug.Log($"[RPGStagePresenter] Remove character: {character.name}");

            if (!_model.CanRemoveFromParty(character))
            {
                _view.ShowFeedback($"{character.name} is not in the party!");
                return;
            }

            // Remove from party
            _model.selectedParty.Remove(character);
            _view.ShowFeedback($"{character.name} removed from party!");

            // Update displays
            UpdatePartyDisplay();
            UpdateBattleButtonState();

            Debug.Log($"[RPGStagePresenter] Party size: {_model.selectedParty.Count}/{_model.maxPartySize}");
        }

        /// <summary>
        /// Handle auto-select party
        /// </summary>
        private void HandleAutoSelect()
        {
            Debug.Log("[RPGStagePresenter] Auto-select party");

            // Clear current party
            _model.selectedParty.Clear();

            // Select best characters (unlocked, highest level)
            var bestCharacters = _model.availableCharacters
                .Where(c => c.isUnlocked)
                .OrderByDescending(c => c.level)
                .ThenByDescending(c => c.attack + c.defense)
                .Take(_model.maxPartySize)
                .ToList();

            _model.selectedParty.AddRange(bestCharacters);

            _view.ShowFeedback($"Auto-selected {_model.selectedParty.Count} characters!");

            // Update displays
            UpdatePartyDisplay();
            UpdateBattleButtonState();
        }

        /// <summary>
        /// Handle clear party
        /// </summary>
        private void HandleClearParty()
        {
            Debug.Log("[RPGStagePresenter] Clear party");

            _model.selectedParty.Clear();
            _view.ShowFeedback("Party cleared!");

            // Update displays
            UpdatePartyDisplay();
            UpdateBattleButtonState();
        }

        /// <summary>
        /// Handle start battle
        /// </summary>
        private void HandleStartBattle()
        {
            Debug.Log("[RPGStagePresenter] Start battle");

            if (!_model.CanStartBattle())
            {
                if (_model.selectedParty.Count == 0)
                {
                    _view.ShowFeedback("Select at least one character!");
                }
                else if (_model.playerData.energy < _model.currentStage.energyCost)
                {
                    _view.ShowFeedback("Not enough energy!");
                }
                return;
            }

            // Spend energy
            GameState.Instance.SpendCurrency(CurrencyType.Energy, _model.currentStage.energyCost);

            // In a real implementation, this would load the battle scene
            _view.ShowFeedback("Starting battle...");

            // Simulate battle (in real game, this would transition to battle screen)
            Debug.Log($"[RPGStagePresenter] Battle started with {_model.selectedParty.Count} characters against {_model.currentBoss.name}");
            Debug.Log($"[RPGStagePresenter] Party composition:");
            foreach (var character in _model.selectedParty)
            {
                Debug.Log($"  - {character.name} (Lv.{character.level}, {character.role})");
            }
        }

        /// <summary>
        /// Handle back button press
        /// </summary>
        private void HandleBackPressed()
        {
            Debug.Log("[RPGStagePresenter] Back button pressed");
            NavigationManager.Instance?.GoBack();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Update character roster display
        /// </summary>
        private void UpdateCharacterRosterDisplay()
        {
            _view.DisplayCharacterRoster(_model.availableCharacters.ToArray());
        }

        /// <summary>
        /// Update party display
        /// </summary>
        private void UpdatePartyDisplay()
        {
            _view.DisplaySelectedParty(_model.selectedParty.ToArray(), _model.maxPartySize);
        }

        /// <summary>
        /// Update battle button state
        /// </summary>
        private void UpdateBattleButtonState()
        {
            bool canStart = _model.CanStartBattle();
            string reason = "";

            if (_model.selectedParty.Count == 0)
            {
                reason = "Select Characters";
            }
            else if (_model.playerData.energy < _model.currentStage.energyCost)
            {
                reason = "Not Enough Energy";
            }

            _view.UpdateStartBattleButton(canStart, reason);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Refresh the screen (reload data)
        /// </summary>
        public void Refresh()
        {
            Debug.Log("[RPGStagePresenter] Refresh");

            // Reload player data
            _model.playerData = GameState.Instance.PlayerData;

            // Update displays
            UpdateCharacterRosterDisplay();
            UpdatePartyDisplay();
            UpdateBattleButtonState();
        }

        /// <summary>
        /// Get current model state (for testing)
        /// </summary>
        public RPGStageModel GetModel()
        {
            return _model;
        }

        /// <summary>
        /// Get party statistics
        /// </summary>
        public string GetPartyStats()
        {
            if (_model.selectedParty.Count == 0)
                return "No party selected";

            int totalHp = _model.selectedParty.Sum(c => c.hp);
            int totalAttack = _model.selectedParty.Sum(c => c.attack);
            int totalDefense = _model.selectedParty.Sum(c => c.defense);
            double avgLevel = _model.selectedParty.Average(c => c.level);

            return $"Party Stats - Avg Level: {avgLevel:F1}, Total HP: {totalHp}, Total ATK: {totalAttack}, Total DEF: {totalDefense}";
        }

        #endregion
    }
}
