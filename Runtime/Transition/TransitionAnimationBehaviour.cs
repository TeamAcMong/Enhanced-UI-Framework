using UnityEngine;

namespace EnhancedUI.Transition
{
    /// <summary>
    /// MonoBehaviour-based transition animation.
    /// Attach to GameObject for more complex animations.
    /// </summary>
    public abstract class TransitionAnimationBehaviour : MonoBehaviour, ITransitionAnimation
    {
        public abstract float Duration { get; }
        public RectTransform PartnerRectTransform { get; set; }

        public abstract void Setup(RectTransform rectTransform);
        public abstract void SetTime(float time);

        public virtual void OnComplete()
        {
            // Override if needed
        }
    }
}
