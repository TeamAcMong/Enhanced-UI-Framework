namespace EnhancedUI.Demo
{
    /// <summary>
    /// Interface for tab sheets to provide tab index information
    /// Used by tab animations to determine slide direction
    /// </summary>
    public interface ITabContent
    {
        /// <summary>
        /// Tab index (0 = leftmost tab, 1 = second tab, etc.)
        /// Used to determine horizontal slide direction in animations
        /// </summary>
        int TabIndex { get; }
    }
}
