# Architecture Overview

## Design Philosophy

Enhanced UI Framework follows these core principles:

1. **Separation of Concerns**: Container manages lifecycle, Screen handles view logic, Transitions are pluggable
2. **Flexibility**: Support multiple asset loaders, animation systems, and architectural patterns
3. **Mobile-First**: Built-in support for safe area, back button, and performance optimization
4. **Developer Experience**: Rich tooling, validation, and debugging support

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    EnhancedUISettings                    │
│            (Global Configuration ScriptableObject)       │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│                   Container Layer                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │PageContainer │  │ModalContainer│  │SheetContainer│  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│                    Screen Layer                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │    Page      │  │    Modal     │  │    Sheet     │  │
│  │  (MonoBeh)   │  │  (MonoBeh)   │  │  (MonoBeh)   │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────┐
│                   Support Systems                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │AssetManagement│  │  Transitions │  │  Lifecycle   │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  │
│  │ Performance  │  │  BackButton  │  │  SafeArea    │  │
│  └──────────────┘  └──────────────┘  └──────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## Core Components

### 1. Containers
Containers manage screen instances and their lifecycle.

**PageContainer**
- Stack-based navigation (LIFO)
- Push/Pop operations
- History management
- Preloading support

**ModalContainer**
- Overlay management
- Backdrop strategies (4 types)
- Stack support (multiple modals)
- Click-to-close backdrop

**SheetContainer**
- Tab-like navigation
- Register/Show/Hide operations
- No history (state preserved)
- Only one active at a time

### 2. Screens
MonoBehaviour components attached to UI prefabs.

**Common Features:**
- Lifecycle event system
- Transition animation support
- External lifecycle registration
- Partner screen awareness

**Type-Specific:**
- Page: Push/Pop lifecycle (10 events)
- Modal: Push/Pop lifecycle (10 events)
- Sheet: Show/Hide lifecycle (6 events)

### 3. Asset Management

**IAssetLoader Interface**
```csharp
public interface IAssetLoader
{
    AssetLoadHandle<T> Load<T>(string key);
    AssetLoadHandle<T> LoadAsync<T>(string key);
    void Release(AssetLoadHandle handle);
}
```

**Built-in Loaders:**
- ResourcesAssetLoader (default)
- AddressableAssetLoader (requires Addressables package)
- PreloadedAssetLoader (for editor/testing)

**Extensions:**
- ScreenPool: Object pooling for frequent screens
- ScreenCache: LRU cache with memory budget

### 4. Lifecycle System

**Event Dispatcher**
```
Priority -1  →  Screen's virtual methods  →  Priority +1
    ↓                     ↓                        ↓
External Listeners   Screen Logic          Presenters (MVP)
```

**Registration Methods:**
1. Override virtual methods in Screen subclass
2. AddLifecycleEvent with priority
3. Lambda/anonymous registration

### 5. Transition System

**Animation Abstraction:**
```
ITransitionAnimation
    ├── TransitionAnimationObject (ScriptableObject)
    │   └── SimpleTransitionAnimationObject
    └── TransitionAnimationBehaviour (MonoBehaviour)
        └── TimelineTransitionAnimationBehaviour
```

**Features:**
- Partner screen awareness (PartnerRectTransform)
- Per-screen animation overrides (regex matching)
- Easing functions (11 built-in types)
- Timeline integration

### 6. Platform Support

**SafeAreaAdapter**
- Auto-detect notch/home indicator
- Configurable per edge (top/bottom/left/right)
- Live updating on orientation change

**BackButtonHandler**
- Priority stack (LIFO)
- IBackButtonReceiver interface
- Default action support
- Auto-quit on Android

**OrientationManager**
- Orientation change detection
- Lock/unlock API
- IOrientationListener interface

## Data Flow

### Push Page Flow
```
User Request
    ↓
PageContainer.Push()
    ↓
Load Asset (via IAssetLoader)
    ↓
Instantiate Prefab
    ↓
Initialize Lifecycle
    ↓
OnLoaded Callback
    ↓
WillPushEnter (new) + WillPushExit (old)
    ↓
Play Transitions (Enter + Exit)
    ↓
DidPushEnter (new) + DidPushExit (old)
    ↓
Complete
```

### Pop Page Flow
```
User Request (or Back Button)
    ↓
PageContainer.Pop()
    ↓
Identify Pages to Pop
    ↓
WillPopExit (top) + WillPopEnter (previous)
    ↓
Play Transitions (Exit + Enter)
    ↓
DidPopExit (top) + DidPopEnter (previous)
    ↓
Cleanup Lifecycle
    ↓
Destroy GameObject
    ↓
Release Asset
    ↓
Complete
```

## Memory Management

### Asset Lifecycle
```
Load → Initialize → Active → Cleanup → Release
  ↓                              ↓         ↓
Handle Created          BeforeRelease  Handle Released
```

### Pooling Strategy
```
Frequent Screens → ScreenPool (object reuse)
     ↓
Occasional Screens → ScreenCache (LRU with budget)
     ↓
Rare Screens → Load on demand, destroy immediately
```

## Extension Points

### Custom Asset Loader
```csharp
public class MyAssetLoader : IAssetLoader
{
    // Implement Load, LoadAsync, Release
}

AssetLoaderProvider.SetCustomAssetLoader(new MyAssetLoader());
```

### Custom Transition
```csharp
[CreateAssetMenu]
public class MyTransition : TransitionAnimationObject
{
    public override float Duration => 0.5f;
    public override void Setup(RectTransform rt) { }
    public override void SetTime(float time) { }
}
```

### Custom Screen Type
```csharp
public class PopupPage : Page
{
    // Add custom behavior
}
```

## Best Practices

1. **Use Addressables for production** - Better memory control and remote updates
2. **Pool frequently used modals** - Loading spinners, toast messages, confirmations
3. **Cache hot screens** - Main menu, settings, shops
4. **Preload critical paths** - Next screens in common user flows
5. **Validate setup in CI/CD** - Use ContainerValidator tool
6. **Profile transitions** - Use PerformanceMonitor to catch GC spikes
7. **Separate UI logic from business logic** - Use MVP pattern for complex screens

## Performance Considerations

### What's Optimized
- Asset loading is async by default
- Transitions run on separate player (no per-component overhead)
- Container caching (find by name/transform)
- Event dispatcher uses sorted lists
- Pooling reduces allocations

### Watch Out For
- Addressables first-load overhead
- Timeline animator allocations
- Multiple nested containers blocking interaction
- Large prefab instantiation (break into chunks)
- Deep hierarchy in screens (flatten when possible)

## Comparison with UnityScreenNavigator

| Feature | UnityScreenNavigator | Enhanced UI Framework |
|---------|---------------------|----------------------|
| Core Containers | ✅ 3 types | ✅ Same |
| Lifecycle Events | ✅ Full | ✅ Same + Debug tools |
| Transitions | ✅ Full | ✅ Same + More presets |
| Asset Loading | ✅ Pluggable | ✅ Same + Remote |
| Object Pooling | ❌ No | ✅ Built-in |
| Safe Area | ❌ Manual | ✅ Auto |
| Back Button | ❌ Manual | ✅ Auto |
| Orientation | ❌ No | ✅ Built-in |
| MVP Support | ❌ Demo only | ✅ Full framework |
| Editor Tools | ❌ Limited | ✅ Wizard, Generator, Validator |

## Next Steps

- [Lifecycle System](Lifecycle.md) - Deep dive into events
- [Transitions](Transitions.md) - Create custom animations
- [Mobile Optimization](MobileOptimization.md) - Performance tuning
- [MVP Pattern](MVP_Pattern.md) - Structure complex screens
