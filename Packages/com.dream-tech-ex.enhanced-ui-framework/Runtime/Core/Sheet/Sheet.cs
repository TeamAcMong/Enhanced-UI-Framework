using System.Collections;
using EnhancedUI.Lifecycle;
using EnhancedUI.Utilities;
#if EUI_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#endif
namespace EnhancedUI
{
    /// <summary>
    /// Sheet screen for tab-like navigation.
    /// Show/Hide operations without history.
    /// </summary>
    public class Sheet : Screen, ISheetLifecycleEvent
    {
        private readonly LifecycleEventDispatcher<ISheetLifecycleEvent> _lifecycleEventDispatcher =
            new LifecycleEventDispatcher<ISheetLifecycleEvent>();

        /// <summary>
        /// Add an external lifecycle event listener
        /// </summary>
        public void AddLifecycleEvent(ISheetLifecycleEvent lifecycleEvent, int priority = 0)
        {
            _lifecycleEventDispatcher.AddListener(lifecycleEvent, priority);
        }

        /// <summary>
        /// Add lifecycle events using lambda expressions
        /// </summary>
        public void AddLifecycleEvent(
#if EUI_UNITASK_SUPPORT
            System.Func<UniTask> onInitialize = null,
            System.Func<UniTask> onCleanup = null,
#else
            System.Func<IEnumerator> onInitialize = null,
            System.Func<IEnumerator> onCleanup = null,
#endif
            System.Action onWillEnter = null,
            System.Action onDidEnter = null,
            System.Action onWillExit = null,
            System.Action onDidExit = null,
            int priority = 0)
        {
            var lifecycleEvent = new AnonymousSheetLifecycleEvent
            {
                OnInitialize = onInitialize,
                OnCleanup = onCleanup,
                OnWillEnter = onWillEnter,
                OnDidEnter = onDidEnter,
                OnWillExit = onWillExit,
                OnDidExit = onDidExit
            };

            AddLifecycleEvent(lifecycleEvent, priority);
        }

        public bool RemoveLifecycleEvent(ISheetLifecycleEvent lifecycleEvent)
        {
            return _lifecycleEventDispatcher.RemoveListener(lifecycleEvent);
        }

        // Lifecycle events
#if EUI_UNITASK_SUPPORT
        public override UniTask Initialize()
        {
            return _lifecycleEventDispatcher.DispatchEventAsync(x => x.Initialize());
        }

        public override UniTask Cleanup()
        {
            return _lifecycleEventDispatcher.DispatchEventAsync(x => x.Cleanup());
        }

        UniTask ISheetLifecycleEvent.Initialize() => Initialize();
        UniTask ISheetLifecycleEvent.Cleanup() => Cleanup();
#else
        public override IEnumerator Initialize()
        {
            return _lifecycleEventDispatcher.DispatchEventCoroutine(x => x.Initialize());
        }

        public override IEnumerator Cleanup()
        {
            return _lifecycleEventDispatcher.DispatchEventCoroutine(x => x.Cleanup());
        }

        IEnumerator ISheetLifecycleEvent.Initialize() => Initialize();
        IEnumerator ISheetLifecycleEvent.Cleanup() => Cleanup();
#endif

        public virtual void WillEnter()
        {
            ScreenLogger.LogLifecycle(name, "WillEnter", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.WillEnter());
        }

        public virtual void DidEnter()
        {
            ScreenLogger.LogLifecycle(name, "DidEnter", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.DidEnter());
        }

        public virtual void WillExit()
        {
            ScreenLogger.LogLifecycle(name, "WillExit", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.WillExit());
        }

        public virtual void DidExit()
        {
            ScreenLogger.LogLifecycle(name, "DidExit", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.DidExit());
        }
    }
}
