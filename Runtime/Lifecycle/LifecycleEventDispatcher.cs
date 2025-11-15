using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.Utilities;
using UnityEngine;

namespace EnhancedUI.Lifecycle
{
    /// <summary>
    /// Dispatches lifecycle events to registered listeners in priority order.
    /// Lower priority values execute first.
    /// </summary>
    public class LifecycleEventDispatcher<TLifecycleEvent> where TLifecycleEvent : ILifecycleEvent
    {
        private readonly PriorityList<TLifecycleEvent> _lifecycleEvents = new PriorityList<TLifecycleEvent>();

        /// <summary>
        /// Add a lifecycle event listener with optional priority
        /// </summary>
        /// <param name="lifecycleEvent">The lifecycle event listener</param>
        /// <param name="priority">Execution priority (lower = earlier, default = 0)</param>
        public void AddListener(TLifecycleEvent lifecycleEvent, int priority = 0)
        {
            if (lifecycleEvent == null)
            {
                ScreenLogger.LogWarning("Attempted to add null lifecycle event");
                return;
            }

            _lifecycleEvents.Add(lifecycleEvent, priority);
        }

        /// <summary>
        /// Remove a lifecycle event listener
        /// </summary>
        public bool RemoveListener(TLifecycleEvent lifecycleEvent)
        {
            return _lifecycleEvents.Remove(lifecycleEvent);
        }

        /// <summary>
        /// Clear all lifecycle event listeners
        /// </summary>
        public void ClearListeners()
        {
            _lifecycleEvents.Clear();
        }

        /// <summary>
        /// Dispatch an event with no return value
        /// </summary>
        public void DispatchEvent(Action<TLifecycleEvent> eventInvoker)
        {
            var events = _lifecycleEvents.GetItems();
            foreach (var lifecycleEvent in events)
            {
                try
                {
                    eventInvoker?.Invoke(lifecycleEvent);
                }
                catch (Exception ex)
                {
                    ScreenLogger.LogException(ex);
                }
            }
        }

        /// <summary>
        /// Dispatch an async event (returns IEnumerator)
        /// </summary>
        public IEnumerator DispatchEventCoroutine(Func<TLifecycleEvent, IEnumerator> eventInvoker)
        {
            var events = _lifecycleEvents.GetItems();
            foreach (var lifecycleEvent in events)
            {
                IEnumerator routine = null;
                try
                {
                    routine = eventInvoker?.Invoke(lifecycleEvent);
                }
                catch (Exception ex)
                {
                    ScreenLogger.LogException(ex);
                }

                if (routine != null)
                {
                    yield return routine;
                }
            }
        }

#if EUI_UNITASK_SUPPORT
        /// <summary>
        /// Dispatch an async event (returns UniTask)
        /// </summary>
        public async Cysharp.Threading.Tasks.UniTask DispatchEventAsync(Func<TLifecycleEvent, Cysharp.Threading.Tasks.UniTask> eventInvoker)
        {
            var events = _lifecycleEvents.GetItems();
            foreach (var lifecycleEvent in events)
            {
                try
                {
                    if (eventInvoker != null)
                    {
                        await eventInvoker.Invoke(lifecycleEvent);
                    }
                }
                catch (Exception ex)
                {
                    ScreenLogger.LogException(ex);
                }
            }
        }
#endif

        /// <summary>
        /// Check if any listeners are registered
        /// </summary>
        public bool HasListeners => _lifecycleEvents.Count > 0;

        /// <summary>
        /// Get the number of registered listeners
        /// </summary>
        public int ListenerCount => _lifecycleEvents.Count;
    }
}
