using System.Collections;
using EnhancedUI.Lifecycle;
using EnhancedUI.Utilities;

namespace EnhancedUI
{
    /// <summary>
    /// Page screen for stack-based navigation.
    /// Push/Pop operations with history management.
    /// </summary>
    public class Page : Screen, IPageLifecycleEvent
    {
        private readonly LifecycleEventDispatcher<IPageLifecycleEvent> _lifecycleEventDispatcher =
            new LifecycleEventDispatcher<IPageLifecycleEvent>();

        /// <summary>
        /// Add an external lifecycle event listener
        /// </summary>
        /// <param name="lifecycleEvent">Lifecycle event listener</param>
        /// <param name="priority">Execution priority (lower = earlier, 0 = same as override methods)</param>
        public void AddLifecycleEvent(IPageLifecycleEvent lifecycleEvent, int priority = 0)
        {
            _lifecycleEventDispatcher.AddListener(lifecycleEvent, priority);
        }

        /// <summary>
        /// Add lifecycle events using lambda expressions
        /// </summary>
        public void AddLifecycleEvent(
#if EUI_UNITASK_SUPPORT
            System.Func<Cysharp.Threading.Tasks.UniTask> onInitialize = null,
            System.Func<Cysharp.Threading.Tasks.UniTask> onCleanup = null,
#else
            System.Func<IEnumerator> onInitialize = null,
            System.Func<IEnumerator> onCleanup = null,
#endif
            System.Action onWillPushEnter = null,
            System.Action onDidPushEnter = null,
            System.Action onWillPushExit = null,
            System.Action onDidPushExit = null,
            System.Action onWillPopEnter = null,
            System.Action onDidPopEnter = null,
            System.Action onWillPopExit = null,
            System.Action onDidPopExit = null,
            int priority = 0)
        {
            var lifecycleEvent = new AnonymousPageLifecycleEvent
            {
                OnInitialize = onInitialize,
                OnCleanup = onCleanup,
                OnWillPushEnter = onWillPushEnter,
                OnDidPushEnter = onDidPushEnter,
                OnWillPushExit = onWillPushExit,
                OnDidPushExit = onDidPushExit,
                OnWillPopEnter = onWillPopEnter,
                OnDidPopEnter = onDidPopEnter,
                OnWillPopExit = onWillPopExit,
                OnDidPopExit = onDidPopExit
            };

            AddLifecycleEvent(lifecycleEvent, priority);
        }

        /// <summary>
        /// Remove a lifecycle event listener
        /// </summary>
        public bool RemoveLifecycleEvent(IPageLifecycleEvent lifecycleEvent)
        {
            return _lifecycleEventDispatcher.RemoveListener(lifecycleEvent);
        }

        // Lifecycle events with dispatcher support
#if EUI_UNITASK_SUPPORT
        public override Cysharp.Threading.Tasks.UniTask Initialize()
        {
            return _lifecycleEventDispatcher.DispatchEventAsync(x => x.Initialize());
        }

        public override Cysharp.Threading.Tasks.UniTask Cleanup()
        {
            return _lifecycleEventDispatcher.DispatchEventAsync(x => x.Cleanup());
        }

        Cysharp.Threading.Tasks.UniTask IPageLifecycleEvent.Initialize() => Initialize();
        Cysharp.Threading.Tasks.UniTask IPageLifecycleEvent.Cleanup() => Cleanup();
#else
        public override IEnumerator Initialize()
        {
            return _lifecycleEventDispatcher.DispatchEventCoroutine(x => x.Initialize());
        }

        public override IEnumerator Cleanup()
        {
            return _lifecycleEventDispatcher.DispatchEventCoroutine(x => x.Cleanup());
        }

        IEnumerator IPageLifecycleEvent.Initialize() => Initialize();
        IEnumerator IPageLifecycleEvent.Cleanup() => Cleanup();
#endif

        public virtual void WillPushEnter()
        {
            ScreenLogger.LogLifecycle(name, "WillPushEnter", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.WillPushEnter());
        }

        public virtual void DidPushEnter()
        {
            ScreenLogger.LogLifecycle(name, "DidPushEnter", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.DidPushEnter());
        }

        public virtual void WillPushExit()
        {
            ScreenLogger.LogLifecycle(name, "WillPushExit", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.WillPushExit());
        }

        public virtual void DidPushExit()
        {
            ScreenLogger.LogLifecycle(name, "DidPushExit", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.DidPushExit());
        }

        public virtual void WillPopEnter()
        {
            ScreenLogger.LogLifecycle(name, "WillPopEnter", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.WillPopEnter());
        }

        public virtual void DidPopEnter()
        {
            ScreenLogger.LogLifecycle(name, "DidPopEnter", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.DidPopEnter());
        }

        public virtual void WillPopExit()
        {
            ScreenLogger.LogLifecycle(name, "WillPopExit", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.WillPopExit());
        }

        public virtual void DidPopExit()
        {
            ScreenLogger.LogLifecycle(name, "DidPopExit", this);
            _lifecycleEventDispatcher.DispatchEvent(x => x.DidPopExit());
        }
    }
}
