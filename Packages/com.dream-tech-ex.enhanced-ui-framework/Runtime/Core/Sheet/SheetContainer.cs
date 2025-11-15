using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.AssetManagement;
using EnhancedUI.Transition;
using EnhancedUI.Utilities;

namespace EnhancedUI
{
    /// <summary>
    /// Container for Sheet screens (tab-like navigation without history)
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class SheetContainer : MonoBehaviour, IUIContainer
    {
        [SerializeField] private string containerName = "SheetContainer";
        [SerializeField] private bool disableInteractionDuringTransition = true;

        private static readonly Dictionary<string, SheetContainer> InstancesByName = new Dictionary<string, SheetContainer>();
        private static readonly Dictionary<Transform, SheetContainer> InstancesByTransform = new Dictionary<Transform, SheetContainer>();

        private readonly Dictionary<string, Sheet> _sheets = new Dictionary<string, Sheet>();
        private readonly Dictionary<string, AssetLoadHandle<GameObject>> _assetLoadHandles = new Dictionary<string, AssetLoadHandle<GameObject>>();

        private ContainerLayerManager _layerManager;
        private IAssetLoader _assetLoader;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;
        private bool _isInTransition;
        private string _activeSheetId;

        public string Name => containerName;
        public RectTransform RectTransform => _rectTransform;
        public bool IsInTransition => _isInTransition;
        public bool Interactable
        {
            get => _canvasGroup.interactable;
            set => _canvasGroup.interactable = value;
        }

        public Sheet ActiveSheet => !string.IsNullOrEmpty(_activeSheetId) ? _sheets[_activeSheetId] : null;
        public int SheetCount => _sheets.Count;

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

            foreach (var sheet in _sheets.Values)
            {
                if (sheet != null && sheet.gameObject != null)
                {
                    StartCoroutine(sheet.InternalCleanup());
                }
            }

            foreach (var handle in _assetLoadHandles.Values)
            {
                _assetLoader.Release(handle);
            }

            _sheets.Clear();
            _assetLoadHandles.Clear();
        }

        public static SheetContainer Find(string containerName)
        {
            return InstancesByName.TryGetValue(containerName, out var container) ? container : null;
        }

        public static SheetContainer Of(Transform transform)
        {
            return InstancesByTransform.TryGetValue(transform, out var container) ? container : null;
        }

        /// <summary>
        /// Register a sheet (load but don't show)
        /// </summary>
        public AsyncProcessHandle<Sheet> Register<TSheet>(string resourceKey, WindowOptions options = null) where TSheet : Sheet
        {
            return Register(typeof(TSheet), resourceKey, options ?? WindowOptions.Default);
        }

        public AsyncProcessHandle<Sheet> Register(Type sheetType, string resourceKey, WindowOptions options = null)
        {
            options = options ?? WindowOptions.Default;
            var handle = new AsyncProcessHandle<Sheet>();
            StartCoroutine(RegisterRoutine(sheetType, resourceKey, options, handle));
            return handle;
        }

        private IEnumerator RegisterRoutine(Type sheetType, string resourceKey, WindowOptions options, AsyncProcessHandle<Sheet> handle)
        {
            // Check if already registered
            var sheetId = resourceKey; // Use resource key as sheet ID for simplicity

            if (_sheets.ContainsKey(sheetId))
            {
                handle.Complete(_sheets[sheetId]);
                yield break;
            }

            // Load sheet
            var assetHandle = options.LoadAsync
                ? _assetLoader.LoadAsync<GameObject>(resourceKey)
                : _assetLoader.Load<GameObject>(resourceKey);

            yield return assetHandle;

            if (assetHandle.HasError)
            {
                ScreenLogger.LogError($"Failed to load sheet: {resourceKey}\n{assetHandle.Exception}");
                handle.CompleteWithError(assetHandle.Exception);
                yield break;
            }

            // Instantiate
            var instance = Instantiate(assetHandle.Result, transform);
            var sheet = instance.GetComponent(sheetType) as Sheet;
            if (sheet == null)
            {
                Destroy(instance);
                var error = new Exception($"Sheet component of type {sheetType.Name} not found on prefab: {resourceKey}");
                ScreenLogger.LogError(error.Message);
                handle.CompleteWithError(error);
                yield break;
            }

            // Setup sheet
            sheet.Identifier = sheetId;
            sheet.name = $"[Sheet] {resourceKey}";
            sheet.gameObject.SetActive(false); // Hidden by default

            _sheets[sheetId] = sheet;
            _assetLoadHandles[sheetId] = assetHandle;

            // Callback
            options.OnLoaded?.Invoke(sheet);

            // Initialize
            yield return sheet.InternalInitialize();

            handle.Complete(sheet);
        }

        /// <summary>
        /// Show a registered sheet
        /// </summary>
        public AsyncProcessHandle<Sheet> Show(string sheetId, bool playAnimation = true)
        {
            var handle = new AsyncProcessHandle<Sheet>();

            if (!_sheets.ContainsKey(sheetId))
            {
                handle.CompleteWithError(new Exception($"Sheet not registered: {sheetId}"));
                return handle;
            }

            StartCoroutine(ShowRoutine(sheetId, playAnimation, handle));
            return handle;
        }

        private IEnumerator ShowRoutine(string sheetId, bool playAnimation, AsyncProcessHandle<Sheet> handle)
        {
            _isInTransition = true;
            SetInteractionEnabled(false);

            var enteringSheet = _sheets[sheetId];
            Sheet exitingSheet = !string.IsNullOrEmpty(_activeSheetId) ? _sheets[_activeSheetId] : null;

            // Activate entering sheet
            enteringSheet.gameObject.SetActive(true);

            // Lifecycle: WillEnter
            enteringSheet.WillEnter();

            // Lifecycle: WillExit (exiting sheet)
            if (exitingSheet != null)
            {
                exitingSheet.WillExit();
            }

            // Play transition
            if (playAnimation)
            {
                var enterAnimation = enteringSheet.GetPushEnterAnimation(exitingSheet?.Identifier ?? string.Empty);
                var exitAnimation = exitingSheet?.GetPushExitAnimation(sheetId);

                if (enterAnimation != null)
                {
                    enterAnimation.PartnerRectTransform = exitingSheet?.RectTransform;
                }
                if (exitAnimation != null)
                {
                    exitAnimation.PartnerRectTransform = enteringSheet.RectTransform;
                }

                var enterPlayer = new TransitionPlayer();
                var exitPlayer = new TransitionPlayer();

                var enterRoutine = enterPlayer.Play(enterAnimation, enteringSheet.RectTransform);
                var exitRoutine = exitPlayer.Play(exitAnimation, exitingSheet?.RectTransform);

                yield return enterRoutine;
                yield return exitRoutine;
            }

            // Lifecycle: DidEnter
            enteringSheet.DidEnter();

            // Lifecycle: DidExit (exiting sheet)
            if (exitingSheet != null)
            {
                exitingSheet.DidExit();
                exitingSheet.gameObject.SetActive(false);
            }

            _activeSheetId = sheetId;

            handle.Complete(enteringSheet);
            _isInTransition = false;
            SetInteractionEnabled(true);
        }

        /// <summary>
        /// Hide the active sheet
        /// </summary>
        public AsyncProcessHandle Hide(bool playAnimation = true)
        {
            var handle = new AsyncProcessHandle();

            if (string.IsNullOrEmpty(_activeSheetId))
            {
                handle.CompleteWithError(new Exception("No active sheet to hide"));
                return handle;
            }

            StartCoroutine(HideRoutine(playAnimation, handle));
            return handle;
        }

        private IEnumerator HideRoutine(bool playAnimation, AsyncProcessHandle handle)
        {
            _isInTransition = true;
            SetInteractionEnabled(false);

            var exitingSheet = _sheets[_activeSheetId];

            // Lifecycle: WillExit
            exitingSheet.WillExit();

            // Play transition
            if (playAnimation)
            {
                var exitAnimation = exitingSheet.GetPopExitAnimation(string.Empty);
                var exitPlayer = new TransitionPlayer();
                yield return exitPlayer.Play(exitAnimation, exitingSheet.RectTransform);
            }

            // Lifecycle: DidExit
            exitingSheet.DidExit();

            exitingSheet.gameObject.SetActive(false);
            _activeSheetId = null;

            handle.Complete();
            _isInTransition = false;
            SetInteractionEnabled(true);
        }

        /// <summary>
        /// Unregister and destroy a sheet
        /// </summary>
        public AsyncProcessHandle Unregister(string sheetId)
        {
            var handle = new AsyncProcessHandle();

            if (!_sheets.ContainsKey(sheetId))
            {
                handle.CompleteWithError(new Exception($"Sheet not registered: {sheetId}"));
                return handle;
            }

            StartCoroutine(UnregisterRoutine(sheetId, handle));
            return handle;
        }

        private IEnumerator UnregisterRoutine(string sheetId, AsyncProcessHandle handle)
        {
            var sheet = _sheets[sheetId];

            // Hide if active
            if (_activeSheetId == sheetId)
            {
                yield return Hide(false);
            }

            // Cleanup
            yield return sheet.InternalCleanup();

            if (sheet.gameObject != null)
            {
                Destroy(sheet.gameObject);
            }

            if (_assetLoadHandles.TryGetValue(sheetId, out var assetHandle))
            {
                _assetLoader.Release(assetHandle);
                _assetLoadHandles.Remove(sheetId);
            }

            _sheets.Remove(sheetId);

            handle.Complete();
        }

        private void SetInteractionEnabled(bool enabled)
        {
            if (disableInteractionDuringTransition)
            {
                if (EnhancedUISettings.Instance.controlInteractionOfAllContainers)
                {
                    foreach (var container in InstancesByName.Values)
                    {
                        container.Interactable = enabled;
                    }
                }
                else
                {
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
