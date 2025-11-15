using System;
using System.Collections;
using EnhancedUI.Lifecycle;
using EnhancedUI.Utilities;

namespace EnhancedUI.MVP
{
    /// <summary>
    /// Base class for Presenters with lifecycle integration
    /// </summary>
    public abstract class PresenterBase<TScreen, TView, TViewState> : IPresenter<TView, TViewState>
        where TScreen : Screen, IView<TViewState>
        where TView : class, IView<TViewState>
        where TViewState : class
    {
        protected TScreen Screen { get; private set; }
        public TView View => Screen as TView;
        public TViewState ViewState => Screen.ViewState;

        protected PresenterBase(TScreen screen)
        {
            Screen = screen ?? throw new ArgumentNullException(nameof(screen));
        }

        public virtual void OnCreated()
        {
            ScreenLogger.Log(ScreenLogger.LogCategory.Info,
                $"Presenter created for {Screen.name}: {GetType().Name}");
        }

#if EUI_UNITASK_SUPPORT
        public virtual Cysharp.Threading.Tasks.UniTask OnViewLoaded()
        {
            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }

        public virtual Cysharp.Threading.Tasks.UniTask OnViewWillDestroy()
        {
            return Cysharp.Threading.Tasks.UniTask.CompletedTask;
        }
#else
        public virtual IEnumerator OnViewLoaded()
        {
            yield break;
        }

        public virtual IEnumerator OnViewWillDestroy()
        {
            yield break;
        }
#endif

        public virtual void OnDestroyed()
        {
            ScreenLogger.Log(ScreenLogger.LogCategory.Info,
                $"Presenter destroyed for {Screen.name}: {GetType().Name}");
        }
    }

    /// <summary>
    /// Presenter base for Page screens
    /// </summary>
    public abstract class PagePresenterBase<TPage, TView, TViewState> : PresenterBase<TPage, TView, TViewState>, IPageLifecycleEvent
        where TPage : Page, IView<TViewState>
        where TView : class, IView<TViewState>
        where TViewState : class
    {
        protected PagePresenterBase(TPage page) : base(page)
        {
            // Register as lifecycle event with priority 1 (after Page's own methods)
            page.AddLifecycleEvent(this, priority: 1);
        }

        // IPageLifecycleEvent implementation - can be overridden

#if EUI_UNITASK_SUPPORT
        public virtual Cysharp.Threading.Tasks.UniTask Initialize()
        {
            return OnViewLoaded();
        }

        public virtual Cysharp.Threading.Tasks.UniTask Cleanup()
        {
            return OnViewWillDestroy();
        }
#else
        public virtual IEnumerator Initialize()
        {
            return OnViewLoaded();
        }

        public virtual IEnumerator Cleanup()
        {
            return OnViewWillDestroy();
        }
#endif

        public virtual void WillPushEnter() { }
        public virtual void DidPushEnter() { }
        public virtual void WillPushExit() { }
        public virtual void DidPushExit() { }
        public virtual void WillPopEnter() { }
        public virtual void DidPopEnter() { }
        public virtual void WillPopExit() { }
        public virtual void DidPopExit() { }
    }

    /// <summary>
    /// Presenter base for Modal screens
    /// </summary>
    public abstract class ModalPresenterBase<TModal, TView, TViewState> : PresenterBase<TModal, TView, TViewState>, IModalLifecycleEvent
        where TModal : Modal, IView<TViewState>
        where TView : class, IView<TViewState>
        where TViewState : class
    {
        protected ModalPresenterBase(TModal modal) : base(modal)
        {
            modal.AddLifecycleEvent(this, priority: 1);
        }

#if EUI_UNITASK_SUPPORT
        public virtual Cysharp.Threading.Tasks.UniTask Initialize()
        {
            return OnViewLoaded();
        }

        public virtual Cysharp.Threading.Tasks.UniTask Cleanup()
        {
            return OnViewWillDestroy();
        }
#else
        public virtual IEnumerator Initialize()
        {
            return OnViewLoaded();
        }

        public virtual IEnumerator Cleanup()
        {
            return OnViewWillDestroy();
        }
#endif

        public virtual void WillPushEnter() { }
        public virtual void DidPushEnter() { }
        public virtual void WillPushExit() { }
        public virtual void DidPushExit() { }
        public virtual void WillPopEnter() { }
        public virtual void DidPopEnter() { }
        public virtual void WillPopExit() { }
        public virtual void DidPopExit() { }
    }

    /// <summary>
    /// Presenter base for Sheet screens
    /// </summary>
    public abstract class SheetPresenterBase<TSheet, TView, TViewState> : PresenterBase<TSheet, TView, TViewState>, ISheetLifecycleEvent
        where TSheet : Sheet, IView<TViewState>
        where TView : class, IView<TViewState>
        where TViewState : class
    {
        protected SheetPresenterBase(TSheet sheet) : base(sheet)
        {
            sheet.AddLifecycleEvent(this, priority: 1);
        }

#if EUI_UNITASK_SUPPORT
        public virtual Cysharp.Threading.Tasks.UniTask Initialize()
        {
            return OnViewLoaded();
        }

        public virtual Cysharp.Threading.Tasks.UniTask Cleanup()
        {
            return OnViewWillDestroy();
        }
#else
        public virtual IEnumerator Initialize()
        {
            return OnViewLoaded();
        }

        public virtual IEnumerator Cleanup()
        {
            return OnViewWillDestroy();
        }
#endif

        public virtual void WillEnter() { }
        public virtual void DidEnter() { }
        public virtual void WillExit() { }
        public virtual void DidExit() { }
    }
}
