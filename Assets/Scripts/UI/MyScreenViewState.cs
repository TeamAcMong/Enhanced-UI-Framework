using System;
using EnhancedUI.MVP;

[Serializable]
public class MyScreenViewState : ViewStateBase
{
    // Add view state properties here
    // Example:
    // public string Title { get; set; }
    // public bool IsLoading { get; set; }

    public void UpdateTitle(string title)
    {
        // Update property and notify
        NotifyStateChanged();
    }
}
