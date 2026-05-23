# Mobile Game — Complete Sample

End-to-end example of Enhanced UI Framework: a lobby with bottom-tab navigation, modal settings, and full MVP wiring.

## What's inside

- `Scenes/DemoScene.unity` — entry scene
- `Prefabs/` — HomeContainer + 4 tab sheets (Home, Battle, Inventory, Shop)
- `Animations/` — tab slide transition assets + Timeline tracks
- `Resources/EnhancedUISettings.asset` — pre-configured framework settings
- `Scripts/App/` — `DemoBootstrap`, `NavigationManager`, `ScreenKeys`
- `Scripts/Screens/` — Page / Sheet / Modal screens with MVP (View + Presenter + Model)
- `Scripts/Components/` — reusable HUD widgets (top bar, currency, level button…)
- `Scripts/Animations/` — `TabSheetSlideAnimation` showing how to write custom transitions

## How to run

1. Import this sample via **Window → Package Manager → Enhanced UI Framework → Samples → Import**.
2. Open `Assets/Samples/Enhanced UI Framework/<version>/MobileGameComplete/Scenes/DemoScene.unity`.
3. Press Play. `DemoBootstrap` will push `HomeContainerPage` automatically.

## Key patterns demonstrated

| Pattern | Where |
|---|---|
| Page stack navigation | `NavigationManager.GoToLevelSelection / GoToGameplay` |
| Modal with backdrop | `NavigationManager.ShowSettings` |
| Sheet tab switching | `HomeContainerPage.SwitchToTab` |
| Tab-aware slide animation | `Scripts/Animations/TabSheetSlideAnimation.cs` (implements `ITabContent`) |
| MVP wiring | `Screens/Home/HomeScreen.cs` + `HomeScreenView` + `HomeScreenPresenter` + `HomeScreenModel` |
| Async lifecycle (`UniTask`) | every `Initialize()` / `Cleanup()` override |
