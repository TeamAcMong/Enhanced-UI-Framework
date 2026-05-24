# Getting Started

Five-minute path from "package installed" to "screen on screen".

> Already added the package? Skip to [step 2](#2-create-the-scene).
> Want to skip the wizard? Jump to [Editor Setup (Manual)](EditorSetup.md).

## 1. Install

Add the **OpenUPM scoped registry** to your project's `Packages/manifest.json` so UPM can pull UniTask (an unavoidable transitive dependency — UPM rejects git URLs inside a package's own deps):

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [ "com.cysharp.unitask" ]
    }
  ],
  "dependencies": {
    "com.dreamtechex.enhanced-ui-framework": "https://github.com/TeamAcMong/Enhanced-UI-Framework.git#1.1.0"
  }
}
```

Save the file — Unity will resolve both packages on next refresh. UniTask comes from OpenUPM, Enhanced UI Framework from GitHub. If you'd rather use the UI: **Edit → Project Settings → Package Manager → Scoped Registries → ＋** with name `OpenUPM`, URL `https://package.openupm.com`, scope `com.cysharp.unitask`. Then **Window → Package Manager → ＋ → Add package from git URL** with the framework's URL.

> If UniTask is missing for any reason, the asmdef's `versionDefines` quietly falls back to `IEnumerator` coroutines — your code keeps compiling. The code samples in these docs assume UniTask is present.

Addressables and TextMeshPro are optional — install them only when you need them.

## 2. Create the scene

Run **Tools → Enhanced UI → Setup Wizard**. It does five things:

1. Creates `Assets/Resources/EnhancedUISettings.asset` (the global config).
2. Creates a `Canvas` set up for Screen-Space Overlay with a `CanvasScaler` tuned for portrait mobile.
3. Adds a `SafeArea` RectTransform under it with `SafeAreaAdapter`.
4. Drops `PageContainer`, `ModalContainer`, and `SheetContainer` under `SafeArea`.
5. Adds `BackButtonHandler` and `EventSystem`.

Each container has a `Name` field — the wizard sets all three to `"Main"` so `Find("Main")` resolves later.

> If you want to wire it by hand, follow [EditorSetup.md](EditorSetup.md) — it covers the same hierarchy step by step.

## 3. Pick an asset loader

This is the single most important decision and the one that trips people up.

Open `EnhancedUISettings.asset` (or **Tools → Enhanced UI → Settings**), look at the **Asset Loading** section, and choose:

| | When to pick | Where your prefabs go | Key you push with |
|---|---|---|---|
| **Resources** | smallest games, fastest start | `Resources/<key>.prefab` | `"<relative-path-without-extension>"` |
| **Addressables** | larger games, remote delivery | anywhere, mark **Addressable**, set Address | the entry's **Address** |
| **Custom** | hand-rolled bundle / inspector refs | anywhere your loader can reach | whatever your loader expects |

Full deep dive: [AssetLoading.md](AssetLoading.md).

## 4. Write your first page

```csharp
using EnhancedUI;
using UnityEngine;
using UnityEngine.UI;

#if EUI_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#endif

public class HomePage : Page
{
    [SerializeField] private Button playButton;

#if EUI_UNITASK_SUPPORT
    public override async UniTask Initialize()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        await base.Initialize();
    }

    public override async UniTask Cleanup()
    {
        playButton.onClick.RemoveAllListeners();
        await base.Cleanup();
    }
#else
    public override System.Collections.IEnumerator Initialize()
    {
        playButton.onClick.AddListener(OnPlayClicked);
        yield return base.Initialize();
    }

    public override System.Collections.IEnumerator Cleanup()
    {
        playButton.onClick.RemoveAllListeners();
        yield return base.Cleanup();
    }
#endif

    public override void DidPushEnter() { /* page is fully on-screen */ }
    public override void WillPopExit()  { /* save state, stop tweens */ }

    private void OnPlayClicked() { /* push the next page */ }
}
```

### Create the prefab

1. In the scene, **Right-click → UI → Panel**, rename to `HomePage`.
2. Add Component → `HomePage` and (`CanvasGroup` if not auto-added).
3. RectTransform: stretch to fill (anchors `(0,0)-(1,1)`, offsets `0,0,0,0`).
4. Drag the play button into the `playButton` field.
5. Save as `Assets/Resources/Pages/HomePage.prefab` (for Resources loader) or anywhere you'd like and mark it Addressable with address `HomePage` (for Addressables loader).
6. **Delete the scene copy** — the container will instantiate it for you.

## 5. Push it

Anywhere in your scene (e.g. on a `Bootstrap` MonoBehaviour's `Start`):

```csharp
using EnhancedUI;
using UnityEngine;
#if EUI_UNITASK_SUPPORT
using Cysharp.Threading.Tasks;
#endif

public class Bootstrap : MonoBehaviour
{
#if EUI_UNITASK_SUPPORT
    private async UniTaskVoid Start()
    {
        var pages = PageContainer.Find("Main");
        await pages.Push<HomePage>("Pages/HomePage"); // key matches Resources path or Addressable address
    }
#else
    private System.Collections.IEnumerator Start()
    {
        var pages = PageContainer.Find("Main");
        yield return pages.Push<HomePage>("Pages/HomePage");
    }
#endif
}
```

Press Play. You should see `HomePage` slide / fade in (depending on the default transition).

## Core concepts in 60 seconds

### Three containers

| Container | Behavior | Use for |
|---|---|---|
| `PageContainer` | LIFO stack with history (`Push` / `Pop`) | full-screen pages, wizards |
| `ModalContainer` | overlay stack with backdrop | dialogs, popups, confirmations |
| `SheetContainer` | tab swap with no history (`Register` once, `Show`/`Hide`) | bottom-tab nav, tab panels |

### The lifecycle (`Page` shown — `Modal` is identical)

```
[Loaded]   ─▶ Initialize           // async, do data binding here
WillPushEnter      ──┐               (next page being shown)
WillPushExit       ──┘ on the page being pushed off-screen
DidPushEnter       ──┐ next page is now visible
DidPushExit        ──┘ pushed-off page is hidden

… time passes; user hits Back …

WillPopEnter       ──┐ previous page being brought back
WillPopExit        ──┘ top page being torn down
DidPopEnter        ──┐ previous page visible again
DidPopExit         ──┘ top page hidden
[Cleanup]          ─▶ Cleanup        // async, release resources
[Destroyed]
```

Override whichever you need; leave the rest. Sheets have a simpler `WillShow/DidShow/WillHide/DidHide` cycle.

### `WindowOptions`

```csharp
new WindowOptions
{
    LoadAsync      = true,           // false = block until prefab is in memory
    PlayAnimation  = true,           // false = snap, no transition
    Stack          = true,           // PageContainer only — false skips history
    OnLoaded       = screen => { },  // fires after Instantiate, before Initialize
    Data           = anyObject,      // arbitrary blob; read it in OnLoaded
}
```

Convenience presets: `WindowOptions.Default`, `.WithoutAnimation`, `.WithoutStack`, `.Immediate`.

## Common patterns

### Pass data to a screen

```csharp
await pages.Push<ShopPage>("Pages/ShopPage", new WindowOptions
{
    Data = new ShopArgs { CategoryId = 5 },
    OnLoaded = screen => ((ShopPage)screen).SetCategory(5),
});
```

### Preload before the user gets there

```csharp
var handle = pages.Preload("Pages/BattlePage"); // off the hot path (loading screen, idle)
// later, push is instant:
await pages.Push<BattlePage>("Pages/BattlePage");
// when you no longer expect to return:
pages.ReleasePreloaded("Pages/BattlePage");
```

### Show a modal

```csharp
var modals = ModalContainer.Find("Main");
await modals.Push<SettingsModal>("Modals/SettingsModal");
// close it:
await modals.Pop();
```

### Pop multiple pages or back to a specific page

```csharp
await pages.Pop(playAnimation: true, popCount: 3);
await pages.Pop(playAnimation: true, destinationPageId: "HomePage");
```

### Tabs with sheets

```csharp
var sheets = SheetContainer.Find("Main");

// register all four tabs up-front (returns sheet IDs)
var home      = await sheets.Register<HomeSheet>("Sheets/HomeSheet");
var battle    = await sheets.Register<BattleSheet>("Sheets/BattleSheet");
var inventory = await sheets.Register<InventorySheet>("Sheets/InventorySheet");
var shop      = await sheets.Register<ShopSheet>("Sheets/ShopSheet");

// switch tab
await sheets.Show(home.Result.Identifier);
```

### Handle the Android back button

```csharp
public class HomePage : Page, IBackButtonReceiver
{
    public bool OnBackButtonPressed()
    {
        // return true if you handled it (will not pop), false to let the container Pop
        return false;
    }
}
```

## Troubleshooting

| Symptom | Likely cause |
|---|---|
| `No Location found for Key=X` | `assetLoaderType = Addressables` but the prefab isn't registered as Addressable with address `X`. See [AssetLoading.md](AssetLoading.md). |
| `Container not found` | `PageContainer.Find("Main")` but no container in scene has `Name = "Main"`. Setup Wizard sets all three to `"Main"`. |
| Black screen / pages don't show | Container `RectTransform` doesn't fill its parent, or `CanvasGroup.alpha = 0` on the screen prefab. |
| Buttons unresponsive | Container `Interactable` is false (the framework toggles this during transitions — check `IsInTransition`). |
| Text shows as boxes | TextMeshPro essentials not imported: **Window → TextMeshPro → Import TMP Essential Resources**. |
| `Push` returns immediately, screen never appears | The framework returns an `AsyncProcessHandle`; you forgot to `await` it (or `yield return`). |

## Next steps

- [Architecture](Architecture.md) — how the pieces fit
- [Asset Loading](AssetLoading.md) — pick a loader and live with it
- [Settings Reference](Settings.md) — every inspector field
- [MVP Pattern](MVP_Pattern.md) — when a screen grows past ~200 lines
- [Editor Setup (Manual)](EditorSetup.md) — wire it by hand without the wizard
