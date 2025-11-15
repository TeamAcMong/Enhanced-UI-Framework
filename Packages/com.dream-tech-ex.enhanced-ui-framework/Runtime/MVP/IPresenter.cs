using System.Collections;

namespace EnhancedUI.MVP
{
    /// <summary>
    /// Interface for Presenter in MVP pattern
    /// </summary>
    public interface IPresenter
    {
        /// <summary>
        /// Called when presenter is created
        /// </summary>
        void OnCreated();

        /// <summary>
        /// Called when view is loaded (after Initialize)
        /// </summary>
#if EUI_UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask OnViewLoaded();
#else
        IEnumerator OnViewLoaded();
#endif

        /// <summary>
        /// Called when view is about to be destroyed (before Cleanup)
        /// </summary>
#if EUI_UNITASK_SUPPORT
        Cysharp.Threading.Tasks.UniTask OnViewWillDestroy();
#else
        IEnumerator OnViewWillDestroy();
#endif

        /// <summary>
        /// Called when presenter is destroyed
        /// </summary>
        void OnDestroyed();
    }

    /// <summary>
    /// Generic presenter interface
    /// </summary>
    public interface IPresenter<TView, TViewState> : IPresenter
        where TView : class, IView<TViewState>
        where TViewState : class
    {
        /// <summary>
        /// Get the view
        /// </summary>
        TView View { get; }

        /// <summary>
        /// Get the view state
        /// </summary>
        TViewState ViewState { get; }
    }
}
