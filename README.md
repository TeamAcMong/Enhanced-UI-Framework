# Enhanced UI Framework

A comprehensive, production-ready UI navigation framework for Unity mobile games.

## Features

### Core Navigation System
- **Page Container**: Stack-based navigation for full-screen pages
- **Modal Container**: Overlay dialogs with multiple backdrop strategies
- **Sheet Container**: Tab-like navigation without history

### Rich Lifecycle Management
- 10+ lifecycle events per screen type (Initialize, WillEnter, DidEnter, WillExit, DidExit, Cleanup, etc.)
- External lifecycle registration with priority
- Inline lambda registration support
- Both coroutine and async/await support

### Flexible Transition System
- ScriptableObject-based animations (reusable across screens)
- MonoBehaviour-based animations for complex scenarios
- Timeline integration support
- Partner screen awareness for contextual transitions
- Regex pattern matching for per-screen animation overrides

### Advanced Asset Management
- Pluggable asset loader (Resources, Addressables, Remote CDN)
- Preloading API for instant transitions
- Object pooling for frequently used screens
- Smart caching with LRU policy
- Memory budget management

### Mobile-Optimized
- ✅ Safe Area auto-adaptation (iOS notch, Android gesture bar)
- ✅ Android back button handling
- ✅ Orientation management
- ✅ Performance optimizations (reduced GC, frame rate control)
- ✅ Memory pressure monitoring

### Developer Experience
- Optional MVP (Model-View-Presenter) framework
- Code generation tools (Screen templates, Presenter boilerplate)
- Debug tools (Lifecycle visualizer, Screen hierarchy, Performance analyzer)
- Comprehensive validation (Runtime, Editor, Build-time)
- 10 sample projects from basic to advanced

## Installation

### Via Package Manager (Git URL)
1. Open Window > Package Manager
2. Click "+" > Add package from git URL
3. Enter: `https://github.com/yourstudio/enhanced-ui-framework.git`

### Via Package Manager (Local)
1. Clone this repository
2. Open Window > Package Manager
3. Click "+" > Add package from disk
4. Select `package.json` in the cloned folder

## Quick Start

### 1. Setup
```csharp
// Create UI Manager in scene
GameObject uiRoot = new GameObject("UIRoot");
// Containers will auto-create on first use
```

### 2. Create a Screen
```csharp
using EnhancedUI;
using UnityEngine;

public class HomePage : Page
{
    public override void Initialize()
    {
        Debug.Log("Home Page Initialized");
    }

    public override void DidPushEnter()
    {
        Debug.Log("Home Page Entered");
    }
}
```

### 3. Navigate
```csharp
// Show a page
var pageContainer = PageContainer.Find("Main");
await pageContainer.Push<HomePage>("Prefabs/HomePage", playAnimation: true);

// Show a modal
var modalContainer = ModalContainer.Find("Main");
await modalContainer.Push<SettingsModal>("Prefabs/SettingsModal", playAnimation: true);

// Go back
await pageContainer.Pop(playAnimation: true);
```

## Documentation

- [Getting Started](Documentation~/GettingStarted.md)
- [Architecture Overview](Documentation~/Architecture.md)
- [Lifecycle System](Documentation~/Lifecycle.md)
- [Transition Animations](Documentation~/Transitions.md)
- [Asset Management](Documentation~/AssetManagement.md)
- [MVP Pattern Guide](Documentation~/MVP_Pattern.md)
- [Mobile Optimization](Documentation~/MobileOptimization.md)
- [API Reference](Documentation~/API_Reference.md)
- [Migration from UnityScreenNavigator](Documentation~/Migration_From_USN.md)

## Samples

Import samples via Package Manager:
1. Open Window > Package Manager
2. Select "Enhanced UI Framework"
3. Expand "Samples" section
4. Import desired samples

## Requirements

- Unity 2021.3 or later
- TextMeshPro (optional, for samples)
- Addressables 1.17.4+ (optional, for addressable asset loading)
- UniTask 2.0.0+ (optional, for async/await support)

## License

MIT License - see LICENSE.md

## Support

- Documentation: [Link to docs]
- Issues: [Link to issue tracker]
- Forum: [Link to forum]
- Email: contact@yourstudio.com
