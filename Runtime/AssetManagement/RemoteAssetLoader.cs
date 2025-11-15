using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using EnhancedUI.Utilities;

namespace EnhancedUI.AssetManagement
{
    /// <summary>
    /// Asset loader that downloads from remote CDN/server with local caching
    /// </summary>
    public class RemoteAssetLoader : IAssetLoader
    {
        private readonly string _baseUrl;
        private readonly string _cacheDirectory;
        private readonly IAssetLoader _fallbackLoader;
        private readonly Dictionary<string, AssetBundle> _loadedBundles = new Dictionary<string, AssetBundle>();
        private readonly int _maxRetries = 3;

        /// <summary>
        /// Create a remote asset loader
        /// </summary>
        /// <param name="baseUrl">Base URL for remote assets (e.g., https://cdn.yourgame.com/ui)</param>
        /// <param name="cacheDirectory">Local cache directory (null = Application.persistentDataPath/AssetCache)</param>
        /// <param name="fallbackLoader">Fallback loader when offline (null = ResourcesAssetLoader)</param>
        public RemoteAssetLoader(string baseUrl, string cacheDirectory = null, IAssetLoader fallbackLoader = null)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _cacheDirectory = cacheDirectory ?? Path.Combine(Application.persistentDataPath, "AssetCache");
            _fallbackLoader = fallbackLoader ?? new ResourcesAssetLoader();

            // Ensure cache directory exists
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
            }

            ScreenLogger.Log(ScreenLogger.LogCategory.AssetLoading,
                $"RemoteAssetLoader initialized: {_baseUrl}");
        }

        public AssetLoadHandle<T> Load<T>(string key) where T : UnityEngine.Object
        {
            // Remote loading must be async, fallback to sync
            ScreenLogger.LogWarning($"RemoteAssetLoader.Load() called synchronously for {key}. Using fallback loader.");
            return _fallbackLoader.Load<T>(key);
        }

        public AssetLoadHandle<T> LoadAsync<T>(string key) where T : UnityEngine.Object
        {
            var handle = new AsyncAssetLoadHandle<T>();
            CoroutineRunner.StartCoroutine(LoadAsyncRoutine(key, handle));
            return handle;
        }

        private IEnumerator LoadAsyncRoutine<T>(string key, AsyncAssetLoadHandle<T> handle) where T : UnityEngine.Object
        {
            // Check if already loaded from bundle
            var bundleName = GetBundleName(key);
            if (_loadedBundles.TryGetValue(bundleName, out var existingBundle))
            {
                var asset = existingBundle.LoadAsset<T>(GetAssetName(key));
                if (asset != null)
                {
                    handle.Complete(asset);
                    yield break;
                }
            }

            // Try download with retries
            AssetBundle bundle = null;
            Exception lastException = null;

            for (int retry = 0; retry < _maxRetries; retry++)
            {
                if (retry > 0)
                {
                    ScreenLogger.Log(ScreenLogger.LogCategory.AssetLoading,
                        $"Retry {retry}/{_maxRetries} for {key}");
                    yield return new WaitForSeconds(Mathf.Pow(2, retry)); // Exponential backoff
                }

                var downloadResult = DownloadBundle(bundleName);
                yield return downloadResult;

                if (downloadResult.Error == null)
                {
                    bundle = downloadResult.Bundle;
                    lastException = null;
                    break;
                }
                else
                {
                    lastException = downloadResult.Error;
                }
            }

            // Check if download succeeded
            if (bundle == null)
            {
                ScreenLogger.LogWarning($"Failed to download {key} after {_maxRetries} retries. Using fallback.");

                // Fallback to local loader
                var fallbackHandle = _fallbackLoader.LoadAsync<T>(key);
                yield return fallbackHandle;

                if (fallbackHandle.HasError)
                {
                    handle.CompleteWithError(new Exception(
                        $"Remote download failed and fallback failed: {lastException?.Message}", lastException));
                }
                else
                {
                    handle.Complete(fallbackHandle.Result);
                }
                yield break;
            }

            // Load asset from bundle
            _loadedBundles[bundleName] = bundle;
            var loadedAsset = bundle.LoadAsset<T>(GetAssetName(key));

            if (loadedAsset == null)
            {
                handle.CompleteWithError(new Exception($"Asset not found in bundle: {key}"));
            }
            else
            {
                handle.Complete(loadedAsset);
            }
        }

        private DownloadResult DownloadBundle(string bundleName)
        {
            var result = new DownloadResult();
            CoroutineRunner.StartCoroutine(DownloadBundleRoutine(bundleName, result));
            return result;
        }

        private IEnumerator DownloadBundleRoutine(string bundleName, DownloadResult result)
        {
            var url = $"{_baseUrl}/{bundleName}";
            var cachePath = Path.Combine(_cacheDirectory, bundleName);

            ScreenLogger.LogAssetLoading(url, isAsync: true);

            // Check if cached
            if (File.Exists(cachePath))
            {
                // Load from cache
                var cacheRequest = AssetBundle.LoadFromFileAsync(cachePath);
                yield return cacheRequest;

                if (cacheRequest.assetBundle != null)
                {
                    result.Bundle = cacheRequest.assetBundle;
                    result.Complete();
                    ScreenLogger.Log(ScreenLogger.LogCategory.AssetLoading,
                        $"Loaded from cache: {bundleName}");
                    yield break;
                }
            }

            // Download from server
            using (var request = UnityWebRequestAssetBundle.GetAssetBundle(url))
            {
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                {
                    result.Progress = operation.progress;
                    yield return null;
                }

                if (request.result != UnityWebRequest.Result.Success)
                {
                    result.Error = new Exception($"Download failed: {request.error}");
                    result.Complete();
                    yield break;
                }

                var bundle = DownloadHandlerAssetBundle.GetContent(request);
                if (bundle == null)
                {
                    result.Error = new Exception("Failed to extract AssetBundle");
                    result.Complete();
                    yield break;
                }

                // Save to cache
                try
                {
                    var bundleData = request.downloadHandler.data;
                    File.WriteAllBytes(cachePath, bundleData);
                    ScreenLogger.Log(ScreenLogger.LogCategory.AssetLoading,
                        $"Cached bundle: {bundleName}");
                }
                catch (Exception ex)
                {
                    ScreenLogger.LogWarning($"Failed to cache bundle: {ex.Message}");
                }

                result.Bundle = bundle;
                result.Complete();
            }
        }

        public void Release(AssetLoadHandle handle)
        {
            // AssetBundles are managed separately
            // Individual assets don't need explicit release
        }

        /// <summary>
        /// Clear all cached bundles from disk
        /// </summary>
        public void ClearCache()
        {
            try
            {
                if (Directory.Exists(_cacheDirectory))
                {
                    Directory.Delete(_cacheDirectory, true);
                    Directory.CreateDirectory(_cacheDirectory);
                    ScreenLogger.Log(ScreenLogger.LogCategory.Info, "Remote asset cache cleared");
                }
            }
            catch (Exception ex)
            {
                ScreenLogger.LogException(ex);
            }
        }

        /// <summary>
        /// Unload all loaded bundles
        /// </summary>
        public void UnloadAllBundles(bool unloadAllLoadedObjects = false)
        {
            foreach (var bundle in _loadedBundles.Values)
            {
                if (bundle != null)
                {
                    bundle.Unload(unloadAllLoadedObjects);
                }
            }
            _loadedBundles.Clear();
        }

        private string GetBundleName(string key)
        {
            // Extract bundle name from key (e.g., "Prefabs/HomePage" -> "prefabs")
            var parts = key.Split('/');
            return parts.Length > 1 ? parts[0].ToLower() : "common";
        }

        private string GetAssetName(string key)
        {
            // Extract asset name from key (e.g., "Prefabs/HomePage" -> "HomePage")
            var parts = key.Split('/');
            return parts[parts.Length - 1];
        }

        private class DownloadResult : IEnumerator
        {
            public AssetBundle Bundle { get; set; }
            public Exception Error { get; set; }
            public float Progress { get; set; }
            private bool _isDone;

            public void Complete()
            {
                _isDone = true;
            }

            public bool MoveNext() => !_isDone;
            public void Reset() { }
            public object Current => null;
        }
    }

    /// <summary>
    /// Simple coroutine runner for RemoteAssetLoader
    /// </summary>
    internal class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        private static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[Remote Asset Loader Runner]");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<CoroutineRunner>();
                }
                return _instance;
            }
        }

        public new static Coroutine StartCoroutine(IEnumerator routine)
        {
            return ((MonoBehaviour)Instance).StartCoroutine(routine);
        }
    }
}
