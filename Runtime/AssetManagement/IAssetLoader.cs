using UnityEngine;

namespace EnhancedUI.AssetManagement
{
    /// <summary>
    /// Interface for loading assets (prefabs, ScriptableObjects, etc.)
    /// Implementations: Resources, Addressables, Remote, Custom
    /// </summary>
    public interface IAssetLoader
    {
        /// <summary>
        /// Load asset synchronously (blocking)
        /// </summary>
        AssetLoadHandle<T> Load<T>(string key) where T : Object;

        /// <summary>
        /// Load asset asynchronously (non-blocking)
        /// </summary>
        AssetLoadHandle<T> LoadAsync<T>(string key) where T : Object;

        /// <summary>
        /// Release a loaded asset
        /// </summary>
        void Release(AssetLoadHandle handle);
    }
}
