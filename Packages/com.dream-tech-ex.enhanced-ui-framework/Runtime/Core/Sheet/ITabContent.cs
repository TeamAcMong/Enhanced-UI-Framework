namespace EnhancedUI
{
    /// <summary>
    /// Optional contract for Sheet screens that participate in horizontal tab navigation.
    /// Tab-aware transition animations (e.g. horizontal slide) read TabIndex to determine
    /// slide direction relative to the partner sheet.
    /// </summary>
    public interface ITabContent
    {
        /// <summary>
        /// Tab index (0 = leftmost tab, 1 = second tab, etc.).
        /// </summary>
        int TabIndex { get; }
    }
}
