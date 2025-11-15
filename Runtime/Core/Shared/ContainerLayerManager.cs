using System.Collections.Generic;
using UnityEngine;

namespace EnhancedUI
{
    /// <summary>
    /// Manages render order for screens within a container
    /// </summary>
    public class ContainerLayerManager
    {
        private readonly Transform _container;
        private readonly Dictionary<string, int> _screenToSiblingIndex = new Dictionary<string, int>();

        public ContainerLayerManager(Transform container)
        {
            _container = container;
        }

        /// <summary>
        /// Set screen to a specific layer order
        /// </summary>
        public void SetLayer(string screenId, Transform screenTransform, int order)
        {
            _screenToSiblingIndex[screenId] = order;
            screenTransform.SetSiblingIndex(order);
        }

        /// <summary>
        /// Move screen to front (highest layer)
        /// </summary>
        public void MoveToFront(string screenId, Transform screenTransform)
        {
            screenTransform.SetAsLastSibling();
            _screenToSiblingIndex[screenId] = screenTransform.GetSiblingIndex();
        }

        /// <summary>
        /// Move screen to back (lowest layer)
        /// </summary>
        public void MoveToBack(string screenId, Transform screenTransform)
        {
            screenTransform.SetAsFirstSibling();
            _screenToSiblingIndex[screenId] = screenTransform.GetSiblingIndex();
        }

        /// <summary>
        /// Get current layer order for a screen
        /// </summary>
        public int GetLayer(string screenId)
        {
            return _screenToSiblingIndex.TryGetValue(screenId, out var order) ? order : -1;
        }

        /// <summary>
        /// Remove screen from tracking
        /// </summary>
        public void Remove(string screenId)
        {
            _screenToSiblingIndex.Remove(screenId);
        }

        /// <summary>
        /// Clear all tracked screens
        /// </summary>
        public void Clear()
        {
            _screenToSiblingIndex.Clear();
        }
    }
}
