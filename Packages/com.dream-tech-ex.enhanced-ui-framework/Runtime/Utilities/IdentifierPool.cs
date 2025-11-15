using System.Collections.Generic;

namespace EnhancedUI.Utilities
{
    /// <summary>
    /// Generates unique identifiers for screens
    /// </summary>
    public static class IdentifierPool
    {
        private static int _nextId = 0;
        private static readonly Dictionary<string, int> _counters = new Dictionary<string, int>();

        /// <summary>
        /// Generate a unique numeric ID
        /// </summary>
        public static int GenerateId()
        {
            return _nextId++;
        }

        /// <summary>
        /// Generate a unique string identifier with optional prefix
        /// </summary>
        public static string GenerateStringId(string prefix = "screen")
        {
            if (!_counters.ContainsKey(prefix))
            {
                _counters[prefix] = 0;
            }

            int count = _counters[prefix]++;
            return $"{prefix}_{count}";
        }

        /// <summary>
        /// Reset all counters (useful for testing)
        /// </summary>
        public static void Reset()
        {
            _nextId = 0;
            _counters.Clear();
        }
    }
}
