using System;
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
                beforeAlignment = _tabIndex < _partnerTabIndex ? AlignmentType.Left : AlignmentType.Right;
                afterAlignment = AlignmentType.Center;
            }
            else
            {
                beforeAlignment = AlignmentType.Center;
                afterAlignment = _tabIndex < _partnerTabIndex ? AlignmentType.Left : AlignmentType.Right;
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
            var progress = duration <= 0.0f ? 1.0f : Mathf.Clamp01(time / duration);
            progress = EaseUtility.Ease(progress, easeType);
            var position = Vector3.Lerp(_beforePosition, _afterPosition, progress);
            _rectTransform.anchoredPosition = position;
        }

        /// <summary>
        /// Get position based on alignment type
        /// </summary>
        private Vector2 GetAlignmentPosition(AlignmentType alignment, Rect rect)
        {
            Vector3 position;
            var width = rect.width;
            var height = rect.height;
            switch (alignment)
            {
                case AlignmentType.Left:
                    position = new Vector3(-width, 0, 0);
                    break;
                case AlignmentType.Top:
                    position = new Vector3(0, height, 0);
                    break;
                case AlignmentType.Right:
                    position = new Vector3(width, 0, 0);
                    break;
                case AlignmentType.Bottom:
                    position = new Vector3(0, -height, 0);
                    break;
                case AlignmentType.Center:
                    position = new Vector3(0, 0, 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }

            return position;
        }
    }
}