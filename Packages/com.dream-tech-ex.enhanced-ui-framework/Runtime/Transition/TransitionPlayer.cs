using System;
using System.Collections;
using UnityEngine;

namespace EnhancedUI.Transition
{
    /// <summary>
    /// Plays transition animations with support for coroutine-based timing
    /// </summary>
    public class TransitionPlayer
    {
        private ITransitionAnimation _animation;
        private RectTransform _target;
        private float _startTime;
        private bool _isPlaying;

        /// <summary>
        /// Play a transition animation
        /// </summary>
        public IEnumerator Play(ITransitionAnimation animation, RectTransform target, Action onComplete = null)
        {
            if (animation == null || target == null)
            {
                onComplete?.Invoke();
                yield break;
            }

            _animation = animation;
            _target = target;
            _isPlaying = true;
            _startTime = Time.time;

            // Setup animation
            try
            {
                animation.Setup(target);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                _isPlaying = false;
                onComplete?.Invoke();
                yield break;
            }

            // Play animation
            float duration = animation.Duration;
            while (_isPlaying && Time.time - _startTime < duration)
            {
                float elapsed = Time.time - _startTime;
                try
                {
                    animation.SetTime(elapsed);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    break;
                }
                yield return null;
            }

            // Complete animation
            if (_isPlaying)
            {
                try
                {
                    animation.SetTime(duration);
                    animation.OnComplete();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }

            _isPlaying = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Stop the currently playing animation
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
        }

        /// <summary>
        /// Skip to the end of the animation
        /// </summary>
        public void SkipToEnd()
        {
            if (_animation != null && _isPlaying)
            {
                try
                {
                    _animation.SetTime(_animation.Duration);
                    _animation.OnComplete();
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                _isPlaying = false;
            }
        }

        public bool IsPlaying => _isPlaying;
    }
}
