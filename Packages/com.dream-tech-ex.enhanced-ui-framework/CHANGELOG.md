# Changelog

All notable changes to Enhanced UI Framework will be documented in this file.

## [1.2.0] - 2026-05-24 - Per-call loader override + AddressableManager scope bridge

Resolves the design-review gap "can't pin a `Push()` to a specific
scope". Adds an opt-in bridge for callers that want each screen to
live in its own `com.game.addressables` scope.

### Added
- **`WindowOptions.Loader`** field. When set, the container uses this
  loader for the call instead of its default (`SetAssetLoader` /
  `AssetLoaderProvider`). Each handle remembers its loader so `Pop`
  releases through the same one — important when the per-call loader
  has its own ref-counting (e.g. a scope adapter).
- **`AddressableManagerScopeAdapter`** — bridges
  `AddressableManager.Loaders.AssetLoader` into the framework's
  `IAssetLoader` interface. Compiled only when
  `com.game.addressables 4.0.0+` is installed (Runtime asmdef
  versionDefine `ADDRESSABLE_MANAGER_PRESENT`). The asmdef also adds
  `AddressableManager` to its references; Unity ignores the reference
  gracefully when the package is absent.
- **Per-handle loader tracking** in `PageContainer`, `ModalContainer`,
  and `SheetContainer` (`_loadersByPage` / `_loadersByModal` /
  `_loadersBySheet`) so the release path uses the same loader that
  produced the handle. Falls back to the container's `_assetLoader`
  for legacy handles loaded before this release.

### Usage
```csharp
// Once at bootstrap (assuming com.game.addressables is installed)
using AddressableManager.Managers;
using EnhancedUI.AssetManagement;

var sessionLoader = ScopeManager.Instance.GetOrCreateScope("Session");
var sessionBridge = new AddressableManagerScopeAdapter(sessionLoader);

// Per-push
await pages.Push<BattlePage>("BattlePage", new WindowOptions
{
    Loader = sessionBridge
});

// Later — releases every page that used sessionBridge
ScopeManager.Instance.ClearScope("Session");
```

### Notes
- Backwards-compatible. Existing code (no `options.Loader`) uses the
  container's default loader exactly as before.
- The adapter's `Release` is a no-op for non-bridged handles —
  consumers can't mix-and-match handles across loaders by mistake.

## [1.1.0] - 2026-05-23

### Added
- `EnhancedUI.ITabContent` is now part of the framework (`Runtime/Core/Sheet/ITabContent.cs`). Tab-aware transition animations can read `TabIndex` from any sheet to decide slide direction without depending on demo code.
- Bundled sample **"Mobile Game — Complete"** under `Samples~/MobileGameComplete` (importable via Package Manager). End-to-end example: lobby with bottom-tab sheets, modal settings, gameplay/level-selection pages, full MVP wiring.
- Documentation: `Documentation~/MVP_Pattern.md`, `Documentation~/EditorSetup.md`.

### Changed
- `package.json` now declares `com.cysharp.unitask` as a hard dependency (was implicit). Minimum Unity bumped to **2022.3** to match the rest of the toolchain.
- Demo and sample assets moved out of `Assets/` and into the standard UPM `Samples~/` folder. Import the sample from **Package Manager → Enhanced UI Framework → Samples**.
- Author field standardised to **DreamTech**.

### Removed
- Stale status / session markdown files (`FINAL_STATUS.md`, `IMPLEMENTATION_*`, `SESSION_SUMMARY.md`, `README_COMPLETE.md`, `EDITOR_TOOLS_SUMMARY.md`, `FINAL_SUMMARY.md`).
- Duplicated `SafeAreaAdapter` / `OrientationManager` from the demo (the framework versions in `Runtime/Platform/` are the source of truth).
- Scratch `Assets/Scripts/UI/` template files (`MyScreen`, `Test`, `SimpleTransition.asset`).

### Migration
- Any code that referenced `EnhancedUI.Demo.ITabContent` should import `EnhancedUI` instead — the interface signature is unchanged.
- Demo scenes are no longer auto-imported; import the **Mobile Game — Complete** sample to restore the previous play-mode experience.

## [1.0.0] - 2024-01-XX

### Added
- **Core Navigation System**
  - PageContainer for stack-based navigation
  - ModalContainer with 4 backdrop strategies
  - SheetContainer for tab-like navigation

- **Lifecycle Management**
  - 10 lifecycle events for Page/Modal
  - 6 lifecycle events for Sheet
  - External lifecycle registration with priority
  - Inline lambda/anonymous event support
  - Both coroutine and async/await support (UniTask optional)

- **Transition System**
  - ITransitionAnimation interface
  - TransitionAnimationObject (ScriptableObject-based)
  - TransitionAnimationBehaviour (MonoBehaviour-based)
  - SimpleTransitionAnimation with 11 easing types
  - Timeline integration via TimelineTransitionAnimationBehaviour
  - Partner screen awareness for contextual transitions
  - Per-screen animation overrides with regex matching

- **Asset Management**
  - IAssetLoader abstraction
  - ResourcesAssetLoader implementation
  - AddressableAssetLoader implementation (optional)
  - PreloadedAssetLoader for editor/testing
  - Preloading API for instant transitions
  - AssetLoadHandle with coroutine/Task/callback support

- **Mobile Optimizations**
  - SafeAreaAdapter for notch/home indicator support
  - BackButtonHandler for Android hardware back button
  - OrientationManager for screen rotation
  - PerformanceMonitor for FPS and memory tracking
  - ScreenPool for object pooling
  - ScreenCache with LRU policy and memory budget

- **MVP Framework (Optional)**
  - IView and IPresenter interfaces
  - PresenterBase with lifecycle integration
  - PagePresenterBase, ModalPresenterBase, SheetPresenterBase
  - ViewStateBase for observable state
  - PresenterFactory for dependency injection

- **Developer Tools**
  - Setup Wizard for initial configuration
  - Screen Generator for boilerplate code
  - Container Editor with runtime information
  - Container Validator for setup verification

- **Documentation**
  - Getting Started guide
  - Architecture overview
  - API reference
  - Sample projects (planned)

### Supported Unity Versions
- Unity 2021.3 or later

### Optional Dependencies
- Addressables 1.17.4+ (for AddressableAssetLoader)
- UniTask 2.0.0+ (for async/await lifecycle)
- UniRx 7.0.0+ (for reactive ViewState)

## Roadmap

### [1.1.0] - Planned
- Screen state serialization/restoration
- Navigation history serialization
- Deeplink support
- Analytics integration hooks
- A/B testing support for UI variants

### [1.2.0] - Planned
- Scene-based screens support
- Multi-canvas support
- Screen template library
- More built-in transition presets
- Gesture-based transitions

### [2.0.0] - Future
- Unity UI Toolkit (UIDocument) support
- Visual scripting integration
- Performance profiler integration
- Cloud-based remote config
