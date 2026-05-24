# `EnhancedUISettings` — Field Reference

The global `ScriptableObject` for framework configuration.

## Where to find it

- **Tools → Enhanced UI → Settings** (or the button in any container's inspector)
- File path: `Assets/Resources/EnhancedUISettings.asset` *(must live in a `Resources/` folder so the framework can `Resources.Load<EnhancedUISettings>("EnhancedUISettings")` at runtime)*
- API: `EnhancedUISettings.Instance` (auto-creates a default in-memory instance if no asset is found)

If the asset doesn't exist, the framework logs a warning and falls back to a default in-memory copy — every field below uses the default value listed in the table.

The custom inspector groups the fields into eight foldout sections. Below is what every field does, what the default is, and what to watch out for.

---

## 🚀 Asset Loading

| Field | Default | Effect |
|---|---|---|
| `assetLoaderType` | `Addressables` | Which `IAssetLoader` the framework builds on first use. `Resources`, `Addressables`, `Custom`. Full details: [AssetLoading.md](AssetLoading.md). |
| `enablePreloading` | `true` | If `false`, `Container.Preload(key)` is a no-op. Disable when your project has its own preload manager. |

> The Inspector shows the loader as a 3-button toggle with contextual hints — e.g. it warns if you pick **Addressables** but `com.unity.addressables` isn't installed, or if you pick **Custom** without calling `AssetLoaderProvider.SetCustomAssetLoader(...)`.

---

## ♻️ Lifecycle

| Field | Default | Effect |
|---|---|---|
| `enableAsyncLifecycle` | `true` | Routes `Initialize` / `Cleanup` through `UniTask` (when `com.cysharp.unitask` is installed). Turn off for coroutine-only projects. |
| `callCleanupWhenDestroy` | `true` | Automatically invokes `Screen.Cleanup()` in `OnDestroy`. Disable only if you have a custom teardown that calls it manually. |

> Turning `callCleanupWhenDestroy` off without manually invoking `Cleanup` will leak event subscriptions — the inspector raises a warning when it's disabled.

---

## 🧠 Memory & Pooling

### Object Pooling

| Field | Default | Effect |
|---|---|---|
| `enableObjectPooling` | `true` | Master switch. When off, no pooling happens regardless of the entries below. |
| `poolConfigurations` | `[]` | List of `{ resourceKey, poolSize }`. Each entry tells the framework to keep up to `poolSize` instances of `resourceKey` alive instead of destroying them. Good candidates: modals, tooltips, list items. |

The framework respects pools per resource key — screens not in this list are loaded/destroyed normally. There is no global "pool everything" toggle by design.

### Caching

| Field | Default | Effect |
|---|---|---|
| `enableSmartCaching` | `true` | Keeps loaded `AssetLoadHandle`s in an LRU cache. When off, every push re-loads the prefab. |
| `memoryBudgetMB` | `100` | Soft cap (MB) on the LRU cache. `0` = unlimited. The inspector shows a colored progress bar: red <20 MB, yellow <50 MB, green ≥50 MB. |

> Budget below ~10 MB will likely evict cached screens between consecutive Push calls — the inspector warns.

---

## 👆 Interaction

| Field | Default | Effect |
|---|---|---|
| `enableInteractionInTransition` | `false` | If `true`, users can tap UI while a transition is animating. Mostly a footgun — leave it off unless you have a specific need. |
| `controlInteractionOfAllContainers` | `true` | When ON, **any** container's transition blocks input on **every** container (global lock). When OFF, each container blocks input only for itself. |

---

## 📱 Mobile Features

| Field | Default | Effect |
|---|---|---|
| `enableSafeArea` | `true` | Activates the `SafeAreaAdapter` component. With it off, the adapter does nothing even if it's in the scene. |
| `enableBackButton` | `true` | Hooks `Application.quitting` and the Android back button. Pages can implement `IBackButtonReceiver` to intercept. |
| `enableOrientationManagement` | `false` | Starts the `OrientationManager`. Only enable if you need portrait/landscape swap support. |

---

## ⚡ Performance

| Field | Default | Effect |
|---|---|---|
| `targetFrameRateDuringTransition` | `0` | If non-zero, temporarily sets `Application.targetFrameRate` while a transition runs. `0` = leave it alone. Useful when you have a battery-saving 30 FPS cap and want 60 FPS only during animations. |
| `optimizeTransitionPerformance` | `true` | Avoids per-frame allocations during transitions. Recommended on mobile; safe to leave on everywhere. |
| `enableAutoGC` | `false` | Calls `GC.Collect()` after heavy transitions. **Only enable on platforms without incremental GC** — forced GC stalls the main thread. |

> The inspector raises warnings for `targetFrameRateDuringTransition < 30` (visible stutter) and for `enableAutoGC = true`.

---

## 🎬 Default Transitions

Three sub-structures (`pageTransitions`, `modalTransitions`, `sheetTransitions`), each with four floats:

| Field | Default | Effect |
|---|---|---|
| `pushEnterDuration` | `0.3` | seconds the *entering* screen animates during a Push |
| `pushExitDuration` | `0.3` | seconds the *exiting* screen animates during a Push |
| `popEnterDuration` | `0.3` | seconds the *entering* screen animates during a Pop |
| `popExitDuration` | `0.3` | seconds the *exiting* screen animates during a Pop |

The inspector renders these as a **Page / Modal / Sheet** tab strip with four sliders and four preset buttons (`Instant`, `Fast 0.15s`, `Normal 0.3s`, `Slow 0.5s`) that fill all four durations at once.

These are *defaults* — individual screen prefabs can override them via per-screen transition components.

---

## 🐞 Debug & Logging

| Field | Default | Effect |
|---|---|---|
| `enableDebugLog` | `false` | Master log switch. The two toggles below are greyed out until this is on. |
| `logLifecycleEvents` | `false` | Verbose log on every `Initialize` / `WillPushEnter` / `DidPopExit` etc. Useful when wiring up lifecycle, very noisy in steady state. |
| `logTransitionEvents` | `false` | Verbose log on every transition Start / Complete. |
| `showPerformanceWarnings` | `true` | Yellow-box warnings in the Console when transitions exceed frame budget. Independent of `enableDebugLog`. |

---

## Quick Actions

Buttons at the bottom of the inspector:

| Button | What it does |
|---|---|
| **Reset to Defaults** | Restores every field to the values listed here (with an "are you sure?" dialog). |
| **Open Control Center** | `Tools → Enhanced UI → Control Center` |
| **Run Health Check** | `Tools → Enhanced UI → Analysis Tools → Health Check` |
| **Documentation** | Opens this repo on GitHub |

---

## See also

- [`AssetLoading.md`](AssetLoading.md) — pick a loader and live with it
- [`GettingStarted.md`](GettingStarted.md) — five-minute setup
- [`Architecture.md`](Architecture.md) — how the settings flow through the system
