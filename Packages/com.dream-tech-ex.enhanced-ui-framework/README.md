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

### Via Package Manager (Git URL — recommended)

Open **Window → Package Manager → ＋ → Add package from git URL**, then paste:

```
https://github.com/TeamAcMong/Enhanced-UI-Framework.git#1.1.0
```

Or pin via `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dreamtechex.enhanced-ui-framework": "https://github.com/TeamAcMong/Enhanced-UI-Framework.git#1.1.0"
  }
}
```

Tags are produced by `deploy.sh` using a git-subtree split, so each tag contains only the package contents (~KBs, not MBs). See [DEPLOY_UPM_SUBTREE.md](../../DEPLOY_UPM_SUBTREE.md) for the publishing flow.

### Via Package Manager (Local)
1. Clone this repository
2. Open Window > Package Manager
3. Click "+" > Add package from disk
4. Select `package.json` inside `Packages/com.dream-tech-ex.enhanced-ui-framework/`

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

Import the bundled sample via Package Manager:
1. Open Window > Package Manager
2. Select "Enhanced UI Framework"
3. Expand "Samples" section
4. Import **Mobile Game — Complete**

The sample lands in `Assets/Samples/Enhanced UI Framework/<version>/MobileGameComplete/`. Open `Scenes/DemoScene.unity` and press Play — `DemoBootstrap` pushes `HomeContainerPage` with four tab sheets and wires `NavigationManager` to the global UI.

## Requirements

- Unity **2022.3** or later (tested on Unity 6)
- UniTask (declared as a hard dependency in `package.json` — Unity will pull it automatically)
- TextMeshPro (optional, only required by the bundled sample)
- Addressables 1.17.4+ (optional, only if you opt into `AddressableAssetLoader`)

## License

MIT License — see [LICENSE.md](LICENSE.md).

## Support

- Issues: <https://github.com/TeamAcMong/Enhanced-UI-Framework/issues>
- Maintainer: **DreamTech**
