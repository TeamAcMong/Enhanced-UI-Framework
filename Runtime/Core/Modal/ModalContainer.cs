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
    /// Container for Modal screens with backdrop support
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ModalContainer : MonoBehaviour, IUIContainer
    {
        [SerializeField] private string containerName = "ModalContainer";
        [SerializeField] private ModalBackdropStrategy backdropStrategy = ModalBackdropStrategy.GeneratePerModal;
        [SerializeField] private Color backdropColor = new Color(0f, 0f, 0f, 0.5f);
        [SerializeField] private bool closeModalOnBackdropClick = true;
        [SerializeField] private bool disableInteractionDuringTransition = true;

        private static readonly Dictionary<string, ModalContainer> InstancesByName = new Dictionary<string, ModalContainer>();
        private static readonly Dictionary<Transform, ModalContainer> InstancesByTransform = new Dictionary<Transform, ModalContainer>();

        private readonly List<string> _orderedModalIds = new List<string>();
        private readonly Dictionary<string, Modal> _modals = new Dictionary<string, Modal>();
        private readonly Dictionary<string, ModalBackdrop> _backdrops = new Dictionary<string, ModalBackdrop>();
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

        public int ModalCount => _orderedModalIds.Count;

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

            foreach (var modal in _modals.Values)
            {
                if (modal != null && modal.gameObject != null)
                {
                    StartCoroutine(modal.InternalCleanup());
                }
            }

            foreach (var handle in _assetLoadHandles.Values)
            {
                _assetLoader.Release(handle);
            }

            foreach (var handle in _preloadedHandles.Values)
            {
                _assetLoader.Release(handle);
            }

            _modals.Clear();
            _orderedModalIds.Clear();
            _backdrops.Clear();
            _assetLoadHandles.Clear();
            _preloadedHandles.Clear();
        }

        public static ModalContainer Find(string containerName)
        {
            return InstancesByName.TryGetValue(containerName, out var container) ? container : null;
        }

        public static ModalContainer Of(Transform transform)
        {
            return InstancesByTransform.TryGetValue(transform, out var container) ? container : null;
        }
        
        public void SetAssetLoader(IAssetLoader assetLoader)
        {
            _assetLoader = assetLoader;
        }

        /// <summary>
        /// Push a new modal
        /// </summary>
        public AsyncProcessHandle<Modal> Push<TModal>(string resourceKey, WindowOptions options = null) where TModal : Modal
        {
            return Push(typeof(TModal), resourceKey, options ?? WindowOptions.Default);
        }

        public AsyncProcessHandle<Modal> Push(Type modalType, string resourceKey, WindowOptions options = null)
        {
            options = options ?? WindowOptions.Default;
            var handle = new AsyncProcessHandle<Modal>();
            StartCoroutine(PushRoutine(modalType, resourceKey, options, handle));
            return handle;
        }

        private IEnumerator PushRoutine(Type modalType, string resourceKey, WindowOptions options, AsyncProcessHandle<Modal> handle)
        {
            _isInTransition = true;
            SetInteractionEnabled(false);

            // Load modal
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
                ScreenLogger.LogError($"Failed to load modal: {resourceKey}\n{assetHandle.Exception}");
                handle.CompleteWithError(assetHandle.Exception);
                _isInTransition = false;
                SetInteractionEnabled(true);
                yield break;
            }

            // Instantiate
            var instance = Instantiate(assetHandle.Result, transform);
            var modal = instance.GetComponent(modalType) as Modal;
            if (modal == null)
            {
                Destroy(instance);
                var error = new Exception($"Modal component of type {modalType.Name} not found on prefab: {resourceKey}");
                ScreenLogger.LogError(error.Message);
                handle.CompleteWithError(error);
                _isInTransition = false;
                SetInteractionEnabled(true);
                yield break;
            }

            // Setup modal
            var modalId = IdentifierPool.GenerateStringId("modal");
            modal.Identifier = modalId;
            modal.name = $"[Modal] {resourceKey}";
            modal.gameObject.SetActive(true);

            _modals[modalId] = modal;
            _assetLoadHandles[modalId] = assetHandle;

            // Create backdrop
            ModalBackdrop backdrop = null;
            bool shouldCreateBackdrop = ShouldCreateBackdrop();

            if (shouldCreateBackdrop)
            {
                backdrop = ModalBackdrop.Create(transform, backdropColor, closeModalOnBackdropClick);
                backdrop.OnClicked = () =>
                {
                    if (closeModalOnBackdropClick)
                    {
                        Pop(true);
                    }
                };
                _backdrops[modalId] = backdrop;

                // Position backdrop
                if (backdropStrategy == ModalBackdropStrategy.ChangeOrderBeforeAnimation)
                {
                    backdrop.transform.SetAsLastSibling();
                }
                else
                {
                    backdrop.transform.SetSiblingIndex(modal.transform.GetSiblingIndex());
                }
            }

            // Callback
            options.OnLoaded?.Invoke(modal);

            // Initialize
            yield return modal.InternalInitialize();

            // Add to stack
            _orderedModalIds.Add(modalId);

            // Get partner (previous modal)
            Modal partnerModal = _orderedModalIds.Count > 1 ? _modals[_orderedModalIds[_orderedModalIds.Count - 2]] : null;
            string partnerIdentifier = partnerModal != null ? partnerModal.Identifier : string.Empty;

            // Lifecycle: WillPushEnter
            modal.WillPushEnter();

            if (partnerModal != null)
            {
                partnerModal.WillPushExit();
            }

            // Play transition
            if (options.PlayAnimation)
            {
                var modalEnterAnim = modal.GetPushEnterAnimation(partnerIdentifier);
                var modalExitAnim = partnerModal?.GetPushExitAnimation(modalId);

                if (modalEnterAnim != null)
                {
                    modalEnterAnim.PartnerRectTransform = partnerModal?.RectTransform;
                }
                if (modalExitAnim != null)
                {
                    modalExitAnim.PartnerRectTransform = modal.RectTransform;
                }

                var modalEnterPlayer = new TransitionPlayer();
                var modalExitPlayer = new TransitionPlayer();
                var backdropPlayer = new TransitionPlayer();

                var modalEnterRoutine = modalEnterPlayer.Play(modalEnterAnim, modal.RectTransform);
                var modalExitRoutine = modalExitPlayer.Play(modalExitAnim, partnerModal?.RectTransform);

                // Backdrop fade in
                IEnumerator backdropRoutine = null;
                if (backdrop != null)
                {
                    backdropRoutine = FadeBackdrop(backdrop, 0f, 1f, 0.3f);
                }

                yield return modalEnterRoutine;
                yield return modalExitRoutine;
                if (backdropRoutine != null)
                {
                    yield return backdropRoutine;
                }
            }
            else
            {
                if (backdrop != null)
                {
                    backdrop.Alpha = 1f;
                }
            }

            // Adjust backdrop order after animation if needed
            if (backdropStrategy == ModalBackdropStrategy.ChangeOrderAfterAnimation && backdrop != null)
            {
                backdrop.transform.SetAsLastSibling();
            }

            // Lifecycle: DidPushEnter
            modal.DidPushEnter();

            if (partnerModal != null)
            {
                partnerModal.DidPushExit();
            }

            // Move modal to front
            modal.transform.SetAsLastSibling();

            handle.Complete(modal);
            _isInTransition = false;
            SetInteractionEnabled(true);
        }

        /// <summary>
        /// Pop the top modal
        /// </summary>
        public AsyncProcessHandle<Modal> Pop(bool playAnimation = true)
        {
            var handle = new AsyncProcessHandle<Modal>();
            StartCoroutine(PopRoutine(playAnimation, handle));
            return handle;
        }

        private IEnumerator PopRoutine(bool playAnimation, AsyncProcessHandle<Modal> handle)
        {
            if (_orderedModalIds.Count == 0)
            {
                handle.CompleteWithError(new Exception("Cannot pop: no modals in stack"));
                yield break;
            }

            _isInTransition = true;
            SetInteractionEnabled(false);

            var exitingModalId = _orderedModalIds[_orderedModalIds.Count - 1];
            var exitingModal = _modals[exitingModalId];

            Modal enteringModal = null;
            string enteringModalId = null;
            if (_orderedModalIds.Count > 1)
            {
                enteringModalId = _orderedModalIds[_orderedModalIds.Count - 2];
                enteringModal = _modals[enteringModalId];
            }

            // Lifecycle: WillPopExit
            exitingModal.WillPopExit();

            if (enteringModal != null)
            {
                enteringModal.WillPopEnter();
            }

            // Play transition
            if (playAnimation)
            {
                var modalEnterAnim = enteringModal?.GetPopEnterAnimation(exitingModalId);
                var modalExitAnim = exitingModal.GetPopExitAnimation(enteringModalId ?? string.Empty);

                if (modalEnterAnim != null)
                {
                    modalEnterAnim.PartnerRectTransform = exitingModal.RectTransform;
                }
                if (modalExitAnim != null)
                {
                    modalExitAnim.PartnerRectTransform = enteringModal?.RectTransform;
                }

                var enterPlayer = new TransitionPlayer();
                var exitPlayer = new TransitionPlayer();

                var enterRoutine = enterPlayer.Play(modalEnterAnim, enteringModal?.RectTransform);
                var exitRoutine = exitPlayer.Play(modalExitAnim, exitingModal.RectTransform);

                // Fade out backdrop
                IEnumerator backdropRoutine = null;
                if (_backdrops.TryGetValue(exitingModalId, out var backdrop))
                {
                    backdropRoutine = FadeBackdrop(backdrop, 1f, 0f, 0.3f);
                }

                yield return enterRoutine;
                yield return exitRoutine;
                if (backdropRoutine != null)
                {
                    yield return backdropRoutine;
                }
            }

            // Lifecycle: DidPopExit
            exitingModal.DidPopExit();

            if (enteringModal != null)
            {
                enteringModal.DidPopEnter();
            }

            // Cleanup
            yield return exitingModal.InternalCleanup();

            if (exitingModal.gameObject != null)
            {
                Destroy(exitingModal.gameObject);
            }

            if (_backdrops.TryGetValue(exitingModalId, out var backdropToRemove))
            {
                Destroy(backdropToRemove.gameObject);
                _backdrops.Remove(exitingModalId);
            }

            if (_assetLoadHandles.TryGetValue(exitingModalId, out var assetHandle))
            {
                _assetLoader.Release(assetHandle);
                _assetLoadHandles.Remove(exitingModalId);
            }

            _modals.Remove(exitingModalId);
            _orderedModalIds.Remove(exitingModalId);

            handle.Complete(enteringModal);
            _isInTransition = false;
            SetInteractionEnabled(true);
        }

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

        public void ReleasePreloaded(string resourceKey)
        {
            if (_preloadedHandles.TryGetValue(resourceKey, out var handle))
            {
                _assetLoader.Release(handle);
                _preloadedHandles.Remove(resourceKey);
            }
        }

        private bool ShouldCreateBackdrop()
        {
            switch (backdropStrategy)
            {
                case ModalBackdropStrategy.GeneratePerModal:
                    return true;
                case ModalBackdropStrategy.OnlyFirstBackdrop:
                    return _orderedModalIds.Count == 0;
                case ModalBackdropStrategy.ChangeOrderBeforeAnimation:
                case ModalBackdropStrategy.ChangeOrderAfterAnimation:
                    return _orderedModalIds.Count == 0;
                default:
                    return true;
            }
        }

        private IEnumerator FadeBackdrop(ModalBackdrop backdrop, float from, float to, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                backdrop.Alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }
            backdrop.Alpha = to;
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
