using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedUI.AssetManagement
{
    /// <summary>
    /// Asset loader for pre-registered assets (useful for editor/testing)
    /// </summary>
    public class PreloadedAssetLoader : IAssetLoader
    {
        private readonly Dictionary<string, UnityEngine.Object> _assets = new Dictionary<string, UnityEngine.Object>();

        /// <summary>
        /// Register an asset with a key
        /// </summary>
        public void Register<T>(string key, T asset) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("Cannot register asset with null or empty key");
                return;
            }

            _assets[key] = asset;
        }

        /// <summary>
        /// Unregister an asset
        /// </summary>
        public bool Unregister(string key)
        {
            return _assets.Remove(key);
        }

        /// <summary>
        /// Clear all registered assets
        /// </summary>
        public void Clear()
        {
            _assets.Clear();
        }

        public AssetLoadHandle<T> Load<T>(string key) where T : UnityEngine.Object
        {
            if (_assets.TryGetValue(key, out var asset))
            {
                if (asset is T typedAsset)
                {
                    return new ImmediateAssetLoadHandle<T>(typedAsset);
                }
                else
                {
                    return new ImmediateAssetLoadHandle<T>(
                        new Exception($"Asset '{key}' is not of type {typeof(T).Name}"));
                }
            }
            else
            {
                return new ImmediateAssetLoadHandle<T>(
                    new Exception($"Asset '{key}' not found in preloaded assets"));
            }
        }

        public AssetLoadHandle<T> LoadAsync<T>(string key) where T : UnityEngine.Object
        {
            // Preloaded assets are always immediately available
            return Load<T>(key);
        }

        public void Release(AssetLoadHandle handle)
        {
            // Preloaded assets are not destroyed
        }
    }
}
