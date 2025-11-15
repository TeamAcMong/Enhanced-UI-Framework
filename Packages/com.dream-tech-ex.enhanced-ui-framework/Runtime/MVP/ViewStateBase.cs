using System;

namespace EnhancedUI.MVP
{
    /// <summary>
    /// Base class for ViewState with property change notification
    /// </summary>
    public abstract class ViewStateBase
    {
        public event Action OnStateChanged;

        protected void NotifyStateChanged()
        {
            OnStateChanged?.Invoke();
        }
    }

#if EUI_UNIRX_SUPPORT
    /// <summary>
    /// ViewState with UniRx ReactiveProperty support
    /// </summary>
    public abstract class ReactiveViewStateBase : ViewStateBase
    {
        // Can use UniRx.ReactiveProperty here
        // Example:
        // public ReactiveProperty<string> Title { get; } = new ReactiveProperty<string>();
    }
#endif
}
