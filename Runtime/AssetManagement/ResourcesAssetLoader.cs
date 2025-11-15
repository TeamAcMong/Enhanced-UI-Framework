using System;
using UnityEngine;

namespace EnhancedUI.AssetManagement
{
    /// <summary>
    /// Asset loader that loads from Resources folder
    /// </summary>
    public class ResourcesAssetLoader : IAssetLoader
    {
        public AssetLoadHandle<T> Load<T>(string key) where T : UnityEngine.Object
        {
            try
            {
                var asset = Resources.Load<T>(key);
                if (asset == null)
                {
                    return new ImmediateAssetLoadHandle<T>(
                        new Exception($"Failed to load asset at path: {key}"));
                }
                return new ImmediateAssetLoadHandle<T>(asset);
            }
            catch (Exception ex)
            {
                return new ImmediateAssetLoadHandle<T>(ex);
            }
        }

        public AssetLoadHandle<T> LoadAsync<T>(string key) where T : UnityEngine.Object
        {
            var handle = new AsyncAssetLoadHandle<T>();
            var request = Resources.LoadAsync<T>(key);

            request.completed += _ =>
            {
                if (request.asset == null)
                {
                    handle.CompleteWithError(new Exception($"Failed to load asset at path: {key}"));
                }
                else
                {
                    handle.Complete(request.asset as T);
                }
            };

            return handle;
        }

        public void Release(AssetLoadHandle handle)
        {
            // Resources doesn't require explicit release
            // Assets are unloaded via Resources.UnloadUnusedAssets()
        }
    }
}
