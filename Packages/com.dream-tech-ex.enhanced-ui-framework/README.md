# Enhanced UI Framework

A mobile-optimized UI navigation framework for Unity.
Container–Screen architecture with **Page** (stack), **Modal** (overlay) and **Sheet** (tabs); rich lifecycle hooks, partner-aware transitions, pluggable asset loaders, object pooling, safe-area + back-button + orientation handling, and an optional MVP layer. UniTask-powered async with a coroutine fallback.

> **Minimum Unity**: 2022.3 LTS — tested up to Unity 6.

## Why this framework?

- **Three container types** for the three navigation patterns you actually need (stack / overlay / tabs).
- **10 lifecycle events per screen** so you never have to guess where to put init/cleanup/animation hooks.
- **Pluggable asset loaders**: Resources, Addressables, Remote CDN, or your own.
- **Mobile out-of-the-box**: safe-area adapter, Android back button, orientation tracking, performance monitor.
- **MVP layer (optional)** when screens grow large enough to need it.
- **Editor tooling**: Setup Wizard, Container Validator, Health Check, Screen Generator, Performance Analyzer.

## Install

### Via Git URL (recommended)

In Unity: **Window → Package Manager → ＋ → Add package from git URL** and paste:

```
https://github.com/TeamAcMong/Enhanced-UI-Framework.git#1.1.0
```

Or pin in `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dreamtechex.enhanced-ui-framework": "https://github.com/TeamAcMong/Enhanced-UI-Framework.git#1.1.0"
  }
}
```

Tags are produced by `deploy.sh` using a git-subtree split, so each tag ships only the package contents (~KB, not the whole Unity project). See [DEPLOY_UPM_SUBTREE.md](../../DEPLOY_UPM_SUBTREE.md) for the publishing flow.

### Via local package

1. Clone this repository.
2. **Window → Package Manager → ＋ → Add package from disk**.
3. Pick `Packages/com.dream-tech-ex.enhanced-ui-framework/package.json`.

## Quick start

### 1. Run the Setup Wizard

**Tools → Enhanced UI → Setup Wizard** creates:

- `EnhancedUISettings` asset under `Assets/Resources/`
- A Canvas root with `SafeArea`, `PageContainer`, `ModalContainer`, `SheetContainer`
- `BackButtonHandler` (mobile)

If you'd rather wire it by hand, see [EditorSetup.md](Documentation~/EditorSetup.md).

### 2. Write a screen

```csharp
using EnhancedUI;
using Cysharp.Threading.Tasks;

public class HomePage : Page
{
    public override async UniTask Initialize()
    {
        // bind view-model, subscribe to events…
        await base.Initialize();
    }

    public override void DidPushEnter() { /* page is on-screen */ }

    public override void WillPopExit() { /* save state, kill tweens */ }
}
```

Save the prefab so the asset loader can find it — see [AssetLoading.md](Documentation~/AssetLoading.md):

| Loader | Where the prefab lives | Key you push with |
|---|---|---|
| `Resources` (default-friendly) | `Assets/.../Resources/Pages/HomePage.prefab` | `"Pages/HomePage"` |
| `Addressables` | anywhere, marked Addressable with address `HomePage` | `"HomePage"` |
| `Custom` | wherever your loader resolves | whatever your loader expects |

### 3. Navigate

```csharp
var pages = PageContainer.Find("Main");

// simplest push
await pages.Push<HomePage>("Pages/HomePage");

// with options
await pages.Push<HomePage>("Pages/HomePage", new WindowOptions
{
    LoadAsync     = true,
    PlayAnimation = true,
    Stack         = true,
    OnLoaded      = page => /* page is loaded, not yet entered */,
    Data          = new { fromTutorial = true },
});

// go back
await pages.Pop();
```

Full API surface lives in [Architecture.md](Documentation~/Architecture.md).

## Try the sample

Window → Package Manager → **Enhanced UI Framework** → Samples → **Mobile Game — Complete** → Import.

Lands at `Assets/Samples/Enhanced UI Framework/<version>/MobileGameComplete/`. Open `Scenes/DemoScene.unity` and press Play — `DemoBootstrap` pushes the home container with four tab sheets. See [the sample README](Samples~/MobileGameComplete/README.md) for the loader requirements and how to switch between Resources / Addressables / Custom.

## Documentation

| Doc | What's in it |
|---|---|
| [Getting Started](Documentation~/GettingStarted.md) | Setup, your first screen, your first transition |
| [Architecture](Documentation~/Architecture.md) | Containers, screens, lifecycle, transitions, asset pipeline |
| [Asset Loading](Documentation~/AssetLoading.md) | Resources vs Addressables vs Custom + how to switch |
| [Settings Reference](Documentation~/Settings.md) | Every field on `EnhancedUISettings` |
| [Editor Setup (Manual)](Documentation~/EditorSetup.md) | Hand-wire containers without the wizard |
| [MVP Pattern](Documentation~/MVP_Pattern.md) | Structure complex screens with Model / View / Presenter |

## Requirements

| | Version | Notes |
|---|---|---|
| Unity | **2022.3** LTS+ | tested up to Unity 6 |
| UniTask | 2.5.10+ | declared as a hard dependency on `com.cysharp.unitask` — resolved via the **OpenUPM scoped registry** (one-time project setup, see below). |
| TextMeshPro | any | only needed by the bundled sample |
| Addressables | 1.17.4+ | optional, only if `assetLoaderType = Addressables` |

### Add the OpenUPM scoped registry (one-time)

UniTask isn't on Unity's default registry, and UPM doesn't allow git URLs inside a *package's* own `dependencies` — so this package depends on UniTask via semver and resolves it through OpenUPM. Add this block to your project's `Packages/manifest.json` (alongside `dependencies`, not inside it):

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.cysharp.unitask"
      ]
    }
  ],
  "dependencies": {
    "com.dreamtechex.enhanced-ui-framework": "https://github.com/TeamAcMong/Enhanced-UI-Framework.git#1.1.0"
  }
}
```

Once the registry is registered, UPM will auto-resolve `com.cysharp.unitask` from OpenUPM the next time the package is added or refreshed. After that, `EUI_UNITASK_SUPPORT` activates automatically (via `versionDefines` in the asmdef) and the framework uses `UniTask`-based lifecycle. If UniTask is absent for any reason, the asmdef quietly falls back to `IEnumerator` coroutines — your code keeps compiling either way.

## Editor menus

| Menu | Use it for |
|---|---|
| `Tools/Enhanced UI/Setup Wizard` | One-click scene setup |
| `Tools/Enhanced UI/Control Center` | Dashboard with status of every container |
| `Tools/Enhanced UI/Settings` | Jump to `EnhancedUISettings` |
| `Tools/Enhanced UI/Generate Screen` | Scaffold a Page / Modal / Sheet + optional MVP |
| `Tools/Enhanced UI/Validate Setup` | Lint your containers |
| `Tools/Enhanced UI/Analysis Tools/Health Check` | Auto-check for common misconfigurations |
| `Tools/Enhanced UI/Analysis Tools/Performance Analyzer` | Frame-time + allocation profiler hook |
| `Tools/Enhanced UI/Debug Tools/Event Tracker` | Live view of lifecycle events as they fire |
| `Tools/Enhanced UI/Debug Tools/Transition Debugger` | Step through a transition frame-by-frame |
| `GameObject/Enhanced UI/{Page,Modal,Sheet} Container` | Drop a container into the current Canvas |

## License

MIT — see [LICENSE.md](LICENSE.md).

## Support

- Issues: <https://github.com/TeamAcMong/Enhanced-UI-Framework/issues>
- Maintainer: **DreamTech**
