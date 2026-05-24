# Mobile Game — Complete Sample

End-to-end example of Enhanced UI Framework: a lobby with bottom-tab navigation, modal settings, and full MVP wiring.

## What's inside

| Folder | Contents |
|---|---|
| `Scenes/DemoScene.unity` | Entry scene wired with PageContainer / ModalContainer / SheetContainer + DemoBootstrap |
| `Prefabs/` | `HomeContainer` (page) + 4 tab sheets (`HomeSheet`, `BattleSheet`, `InventorySheet`, `ShopSheet`) |
| `Animations/` | Tab slide transition assets + Timeline tracks |
| `Resources/EnhancedUISettings.asset` | Pre-configured framework settings |
| `Scripts/App/` | `DemoBootstrap`, `NavigationManager`, `ScreenKeys` |
| `Scripts/Screens/` | Page / Sheet / Modal screens with MVP (View + Presenter + Model) |
| `Scripts/Components/` | Reusable HUD widgets (top bar, currency, level button…) |
| `Scripts/Animations/` | `TabSheetSlideAnimation` showing how to write custom transitions |

## How to run

1. **Window → Package Manager → Enhanced UI Framework → Samples → Import** (Mobile Game — Complete).
2. **Configure the asset loader** for the sample (see next section — required, the sample doesn't auto-register anything).
3. Open `Assets/Samples/Enhanced UI Framework/<version>/MobileGameComplete/Scenes/DemoScene.unity`.
4. Press Play. `DemoBootstrap` will push `HomeContainerPage`.

## ⚠️ Asset loader setup (required)

The sample pushes screens with bare-name keys: `"HomeContainer"`, `"HomeSheet"`, `"BattleSheet"`, `"InventorySheet"`, `"ShopSheet"`. The shipped `EnhancedUISettings.asset` sets **`assetLoaderType = Addressables`**, so the loader expects each prefab to be registered as an Addressable entry with one of those keys as its **Address**.

You have three options after importing — pick one:

### Option A — Mark sample prefabs as Addressable (matches shipped settings)

1. Open **Window → Asset Management → Addressables → Groups**.
2. Drag the five prefabs from `Samples/Enhanced UI Framework/<version>/MobileGameComplete/Prefabs/` into a group (create `UIPrefabs` if it doesn't exist).
3. For each entry, click the address and **rename it to the bare name** (`HomeContainer`, `HomeSheet`, etc.) — the default address is the full asset path, which won't match.
4. Press Play.

### Option B — Switch the sample to the Resources loader

1. Open the imported `Samples/Enhanced UI Framework/<version>/MobileGameComplete/Resources/EnhancedUISettings.asset`.
2. In **Asset Loading**, change **Loader** to **Resources**.
3. Move the five prefabs from `…/MobileGameComplete/Prefabs/` into `…/MobileGameComplete/Resources/` (flat, alongside `EnhancedUISettings.asset`).
4. Press Play. *(Keys stay the same — `Resources.Load("HomeContainer")` will find a prefab at any path that ends in `Resources/HomeContainer.prefab`.)*

### Option C — Use your own custom loader

If your project already has a custom `IAssetLoader` (asset bundles, remote CDN, inspector-assigned table), set the sample's `assetLoaderType` to **Custom** and call `AssetLoaderProvider.SetCustomAssetLoader(...)` from your own bootstrap **before** `DemoBootstrap.Start` runs. See [AssetLoading.md → Custom](../../Documentation~/AssetLoading.md#custom).

> If you see `UnityEngine.AddressableAssets.InvalidKeyException: No Location found for Key=HomeContainer`, you skipped this section — you're on Addressables but never registered the entry.

## Key patterns demonstrated

| Pattern | Where |
|---|---|
| Page stack navigation | `NavigationManager.GoToLevelSelection / GoToGameplay` |
| Modal with backdrop | `NavigationManager.ShowSettings` |
| Sheet tab switching | `HomeContainerPage.SwitchToTab` |
| Tab-aware slide animation | `Scripts/Animations/TabSheetSlideAnimation.cs` (implements `ITabContent`) |
| MVP wiring | `Screens/Home/HomeScreen.cs` + `HomeScreenView` + `HomeScreenPresenter` + `HomeScreenModel` |
| Async lifecycle (`UniTask`) | every `Initialize()` / `Cleanup()` override |

## Tab navigation: `SheetSwipePager`

The bundled `SheetSwipePager` (`Scripts/Input/SheetSwipePager.cs`) implements drag-to-follow + snap-to-page navigation for the bottom-tab sheets — same feel as iOS `UIPageViewController` / Android `ViewPager2`.

| What it does | Notes |
|---|---|
| Pointer follows finger 1:1 during a drag (configurable ratio) | Both the current and adjacent sheet are repositioned in real time |
| Releases commit via **dual-trigger**: position ≥ 25 % width OR flick velocity ≥ 800 px/s | Both thresholds are Inspector-exposed |
| Rubber-band resistance at the first/last tab | Set `edgeResistance` to `0` for a hard stop |
| Vertical drags exit the gesture so a nested vertical `ScrollRect` can take over | Axis is locked after `axisLockThreshold` (default 10 px) of finger travel |
| `JumpToTab(id)` mirrors swipe visuals for programmatic navigation | `BottomTabBar` calls this when a tab button is clicked, so click and swipe look identical |
| Snap tween uses the framework's built-in `EaseUtility` / `EaseType` | No DOTween dependency |

Setup notes:

- The pager must live on a GameObject with a raycastable `Graphic`. The component auto-adds a fully transparent `Image` if none is present.
- Place it on the `SheetContainer` GameObject so its rect matches the swipe area exactly (the auto-setup tool does this for you).
- If a sheet contains a `ScrollRect`, that `ScrollRect` takes drag priority — the pager only sees gestures that start in empty areas of the sheet. A custom `ScrollRect` that re-delegates the horizontal axis is on the P2 backlog.

> **Heads-up after this update**: the legacy `SwipeDetector` has been removed and the bundled `Prefabs/HomeContainer.prefab` still references it (Missing Script warning). Re-run `Tools/Enhanced UI Demo/Create Full Demo Scene (One-Click)` once after importing to regenerate the prefab with `SheetSwipePager` wired correctly.

## Editor utilities included

| Menu | What it does |
|---|---|
| `Tools/Enhanced UI Demo/Create Full Demo Scene (One-Click)` | Builds a fresh DemoScene from scratch with full UI hierarchy — useful after a botched edit |
| `Tools/Enhanced UI Demo/Prefab Quick Setup` | Scaffolds a screen prefab matching the sample's conventions |

## Editing the sample (framework authors only)

If you cloned this repo and want to edit the *canonical* sample source (under `Packages/com.dream-tech-ex.enhanced-ui-framework/Samples~/MobileGameComplete/`), Unity ignores `Samples~/` by default. Create a directory junction so Unity can see it:

```powershell
cmd /c mklink /J "Assets\_SampleDev" "Packages\com.dream-tech-ex.enhanced-ui-framework\Samples~\MobileGameComplete"
```

Then open `Assets/_SampleDev/Scenes/DemoScene.unity`. Changes write directly to the canonical `Samples~/` location. Add `Assets/_SampleDev` and `Assets/_SampleDev.meta` to `.gitignore`; delete the junction before tagging a release (`./deploy.sh --semver "x.y.z"`).
