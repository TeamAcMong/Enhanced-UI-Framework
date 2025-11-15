using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedUI.Extensions
{
    /// <summary>
    /// Extension methods for Sheet and SheetContainer
    /// </summary>
    public static class SheetExtensions
    {
        private static readonly Dictionary<Sheet, object> _sheetData = new Dictionary<Sheet, object>();

        #region SheetContainer Extensions

        /// <summary>
        /// Register and show a sheet in one call
        /// </summary>
        public static AsyncProcessHandle<Sheet> RegisterAndShow<TSheet>(
            this SheetContainer container,
            string resourceKey,
            bool playAnimation = true,
            Action<TSheet> onLoaded = null) where TSheet : Sheet
        {
            var handle = new AsyncProcessHandle<Sheet>();
            var mb = container as MonoBehaviour;

            mb.StartCoroutine(RegisterAndShowRoutine());

            IEnumerator RegisterAndShowRoutine()
            {
                // Register
                var registerHandle = container.Register<TSheet>(resourceKey, new WindowOptions
                {
                    OnLoaded = onLoaded != null ? (screen => onLoaded(screen as TSheet)) : null
                });
                yield return registerHandle;

                if (registerHandle.HasError)
                {
                    handle.CompleteWithError(registerHandle.Exception);
                    yield break;
                }

                // Show
                var showHandle = container.Show(resourceKey, playAnimation);
                yield return showHandle;

                if (showHandle.HasError)
                {
                    handle.CompleteWithError(showHandle.Exception);
                }
                else
                {
                    handle.Complete(showHandle.Result);
                }
            }

            return handle;
        }

        /// <summary>
        /// Register multiple sheets at once
        /// </summary>
        public static AsyncProcessHandle RegisterAll(
            this SheetContainer container,
            params string[] resourceKeys)
        {
            var handle = new AsyncProcessHandle();
            var mb = container as MonoBehaviour;

            mb.StartCoroutine(RegisterAllRoutine());

            IEnumerator RegisterAllRoutine()
            {
                foreach (var key in resourceKeys)
                {
                    var registerHandle = container.Register<Sheet>(key);
                    yield return registerHandle;

                    if (registerHandle.HasError)
                    {
                        Debug.LogError($"Failed to register sheet: {key}\n{registerHandle.Exception}");
                    }
                }
                handle.Complete();
            }

            return handle;
        }

        /// <summary>
        /// Unregister all sheets
        /// </summary>
        public static AsyncProcessHandle UnregisterAll(this SheetContainer container)
        {
            var handle = new AsyncProcessHandle();
            var mb = container as MonoBehaviour;

            mb.StartCoroutine(UnregisterAllRoutine());

            IEnumerator UnregisterAllRoutine()
            {
                // This would require access to internal _sheets dictionary
                // For now, this is a placeholder
                // You'd need to expose a GetAllSheetIds() method in SheetContainer
                handle.Complete();
                yield break;
            }

            return handle;
        }

        /// <summary>
        /// Switch from one sheet to another
        /// </summary>
        public static AsyncProcessHandle<Sheet> Switch(
            this SheetContainer container,
            string toSheetId,
            bool playAnimation = true)
        {
            var handle = new AsyncProcessHandle<Sheet>();
            var mb = container as MonoBehaviour;

            mb.StartCoroutine(SwitchRoutine());

            IEnumerator SwitchRoutine()
            {
                // Hide current if any
                if (container.ActiveSheet != null)
                {
                    var hideHandle = container.Hide(playAnimation);
                    yield return hideHandle;
                }

                // Show new
                var showHandle = container.Show(toSheetId, playAnimation);
                yield return showHandle;

                if (showHandle.HasError)
                {
                    handle.CompleteWithError(showHandle.Exception);
                }
                else
                {
                    handle.Complete(showHandle.Result);
                }
            }

            return handle;
        }

        /// <summary>
        /// Toggle sheet (show if hidden, hide if shown)
        /// </summary>
        public static AsyncProcessHandle<Sheet> Toggle(
            this SheetContainer container,
            string sheetId,
            bool playAnimation = true)
        {
            var handle = new AsyncProcessHandle<Sheet>();
            var mb = container as MonoBehaviour;

            mb.StartCoroutine(ToggleRoutine());

            IEnumerator ToggleRoutine()
            {
                if (container.ActiveSheet != null && container.ActiveSheet.Identifier == sheetId)
                {
                    // Hide
                    var hideHandle = container.Hide(playAnimation);
                    yield return hideHandle;
                    handle.Complete(null);
                }
                else
                {
                    // Show
                    var showHandle = container.Show(sheetId, playAnimation);
                    yield return showHandle;

                    if (showHandle.HasError)
                    {
                        handle.CompleteWithError(showHandle.Exception);
                    }
                    else
                    {
                        handle.Complete(showHandle.Result);
                    }
                }
            }

            return handle;
        }

        /// <summary>
        /// Hide the active sheet
        /// </summary>
        public static AsyncProcessHandle HideActive(
            this SheetContainer container,
            bool playAnimation = true)
        {
            return container.Hide(playAnimation);
        }

        /// <summary>
        /// Check if a specific sheet is active
        /// </summary>
        public static bool IsActive(this SheetContainer container, string sheetId)
        {
            return container.ActiveSheet != null && container.ActiveSheet.Identifier == sheetId;
        }

        #endregion

        #region Sheet Extensions

        /// <summary>
        /// Set data for a sheet
        /// </summary>
        public static void SetData<T>(this Sheet sheet, T data)
        {
            _sheetData[sheet] = data;
        }

        /// <summary>
        /// Get data from a sheet
        /// </summary>
        public static T GetData<T>(this Sheet sheet)
        {
            if (_sheetData.TryGetValue(sheet, out var data))
            {
                return (T)data;
            }
            return default;
        }

        /// <summary>
        /// Check if sheet has data
        /// </summary>
        public static bool HasData(this Sheet sheet)
        {
            return _sheetData.ContainsKey(sheet);
        }

        /// <summary>
        /// Clear data for a sheet
        /// </summary>
        public static void ClearData(this Sheet sheet)
        {
            _sheetData.Remove(sheet);
        }

        /// <summary>
        /// Show this sheet
        /// </summary>
        public static AsyncProcessHandle<Sheet> Show(this Sheet sheet, bool playAnimation = true)
        {
            var container = SheetContainer.Of(sheet.transform);
            if (container == null)
            {
                Debug.LogError($"SheetContainer not found for {sheet.name}");
                var errorHandle = new AsyncProcessHandle<Sheet>();
                errorHandle.CompleteWithError(new Exception("SheetContainer not found"));
                return errorHandle;
            }

            return container.Show(sheet.Identifier, playAnimation);
        }

        /// <summary>
        /// Hide this sheet
        /// </summary>
        public static AsyncProcessHandle Hide(this Sheet sheet, bool playAnimation = true)
        {
            var container = SheetContainer.Of(sheet.transform);
            if (container == null)
            {
                Debug.LogError($"SheetContainer not found for {sheet.name}");
                var errorHandle = new AsyncProcessHandle();
                errorHandle.CompleteWithError(new Exception("SheetContainer not found"));
                return errorHandle;
            }

            // Only hide if this sheet is active
            if (container.ActiveSheet == sheet)
            {
                return container.Hide(playAnimation);
            }

            // Already hidden
            var handle = new AsyncProcessHandle();
            handle.Complete();
            return handle;
        }

        /// <summary>
        /// Switch to another sheet from this sheet
        /// </summary>
        public static AsyncProcessHandle<Sheet> SwitchTo(
            this Sheet sheet,
            string toSheetId,
            bool playAnimation = true)
        {
            var container = SheetContainer.Of(sheet.transform);
            if (container == null)
            {
                Debug.LogError($"SheetContainer not found for {sheet.name}");
                var errorHandle = new AsyncProcessHandle<Sheet>();
                errorHandle.CompleteWithError(new Exception("SheetContainer not found"));
                return errorHandle;
            }

            return container.Switch(toSheetId, playAnimation);
        }

        /// <summary>
        /// Check if this sheet is currently active
        /// </summary>
        public static bool IsActive(this Sheet sheet)
        {
            var container = SheetContainer.Of(sheet.transform);
            return container != null && container.ActiveSheet == sheet;
        }

        #endregion
    }
}
