# Enhanced UI Framework

Mobile-optimized UI navigation framework for Unity. Container–Screen architecture with **Page** (stack), **Modal** (overlay) and **Sheet** (tabs), rich lifecycle hooks, partner-aware transitions, pluggable asset loaders, object pooling, safe-area + back-button + orientation handling, and an optional MVP layer. Powered by UniTask with a coroutine fallback.

> **Package source:** [`Packages/com.dream-tech-ex.enhanced-ui-framework/`](Packages/com.dream-tech-ex.enhanced-ui-framework/) — this Unity project is also the package's authoring workspace and ships its sample under `Samples~/MobileGameComplete`.

## Install

Add to `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.dreamtechex.enhanced-ui-framework": "https://github.com/TeamAcMong/Enhanced-UI-Framework.git#1.1.0"
  }
}
```

Or in Unity: **Window → Package Manager → ＋ → Add package from git URL**:

```
https://github.com/TeamAcMong/Enhanced-UI-Framework.git#1.1.0
```

Tags publish only the package subtree (~KBs), not the whole Unity project. See [DEPLOY_UPM_SUBTREE.md](DEPLOY_UPM_SUBTREE.md) for the release flow.

## Quick start

```csharp
using EnhancedUI;

public class HomePage : Page
{
    public override async UniTask Initialize()
    {
        // load view model, subscribe to events…
        await base.Initialize();
    }

    public override void DidPushEnter() => /* page is on-screen */ ;
}

// elsewhere
var pages = PageContainer.Find("Main");
await pages.Push<HomePage>("Prefabs/HomePage");
```

Full guide: [`Packages/com.dream-tech-ex.enhanced-ui-framework/README.md`](Packages/com.dream-tech-ex.enhanced-ui-framework/README.md).

## Sample

Import **Mobile Game — Complete** from **Package Manager → Enhanced UI Framework → Samples**. End-to-end lobby with bottom-tab sheets, modal settings, gameplay screens, and full MVP wiring.

## Requirements

- Unity **2022.3** or newer (validated on Unity 6)
- UniTask (auto-installed as a dependency)
- Optional: Addressables 1.17+, TextMeshPro (for the sample)

## License

MIT — see [Packages/com.dream-tech-ex.enhanced-ui-framework/LICENSE.md](Packages/com.dream-tech-ex.enhanced-ui-framework/LICENSE.md).
