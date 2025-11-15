# Changelog

All notable changes to Enhanced UI Framework will be documented in this file.

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
