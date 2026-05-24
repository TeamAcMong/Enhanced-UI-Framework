# Asset Loading

How prefabs become screens, and how to pick the right loader.

## The contract

Every container call (`Push`, `Show`, `Preload`) takes a **resource key** — an opaque string. The framework hands that string to an `IAssetLoader`, which returns a prefab. Containers don't care where the prefab came from.

```csharp
public interface IAssetLoader
{
    AssetLoadHandle<T> Load<T>(string key) where T : UnityEngine.Object;
    AssetLoadHandle<T> LoadAsync<T>(string key) where T : UnityEngine.Object;
    void Release(AssetLoadHandle handle);
}
```

The loader is chosen once per process from `EnhancedUISettings.assetLoaderType`. You can also override it at runtime with `AssetLoaderProvider.SetCustomAssetLoader(IAssetLoader)`.

## Pick one

| Loader | Pick when | Trade-off |
|---|---|---|
| **Resources** | Small game, no remote content, you want zero setup | Everything ships in the build; one big bundle |
| **Addressables** | Larger game, remote delivery, partial loading | Have to register every prefab; first-time learning curve |
| **Custom** | Hand-rolled asset bundles, inspector-assigned prefab lists, hot-reloading dev workflow | You write the loader |

If you don't know yet, start with **Resources** and migrate later — `assetLoaderType` is one enum flip.

---

## Resources

`ResourcesAssetLoader` calls `Resources.LoadAsync<GameObject>(key)` under the hood.

### Setup

1. Open `EnhancedUISettings` → set **Asset Loading → Loader** to **Resources**.
2. Put your prefab anywhere under a folder named `Resources/` — Unity finds them recursively.
3. The **key** is the path *inside* `Resources/`, without the `.prefab` extension.

```
Assets/
  MyGame/
    UI/
      Resources/                    ← any folder named Resources counts
        Pages/
          HomePage.prefab           ← key: "Pages/HomePage"
          BattlePage.prefab         ← key: "Pages/BattlePage"
        Modals/
          SettingsModal.prefab      ← key: "Modals/SettingsModal"
```

```csharp
await pages.Push<HomePage>("Pages/HomePage");
await modals.Push<SettingsModal>("Modals/SettingsModal");
```

### Pros / cons

| Pros | Cons |
|---|---|
| ✅ Zero setup | ❌ All Resources contents are pulled into every build |
| ✅ Works in every Unity version | ❌ Slow load on huge projects |
| ✅ Synchronous fallback available | ❌ Can't update content without rebuilding |

---

## Addressables

`AddressableAssetLoader` calls `Addressables.LoadAssetAsync<GameObject>(key)`. The package conditionally compiles this loader under `EUI_ADDRESSABLES_SUPPORT`, so you must have `com.unity.addressables` 1.17.4+ installed.

### Setup

1. **Window → Package Manager** → install **Addressables**.
2. **Window → Asset Management → Addressables → Groups** → create or pick a group.
3. Drag every screen prefab into the group.
4. For each entry, set the **Address** to a stable string — that's your key.
5. Open `EnhancedUISettings` → set **Asset Loading → Loader** to **Addressables**.

```
group: UIPrefabs
  ┣ HomePage.prefab          → Address: "HomePage"
  ┣ BattlePage.prefab        → Address: "BattlePage"
  ┗ SettingsModal.prefab     → Address: "SettingsModal"
```

```csharp
await pages.Push<HomePage>("HomePage");
await modals.Push<SettingsModal>("SettingsModal");
```

### Common pitfalls

- ❌ **`InvalidKeyException: No Location found for Key=X`** — the prefab isn't Addressable, or its Address doesn't match the string you pushed.
- ❌ **Forgetting to build the catalog** — for Player builds you must run *Build → New Build → Default Build Script*. (Editor and Play Mode work without a built catalog.)
- ❌ **Loader not switching** — `AssetLoaderProvider` caches the loader instance. If you change `assetLoaderType` at runtime, call `AssetLoaderProvider.ResetCache()` afterwards.

### Pros / cons

| Pros | Cons |
|---|---|
| ✅ Build only what you ship | ❌ Per-prefab setup |
| ✅ Remote content updates | ❌ Catalog must be built for player |
| ✅ Memory profiling per group | ❌ Compile-time dependency on Addressables |

---

## Custom

Pick **Custom** when neither of the above fits — e.g. you want prefabs assigned in the Inspector, or a remote CDN with your own caching, or a test-time mock.

### 1. Implement the loader

```csharp
using EnhancedUI.AssetManagement;
using UnityEngine;

public class InspectorAssignedLoader : IAssetLoader
{
    private readonly System.Collections.Generic.Dictionary<string, GameObject> _table;

    public InspectorAssignedLoader(System.Collections.Generic.Dictionary<string, GameObject> table)
    {
        _table = table;
    }

    public AssetLoadHandle<T> Load<T>(string key) where T : Object
    {
        if (!_table.TryGetValue(key, out var prefab))
            return new ImmediateAssetLoadHandle<T>(
                new System.Exception($"No prefab for key '{key}'"));

        return new ImmediateAssetLoadHandle<T>(prefab as T);
    }

    public AssetLoadHandle<T> LoadAsync<T>(string key) where T : Object
        => Load<T>(key); // already in memory, nothing to await

    public void Release(AssetLoadHandle handle) { /* nothing to release */ }
}
```

> The framework ships `ImmediateAssetLoadHandle<T>` (sync) and `AsyncAssetLoadHandle<T>` (in-flight) so you rarely need a custom subclass. Pass an asset on success, or an `Exception` on failure.

### 2. Register it before any Push

```csharp
using EnhancedUI.AssetManagement;

public class Bootstrap : MonoBehaviour
{
    [System.Serializable]
    public struct Entry { public string key; public GameObject prefab; }

    [SerializeField] private Entry[] entries;

    private void Awake()
    {
        var table = new System.Collections.Generic.Dictionary<string, GameObject>();
        foreach (var e in entries) table[e.key] = e.prefab;

        AssetLoaderProvider.SetCustomAssetLoader(new InspectorAssignedLoader(table));
    }
}
```

`SetCustomAssetLoader` is sticky — it overrides whatever loader the settings would have built. Call `AssetLoaderProvider.ClearCustomAssetLoader()` if you want to revert.

### 3. Don't forget `assetLoaderType`

Set **Loader** to **Custom** in `EnhancedUISettings` so the framework knows it shouldn't try to auto-build a Resources or Addressables loader as a fallback during the brief window between scene load and your `Awake` running. (If `_customLoader` is set in time, this isn't strictly required — but it's good hygiene.)

### Use cases

- **Sample / demo scenes** — assign prefabs in the Inspector, ship just-works experience.
- **Asset bundles** — wrap your bundle manifest in an `IAssetLoader`.
- **Remote CDN with caching** — see `RemoteAssetLoader` for an example (delegates to a fallback loader on miss).
- **Unit / integration tests** — `PreloadedAssetLoader` stores GameObjects directly.

---

## Switching loaders mid-game

You almost never want to, but the API exists:

```csharp
AssetLoaderProvider.SetCustomAssetLoader(new MyOtherLoader());
// or revert to whatever the settings asset says:
AssetLoaderProvider.ClearCustomAssetLoader();
AssetLoaderProvider.ResetCache();
```

You can also assign a per-container loader:

```csharp
pageContainer.SetAssetLoader(new MyOtherLoader());
```

Per-container loaders take precedence over the global one. Use sparingly — debugging "why does only this container behave differently" is no fun.

---

## Built-in loaders quick reference

| Class | Header | Notes |
|---|---|---|
| `ResourcesAssetLoader` | always available | sync + async via `Resources.LoadAsync` |
| `AddressableAssetLoader` | `#if EUI_ADDRESSABLES_SUPPORT` | only compiled if `com.unity.addressables` is in `manifest.json` |
| `PreloadedAssetLoader` | always available | accepts a `Dictionary<string, Object>`; useful for tests |
| `RemoteAssetLoader` | always available | downloads from a base URL, caches to disk, falls back to a wrapped loader |

All four implement `IAssetLoader` — combine, decorate, or roll your own.

---

## See also

- [`EnhancedUISettings` reference](Settings.md) — the Asset Loading section
- [`Architecture`](Architecture.md) — where the loader sits in the data flow
