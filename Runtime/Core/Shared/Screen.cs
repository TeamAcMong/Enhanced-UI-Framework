using System.Collections;
using UnityEngine;
using EnhancedUI.Lifecycle;
using EnhancedUI.Transition;
using EnhancedUI.Utilities;

namespace EnhancedUI
{
    /// <summary>
    /// Base class for all screens (Page, Modal, Sheet)
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class Screen : MonoBehaviour
    {
        [SerializeField] private TransitionAnimationContainer pushEnterAnimationContainer = new TransitionAnimationContainer();
        [SerializeField] private TransitionAnimationContainer pushExitAnimationContainer = new TransitionAnimationContainer();
        [SerializeField] private TransitionAnimationContainer popEnterAnimationContainer = new TransitionAnimationContainer();
        [SerializeField] private TransitionAnimationContainer popExitAnimationContainer = new TransitionAnimationContainer();

        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private string _identifier;
        private bool _isInitialized;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();
                return _rectTransform;
            }
        }

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (_canvasGroup == null)
                    _canvasGroup = GetComponent<CanvasGroup>();
                return _canvasGroup;
            }
        }

        /// <summary>
        /// Unique identifier for this screen instance
        /// </summary>
        public string Identifier
        {
            get => _identifier;
            internal set => _identifier = value;
        }

        /// <summary>
        /// Check if Initialize() has been called
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Interactivity control
        /// </summary>
        public bool Interactable
        {
            get => CanvasGroup.interactable;
            set => CanvasGroup.interactable = value;
        }

        /// <summary>
        /// Alpha control
        /// </summary>
        public float Alpha
        {
            get => CanvasGroup.alpha;
            set => CanvasGroup.alpha = value;
        }

        protected virtual void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        protected virtual void OnDestroy()
        {
            if (EnhancedUISettings.Instance.callCleanupWhenDestroy && _isInitialized)
            {
                StartCoroutine(InternalCleanup());
            }
        }

        /// <summary>
        /// Called after loading, before first display
        /// </summary>
#if EUI_UNITASK_SUPPORT
        public virtual Cysharp.Threading.Tasks.UniTask Initialize()
        {
            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator Initialize()
        {
            yield break;
        }
#endif

        /// <summary>
        /// Called before destruction
        /// </summary>
#if EUI_UNITASK_SUPPORT
        public virtual Cysharp.Threading.Tasks.UniTask Cleanup()
        {
            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator Cleanup()
        {
            yield break;
        }
#endif

        internal IEnumerator InternalInitialize()
        {
            if (_isInitialized)
                yield break;

            ScreenLogger.LogLifecycle(name, "Initialize", this);

#if EUI_UNITASK_SUPPORT
            yield return Cysharp.Threading.Tasks.UniTask.ToCoroutine(() => Initialize());
#else
            yield return Initialize();
#endif

            _isInitialized = true;
        }

        internal IEnumerator InternalCleanup()
        {
            if (!_isInitialized)
                yield break;

            ScreenLogger.LogLifecycle(name, "Cleanup", this);

#if EUI_UNITASK_SUPPORT
            yield return Cysharp.Threading.Tasks.UniTask.ToCoroutine(() => Cleanup());
#else
            yield return Cleanup();
#endif

            _isInitialized = false;
        }

        /// <summary>
        /// Get transition animation for push enter
        /// </summary>
        public ITransitionAnimation GetPushEnterAnimation(string partnerIdentifier)
        {
            return pushEnterAnimationContainer.GetAnimation(partnerIdentifier);
        }

        /// <summary>
        /// Get transition animation for push exit
        /// </summary>
        public ITransitionAnimation GetPushExitAnimation(string partnerIdentifier)
        {
            return pushExitAnimationContainer.GetAnimation(partnerIdentifier);
        }

        /// <summary>
        /// Get transition animation for pop enter
        /// </summary>
        public ITransitionAnimation GetPopEnterAnimation(string partnerIdentifier)
        {
            return popEnterAnimationContainer.GetAnimation(partnerIdentifier);
        }

        /// <summary>
        /// Get transition animation for pop exit
        /// </summary>
        public ITransitionAnimation GetPopExitAnimation(string partnerIdentifier)
        {
            return popExitAnimationContainer.GetAnimation(partnerIdentifier);
        }
    }
}
