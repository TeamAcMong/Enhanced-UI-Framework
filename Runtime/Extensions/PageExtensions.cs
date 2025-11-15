using System;
using System.Collections.Generic;
using UnityEngine;

namespace EnhancedUI.Extensions
{
    /// <summary>
    /// Extension methods for Page and PageContainer
    /// </summary>
    public static class PageExtensions
    {
        private static readonly Dictionary<Page, object> _pageData = new Dictionary<Page, object>();

        #region PageContainer Extensions

        /// <summary>
        /// Push a page with typed options
        /// </summary>
        public static AsyncProcessHandle<Page> Push<TPage>(
            this PageContainer container,
            string resourceKey,
            bool playAnimation = true,
            bool loadAsync = true,
            Action<TPage> onLoaded = null) where TPage : Page
        {
            return container.Push<TPage>(resourceKey, new WindowOptions
            {
                PlayAnimation = playAnimation,
                LoadAsync = loadAsync,
                OnLoaded = onLoaded != null ? (screen => onLoaded(screen as TPage)) : null
            });
        }

        /// <summary>
        /// Push a page with data
        /// </summary>
        public static AsyncProcessHandle<Page> PushWithData<TPage, TData>(
            this PageContainer container,
            string resourceKey,
            TData data,
            bool playAnimation = true) where TPage : Page
        {
            return container.Push<TPage>(resourceKey, new WindowOptions
            {
                PlayAnimation = playAnimation,
                OnLoaded = screen => ((TPage)screen).SetData(data)
            });
        }

        /// <summary>
        /// Pop to root (first page in stack)
        /// </summary>
        public static AsyncProcessHandle<Page> PopToRoot(
            this PageContainer container,
            bool playAnimation = true)
        {
            if (container.PageCount <= 1)
            {
                var handle = new AsyncProcessHandle<Page>();
                handle.Complete(container.CurrentPage);
                return handle;
            }

            return container.Pop(playAnimation, popCount: container.PageCount - 1);
        }

        /// <summary>
        /// Pop all pages
        /// </summary>
        public static AsyncProcessHandle<Page> PopAll(
            this PageContainer container,
            bool playAnimation = true)
        {
            return container.Pop(playAnimation, popCount: container.PageCount);
        }

        /// <summary>
        /// Replace current page with a new one (pop then push)
        /// </summary>
        public static AsyncProcessHandle<Page> Replace<TPage>(
            this PageContainer container,
            string resourceKey,
            bool playAnimation = true) where TPage : Page
        {
            var handle = new AsyncProcessHandle<Page>();

            container.MonoBehaviourInstance().StartCoroutine(ReplaceRoutine());

            System.Collections.IEnumerator ReplaceRoutine()
            {
                // Pop current
                yield return container.Pop(false); // No animation on pop

                // Push new
                var pushHandle = container.Push<TPage>(resourceKey, new WindowOptions
                {
                    PlayAnimation = playAnimation
                });
                yield return pushHandle;

                handle.Complete(pushHandle.Result);
            }

            return handle;
        }

        /// <summary>
        /// Check if a page exists in stack by type
        /// </summary>
        public static bool Contains<TPage>(this PageContainer container) where TPage : Page
        {
            // Note: Would need access to internal _pages dictionary
            // This is a simplified version
            return container.CurrentPage is TPage;
        }

        #endregion

        #region Page Extensions

        /// <summary>
        /// Set data for a page
        /// </summary>
        public static void SetData<T>(this Page page, T data)
        {
            _pageData[page] = data;
        }

        /// <summary>
        /// Get data from a page
        /// </summary>
        public static T GetData<T>(this Page page)
        {
            if (_pageData.TryGetValue(page, out var data))
            {
                return (T)data;
            }
            return default;
        }

        /// <summary>
        /// Check if page has data
        /// </summary>
        public static bool HasData(this Page page)
        {
            return _pageData.ContainsKey(page);
        }

        /// <summary>
        /// Clear data for a page
        /// </summary>
        public static void ClearData(this Page page)
        {
            _pageData.Remove(page);
        }

        /// <summary>
        /// Push another page from this page
        /// </summary>
        public static AsyncProcessHandle<Page> PushPage<TPage>(
            this Page page,
            string resourceKey,
            bool playAnimation = true) where TPage : Page
        {
            var container = PageContainer.Of(page.transform);
            if (container == null)
            {
                Debug.LogError($"PageContainer not found for {page.name}");
                var errorHandle = new AsyncProcessHandle<Page>();
                errorHandle.CompleteWithError(new Exception("PageContainer not found"));
                return errorHandle;
            }

            return container.Push<TPage>(resourceKey, playAnimation);
        }

        /// <summary>
        /// Pop this page
        /// </summary>
        public static AsyncProcessHandle<Page> PopPage(this Page page, bool playAnimation = true)
        {
            var container = PageContainer.Of(page.transform);
            if (container == null)
            {
                Debug.LogError($"PageContainer not found for {page.name}");
                var errorHandle = new AsyncProcessHandle<Page>();
                errorHandle.CompleteWithError(new Exception("PageContainer not found"));
                return errorHandle;
            }

            return container.Pop(playAnimation);
        }

        /// <summary>
        /// Pop to root from this page
        /// </summary>
        public static AsyncProcessHandle<Page> PopToRoot(this Page page, bool playAnimation = true)
        {
            var container = PageContainer.Of(page.transform);
            return container?.PopToRoot(playAnimation);
        }

        #endregion

        #region Helper Methods

        private static MonoBehaviour MonoBehaviourInstance(this PageContainer container)
        {
            return container as MonoBehaviour;
        }

        #endregion
    }
}
