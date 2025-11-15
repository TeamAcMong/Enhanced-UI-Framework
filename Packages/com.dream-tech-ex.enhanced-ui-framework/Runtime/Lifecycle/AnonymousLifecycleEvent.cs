using System;
using System.Collections;
using System.Threading.Tasks;

namespace EnhancedUI.Lifecycle
{
    /// <summary>
    /// Anonymous lifecycle event for Page that uses delegates (lambda support)
    /// </summary>
    public class AnonymousPageLifecycleEvent : IPageLifecycleEvent
    {
#if EUI_UNITASK_SUPPORT
        public Func<Cysharp.Threading.Tasks.UniTask> OnInitialize { get; set; }
        public Func<Cysharp.Threading.Tasks.UniTask> OnCleanup { get; set; }

        public Cysharp.Threading.Tasks.UniTask Initialize()
        {
            return OnInitialize?.Invoke() ?? Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }

        public Cysharp.Threading.Tasks.UniTask Cleanup()
        {
            return OnCleanup?.Invoke() ?? Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }
#else
        public Func<IEnumerator> OnInitialize { get; set; }
        public Func<IEnumerator> OnCleanup { get; set; }

        public IEnumerator Initialize()
        {
            return OnInitialize?.Invoke();
        }

        public IEnumerator Cleanup()
        {
            return OnCleanup?.Invoke();
        }
#endif

        public Action OnWillPushEnter { get; set; }
        public Action OnDidPushEnter { get; set; }
        public Action OnWillPushExit { get; set; }
        public Action OnDidPushExit { get; set; }
        public Action OnWillPopEnter { get; set; }
        public Action OnDidPopEnter { get; set; }
        public Action OnWillPopExit { get; set; }
        public Action OnDidPopExit { get; set; }

        public void WillPushEnter() => OnWillPushEnter?.Invoke();
        public void DidPushEnter() => OnDidPushEnter?.Invoke();
        public void WillPushExit() => OnWillPushExit?.Invoke();
        public void DidPushExit() => OnDidPushExit?.Invoke();
        public void WillPopEnter() => OnWillPopEnter?.Invoke();
        public void DidPopEnter() => OnDidPopEnter?.Invoke();
        public void WillPopExit() => OnWillPopExit?.Invoke();
        public void DidPopExit() => OnDidPopExit?.Invoke();
    }

    /// <summary>
    /// Anonymous lifecycle event for Modal that uses delegates (lambda support)
    /// </summary>
    public class AnonymousModalLifecycleEvent : IModalLifecycleEvent
    {
#if EUI_UNITASK_SUPPORT
        public Func<Cysharp.Threading.Tasks.UniTask> OnInitialize { get; set; }
        public Func<Cysharp.Threading.Tasks.UniTask> OnCleanup { get; set; }

        public Cysharp.Threading.Tasks.UniTask Initialize()
        {
            return OnInitialize?.Invoke() ?? Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }

        public Cysharp.Threading.Tasks.UniTask Cleanup()
        {
            return OnCleanup?.Invoke() ?? Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }
#else
        public Func<IEnumerator> OnInitialize { get; set; }
        public Func<IEnumerator> OnCleanup { get; set; }

        public IEnumerator Initialize()
        {
            return OnInitialize?.Invoke();
        }

        public IEnumerator Cleanup()
        {
            return OnCleanup?.Invoke();
        }
#endif

        public Action OnWillPushEnter { get; set; }
        public Action OnDidPushEnter { get; set; }
        public Action OnWillPushExit { get; set; }
        public Action OnDidPushExit { get; set; }
        public Action OnWillPopEnter { get; set; }
        public Action OnDidPopEnter { get; set; }
        public Action OnWillPopExit { get; set; }
        public Action OnDidPopExit { get; set; }

        public void WillPushEnter() => OnWillPushEnter?.Invoke();
        public void DidPushEnter() => OnDidPushEnter?.Invoke();
        public void WillPushExit() => OnWillPushExit?.Invoke();
        public void DidPushExit() => OnDidPushExit?.Invoke();
        public void WillPopEnter() => OnWillPopEnter?.Invoke();
        public void DidPopEnter() => OnDidPopEnter?.Invoke();
        public void WillPopExit() => OnWillPopExit?.Invoke();
        public void DidPopExit() => OnDidPopExit?.Invoke();
    }

    /// <summary>
    /// Anonymous lifecycle event for Sheet that uses delegates (lambda support)
    /// </summary>
    public class AnonymousSheetLifecycleEvent : ISheetLifecycleEvent
    {
#if EUI_UNITASK_SUPPORT
        public Func<Cysharp.Threading.Tasks.UniTask> OnInitialize { get; set; }
        public Func<Cysharp.Threading.Tasks.UniTask> OnCleanup { get; set; }

        public Cysharp.Threading.Tasks.UniTask Initialize()
        {
            return OnInitialize?.Invoke() ?? Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }

        public Cysharp.Threading.Tasks.UniTask Cleanup()
        {
            return OnCleanup?.Invoke() ?? Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }
#else
        public Func<IEnumerator> OnInitialize { get; set; }
        public Func<IEnumerator> OnCleanup { get; set; }

        public IEnumerator Initialize()
        {
            return OnInitialize?.Invoke();
        }

        public IEnumerator Cleanup()
        {
            return OnCleanup?.Invoke();
        }
#endif

        public Action OnWillEnter { get; set; }
        public Action OnDidEnter { get; set; }
        public Action OnWillExit { get; set; }
        public Action OnDidExit { get; set; }

        public void WillEnter() => OnWillEnter?.Invoke();
        public void DidEnter() => OnDidEnter?.Invoke();
        public void WillExit() => OnWillExit?.Invoke();
        public void DidExit() => OnDidExit?.Invoke();
    }
}
