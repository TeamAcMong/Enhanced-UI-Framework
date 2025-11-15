using UnityEngine;

namespace EnhancedUI
{
    /// <summary>
    /// Interface for UI containers (Page, Modal, Sheet)
    /// </summary>
    public interface IUIContainer
    {
        /// <summary>
        /// Container name (unique identifier)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// RectTransform of the container
        /// </summary>
        RectTransform RectTransform { get; }

        /// <summary>
        /// Check if container is in transition
        /// </summary>
        bool IsInTransition { get; }

        /// <summary>
        /// Interactivity control
        /// </summary>
        bool Interactable { get; set; }
    }
}
