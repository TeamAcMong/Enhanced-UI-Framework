using UnityEngine;

namespace EnhancedUI.Transition
{
    /// <summary>
    /// Simple transition animation with easing.
    /// Animates position, scale, and alpha.
    /// </summary>
    [CreateAssetMenu(fileName = "SimpleTransition", menuName = "Enhanced UI/Transition/Simple Transition", order = 1)]
    public class SimpleTransitionAnimationObject : TransitionAnimationObject
    {
        [SerializeField] private float delay = 0f;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private EaseType easeType = EaseType.QuarticEaseOut;

        [Header("Before State")]
        [SerializeField] private AlignmentType beforeAlignment = AlignmentType.Center;
        [SerializeField] private Vector3 beforeScale = Vector3.one;
        [SerializeField] private float beforeAlpha = 1f;

        [Header("After State")]
        [SerializeField] private AlignmentType afterAlignment = AlignmentType.Center;
        [SerializeField] private Vector3 afterScale = Vector3.one;
        [SerializeField] private float afterAlpha = 1f;

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private Vector2 _beforePosition;
        private Vector2 _afterPosition;

        public override float Duration => delay + duration;

        public override void Setup(RectTransform rectTransform)
        {
            _rectTransform = rectTransform;
            _canvasGroup = rectTransform.GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>();
            }

            var rect = _rectTransform.rect;
            _beforePosition = GetAlignmentPosition(beforeAlignment, rect);
            _afterPosition = GetAlignmentPosition(afterAlignment, rect);
        }

        public override void SetTime(float time)
        {
            if (_rectTransform == null)
                return;

            var progress = Mathf.Clamp01((time - delay) / duration);
            var easedProgress = EaseUtility.Ease(progress, easeType);

            // Position
            _rectTransform.anchoredPosition = Vector2.Lerp(_beforePosition, _afterPosition, easedProgress);

            // Scale
            _rectTransform.localScale = Vector3.Lerp(beforeScale, afterScale, easedProgress);

            // Alpha
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = Mathf.Lerp(beforeAlpha, afterAlpha, easedProgress);
            }
        }

        private Vector2 GetAlignmentPosition(AlignmentType alignment, Rect rect)
        {
            switch (alignment)
            {
                case AlignmentType.Left:
                    return new Vector2(-rect.width, 0f);
                case AlignmentType.Right:
                    return new Vector2(rect.width, 0f);
                case AlignmentType.Top:
                    return new Vector2(0f, rect.height);
                case AlignmentType.Bottom:
                    return new Vector2(0f, -rect.height);
                case AlignmentType.Center:
                default:
                    return Vector2.zero;
            }
        }
    }

    public enum AlignmentType
    {
        Left,
        Right,
        Top,
        Bottom,
        Center
    }

    public enum EaseType
    {
        Linear,
        QuadraticEaseIn,
        QuadraticEaseOut,
        QuadraticEaseInOut,
        CubicEaseIn,
        CubicEaseOut,
        CubicEaseInOut,
        QuarticEaseIn,
        QuarticEaseOut,
        QuarticEaseInOut,
        QuinticEaseIn,
        QuinticEaseOut,
        QuinticEaseInOut
    }

    public static class EaseUtility
    {
        public static float Ease(float t, EaseType easeType)
        {
            switch (easeType)
            {
                case EaseType.Linear:
                    return t;
                case EaseType.QuadraticEaseIn:
                    return t * t;
                case EaseType.QuadraticEaseOut:
                    return t * (2 - t);
                case EaseType.QuadraticEaseInOut:
                    return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
                case EaseType.CubicEaseIn:
                    return t * t * t;
                case EaseType.CubicEaseOut:
                    return (--t) * t * t + 1;
                case EaseType.CubicEaseInOut:
                    return t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;
                case EaseType.QuarticEaseIn:
                    return t * t * t * t;
                case EaseType.QuarticEaseOut:
                    return 1 - (--t) * t * t * t;
                case EaseType.QuarticEaseInOut:
                    return t < 0.5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;
                case EaseType.QuinticEaseIn:
                    return t * t * t * t * t;
                case EaseType.QuinticEaseOut:
                    return 1 + (--t) * t * t * t * t;
                case EaseType.QuinticEaseInOut:
                    return t < 0.5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;
                default:
                    return t;
            }
        }
    }
}
