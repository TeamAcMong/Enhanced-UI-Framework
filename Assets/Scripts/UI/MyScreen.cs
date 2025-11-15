using UnityEngine;
using EnhancedUI;
using EnhancedUI.MVP;

public class MyScreen : Page, IView<MyScreenViewState>
{
    [SerializeField] private MyScreenViewState _viewState;

    
    public MyScreenViewState ViewState => _viewState;

    public void OnViewInitialized()
    {
        Debug.Log("MyScreen initialized");
    }

    public void OnViewDestroyed()
    {
        Debug.Log("MyScreen destroyed");
    }
}
