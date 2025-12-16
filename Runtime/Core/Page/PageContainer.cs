using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EnhancedUI.AssetManagement;
using EnhancedUI.Transition;
using EnhancedUI.Utilities;

namespace EnhancedUI
{
    /// <summary>
    /// Container for Page screens with stack-based navigation
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class PageContainer : MonoBehaviour, IUIContainer
    {
        [SerializeField] private string containerName = "PageContainer";
        [SerializeField] private bool disableInteractionDuringTransition = true;

        private static readonly Dictionary<string, PageContainer> InstancesByName = new Dictionary<string, PageContainer>();
        private static readonly Dictionary<Transform, PageContainer> InstancesByTransform = new Dictionary<Transform, PageContainer>();

        private readonly List<string> _orderedPageIds = new List<string>();
        private readonly Dictionary<string, Page> _pages = new Dictionary<string, Page>();
        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _assetLoadHandles = new Dictionary<string, AssetLoadHandle<GameObject>>();
        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _preloadedHandles = new Dictionary<string, AssetLoadHandle<GameObject>>();

        private ContainerLayerManager _layerManager;
        private IAssetLoader _assetLoader;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private bool _isInTransition;

        public string Name => containerName;
        public RectTransform RectTransform => _rectTransform;
        public bool IsInTransition => _isInTransition;
        public bool Interactable
        {
            get => _canvasGroup.interactable;
            set => _canvasGroup.interactable = value;
        }

        /// <summary>
        /// Get currently active page
        /// </summary>
        public Page CurrentPage => _orderedPageIds.Count > 0 ? _pages[_orderedPageIds[_orderedPageIds.Count - 1]] : null;

        /// <summary>
        /// Get total page count in stack
        /// </summary>
        public int PageCount => _orderedPageIds.Count;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup = GetComponent<CanvasGroup>();
            _assetLoader = AssetLoaderProvider.GetAssetLoader();
            _layerManager = new ContainerLayerManager(transform);

            if (!InstancesByName.ContainsKey(containerName))
            {
                InstancesByName.Add(containerName, this);
            }

            InstancesByTransform.Add(transform, this);
        }

        private void OnDestroy()
        {
            InstancesByName.Remove(containerName);
            InstancesByTransform.Remove(transform);

            // Cleanup lifecycle
            foreach (var page in _pages.Values)
            {
                if (page != null && page.gameObject != null)
                {
                    StartCoroutine(page.InternalCleanup());
                }
            }

            // Release asset handles
            foreach (var handle in _assetLoadHandles.Values)
            {
                _assetLoader.Release(handle);
            }

            foreach (var handle in _preloadedHandles.Values)
            {
                _assetLoader.Release(handle);
            }

            _pages.Clear();
            _orderedPageIds.Clear();
            _assetLoadHandles.Clear();
            _preloadedHandles.Clear();
        }

        /// <summary>
        /// Find container by name
        /// </summary>
        public static PageContainer Find(string containerName)
        {
            return InstancesByName.TryGetValue(containerName, out var container) ? container : null;
        }

        /// <summary>
        /// Find container by transform hierarchy
        /// </summary>
        public static PageContainer Of(Transform transform)
        {
            return InstancesByTransform.TryGetValue(transform, out var container) ? container : null;
        }
        
        public void SetAssetLoader(IAssetLoader assetLoader)
        {
            _assetLoader = assetLoader;
        }

        /// <summary>
        /// Push a new page onto the stack
        /// </summary>
        public AsyncProcessHandle<Page> Push<TPage>(string resourceKey, WindowOptions options = null) where TPage : Page
        {
            return Push(typeof(TPage), resourceKey, options ?? WindowOptions.Default);
        }

        /// <summary>
        /// Push a new page onto the stack (non-generic)
        /// </summary>
        public AsyncProcessHandle<Page> Push(Type pageType, string resourceKey, WindowOptions options = null)
        {
            options = options ?? WindowOptions.Default;
            var handle = new AsyncProcessHandle<Page>();

            StartCoroutine(PushRoutine(pageType, resourceKey, options, handle));

            return handle;
        }

        private IEnumerator PushRoutine(Type pageType, string resourceKey, WindowOptions options, AsyncProcessHandle<Page> handle)
        {
            _isInTransition = true;
            SetInteractionEnabled(false);

            // Load page
            AssetLoadHandle<GameObject> assetHandle = null;
            if (_preloadedHandles.TryGetValue(resourceKey, out var preloadedHandle))
            {
                assetHandle = preloadedHandle;
                _preloadedHandles.Remove(resourceKey);
            }
            else
            {
                assetHandle = options.LoadAsync
                    ? _assetLoader.LoadAsync<GameObject>(resourceKey)
                    : _assetLoader.Load<GameObject>(resourceKey);

                yield return assetHandle;
            }

            if (assetHandle.HasError)
            {
                ScreenLogger.LogError($"Failed to load page: {resourceKey}\n{assetHandle.Exception}");
                handle.CompleteWithError(assetHandle.Exception);
                _isInTransition = false;
                SetInteractionEnabled(true);
                yield break;
            }

            // Instantiate
            var instance = Instantiate(assetHandle.Result, transform);
            var page = instance.GetComponent(pageType) as Page;
            if (page == null)
            {
                Destroy(instance);
                var error = new Exception($"Page component of type {pageType.Name} not found on prefab: {resourceKey}");
                ScreenLogger.LogError(error.Message);
                handle.CompleteWithError(error);
                _isInTransition = false;
                SetInteractionEnabled(true);
                yield break;
            }

            // Setup page
            var pageId = IdentifierPool.GenerateStringId("page");
            page.Identifier = pageId;
            page.name = $"[Page] {resourceKey}";
            page.gameObject.SetActive(true);

            _pages[pageId] = page;
            _assetLoadHandles[pageId] = assetHandle;

            // OnLoaded callback
            options.OnLoaded?.Invoke(page);

            // Initialize
            yield return page.InternalInitialize();

            // Get partner page (previous page)
            Page partnerPage = CurrentPage;
            string partnerIdentifier = partnerPage != null ? partnerPage.Identifier : string.Empty;

            // Add to stack
            if (options.Stack)
            {
                _orderedPageIds.Add(pageId);
            }

            // Lifecycle: WillPushEnter (entering page)
            page.WillPushEnter();

            // Lifecycle: WillPushExit (exiting page - being covered)
            if (partnerPage != null)
            {
                partnerPage.WillPushExit();
            }

            // Play transition
            if (options.PlayAnimation)
            {
                // Setup animations
                var enterAnimation = page.GetPushEnterAnimation(partnerIdentifier);
                var exitAnimation = partnerPage?.GetPushExitAnimation(pageId);

                if (enterAnimation != null)
                {
                    enterAnimation.PartnerRectTransform = partnerPage?.RectTransform;
                }
                if (exitAnimation != null)
                {
                    exitAnimation.PartnerRectTransform = page.RectTransform;
                }

                // Play animations
                var enterPlayer = new TransitionPlayer();
                var exitPlayer = new TransitionPlayer();

                var enterRoutine = enterPlayer.Play(enterAnimation, page.RectTransform);
                var exitRoutine = exitPlayer.Play(exitAnimation, partnerPage?.RectTransform);

                yield return enterRoutine;
                yield return exitRoutine;
            }

            // Lifecycle: DidPushEnter (entering page)
            page.DidPushEnter();

            // Lifecycle: DidPushExit (exiting page)
            if (partnerPage != null)
            {
                partnerPage.DidPushExit();
            }

            handle.Complete(page);
            _isInTransition = false;
            SetInteractionEnabled(true);
        }

        /// <summary>
        /// Pop the top page from the stack
        /// </summary>
        public AsyncProcessHandle<Page> Pop(bool playAnimation = true, int popCount = 1, string destinationPageId = null)
        {
            var handle = new AsyncProcessHandle<Page>();
            StartCoroutine(PopRoutine(playAnimation, popCount, destinationPageId, handle));
            return handle;
        }

        private IEnumerator PopRoutine(bool playAnimation, int popCount, string destinationPageId, AsyncProcessHandle<Page> handle)
        {
            if (_orderedPageIds.Count == 0)
            {
                handle.CompleteWithError(new Exception("Cannot pop: no pages in stack"));
                yield break;
            }

            _isInTransition = true;
            SetInteractionEnabled(false);

            // Determine pages to pop
            List<string> pagesToPop = new List<string>();

            if (!string.IsNullOrEmpty(destinationPageId))
            {
                // Pop until destination
                int destIndex = _orderedPageIds.IndexOf(destinationPageId);
                if (destIndex < 0)
                {
                    handle.CompleteWithError(new Exception($"Destination page not found: {destinationPageId}"));
                    _isInTransition = false;
                    SetInteractionEnabled(true);
                    yield break;
                }

                for (int i = _orderedPageIds.Count - 1; i > destIndex; i--)
                {
                    pagesToPop.Add(_orderedPageIds[i]);
                }
            }
            else
            {
                // Pop specified count
                int count = Mathf.Min(popCount, _orderedPageIds.Count);
                for (int i = 0; i < count; i++)
                {
                    pagesToPop.Add(_orderedPageIds[_orderedPageIds.Count - 1 - i]);
                }
            }

            if (pagesToPop.Count == 0)
            {
                handle.CompleteWithError(new Exception("No pages to pop"));
                _isInTransition = false;
                SetInteractionEnabled(true);
                yield break;
            }

            // Get entering and exiting pages
            var exitingPage = _pages[pagesToPop[0]];
            var enteringPageId = _orderedPageIds[_orderedPageIds.Count - pagesToPop.Count - 1];
            var enteringPage = enteringPageId != null ? _pages[enteringPageId] : null;

            // Lifecycle: WillPopExit (exiting page)
            exitingPage.WillPopExit();

            // Lifecycle: WillPopEnter (entering page)
            if (enteringPage != null)
            {
                enteringPage.WillPopEnter();
            }

            // Play transition
            if (playAnimation)
            {
                var enterAnimation = enteringPage?.GetPopEnterAnimation(exitingPage.Identifier);
                var exitAnimation = exitingPage.GetPopExitAnimation(enteringPageId ?? string.Empty);

                if (enterAnimation != null)
                {
                    enterAnimation.PartnerRectTransform = exitingPage.RectTransform;
                }
                if (exitAnimation != null)
                {
                    exitAnimation.PartnerRectTransform = enteringPage?.RectTransform;
                }

                var enterPlayer = new TransitionPlayer();
                var exitPlayer = new TransitionPlayer();

                var enterRoutine = enterPlayer.Play(enterAnimation, enteringPage?.RectTransform);
                var exitRoutine = exitPlayer.Play(exitAnimation, exitingPage.RectTransform);

                yield return enterRoutine;
                yield return exitRoutine;
            }

            // Lifecycle: DidPopExit (exiting page)
            exitingPage.DidPopExit();

            // Lifecycle: DidPopEnter (entering page)
            if (enteringPage != null)
            {
                enteringPage.DidPopEnter();
            }

            // Cleanup popped pages
            foreach (var pageId in pagesToPop)
            {
                var page = _pages[pageId];

                // Cleanup
                yield return page.InternalCleanup();

                // Destroy
                if (page.gameObject != null)
                {
                    Destroy(page.gameObject);
                }

                // Release asset
                if (_assetLoadHandles.TryGetValue(pageId, out var assetHandle))
                {
                    _assetLoader.Release(assetHandle);
                    _assetLoadHandles.Remove(pageId);
                }

                _pages.Remove(pageId);
                _orderedPageIds.Remove(pageId);
            }

            handle.Complete(enteringPage);
            _isInTransition = false;
            SetInteractionEnabled(true);
        }

        /// <summary>
        /// Preload a page without showing it
        /// </summary>
        public AssetLoadHandle<GameObject> Preload(string resourceKey)
        {
            if (_preloadedHandles.ContainsKey(resourceKey))
            {
                return _preloadedHandles[resourceKey];
            }

            var handle = _assetLoader.LoadAsync<GameObject>(resourceKey);
            _preloadedHandles[resourceKey] = handle;
            return handle;
        }

        /// <summary>
        /// Release a preloaded page
        /// </summary>
        public void ReleasePreloaded(string resourceKey)
        {
            if (_preloadedHandles.TryGetValue(resourceKey, out var handle))
            {
                _assetLoader.Release(handle);
                _preloadedHandles.Remove(resourceKey);
            }
        }

        private void SetInteractionEnabled(bool enabled)
        {
            if (disableInteractionDuringTransition)
            {
                if (EnhancedUISettings.Instance.controlInteractionOfAllContainers)
                {
                    // Disable all containers
                    foreach (var container in InstancesByName.Values)
                    {
                        container.Interactable = enabled;
                    }
                }
                else
                {
                    // Only this container
                    Interactable = enabled;
                }
            }
            else if (EnhancedUISettings.Instance.enableInteractionInTransition)
            {
                Interactable = true;
            }
        }
    }
}
