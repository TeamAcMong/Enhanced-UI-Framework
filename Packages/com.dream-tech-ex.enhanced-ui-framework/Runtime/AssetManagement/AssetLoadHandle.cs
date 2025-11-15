using System;
using System.Collections;
using UnityEngine;

namespace EnhancedUI.AssetManagement
{
    /// <summary>
    /// Handle for asset loading operations
    /// </summary>
    public abstract class AssetLoadHandle : IEnumerator
    {
        public abstract bool IsDone { get; }
        public abstract float Progress { get; }
        public abstract Exception Exception { get; }
        public abstract UnityEngine.Object Asset { get; }

        public bool HasError => Exception != null;

        // IEnumerator implementation for coroutine support
        public bool MoveNext() => !IsDone;
        public void Reset() { }
        public object Current => null;
    }

    /// <summary>
    /// Generic typed version of AssetLoadHandle
    /// </summary>
    public abstract class AssetLoadHandle<T> : AssetLoadHandle where T : UnityEngine.Object
    {
        public abstract T Result { get; }

        public override UnityEngine.Object Asset => Result;

        /// <summary>
        /// Add a completion callback
        /// </summary>
        public abstract AssetLoadHandle<T> OnComplete(Action<AssetLoadHandle<T>> callback);
    }

    /// <summary>
    /// Immediate asset load handle (for synchronous loading)
    /// </summary>
    public class ImmediateAssetLoadHandle<T> : AssetLoadHandle<T> where T : UnityEngine.Object
    {
        private readonly T _result;
        private readonly Exception _exception;

        public ImmediateAssetLoadHandle(T result)
        {
            _result = result;
            _exception = null;
        }

        public ImmediateAssetLoadHandle(Exception exception)
        {
            _result = null;
            _exception = exception;
        }

        public override bool IsDone => true;
        public override float Progress => 1f;
        public override Exception Exception => _exception;
        public override T Result => _result;

        public override AssetLoadHandle<T> OnComplete(Action<AssetLoadHandle<T>> callback)
        {
            callback?.Invoke(this);
            return this;
        }
    }

    /// <summary>
    /// Async asset load handle (for asynchronous loading)
    /// </summary>
    public class AsyncAssetLoadHandle<T> : AssetLoadHandle<T> where T : UnityEngine.Object
    {
        private bool _isDone;
        private float _progress;
        private T _result;
        private Exception _exception;
        private Action<AssetLoadHandle<T>> _onComplete;

        public override bool IsDone => _isDone;
        public override float Progress => _progress;
        public override Exception Exception => _exception;
        public override T Result => _result;

        public void SetProgress(float progress)
        {
            _progress = Mathf.Clamp01(progress);
        }

        public void Complete(T result)
        {
            _result = result;
            _progress = 1f;
            _isDone = true;
            _onComplete?.Invoke(this);
        }

        public void CompleteWithError(Exception exception)
        {
            _exception = exception;
            _progress = 1f;
            _isDone = true;
            _onComplete?.Invoke(this);
        }

        public override AssetLoadHandle<T> OnComplete(Action<AssetLoadHandle<T>> callback)
        {
            if (_isDone)
            {
                callback?.Invoke(this);
            }
            else
            {
                _onComplete += callback;
            }
            return this;
        }
    }
}
