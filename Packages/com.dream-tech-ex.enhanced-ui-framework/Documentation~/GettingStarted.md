# Getting Started with Enhanced UI Framework

## Installation

### Via Package Manager
1. Open Unity Package Manager (Window > Package Manager)
2. Click "+" > Add package from git URL
3. Enter the repository URL

### Via Local Package
1. Clone or download this repository
2. In Unity, open Package Manager
3. Click "+" > Add package from disk
4. Select the `package.json` file

## Quick Setup

### 1. Run Setup Wizard
```
Tools > Enhanced UI > Setup Wizard
```

This will:
- Create EnhancedUISettings asset
- Create sample PageContainer, ModalContainer, SheetContainer
- Setup BackButtonHandler for mobile
- Configure safe area support

### 2. Create Your First Page

#### Option A: Use Screen Generator
```
Tools > Enhanced UI > Generate Screen
```
- Enter screen name (e.g., "HomePage")
- Select "Page" as screen type
- Check "Generate Presenter" and "Generate ViewState" if using MVP
- Click Generate

#### Option B: Manual Creation

**Create the script:**
```csharp
using UnityEngine;
using EnhancedUI;

public class HomePage : Page
{
    public override void Initialize()
    {
        Debug.Log("HomePage initialized");
    }

    public override void DidPushEnter()
    {
        Debug.Log("HomePage entered");
    }
}
```

**Create the prefab:**
1. Create a GameObject with RectTransform
2. Add CanvasGroup component
3. Add your HomePage script
4. Save as prefab in Resources or mark as Addressable

### 3. Navigate to Your Page

```csharp
using UnityEngine;
using EnhancedUI;

public class GameBootstrap : MonoBehaviour
{
    private void Start()
    {
        var pageContainer = PageContainer.Find("PageContainer");

        // Simple push
        pageContainer.Push<HomePage>("Prefabs/HomePage", new WindowOptions
        {
            PlayAnimation = true,
            LoadAsync = true
        });
    }
}
```

## Core Concepts

### Containers
Three types of containers for different UI patterns:

- **PageContainer**: Stack-based navigation (Push/Pop)
- **ModalContainer**: Overlay dialogs with backdrop
- **SheetContainer**: Tab-like navigation (Show/Hide)

### Screens
Three types of screens corresponding to containers:

- **Page**: Full-screen pages with history
- **Modal**: Popup/dialog windows
- **Sheet**: Tab content without history

### Lifecycle Events
Rich lifecycle for fine-grained control:

**Page Lifecycle:**
- Initialize → WillPushEnter → DidPushEnter → [Active]
- WillPopExit → DidPopExit → Cleanup → [Destroyed]

**Going Back:**
- WillPopEnter → DidPopEnter → [Re-Activated]

### Transitions
Flexible animation system:

```csharp
// Assign in inspector or create ScriptableObject
[CreateAssetMenu(menuName = "Enhanced UI/Transition/My Slide")]
public class MySlideTransition : TransitionAnimationObject
{
    public override float Duration => 0.3f;

    public override void Setup(RectTransform rectTransform)
    {
        // Setup animation
    }

    public override void SetTime(float time)
    {
        // Update animation at time
    }
}
```

## Mobile Features

### Safe Area
Automatically adapts to notch and home indicator:

```csharp
[SerializeField] private SafeAreaAdapter safeAreaAdapter;
```

Or add SafeAreaAdapter component to your screen root.

### Back Button
Auto-handling of Android back button:

```csharp
public class HomePage : Page, IBackButtonReceiver
{
    public bool OnBackButtonPressed()
    {
        // Custom back behavior
        return true; // Handled
    }
}
```

## Next Steps

- [Architecture Overview](Architecture.md) - Understand the framework structure
- [Lifecycle System](Lifecycle.md) - Deep dive into lifecycle events
- [Transitions](Transitions.md) - Create custom animations
- [MVP Pattern](MVP_Pattern.md) - Use Model-View-Presenter pattern
- [Mobile Optimization](MobileOptimization.md) - Performance tips

## Common Patterns

### Preloading
```csharp
// Preload in advance
var handle = pageContainer.Preload("Prefabs/ShopPage");
yield return handle;

// Instant transition (already loaded)
pageContainer.Push<ShopPage>("Prefabs/ShopPage");

// Release when done
pageContainer.ReleasePreloaded("Prefabs/ShopPage");
```

### Passing Data
```csharp
pageContainer.Push<ShopPage>("Prefabs/ShopPage", new WindowOptions
{
    Data = new ShopData { CategoryId = 5 },
    OnLoaded = page =>
    {
        var shopPage = page as ShopPage;
        shopPage.SetCategory(5);
    }
});
```

### Pop Multiple Pages
```csharp
// Pop 3 pages
pageContainer.Pop(playAnimation: true, popCount: 3);

// Pop to specific page
pageContainer.Pop(playAnimation: true, destinationPageId: "HomePage");
```

## Troubleshooting

### "Container not found"
Ensure containers are created via Setup Wizard or manually added to your Canvas.

### "Asset not found"
Check that prefabs are in Resources folder or marked as Addressables.

### "Animation not playing"
Verify TransitionAnimationObject is assigned in the screen's AnimationContainer.

### Performance issues
- Enable object pooling in settings
- Use smart caching for frequently accessed screens
- Profile with Performance Monitor

## Support

- GitHub Issues: [Link]
- Forum: [Link]
- Email: contact@yourstudio.com
