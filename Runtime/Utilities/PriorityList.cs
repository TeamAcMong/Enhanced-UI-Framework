using System;
using System.Collections.Generic;

namespace EnhancedUI.Utilities
{
    /// <summary>
    /// List that maintains items sorted by priority.
    /// Lower priority values execute first.
    /// </summary>
    public class PriorityList<T>
    {
        private readonly List<PriorityItem<T>> _items = new List<PriorityItem<T>>();
        private bool _isDirty;

        public int Count => _items.Count;

        /// <summary>
        /// Add an item with the specified priority
        /// </summary>
        public void Add(T item, int priority = 0)
        {
            _items.Add(new PriorityItem<T> { Item = item, Priority = priority });
            _isDirty = true;
        }

        /// <summary>
        /// Remove an item
        /// </summary>
        public bool Remove(T item)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (EqualityComparer<T>.Default.Equals(_items[i].Item, item))
                {
                    _items.RemoveAt(i);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Clear all items
        /// </summary>
        public void Clear()
        {
            _items.Clear();
            _isDirty = false;
        }

        /// <summary>
        /// Get all items sorted by priority
        /// </summary>
        public List<T> GetItems()
        {
            if (_isDirty)
            {
                _items.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                _isDirty = false;
            }

            var result = new List<T>(_items.Count);
            foreach (var item in _items)
            {
                result.Add(item.Item);
            }
            return result;
        }

        /// <summary>
        /// Iterate through items in priority order
        /// </summary>
        public void ForEach(Action<T> action)
        {
            var items = GetItems();
            foreach (var item in items)
            {
                action?.Invoke(item);
            }
        }

        /// <summary>
        /// Check if list contains an item
        /// </summary>
        public bool Contains(T item)
        {
            foreach (var priorityItem in _items)
            {
                if (EqualityComparer<T>.Default.Equals(priorityItem.Item, item))
                    return true;
            }
            return false;
        }
    }

    internal struct PriorityItem<T>
    {
        public T Item;
        public int Priority;
    }
}
