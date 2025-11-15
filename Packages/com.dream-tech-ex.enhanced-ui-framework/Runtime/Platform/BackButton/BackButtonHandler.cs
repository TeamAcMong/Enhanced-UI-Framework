using System;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.Utilities;

namespace EnhancedUI.Platform.BackButton
{
    /// <summary>
    /// Global handler for back button (Android hardware back button)
    /// </summary>
    public class BackButtonHandler : MonoBehaviour
    {
        private static BackButtonHandler _instance;

        private readonly Stack<IBackButtonReceiver> _receivers = new Stack<IBackButtonReceiver>();
        private Action _defaultBackAction;

        public static BackButtonHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[Back Button Handler]");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<BackButtonHandler>();
                }
                return _instance;
            }
        }

        private void Update()
        {
            if (!EnhancedUISettings.Instance.enableBackButton)
                return;

            // Check for back button input (Android back button or Escape key)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                HandleBackButton();
            }
        }

        /// <summary>
        /// Push a back button receiver (higher priority)
        /// </summary>
        public void PushReceiver(IBackButtonReceiver receiver)
        {
            if (receiver == null)
            {
                ScreenLogger.LogWarning("Attempted to push null back button receiver");
                return;
            }

            _receivers.Push(receiver);
            ScreenLogger.Log(ScreenLogger.LogCategory.Info, $"Back button receiver pushed: {receiver.GetType().Name}");
        }

        /// <summary>
        /// Pop the top receiver
        /// </summary>
        public void PopReceiver()
        {
            if (_receivers.Count > 0)
            {
                var receiver = _receivers.Pop();
                ScreenLogger.Log(ScreenLogger.LogCategory.Info, $"Back button receiver popped: {receiver.GetType().Name}");
            }
        }

        /// <summary>
        /// Remove a specific receiver
        /// </summary>
        public bool RemoveReceiver(IBackButtonReceiver receiver)
        {
            if (receiver == null)
                return false;

            // Create temporary stack to find and remove
            var temp = new Stack<IBackButtonReceiver>();
            bool found = false;

            while (_receivers.Count > 0)
            {
                var current = _receivers.Pop();
                if (current == receiver)
                {
                    found = true;
                    break;
                }
                temp.Push(current);
            }

            // Restore stack
            while (temp.Count > 0)
            {
                _receivers.Push(temp.Pop());
            }

            if (found)
            {
                ScreenLogger.Log(ScreenLogger.LogCategory.Info, $"Back button receiver removed: {receiver.GetType().Name}");
            }

            return found;
        }

        /// <summary>
        /// Set default action when no receivers handle the back button
        /// </summary>
        public void SetDefaultAction(Action action)
        {
            _defaultBackAction = action;
        }

        /// <summary>
        /// Clear all receivers
        /// </summary>
        public void ClearReceivers()
        {
            _receivers.Clear();
            ScreenLogger.Log(ScreenLogger.LogCategory.Info, "All back button receivers cleared");
        }

        private void HandleBackButton()
        {
            // Try receivers in LIFO order
            foreach (var receiver in _receivers)
            {
                try
                {
                    if (receiver.OnBackButtonPressed())
                    {
                        ScreenLogger.Log(ScreenLogger.LogCategory.Info, $"Back button handled by: {receiver.GetType().Name}");
                        return; // Handled
                    }
                }
                catch (Exception ex)
                {
                    ScreenLogger.LogException(ex);
                }
            }

            // No receiver handled it, use default action
            if (_defaultBackAction != null)
            {
                ScreenLogger.Log(ScreenLogger.LogCategory.Info, "Back button handled by default action");
                _defaultBackAction.Invoke();
            }
            else
            {
                // Default behavior: quit application (mobile)
#if UNITY_ANDROID && !UNITY_EDITOR
                ScreenLogger.Log(ScreenLogger.LogCategory.Info, "Back button: quitting application");
                Application.Quit();
#endif
            }
        }

        /// <summary>
        /// Helper to auto-register screens as back button receivers
        /// </summary>
        public static void RegisterScreen(Screen screen)
        {
            if (screen is IBackButtonReceiver receiver)
            {
                Instance.PushReceiver(receiver);
            }
        }

        /// <summary>
        /// Helper to auto-unregister screens
        /// </summary>
        public static void UnregisterScreen(Screen screen)
        {
            if (screen is IBackButtonReceiver receiver)
            {
                Instance.RemoveReceiver(receiver);
            }
        }
    }
}
