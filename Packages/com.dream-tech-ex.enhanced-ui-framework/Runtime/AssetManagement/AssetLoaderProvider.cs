using UnityEngine;
using EnhancedUI.Utilities;

namespace EnhancedUI.AssetManagement
{
    /// <summary>
    /// Provides the asset loader instance based on settings
    /// </summary>
    public static class AssetLoaderProvider
    {
        private static IAssetLoader _customLoader;
        private static IAssetLoader _cachedLoader;

        /// <summary>
        /// Get the current asset loader instance
        /// </summary>
        public static IAssetLoader GetAssetLoader()
        {
            if (_customLoader != null)
                return _customLoader;

            if (_cachedLoader != null)
                return _cachedLoader;

            var settings = EnhancedUISettings.Instance;
            _cachedLoader = CreateAssetLoader(settings.assetLoaderType);
            return _cachedLoader;
        }

        /// <summary>
        /// Set a custom asset loader
        /// </summary>
        public static void SetCustomAssetLoader(IAssetLoader loader)
        {
            _customLoader = loader;
            ScreenLogger.Log(ScreenLogger.LogCategory.Info,
                $"Custom asset loader set: {loader?.GetType().Name ?? "null"}");
        }

        /// <summary>
        /// Clear custom loader and revert to settings-based loader
        /// </summary>
        public static void ClearCustomAssetLoader()
        {
            _customLoader = null;
        }

        /// <summary>
        /// Reset cached loader (useful when settings change)
        /// </summary>
        public static void ResetCache()
        {
            _cachedLoader = null;
        }

        private static IAssetLoader CreateAssetLoader(AssetLoaderType type)
        {
            switch (type)
            {
                case AssetLoaderType.Resources:
                    ScreenLogger.Log(ScreenLogger.LogCategory.Info, "Using ResourcesAssetLoader");
                    return new ResourcesAssetLoader();

                case AssetLoaderType.Addressables:
#if EUI_ADDRESSABLES_SUPPORT
                    ScreenLogger.Log(ScreenLogger.LogCategory.Info, "Using AddressableAssetLoader");
                    return new AddressableAssetLoader();
#else
                    ScreenLogger.LogWarning("Addressables not available. Falling back to ResourcesAssetLoader. " +
                                          "Install Addressables package to use this feature.");
                    return new ResourcesAssetLoader();
#endif

                case AssetLoaderType.Custom:
                    ScreenLogger.LogWarning("Custom asset loader type selected but no custom loader set. " +
                                          "Use AssetLoaderProvider.SetCustomAssetLoader(). Falling back to ResourcesAssetLoader.");
                    return new ResourcesAssetLoader();

                default:
                    return new ResourcesAssetLoader();
            }
        }
    }
}
