using UnityEngine;

namespace EnhancedUI.Transition
{
    /// <summary>
    /// ScriptableObject-based transition animation.
    /// Can be created as an asset and reused across multiple screens.
    /// </summary>
    public abstract class TransitionAnimationObject : ScriptableObject, ITransitionAnimation
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
