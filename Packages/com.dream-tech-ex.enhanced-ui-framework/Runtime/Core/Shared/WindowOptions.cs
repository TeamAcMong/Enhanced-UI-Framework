using System;
using UnityEngine;

namespace EnhancedUI
{
    /// <summary>
    /// Options for screen operations (Push, Pop, Show, etc.)
    /// </summary>
    public class WindowOptions
    {
        /// <summary>
        /// Load asset asynchronously
        /// </summary>
        public bool LoadAsync { get; set; } = true;

        /// <summary>
        /// Play transition animation
        /// </summary>
        public bool PlayAnimation { get; set; } = true;

        /// <summary>
        /// Add to history stack (Page only)
        /// </summary>
        public bool Stack { get; set; } = true;

        /// <summary>
        /// Callback after loading completes (before Initialize)
        /// </summary>
        public Action<Screen> OnLoaded { get; set; }

        /// <summary>
        /// Custom data to pass to the screen
        /// </summary>
        public object Data { get; set; }

        public static WindowOptions Default => new WindowOptions();

        public static WindowOptions WithoutAnimation => new WindowOptions { PlayAnimation = false };

        public static WindowOptions WithoutStack => new WindowOptions { Stack = false };

        public static WindowOptions Immediate => new WindowOptions
        {
            LoadAsync = false,
            PlayAnimation = false
        };
    }

    /// <summary>
    /// Generic version with typed screen
    /// </summary>
    public class WindowOptions<TScreen> : WindowOptions where TScreen : Screen
    {
        /// <summary>
        /// Typed callback after loading
        /// </summary>
        public new Action<TScreen> OnLoaded
        {
            get => _onLoaded;
            set
            {
                _onLoaded = value;
                base.OnLoaded = screen => value?.Invoke(screen as TScreen);
            }
        }

        private Action<TScreen> _onLoaded;
    }
}
