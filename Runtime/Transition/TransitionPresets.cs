using UnityEngine;

namespace EnhancedUI.Transition
{
    /// <summary>
    /// Factory for creating common transition animation presets
    /// </summary>
    public static class TransitionPresets
    {
        // Default durations
        private const float DEFAULT_DURATION = 0.3f;
        private const float QUICK_DURATION = 0.2f;
        private const float SLOW_DURATION = 0.5f;

        #region Slide Transitions

        /// <summary>
        /// Slide in from left
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideInFromLeft(float duration = DEFAULT_DURATION)
        {
            return CreateSlide(AlignmentType.Left, AlignmentType.Center, duration, EaseType.QuarticEaseOut);
        }

        /// <summary>
        /// Slide in from right
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideInFromRight(float duration = DEFAULT_DURATION)
        {
            return CreateSlide(AlignmentType.Right, AlignmentType.Center, duration, EaseType.QuarticEaseOut);
        }

        /// <summary>
        /// Slide in from top
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideInFromTop(float duration = DEFAULT_DURATION)
        {
            return CreateSlide(AlignmentType.Top, AlignmentType.Center, duration, EaseType.QuarticEaseOut);
        }

        /// <summary>
        /// Slide in from bottom
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideInFromBottom(float duration = DEFAULT_DURATION)
        {
            return CreateSlide(AlignmentType.Bottom, AlignmentType.Center, duration, EaseType.QuarticEaseOut);
        }

        /// <summary>
        /// Slide out to left
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideOutToLeft(float duration = DEFAULT_DURATION)
        {
            return CreateSlide(AlignmentType.Center, AlignmentType.Left, duration, EaseType.QuarticEaseIn);
        }

        /// <summary>
        /// Slide out to right
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideOutToRight(float duration = DEFAULT_DURATION)
        {
            return CreateSlide(AlignmentType.Center, AlignmentType.Right, duration, EaseType.QuarticEaseIn);
        }

        /// <summary>
        /// Slide out to top
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideOutToTop(float duration = DEFAULT_DURATION)
        {
            return CreateSlide(AlignmentType.Center, AlignmentType.Top, duration, EaseType.QuarticEaseIn);
        }

        /// <summary>
        /// Slide out to bottom
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideOutToBottom(float duration = DEFAULT_DURATION)
        {
            return CreateSlide(AlignmentType.Center, AlignmentType.Bottom, duration, EaseType.QuarticEaseIn);
        }

        #endregion

        #region Fade Transitions

        /// <summary>
        /// Fade in (alpha 0 to 1)
        /// </summary>
        public static SimpleTransitionAnimationObject CreateFadeIn(float duration = QUICK_DURATION)
        {
            return CreateFade(0f, 1f, duration);
        }

        /// <summary>
        /// Fade out (alpha 1 to 0)
        /// </summary>
        public static SimpleTransitionAnimationObject CreateFadeOut(float duration = QUICK_DURATION)
        {
            return CreateFade(1f, 0f, duration);
        }

        #endregion

        #region Scale Transitions

        /// <summary>
        /// Zoom in (scale 0 to 1 with fade)
        /// </summary>
        public static SimpleTransitionAnimationObject CreateZoomIn(float duration = DEFAULT_DURATION)
        {
            return CreateScale(Vector3.zero, Vector3.one, 0f, 1f, duration, EaseType.QuarticEaseOut);
        }

        /// <summary>
        /// Zoom out (scale 1 to 0 with fade)
        /// </summary>
        public static SimpleTransitionAnimationObject CreateZoomOut(float duration = DEFAULT_DURATION)
        {
            return CreateScale(Vector3.one, Vector3.zero, 1f, 0f, duration, EaseType.QuarticEaseIn);
        }

        /// <summary>
        /// Pop in (scale 0.3 to 1 with fade) - Great for modals
        /// </summary>
        public static SimpleTransitionAnimationObject CreatePopIn(float duration = DEFAULT_DURATION)
        {
            return CreateScale(Vector3.one * 0.3f, Vector3.one, 0f, 1f, duration, EaseType.QuarticEaseOut);
        }

        /// <summary>
        /// Pop out (scale 1 to 0.3 with fade)
        /// </summary>
        public static SimpleTransitionAnimationObject CreatePopOut(float duration = DEFAULT_DURATION)
        {
            return CreateScale(Vector3.one, Vector3.one * 0.3f, 1f, 0f, duration, EaseType.QuarticEaseIn);
        }

        /// <summary>
        /// Bounce in (overshoot scale)
        /// </summary>
        public static SimpleTransitionAnimationObject CreateBounceIn(float duration = DEFAULT_DURATION)
        {
            return CreateScale(Vector3.zero, Vector3.one, 0f, 1f, duration, EaseType.QuinticEaseOut);
        }

        #endregion

        #region Combined Transitions

        /// <summary>
        /// Slide in from left with fade
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideInLeftWithFade(float duration = DEFAULT_DURATION)
        {
            return CreateSlideWithFade(AlignmentType.Left, AlignmentType.Center, 0f, 1f, duration);
        }

        /// <summary>
        /// Slide in from right with fade
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideInRightWithFade(float duration = DEFAULT_DURATION)
        {
            return CreateSlideWithFade(AlignmentType.Right, AlignmentType.Center, 0f, 1f, duration);
        }

        /// <summary>
        /// Slide out to left with fade
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideOutLeftWithFade(float duration = DEFAULT_DURATION)
        {
            return CreateSlideWithFade(AlignmentType.Center, AlignmentType.Left, 1f, 0f, duration);
        }

        /// <summary>
        /// Slide out to right with fade
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSlideOutRightWithFade(float duration = DEFAULT_DURATION)
        {
            return CreateSlideWithFade(AlignmentType.Center, AlignmentType.Right, 1f, 0f, duration);
        }

        #endregion

        #region Mobile-Specific Transitions

        /// <summary>
        /// Bottom sheet slide up (common on mobile)
        /// </summary>
        public static SimpleTransitionAnimationObject CreateBottomSheetSlideUp(float duration = DEFAULT_DURATION)
        {
            return CreateSlideWithFade(AlignmentType.Bottom, AlignmentType.Center, 0.7f, 1f, duration);
        }

        /// <summary>
        /// Bottom sheet slide down
        /// </summary>
        public static SimpleTransitionAnimationObject CreateBottomSheetSlideDown(float duration = DEFAULT_DURATION)
        {
            return CreateSlideWithFade(AlignmentType.Center, AlignmentType.Bottom, 1f, 0.7f, duration);
        }

        /// <summary>
        /// Side panel slide from left (drawer)
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSidePanelSlideIn(float duration = DEFAULT_DURATION)
        {
            return CreateSlideWithFade(AlignmentType.Left, AlignmentType.Center, 0.5f, 1f, duration);
        }

        /// <summary>
        /// Side panel slide to left
        /// </summary>
        public static SimpleTransitionAnimationObject CreateSidePanelSlideOut(float duration = DEFAULT_DURATION)
        {
            return CreateSlideWithFade(AlignmentType.Center, AlignmentType.Left, 1f, 0.5f, duration);
        }

        #endregion

        #region Instant (No Animation)

        /// <summary>
        /// Instant show (no animation)
        /// </summary>
        public static SimpleTransitionAnimationObject CreateInstant()
        {
            return CreateFade(1f, 1f, 0f);
        }

        #endregion

        #region Private Helpers

        private static SimpleTransitionAnimationObject CreateSlide(
            AlignmentType from,
            AlignmentType to,
            float duration,
            EaseType easeType)
        {
            var transition = ScriptableObject.CreateInstance<SimpleTransitionAnimationObject>();

            // Use reflection to set private fields (since they're serialized fields)
            var type = typeof(SimpleTransitionAnimationObject);
            type.GetField("duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, duration);
            type.GetField("easeType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, easeType);
            type.GetField("beforeAlignment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, from);
            type.GetField("afterAlignment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, to);
            type.GetField("beforeAlpha", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, 1f);
            type.GetField("afterAlpha", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, 1f);

            return transition;
        }

        private static SimpleTransitionAnimationObject CreateFade(
            float fromAlpha,
            float toAlpha,
            float duration)
        {
            var transition = ScriptableObject.CreateInstance<SimpleTransitionAnimationObject>();

            var type = typeof(SimpleTransitionAnimationObject);
            type.GetField("duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, duration);
            type.GetField("easeType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, EaseType.Linear);
            type.GetField("beforeAlpha", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, fromAlpha);
            type.GetField("afterAlpha", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, toAlpha);

            return transition;
        }

        private static SimpleTransitionAnimationObject CreateScale(
            Vector3 fromScale,
            Vector3 toScale,
            float fromAlpha,
            float toAlpha,
            float duration,
            EaseType easeType)
        {
            var transition = ScriptableObject.CreateInstance<SimpleTransitionAnimationObject>();

            var type = typeof(SimpleTransitionAnimationObject);
            type.GetField("duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, duration);
            type.GetField("easeType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, easeType);
            type.GetField("beforeScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, fromScale);
            type.GetField("afterScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, toScale);
            type.GetField("beforeAlpha", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, fromAlpha);
            type.GetField("afterAlpha", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, toAlpha);

            return transition;
        }

        private static SimpleTransitionAnimationObject CreateSlideWithFade(
            AlignmentType from,
            AlignmentType to,
            float fromAlpha,
            float toAlpha,
            float duration)
        {
            var transition = ScriptableObject.CreateInstance<SimpleTransitionAnimationObject>();

            var type = typeof(SimpleTransitionAnimationObject);
            type.GetField("duration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, duration);
            type.GetField("easeType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, EaseType.QuarticEaseOut);
            type.GetField("beforeAlignment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, from);
            type.GetField("afterAlignment", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, to);
            type.GetField("beforeAlpha", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, fromAlpha);
            type.GetField("afterAlpha", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(transition, toAlpha);

            return transition;
        }

        #endregion
    }
}
