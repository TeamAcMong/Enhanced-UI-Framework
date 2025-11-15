using System;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.Utilities;

namespace EnhancedUI.Platform.Orientation
{
    /// <summary>
    /// Manages screen orientation changes
    /// </summary>
    public class OrientationManager : MonoBehaviour
    {
        private static OrientationManager _instance;

        private readonly List<IOrientationListener> _listeners = new List<IOrientationListener>();
        private ScreenOrientation _lastOrientation;

        public static OrientationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[Orientation Manager]");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<OrientationManager>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            _lastOrientation = UnityEngine.Screen.orientation;
        }

        private void Update()
        {
            if (!EnhancedUISettings.Instance.enableOrientationManagement)
                return;

            // Check for orientation change
            if (UnityEngine.Screen.orientation != _lastOrientation)
            {
                ScreenLogger.Log(ScreenLogger.LogCategory.Info,
                    $"Orientation changed: {_lastOrientation} -> {UnityEngine.Screen.orientation}");

                NotifyOrientationChanged(UnityEngine.Screen.orientation);
                _lastOrientation = UnityEngine.Screen.orientation;
            }
        }

        /// <summary>
        /// Add an orientation listener
        /// </summary>
        public void AddListener(IOrientationListener listener)
        {
            if (listener == null)
                return;

            if (!_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }

        /// <summary>
        /// Remove an orientation listener
        /// </summary>
        public void RemoveListener(IOrientationListener listener)
        {
            _listeners.Remove(listener);
        }

        /// <summary>
        /// Clear all listeners
        /// </summary>
        public void ClearListeners()
        {
            _listeners.Clear();
        }

        /// <summary>
        /// Lock orientation
        /// </summary>
        public void LockOrientation(ScreenOrientation orientation)
        {
            UnityEngine.Screen.orientation = orientation;
            UnityEngine.Screen.autorotateToPortrait = false;
            UnityEngine.Screen.autorotateToPortraitUpsideDown = false;
            UnityEngine.Screen.autorotateToLandscapeLeft = false;
            UnityEngine.Screen.autorotateToLandscapeRight = false;

            ScreenLogger.Log(ScreenLogger.LogCategory.Info, $"Orientation locked to: {orientation}");
        }

        /// <summary>
        /// Unlock orientation and allow auto-rotation
        /// </summary>
        public void UnlockOrientation(bool portrait = true, bool landscape = true)
        {
            UnityEngine.Screen.orientation = ScreenOrientation.AutoRotation;
            UnityEngine.Screen.autorotateToPortrait = portrait;
            UnityEngine.Screen.autorotateToPortraitUpsideDown = portrait;
            UnityEngine.Screen.autorotateToLandscapeLeft = landscape;
            UnityEngine.Screen.autorotateToLandscapeRight = landscape;

            ScreenLogger.Log(ScreenLogger.LogCategory.Info,
                $"Orientation unlocked (portrait: {portrait}, landscape: {landscape})");
        }

        private void NotifyOrientationChanged(ScreenOrientation newOrientation)
        {
            foreach (var listener in _listeners.ToArray()) // ToArray to avoid modification during iteration
            {
                try
                {
                    listener.OnOrientationChanged(newOrientation);
                }
                catch (Exception ex)
                {
                    ScreenLogger.LogException(ex);
                }
            }
        }
    }

    /// <summary>
    /// Interface for objects that listen to orientation changes
    /// </summary>
    public interface IOrientationListener
    {
        void OnOrientationChanged(ScreenOrientation newOrientation);
    }
}
