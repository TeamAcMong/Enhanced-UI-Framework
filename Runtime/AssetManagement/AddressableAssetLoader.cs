#if EUI_ADDRESSABLES_SUPPORT
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace EnhancedUI.AssetManagement
{
    /// <summary>
    /// Asset loader that loads via Addressables system
    /// Requires: com.unity.addressables 1.17.4+
    /// </summary>
    public class AddressableAssetLoader : IAssetLoader
    {
        public AssetLoadHandle<T> Load<T>(string key) where T : UnityEngine.Object
        {
            // Synchronous load (requires Addressables 1.17.4+)
            try
            {
                var asyncOp = Addressables.LoadAssetAsync<T>(key);
                var asset = asyncOp.WaitForCompletion();

                if (asyncOp.Status == AsyncOperationStatus.Succeeded)
                {
                    return new AddressableAssetLoadHandle<T>(asyncOp);
                }
                else
                {
                    return new ImmediateAssetLoadHandle<T>(
                        new Exception($"Failed to load addressable: {key}\n{asyncOp.OperationException}"));
                }
            }
            catch (Exception ex)
            {
                return new ImmediateAssetLoadHandle<T>(ex);
            }
        }

        public AssetLoadHandle<T> LoadAsync<T>(string key) where T : UnityEngine.Object
        {
            var handle = new AddressableAssetLoadHandle<T>();
            var asyncOp = Addressables.LoadAssetAsync<T>(key);

            asyncOp.Completed += op =>
            {
                if (op.Status == AsyncOperationStatus.Succeeded)
                {
                    handle.SetOperation(op);
                }
                else
                {
                    handle.CompleteWithError(new Exception($"Failed to load addressable: {key}\n{op.OperationException}"));
                }
            };

            return handle;
        }

        public void Release(AssetLoadHandle handle)
        {
            if (handle is IAddressableAssetLoadHandle addressableHandle)
            {
                addressableHandle.Release();
            }
        }
    }

    internal interface IAddressableAssetLoadHandle
    {
        void Release();
    }

    internal abstract class AddressableAssetLoadHandleBase<T> : AssetLoadHandle<T>, IAddressableAssetLoadHandle where T : UnityEngine.Object
    {
        public abstract void Release();
    }

    internal class AddressableAssetLoadHandle<T> : AddressableAssetLoadHandleBase<T> where T : UnityEngine.Object
    {
        private AsyncOperationHandle<T> _operation;
        private bool _hasOperation;
        private Action<AssetLoadHandle<T>> _onComplete;
        private Exception _exception;

        public override bool IsDone => _hasOperation && _operation.IsDone;
        public override float Progress => _hasOperation ? _operation.PercentComplete : 0f;
        public override Exception Exception => _exception ?? (_hasOperation ? _operation.OperationException : null);
        public override T Result => _hasOperation && _operation.IsValid() ? _operation.Result : null;

        public AddressableAssetLoadHandle()
        {
            _hasOperation = false;
        }

        public AddressableAssetLoadHandle(AsyncOperationHandle<T> operation)
        {
            _operation = operation;
            _hasOperation = true;
        }

        public void SetOperation(AsyncOperationHandle<T> operation)
        {
            _operation = operation;
            _hasOperation = true;
            _onComplete?.Invoke(this);
        }

        public void CompleteWithError(Exception exception)
        {
            _exception = exception;
            _hasOperation = true;
            _onComplete?.Invoke(this);
        }

        public override AssetLoadHandle<T> OnComplete(Action<AssetLoadHandle<T>> callback)
        {
            if (IsDone)
            {
                callback?.Invoke(this);
            }
            else
            {
                _onComplete += callback;
            }
            return this;
        }

        public override void Release()
        {
            if (_hasOperation && _operation.IsValid())
            {
                Addressables.Release(_operation);
                _hasOperation = false;
            }
        }
    }
}
#endif
