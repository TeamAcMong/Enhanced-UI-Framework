using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace EnhancedUI
{
    /// <summary>
    /// Handle for asynchronous processes that supports multiple wait patterns:
    /// - Coroutine (yield return)
    /// - Task (await)
    /// - Callback (OnTerminate event)
    /// </summary>
    public class AsyncProcessHandle : IEnumerator
    {
        private Action<AsyncProcessHandle> _onTerminate;
        private Action<Exception> _onError;
        private bool _isTerminated;
        private Exception _exception;
        private object _result;

        public bool IsTerminated => _isTerminated;
        public bool HasError => _exception != null;
        public Exception Exception => _exception;
        public object Result => _result;

        /// <summary>
        /// Event raised when the process completes (successfully or with error)
        /// </summary>
        public event Action<AsyncProcessHandle> OnTerminate
        {
            add
            {
                if (_isTerminated)
                {
                    value?.Invoke(this);
                }
                else
                {
                    _onTerminate += value;
                }
            }
            remove => _onTerminate -= value;
        }

        /// <summary>
        /// Event raised when the process encounters an error
        /// </summary>
        public event Action<Exception> OnError
        {
            add
            {
                if (_isTerminated && _exception != null)
                {
                    value?.Invoke(_exception);
                }
                else
                {
                    _onError += value;
                }
            }
            remove => _onError -= value;
        }

        /// <summary>
        /// Task that completes when the process terminates
        /// </summary>
        public Task Task
        {
            get
            {
                if (_isTerminated)
                {
                    return _exception != null
                        ? System.Threading.Tasks.Task.FromException(_exception)
                        : System.Threading.Tasks.Task.CompletedTask;
                }

                var tcs = new TaskCompletionSource<bool>();
                OnTerminate += handle =>
                {
                    if (handle.HasError)
                        tcs.SetException(handle.Exception);
                    else
                        tcs.SetResult(true);
                };
                return tcs.Task;
            }
        }

        /// <summary>
        /// Complete the process successfully
        /// </summary>
        public void Complete(object result = null)
        {
            if (_isTerminated)
                return;

            _result = result;
            _isTerminated = true;
            _onTerminate?.Invoke(this);
        }

        /// <summary>
        /// Complete the process with an error
        /// </summary>
        public void CompleteWithError(Exception exception)
        {
            if (_isTerminated)
                return;

            _exception = exception;
            _isTerminated = true;
            _onError?.Invoke(exception);
            _onTerminate?.Invoke(this);
        }

        // IEnumerator implementation for coroutine support
        public bool MoveNext() => !_isTerminated;
        public void Reset() { }
        public object Current => null;
    }

    /// <summary>
    /// Generic version of AsyncProcessHandle that returns a typed result
    /// </summary>
    public class AsyncProcessHandle<T> : IEnumerator
    {
        private Action<AsyncProcessHandle<T>> _onTerminate;
        private Action<Exception> _onError;
        private bool _isTerminated;
        private Exception _exception;
        private T _result;

        public bool IsTerminated => _isTerminated;
        public bool HasError => _exception != null;
        public Exception Exception => _exception;
        public T Result => _result;

        public event Action<AsyncProcessHandle<T>> OnTerminate
        {
            add
            {
                if (_isTerminated)
                {
                    value?.Invoke(this);
                }
                else
                {
                    _onTerminate += value;
                }
            }
            remove => _onTerminate -= value;
        }

        public event Action<Exception> OnError
        {
            add
            {
                if (_isTerminated && _exception != null)
                {
                    value?.Invoke(_exception);
                }
                else
                {
                    _onError += value;
                }
            }
            remove => _onError -= value;
        }

        public Task<T> Task
        {
            get
            {
                if (_isTerminated)
                {
                    return _exception != null
                        ? System.Threading.Tasks.Task.FromException<T>(_exception)
                        : System.Threading.Tasks.Task.FromResult(_result);
                }

                var tcs = new TaskCompletionSource<T>();
                OnTerminate += handle =>
                {
                    if (handle.HasError)
                        tcs.SetException(handle.Exception);
                    else
                        tcs.SetResult(handle.Result);
                };
                return tcs.Task;
            }
        }

        public void Complete(T result)
        {
            if (_isTerminated)
                return;

            _result = result;
            _isTerminated = true;
            _onTerminate?.Invoke(this);
        }

        public void CompleteWithError(Exception exception)
        {
            if (_isTerminated)
                return;

            _exception = exception;
            _isTerminated = true;
            _onError?.Invoke(exception);
            _onTerminate?.Invoke(this);
        }

        // IEnumerator implementation
        public bool MoveNext() => !_isTerminated;
        public void Reset() { }
        public object Current => null;
    }
}
