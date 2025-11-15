using UnityEngine;
using UnityEngine.Playables;

namespace EnhancedUI.Transition
{
    /// <summary>
    /// Transition animation that uses Unity Timeline.
    /// Attach to a GameObject with PlayableDirector.
    /// </summary>
    [RequireComponent(typeof(PlayableDirector))]
    public class TimelineTransitionAnimationBehaviour : TransitionAnimationBehaviour
    {
        [SerializeField] private PlayableDirector playableDirector;

        public override float Duration => playableDirector != null && playableDirector.playableAsset != null
            ? (float)playableDirector.playableAsset.duration
            : 0f;

        private void Reset()
        {
            playableDirector = GetComponent<PlayableDirector>();
        }

        private void OnValidate()
        {
            if (playableDirector == null)
            {
                playableDirector = GetComponent<PlayableDirector>();
            }

            if (playableDirector != null && playableDirector.playOnAwake)
            {
                Debug.LogWarning($"[{name}] PlayableDirector.playOnAwake should be disabled for TimelineTransitionAnimationBehaviour");
            }
        }

        public override void Setup(RectTransform rectTransform)
        {
            if (playableDirector == null)
            {
                playableDirector = GetComponent<PlayableDirector>();
            }

            if (playableDirector != null)
            {
                playableDirector.time = 0;
                playableDirector.Evaluate();
            }
        }

        public override void SetTime(float time)
        {
            if (playableDirector == null || playableDirector.playableAsset == null)
                return;

            playableDirector.time = Mathf.Clamp(time, 0, (float)playableDirector.playableAsset.duration);
            playableDirector.Evaluate();
        }

        public override void OnComplete()
        {
            base.OnComplete();

            if (playableDirector != null)
            {
                playableDirector.Stop();
            }
        }
    }
}
