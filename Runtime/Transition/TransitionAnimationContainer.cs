using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EnhancedUI.Transition
{
    /// <summary>
    /// Container for screen transition animations with partner-based matching.
    /// Allows different animations based on which screen is transitioning.
    /// </summary>
    [Serializable]
    public class TransitionAnimationContainer
    {
        [SerializeField] private List<TransitionAnimationEntry> entries = new List<TransitionAnimationEntry>();

        /// <summary>
        /// Get animation for a specific partner screen identifier
        /// </summary>
        public ITransitionAnimation GetAnimation(string partnerScreenIdentifier)
        {
            // Try exact match first
            foreach (var entry in entries)
            {
                if (entry.partnerScreenIdentifier == partnerScreenIdentifier)
                {
                    return GetAnimationFromEntry(entry);
                }
            }

            // Try regex match
            foreach (var entry in entries)
            {
                if (entry.isRegex && !string.IsNullOrEmpty(entry.partnerScreenIdentifier))
                {
                    try
                    {
                        if (Regex.IsMatch(partnerScreenIdentifier, entry.partnerScreenIdentifier))
                        {
                            return GetAnimationFromEntry(entry);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"Invalid regex pattern: {entry.partnerScreenIdentifier}\n{ex.Message}");
                    }
                }
            }

            // Return default (first entry with empty identifier)
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.partnerScreenIdentifier))
                {
                    return GetAnimationFromEntry(entry);
                }
            }

            return null;
        }

        private ITransitionAnimation GetAnimationFromEntry(TransitionAnimationEntry entry)
        {
            if (entry.animationObject != null)
                return entry.animationObject;

            if (entry.animationBehaviour != null)
                return entry.animationBehaviour;

            return null;
        }

        /// <summary>
        /// Add an animation entry
        /// </summary>
        public void AddEntry(string partnerIdentifier, TransitionAnimationObject animation, bool isRegex = false)
        {
            entries.Add(new TransitionAnimationEntry
            {
                partnerScreenIdentifier = partnerIdentifier,
                animationObject = animation,
                isRegex = isRegex
            });
        }

        /// <summary>
        /// Add an animation entry (MonoBehaviour version)
        /// </summary>
        public void AddEntry(string partnerIdentifier, TransitionAnimationBehaviour animation, bool isRegex = false)
        {
            entries.Add(new TransitionAnimationEntry
            {
                partnerScreenIdentifier = partnerIdentifier,
                animationBehaviour = animation,
                isRegex = isRegex
            });
        }
    }

    [Serializable]
    public class TransitionAnimationEntry
    {
        [Tooltip("Partner screen identifier (empty = default). Supports regex if isRegex is enabled.")]
        public string partnerScreenIdentifier;

        [Tooltip("ScriptableObject-based animation")]
        public TransitionAnimationObject animationObject;

        [Tooltip("MonoBehaviour-based animation (on the screen GameObject)")]
        public TransitionAnimationBehaviour animationBehaviour;

        [Tooltip("Use regex pattern matching for partner identifier")]
        public bool isRegex;
    }
}
