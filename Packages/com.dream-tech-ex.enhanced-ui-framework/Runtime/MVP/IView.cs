namespace EnhancedUI.MVP
{
    /// <summary>
    /// Interface for View in MVP pattern
    /// </summary>
    public interface IView
    {
        /// <summary>
        /// Called when view is initialized
        /// </summary>
        void OnViewInitialized();

        /// <summary>
        /// Called when view is destroyed
        /// </summary>
        void OnViewDestroyed();
    }

    /// <summary>
    /// Generic view interface with typed ViewState
    /// </summary>
    public interface IView<TViewState> : IView where TViewState : class
    {
        /// <summary>
        /// Get the view state
        /// </summary>
        TViewState ViewState { get; }
    }
}
