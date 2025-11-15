using System;
using System.Collections;
using System.Threading.Tasks;

namespace EnhancedUI.Lifecycle
{
    /// <summary>
    /// Base interface for lifecycle events
    /// </summary>
    public interface ILifecycleEvent
    {
    }

    /// <summary>
    /// Lifecycle events for Page screens
    /// </summary>
    public interface IPageLifecycleEvent : ILifecycleEvent
    {
        /// <summary>
        /// Called after the page is loaded, before first display
        /// </summary>
#if EUI_UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask Initialize();
#else
        IEnumerator Initialize();
#endif

        /// <summary>
        /// Called before the page enters via push operation
        /// </summary>
        void WillPushEnter();

        /// <summary>
        /// Called after the page has entered via push operation
        /// </summary>
        void DidPushEnter();

        /// <summary>
        /// Called before the page exits via push operation (being covered)
        /// </summary>
        void WillPushExit();

        /// <summary>
        /// Called after the page has exited via push operation
        /// </summary>
        void DidPushExit();

        /// <summary>
        /// Called before the page re-enters via pop operation (being uncovered)
        /// </summary>
        void WillPopEnter();

        /// <summary>
        /// Called after the page has re-entered via pop operation
        /// </summary>
        void DidPopEnter();

        /// <summary>
        /// Called before the page exits via pop operation (being destroyed)
        /// </summary>
        void WillPopExit();

        /// <summary>
        /// Called after the page has exited via pop operation
        /// </summary>
        void DidPopExit();

        /// <summary>
        /// Called before the page is destroyed
        /// </summary>
#if EUI_UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask Cleanup();
#else
        IEnumerator Cleanup();
#endif
    }

    /// <summary>
    /// Lifecycle events for Modal screens
    /// </summary>
    public interface IModalLifecycleEvent : ILifecycleEvent
    {
#if EUI_UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask Initialize();
#else
        IEnumerator Initialize();
#endif

        void WillPushEnter();
        void DidPushEnter();
        void WillPushExit();
        void DidPushExit();
        void WillPopEnter();
        void DidPopEnter();
        void WillPopExit();
        void DidPopExit();

#if EUI_UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask Cleanup();
#else
        IEnumerator Cleanup();
#endif
    }

    /// <summary>
    /// Lifecycle events for Sheet screens
    /// </summary>
    public interface ISheetLifecycleEvent : ILifecycleEvent
    {
#if EUI_UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask Initialize();
#else
        IEnumerator Initialize();
#endif

        void WillEnter();
        void DidEnter();
        void WillExit();
        void DidExit();

#if EUI_UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask Cleanup();
#else
        IEnumerator Cleanup();
#endif
    }
}
