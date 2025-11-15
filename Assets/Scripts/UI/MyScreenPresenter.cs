using System.Collections;
using EnhancedUI;
using EnhancedUI.MVP;
using EnhancedUI.Lifecycle;

public interface IMyScreenView : IView<MyScreenViewState>
{
    // Add view-specific methods here
}

public class MyScreenPresenter : PagePresenterBase<MyScreen, IMyScreenView, MyScreenViewState>
{
    public MyScreenPresenter(MyScreen screen) : base(screen)
    {
    }

#if EUI_UNITASK_SUPPORT
    public override async Cysharp.Threading.Tasks.UniTask OnViewLoaded()
    {
        // Load data, initialize presenter logic
        await base.OnViewLoaded();
    }
#else
    public override IEnumerator OnViewLoaded()
    {
        // Load data, initialize presenter logic
        yield return base.OnViewLoaded();
    }
#endif

    public override void DidPushEnter()
    {
        base.DidPushEnter();
        // Handle screen enter
    }
}
