# Editor Setup (Manual)

Setting up containers without **Tools → Enhanced UI → Setup Wizard**.

If you're starting a new scene from scratch, the wizard does everything below in one click. Use this guide when:

- You're integrating the framework into an existing scene that already has its own Canvas.
- You want to understand exactly what the wizard does.
- The wizard makes choices (anchors, scaler) you want to change.

## 1. Settings asset

Create `EnhancedUISettings`:

- **Assets → Create → Enhanced UI → Settings**, or
- **Tools → Enhanced UI → Settings** (creates one on demand).

Save to **`Assets/Resources/EnhancedUISettings.asset`** — the `Resources/` location is required; the framework loads it via `Resources.Load<EnhancedUISettings>("EnhancedUISettings")`.

Field-by-field reference: [Settings.md](Settings.md).

## 2. Canvas

Right-click in Hierarchy → **UI → Canvas**. Recommended config for portrait mobile:

| Component | Setting | Value |
|---|---|---|
| `Canvas` | Render Mode | `Screen Space - Overlay` |
| `Canvas` | Pixel Perfect | ✓ |
| `CanvasScaler` | UI Scale Mode | `Scale With Screen Size` |
| `CanvasScaler` | Reference Resolution | `1080 × 1920` (portrait) or `1920 × 1080` (landscape) |
| `CanvasScaler` | Screen Match Mode | `Match Width Or Height` |
| `CanvasScaler` | Match | `0.5` |
| `GraphicRaycaster` | (defaults) | |

> If you're integrating into an existing canvas, you can skip this step — the framework doesn't care which Canvas it lives on.

## 3. Safe area

Under the Canvas, create an empty GameObject called `SafeArea`:

- RectTransform: stretch all (anchors `(0,0)-(1,1)`, offsets `0,0,0,0`).
- Add Component → `SafeAreaAdapter`.

Everything that should respect the notch / home indicator should be a *child* of this GameObject.

## 4. Containers

Under `SafeArea`, create three empty GameObjects. For each one:

1. Add Component matching its name (`PageContainer`, `ModalContainer`, or `SheetContainer`).
2. Add Component → `CanvasGroup` (auto-added when you add the container, but verify).
3. RectTransform: stretch all.
4. In the container component, set **Container Name** to `"Main"` (or whatever you'll pass to `PageContainer.Find(...)`).

Recommended hierarchy:

```
Canvas
└── SafeArea (SafeAreaAdapter)
    ├── PageContainer  (Name = "Main")
    │   └── … pages get instantiated here at runtime
    ├── ModalContainer (Name = "Main")
    │   └── ModalBackdrop (Image, optional — see below)
    └── SheetContainer (Name = "Main")
        └── … sheets get instantiated here at runtime
```

> Pages and sheets fill the screen, so put `PageContainer` and `SheetContainer` at sibling positions. `ModalContainer` sits on top (later sibling = drawn on top in UGUI).

> Shortcut: **GameObject → Enhanced UI → Page Container / Modal Container / Sheet Container** creates a pre-configured container under the current selection.

### Modal backdrop (optional)

If you want a dimming backdrop behind modals, add an `Image` child of `ModalContainer`:

```
ModalContainer
└── ModalBackdrop
    - RectTransform: stretch all
    - Image: Color (0,0,0,0.5)
    - Image: Raycast Target = true
    - Button (optional): OnClick → ModalContainer.Pop()
```

`ModalContainer` automatically toggles the backdrop on push/pop based on the configured **backdrop strategy** (set per-modal, not here).

## 5. Back button (mobile)

Anywhere in the scene, create a GameObject called `BackButtonHandler` and add the `BackButtonHandler` component.

Pages and modals that want to intercept back can implement `IBackButtonReceiver`:

```csharp
public class HomePage : Page, IBackButtonReceiver
{
    public bool OnBackButtonPressed()
    {
        // Return true if you handled it (suppress default Pop), false to let the container handle it.
        return false;
    }
}
```

`BackButtonHandler` listens to the Android back button and the Escape key. Turn it off project-wide via `EnhancedUISettings.enableBackButton = false`.

## 6. EventSystem

If your scene doesn't have one, **GameObject → UI → Event System**. The framework doesn't strictly require it, but no UI input works without one.

## 7. Validate

Open **Tools → Enhanced UI → Validate Setup**. The validator lints for:

- Missing or duplicate container names
- Containers without `CanvasGroup`
- Containers outside a Canvas
- Wrong RectTransform anchors

Fix any reds before you start instantiating screens.

---

## Wiring screens

Once the containers exist, see [GettingStarted.md § 4 and § 5](GettingStarted.md#4-write-your-first-page) for writing your first page and pushing it.

For a fully wired sample (bottom-tab nav + modal settings + MVP), import **Mobile Game — Complete** from Package Manager → Samples and follow [its README](../Samples~/MobileGameComplete/README.md).

## Troubleshooting

| Symptom | Fix |
|---|---|
| `Container not found: Main` | The `Name` field on the container component doesn't match the string you passed to `Find`. They are case-sensitive. |
| Containers don't fill the screen | RectTransform anchors aren't stretch — set to `(0,0)-(1,1)` with all offsets `0`. |
| Modals appear *behind* pages | Move `ModalContainer` after `PageContainer` in the hierarchy (later = on top in UGUI). |
| Pages instantiate but you can't tap them | `CanvasGroup.interactable` is off — likely a stuck transition. Check `IsInTransition`. |
| Safe area doesn't apply on device | `SafeAreaAdapter` not on a parent of your content, or `enableSafeArea` is false in settings. |
