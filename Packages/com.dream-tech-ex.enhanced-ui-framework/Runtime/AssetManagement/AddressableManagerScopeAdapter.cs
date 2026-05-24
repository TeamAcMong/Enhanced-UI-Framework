#if ADDRESSABLE_MANAGER_PRESENT
using System;
using UnityEngine;
using AmLoader = AddressableManager.Loaders.AssetLoader;
using AmHandle = AddressableManager.Core;
#if EUI_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#endif

namespace EnhancedUI.AssetManagement
{
    /// <summary>
    /// Bridges <c>AddressableManager.Loaders.AssetLoader</c> (the scope-aware
    /// loader from <c>com.game.addressables</c>) into Enhanced UI Framework's
    /// <see cref="IAssetLoader"/> interface.
    ///
    /// Pass an instance to <see cref="WindowOptions.Loader"/> to route a
    /// single <c>Push&lt;TPage&gt;()</c> through an AddressableManager scope
    /// without changing the container's default loader.
    ///
    /// Compiled only when <c>com.game.addressables 4.0.0+</c> is installed
    /// (Runtime asmdef versionDefine <c>ADDRESSABLE_MANAGER_PRESENT</c>).
    ///
    /// Typical usage:
    /// <code>
    /// // Once at bootstrap
    /// using AddressableManager.Managers;
    /// using EnhancedUI.AssetManagement;
    ///
    /// var sessionLoader = ScopeManager.Instance.GetOrCreateScope("Session");
    /// var sessionBridge = new AddressableManagerScopeAdapter(sessionLoader);
    ///
    /// // Per-push
    /// await pages.Push&lt;BattlePage&gt;("BattlePage", new WindowOptions
    /// {
    ///     Loader = sessionBridge
    /// });
    /// // ScopeManager.Instance.ClearScope("Session") releases everything.
    /// </code>
    /// </summary>
    public sealed class AddressableManagerScopeAdapter : IAssetLoader
    {
        private readonly AmLoader _inner;

        public AddressableManagerScopeAdapter(AmLoader inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        /// <summary>Synchronous loads aren't natively supported by the underlying scope loader (it's task-based); this kicks off the async load and returns a handle that completes when the task resolves. Awaiting the handle yields the asset.</summary>
        public AssetLoadHandle<T> Load<T>(string key) where T : UnityEngine.Object
            => LoadAsync<T>(key);

        public AssetLoadHandle<T> LoadAsync<T>(string key) where T : UnityEngine.Object
        {
            var handle = new BridgedHandle<T>(_inner);
            handle.Start(key);
            return handle;
        }

        public void Release(AssetLoadHandle handle)
        {
            if (handle is IBridgedHandle bridged)
            {
                bridged.ReleaseInner();
            }
            // If the caller passes a non-bridged handle by mistake, do nothing
            // — the underlying scope loader manages its own ref-counts, and
            // releasing a foreign handle through us would corrupt them.
        }

        // -- internals ------------------------------------------------------

        private interface IBridgedHandle
        {
            void ReleaseInner();
        }

        /// <summary>
        /// <see cref="AssetLoadHandle{T}"/> implementation that wraps the
        /// AddressableManager <c>IAssetHandle&lt;T&gt;</c>. Completion is
        /// driven by the underlying loader's Task/UniTask.
        /// </summary>
        private sealed class BridgedHandle<T> : AssetLoadHandle<T>, IBridgedHandle where T : UnityEngine.Object
        {
            private readonly AmLoader _loader;
            private AmHandle.IAssetHandle<T> _inner;
            private T _result;
            private Exception _exception;
            private bool _isDone;
            private float _progress;
            private Action<AssetLoadHandle<T>> _onComplete;

            public override bool IsDone => _isDone;
            public override float Progress => _progress;
            public override Exception Exception => _exception;
            public override T Result => _result;

            public BridgedHandle(AmLoader loader)
            {
                _loader = loader;
            }

            internal void Start(string key)
            {
#if EUI_UNITASK_SUPPORT
                RunUniTask(key).Forget();
#else
                _ = RunTask(key);
#endif
            }

#if EUI_UNITASK_SUPPORT
            private async UniTask RunUniTask(string key)
            {
                try
                {
                    _inner = await _loader.LoadAssetAsync<T>(key);
                    Settle();
                }
                catch (Exception ex)
                {
                    Fail(ex);
                }
            }
#else
            private async System.Threading.Tasks.Task RunTask(string key)
            {
                try
                {
                    _inner = await _loader.LoadAssetAsync<T>(key);
                    Settle();
                }
                catch (Exception ex)
                {
                    Fail(ex);
                }
            }
#endif

            private void Settle()
            {
                if (_inner == null || !_inner.IsValid)
                {
                    Fail(new Exception("AddressableManager loader returned an invalid handle."));
                    return;
                }
                _result = _inner.Asset;
                _progress = 1f;
                _isDone = true;
                _onComplete?.Invoke(this);
            }

            private void Fail(Exception ex)
            {
                _exception = ex;
                _progress = 1f;
                _isDone = true;
                _onComplete?.Invoke(this);
            }

            public override AssetLoadHandle<T> OnComplete(Action<AssetLoadHandle<T>> callback)
            {
                if (_isDone) callback?.Invoke(this);
                else _onComplete += callback;
                return this;
            }

            void IBridgedHandle.ReleaseInner()
            {
                _inner?.Release();
                _inner = null;
            }
        }
    }
}
#endif
