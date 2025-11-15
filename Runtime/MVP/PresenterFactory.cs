using System;
using System.Collections.Generic;

namespace EnhancedUI.MVP
{
    /// <summary>
    /// Factory for creating presenters with dependency injection support
    /// </summary>
    public class PresenterFactory
    {
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<Type, object> _singletons = new Dictionary<Type, object>();

        /// <summary>
        /// Register a factory function for a presenter type
        /// </summary>
        public void Register<TPresenter>(Func<TPresenter> factory) where TPresenter : IPresenter
        {
            _factories[typeof(TPresenter)] = () => factory();
        }

        /// <summary>
        /// Register a singleton instance
        /// </summary>
        public void RegisterSingleton<TPresenter>(TPresenter instance) where TPresenter : IPresenter
        {
            _singletons[typeof(TPresenter)] = instance;
        }

        /// <summary>
        /// Create a presenter instance
        /// </summary>
        public TPresenter Create<TPresenter>() where TPresenter : IPresenter
        {
            var type = typeof(TPresenter);

            // Check singleton first
            if (_singletons.TryGetValue(type, out var singleton))
            {
                return (TPresenter)singleton;
            }

            // Try factory
            if (_factories.TryGetValue(type, out var factory))
            {
                var instance = (TPresenter)factory();
                instance.OnCreated();
                return instance;
            }

            throw new InvalidOperationException($"No factory registered for presenter type: {type.Name}");
        }

        /// <summary>
        /// Clear all registrations
        /// </summary>
        public void Clear()
        {
            _factories.Clear();
            _singletons.Clear();
        }
    }
}
