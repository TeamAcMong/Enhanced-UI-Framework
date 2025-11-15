namespace EnhancedUI.Platform.BackButton
{
    /// <summary>
    /// Interface for screens that handle back button events
    /// </summary>
    public interface IBackButtonReceiver
    {
        /// <summary>
        /// Called when back button is pressed
        /// </summary>
        /// <returns>True if handled, false to pass to next receiver</returns>
        bool OnBackButtonPressed();
    }
}
