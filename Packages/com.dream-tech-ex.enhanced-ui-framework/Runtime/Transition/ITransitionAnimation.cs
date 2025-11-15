using UnityEngine;

namespace EnhancedUI.Transition
{
    /// <summary>
    /// Interface for transition animations.
    /// Can be implemented via ScriptableObject or MonoBehaviour.
    /// </summary>
    public interface ITransitionAnimation
    {
        /// <summary>
        /// Duration of the animation in seconds
        /// </summary>
        float Duration { get; }

        /// <summary>
        /// Partner screen's RectTransform (the screen being transitioned from/to)
        /// </summary>
        RectTransform PartnerRectTransform { get; set; }

        /// <summary>
        /// Setup the animation before playing
        /// </summary>
        void Setup(RectTransform rectTransform);

        /// <summary>
        /// Set the animation to a specific time
        /// </summary>
        /// <param name="time">Time in seconds (0 to Duration)</param>
        void SetTime(float time);

        /// <summary>
        /// Called when animation completes
        /// </summary>
        void OnComplete();
    }
}
