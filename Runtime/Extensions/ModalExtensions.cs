using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedUI.Extensions
{
    /// <summary>
    /// Extension methods for Modal and ModalContainer
    /// </summary>
    public static class ModalExtensions
    {
        private static readonly Dictionary<Modal, object> _modalData = new Dictionary<Modal, object>();
        private static readonly Dictionary<Modal, Coroutine> _autoCloseCoroutines = new Dictionary<Modal, Coroutine>();

        #region ModalContainer Extensions

        /// <summary>
        /// Push a modal with typed options
        /// </summary>
        public static AsyncProcessHandle<Modal> Push<TModal>(
            this ModalContainer container,
            string resourceKey,
            bool playAnimation = true,
            bool loadAsync = true,
            Action<TModal> onLoaded = null) where TModal : Modal
        {
            return container.Push<TModal>(resourceKey, new WindowOptions
            {
                PlayAnimation = playAnimation,
                LoadAsync = loadAsync,
                OnLoaded = onLoaded != null ? (screen => onLoaded(screen as TModal)) : null
            });
        }

        /// <summary>
        /// Push a modal with data
        /// </summary>
        public static AsyncProcessHandle<Modal> PushWithData<TModal, TData>(
            this ModalContainer container,
            string resourceKey,
            TData data,
            bool playAnimation = true) where TModal : Modal
        {
            return container.Push<TModal>(resourceKey, new WindowOptions
            {
                PlayAnimation = playAnimation,
                OnLoaded = modal => ((TModal)modal).SetData(data)
            });
        }

        /// <summary>
        /// Show a simple alert modal (requires AlertModal prefab)
        /// </summary>
        public static AsyncProcessHandle<Modal> ShowAlert(
            this ModalContainer container,
            string title,
            string message,
            string buttonText = "OK",
            Action onConfirm = null)
        {
            // This assumes you have a generic AlertModal prefab
            // Implementation would depend on your specific AlertModal setup
            var handle = new AsyncProcessHandle<Modal>();

            var pushHandle = container.Push<Modal>("Prefabs/AlertModal", new WindowOptions
            {
                PlayAnimation = true,
                OnLoaded = screen =>
                {
                    // Set data
                    var modal = (Modal)screen;
                    modal.SetData(new AlertData
                    {
                        Title = title,
                        Message = message,
                        ButtonText = buttonText,
                        OnConfirm = onConfirm
                    });
                }
            });

            pushHandle.OnTerminate += h => handle.Complete(h.Result);
            return handle;
        }

        /// <summary>
        /// Show a confirmation modal with Yes/No options (requires ConfirmationModal prefab)
        /// </summary>
        public static AsyncProcessHandle<Modal> ShowConfirmation(
            this ModalContainer container,
            string title,
            string message,
            Action<bool> callback,
            string yesText = "Yes",
            string noText = "No")
        {
            var handle = new AsyncProcessHandle<Modal>();

            var pushHandle = container.Push<Modal>("Prefabs/ConfirmationModal", new WindowOptions
            {
                PlayAnimation = true,
                OnLoaded = screen =>
                {
                    var modal = (Modal)screen;
                    modal.SetData(new ConfirmationData
                    {
                        Title = title,
                        Message = message,
                        YesText = yesText,
                        NoText = noText,
                        Callback = callback
                    });
                }
            });

            pushHandle.OnTerminate += h => handle.Complete(h.Result);
            return handle;
        }

        /// <summary>
        /// Show a toast message (requires ToastModal prefab)
        /// </summary>
        public static AsyncProcessHandle<Modal> ShowToast(
            this ModalContainer container,
            string message,
            float duration = 2f,
            ToastType type = ToastType.Info)
        {
            var handle = new AsyncProcessHandle<Modal>();

            var pushHandle = container.Push<Modal>("Prefabs/ToastModal", new WindowOptions
            {
                PlayAnimation = true,
                OnLoaded = screen =>
                {
                    var modal = (Modal)screen;
                    modal.SetData(new ToastData
                    {
                        Message = message,
                        Type = type
                    });

                    // Auto-close after duration
                    modal.AutoCloseAfter(duration);
                }
            });

            pushHandle.OnTerminate += h => handle.Complete(h.Result);
            return handle;
        }

        /// <summary>
        /// Pop all modals
        /// </summary>
        public static AsyncProcessHandle PopAll(
            this ModalContainer container,
            bool playAnimation = true)
        {
            var handle = new AsyncProcessHandle();
            var mb = container as MonoBehaviour;

            mb.StartCoroutine(PopAllRoutine());

            IEnumerator PopAllRoutine()
            {
                while (container.ModalCount > 0)
                {
                    yield return container.Pop(playAnimation);
                }
                handle.Complete();
            }

            return handle;
        }

        #endregion

        #region Modal Extensions

        /// <summary>
        /// Set data for a modal
        /// </summary>
        public static void SetData<T>(this Modal modal, T data)
        {
            _modalData[modal] = data;
        }

        /// <summary>
        /// Get data from a modal
        /// </summary>
        public static T GetData<T>(this Modal modal)
        {
            if (_modalData.TryGetValue(modal, out var data))
            {
                return (T)data;
            }
            return default;
        }

        /// <summary>
        /// Check if modal has data
        /// </summary>
        public static bool HasData(this Modal modal)
        {
            return _modalData.ContainsKey(modal);
        }

        /// <summary>
        /// Clear data for a modal
        /// </summary>
        public static void ClearData(this Modal modal)
        {
            _modalData.Remove(modal);
        }

        /// <summary>
        /// Auto-close this modal after a delay
        /// </summary>
        public static void AutoCloseAfter(this Modal modal, float seconds)
        {
            var container = ModalContainer.Of(modal.transform);
            if (container == null)
            {
                Debug.LogError($"ModalContainer not found for {modal.name}");
                return;
            }

            // Cancel existing auto-close if any
            if (_autoCloseCoroutines.TryGetValue(modal, out var existingCoroutine))
            {
                (container as MonoBehaviour).StopCoroutine(existingCoroutine);
            }

            // Start new auto-close
            var coroutine = (container as MonoBehaviour).StartCoroutine(AutoCloseRoutine());
            _autoCloseCoroutines[modal] = coroutine;

            IEnumerator AutoCloseRoutine()
            {
                yield return new WaitForSeconds(seconds);
                _autoCloseCoroutines.Remove(modal);
                container.Pop(true);
            }
        }

        /// <summary>
        /// Cancel auto-close for this modal
        /// </summary>
        public static void CancelAutoClose(this Modal modal)
        {
            if (_autoCloseCoroutines.TryGetValue(modal, out var coroutine))
            {
                var container = ModalContainer.Of(modal.transform);
                if (container != null)
                {
                    (container as MonoBehaviour).StopCoroutine(coroutine);
                }
                _autoCloseCoroutines.Remove(modal);
            }
        }

        /// <summary>
        /// Close this modal
        /// </summary>
        public static AsyncProcessHandle<Modal> Close(this Modal modal, bool playAnimation = true)
        {
            var container = ModalContainer.Of(modal.transform);
            if (container == null)
            {
                Debug.LogError($"ModalContainer not found for {modal.name}");
                var errorHandle = new AsyncProcessHandle<Modal>();
                errorHandle.CompleteWithError(new Exception("ModalContainer not found"));
                return errorHandle;
            }

            // Cancel auto-close if any
            modal.CancelAutoClose();

            return container.Pop(playAnimation);
        }

        #endregion

        #region Data Classes

        public class AlertData
        {
            public string Title;
            public string Message;
            public string ButtonText;
            public Action OnConfirm;
        }

        public class ConfirmationData
        {
            public string Title;
            public string Message;
            public string YesText;
            public string NoText;
            public Action<bool> Callback;
        }

        public class ToastData
        {
            public string Message;
            public ToastType Type;
        }

        public enum ToastType
        {
            Info,
            Success,
            Warning,
            Error
        }

        #endregion
    }
}
