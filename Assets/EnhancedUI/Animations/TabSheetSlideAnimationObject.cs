using EnhancedUI.Transition;
using UnityEngine;

namespace EnhancedUI.Demo.Animations
{
    [CreateAssetMenu(fileName = "TabSheetSlideAnimationObject", menuName = "Enhanced UI/Transition/TabSheetSlideAnimation", order = 1)]
    public class TabSheetSlideAnimationObject : TransitionAnimationObject
    {
        [Header("Animation Settings")]
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private EaseType easeType = EaseType.QuadraticEaseOut;

        [Header("Animation Type")]
        [SerializeField] private bool isEnterAnimation = true;

        private RectTransform _rectTransform;
        private Vector2 _beforePosition;
        private Vector2 _afterPosition;
        private int _tabIndex;
        private int _partnerTabIndex;

        public override float Duration => duration;

        public override void Setup(RectTransform rectTransform)
        {
            _rectTransform = rectTransform;

            // Get tab indices from ITabContent interface
            var tabContent = _rectTransform.GetComponent<ITabContent>();
            var partnerTabContent = PartnerRectTransform?.GetComponent<ITabContent>();

            if (tabContent != null)
            {
                _tabIndex = tabContent.TabIndex;
            }
            else
            {
                Debug.LogWarning($"[TabSheetSlideAnimation] {_rectTransform.name} does not implement ITabContent!");
                _tabIndex = 0;
            }

            if (partnerTabContent != null)
            {
                _partnerTabIndex = partnerTabContent.TabIndex;
            }
            else
            {
                // No partner (first sheet being shown)
                _partnerTabIndex = _tabIndex;
            }

            // Determine slide direction based on tab order
            AlignmentType beforeAlignment;
            AlignmentType afterAlignment;

            if (isEnterAnimation)
            {
                // Enter animation: slide in from off-screen
                if (_tabIndex < _partnerTabIndex)
                {
                    // Moving to a left tab: enter from left
                    beforeAlignment = AlignmentType.Left;
                }
                else if (_tabIndex > _partnerTabIndex)
                {
                    // Moving to a right tab: enter from right
                    beforeAlignment = AlignmentType.Right;
                }
                else
                {
                    // Same tab (initial load): already centered
                    beforeAlignment = AlignmentType.Center;
                }

                afterAlignment = AlignmentType.Center;
            }
            else
            {
                // Exit animation: slide out to off-screen
                beforeAlignment = AlignmentType.Center;

                if (_tabIndex < _partnerTabIndex)
                {
                    // Moving away from a left tab: exit to left
                    afterAlignment = AlignmentType.Left;
                }
                else if (_tabIndex > _partnerTabIndex)
                {
                    // Moving away from a right tab: exit to right
                    afterAlignment = AlignmentType.Right;
                }
                else
                {
                    // Same tab: stay centered
                    afterAlignment = AlignmentType.Center;
                }
            }

            // Calculate positions
            _beforePosition = GetAlignmentPosition(beforeAlignment, _rectTransform.rect);
            _afterPosition = GetAlignmentPosition(afterAlignment, _rectTransform.rect);

            // Debug logging
            if (Application.isEditor)
            {
                Debug.Log($"[TabSheetSlideAnimation] Setup: {_rectTransform.name} " +
                         $"(Tab {_tabIndex}) {(isEnterAnimation ? "Enter" : "Exit")} " +
                         $"Partner: Tab {_partnerTabIndex} | " +
                         $"{beforeAlignment} → {afterAlignment}");
            }
        }

        public override void SetTime(float time)
        {
            if (_rectTransform == null)
                return;

            if (duration <= 0f)
            {
                // Instant transition
                _rectTransform.anchoredPosition = _afterPosition;
                return;
            }

            // Calculate progress with easing
            var progress = Mathf.Clamp01(time / duration);
            progress = EaseUtility.Ease(progress, easeType);

            // Interpolate position
            var position = Vector2.Lerp(_beforePosition, _afterPosition, progress);
            _rectTransform.anchoredPosition = position;
        }

        /// <summary>
        /// Get position based on alignment type
        /// </summary>
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
}