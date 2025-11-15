# Enhanced UI Framework - Mobile Game Demo

A comprehensive mobile game UI demo showcasing the Enhanced UI Framework's capabilities with multiple screen types, MVP pattern, and mobile-optimized components.

## 📁 Project Structure

```
Assets/Demo/
├── Common/                      # Shared components and utilities
│   ├── Scripts/
│   │   ├── Components/         # Reusable UI components
│   │   │   ├── CurrencyDisplay.cs
│   │   │   ├── TopBar.cs
│   │   │   ├── BottomNavigation.cs
│   │   │   ├── SideMenuButton.cs
│   │   │   ├── LevelButton.cs
│   │   │   └── LoadingIndicator.cs
│   │   ├── Models/             # Data models
│   │   │   ├── CurrencyType.cs
│   │   │   ├── PlayerData.cs
│   │   │   └── GameState.cs
│   │   └── Utils/              # Utility classes
│   │       ├── SafeAreaAdapter.cs
│   │       └── OrientationManager.cs
│   ├── Prefabs/                # Common prefabs (to be created)
│   └── Resources/              # Resources (sprites, etc.)
│       └── Sprites/
├── Screens/                     # Individual screen implementations
│   ├── Home/                   # Home screen (main hub)
│   ├── LevelSelection/         # Level selection screen
│   ├── Gameplay/               # Gameplay screen
│   ├── Battle/                 # Battle/Arena screen
│   ├── Shop/                   # Shop screen
│   ├── Inventory/              # Inventory screen
│   ├── Settings/               # Settings modal
│   └── RPGStage/               # RPG-style landscape screen
└── Scenes/                      # Demo scenes
```

## ✅ Phase 1 Complete: Core Components

### Data Models
- **CurrencyType**: Enum defining currency types (Energy, Gems, Gold, Keys, Stars)
- **PlayerData**: Player information, currencies, progress, inventory
- **GameState**: Singleton state manager with events for currency/progress changes

### Reusable Components
- **CurrencyDisplay**: Displays currency icon + value with auto-update and animation
- **TopBar**: Player info + multiple currency displays + settings/notifications
- **BottomNavigation**: Tab bar navigation with selection states and notifications
- **SideMenuButton**: Animated side menu buttons with notification badges
- **LevelButton**: Level selection buttons with lock/unlock and star ratings
- **LoadingIndicator**: Animated loading spinner with fade transitions

### Utilities
- **SafeAreaAdapter**: Handles device safe areas (notches, rounded corners)
- **OrientationManager**: Manages screen orientation with change notifications

## 🎯 Key Features

### Auto-Updating Currency System
```csharp
// Currency automatically updates across all displays
GameState.Instance.AddCurrency(CurrencyType.Gold, 100);
GameState.Instance.SpendCurrency(CurrencyType.Gems, 50);
```

### Event-Driven Architecture
```csharp
GameState.Instance.OnCurrencyChanged += (type, oldValue, newValue) => {
    Debug.Log($"{type} changed: {oldValue} -> {newValue}");
};
```

### Mobile Optimization
- Safe area adaptation for notches and rounded corners
- Orientation change handling (portrait ↔ landscape)
- Touch-optimized button sizes and animations
- Performance-optimized with object pooling support

## 🚀 Next Steps

### Phase 2: Screen Implementation (MVP Pattern)
Each screen follows the MVP (Model-View-Presenter) pattern:

1. **HomeScreen** (Portrait)
   - Main CTA button ("CHƠI" / Play)
   - Side menu buttons (Mail, Events, Shop, etc.)
   - Top bar with player info
   - Bottom navigation
   - Daily rewards, notifications

2. **LevelSelectionScreen** (Portrait)
   - Chapter selection
   - Level grid with lock/unlock states
   - Star ratings display
   - Progress tracking

3. **GameplayScreen** (Portrait)
   - 3D viewport for gameplay
   - Resource displays
   - Upgrade slots
   - Pause menu

4. **BattleArenaScreen** (Portrait)
   - Battle type selection
   - Cost display
   - Play button with requirements
   - Leaderboard preview

5. **ShopScreen**
   - Currency packages
   - Special offers
   - Daily deals
   - Purchase flow

6. **InventoryScreen**
   - Grid layout for items
   - Item details
   - Equip/Use actions
   - Sorting/filtering

7. **SettingsModal**
   - Audio settings
   - Notifications
   - Account management
   - About/Credits

8. **RPGStageScreen** (Landscape)
   - Character roster
   - Boss display
   - Tab navigation
   - Horizontal layout

### Phase 3: Navigation Flow
- Define screen transition animations
- Implement navigation paths between screens
- Add back button handling
- Create deep linking support

### Phase 4: Polish & Optimization
- Add particle effects
- Sound integration
- Haptic feedback
- Performance profiling
- Memory optimization

## 💡 Usage Examples

### Creating a Currency Display
```csharp
// Automatically updates when currency changes
currencyDisplay.SetCurrencyType(CurrencyType.Gold);
currencyDisplay.UpdateDisplay();
```

### Setting Up Top Bar
```csharp
topBar.UpdatePlayerInfo(); // Pulls from GameState
topBar.SetNotificationCount(5); // Show badge with count
topBar.SetVisibleCurrencies(true, true, false); // Energy, Gems, no Gold
```

### Configuring Bottom Navigation
```csharp
bottomNav.SelectTab("home"); // By ID
bottomNav.SetTabNotification("shop", true, 3); // Show badge with count
bottomNav.OnTabChanged += (index, tabId) => {
    Debug.Log($"Tab changed to: {tabId}");
};
```

### Safe Area Handling
```csharp
// Attach SafeAreaAdapter to Canvas or UI root
safeAreaAdapter.ApplySafeArea();
safeAreaAdapter.SetAdaptationEdges(top: true, bottom: true, left: false, right: false);
```

## 📋 Requirements

- Unity 2021.3 or higher
- TextMeshPro package
- Enhanced UI Framework (included)

## 🎨 Design Principles

1. **Mobile-First**: Optimized for touch input and mobile devices
2. **Reusable Components**: DRY principle, build once use everywhere
3. **Performance**: Cached references, object pooling, efficient updates
4. **Accessibility**: Clear visual hierarchy, readable fonts, color contrast
5. **Animations**: Smooth transitions, visual feedback for all interactions
6. **Maintainability**: Clean code, MVP pattern, clear separation of concerns

## 📝 Notes

### Reference Images
The demo is based on 5 reference images showing:
- 4 portrait screens (Home, Level Selection, Gameplay, Battle)
- 1 landscape screen (RPG Stage)

### MVP Pattern Structure
Each screen consists of:
- **Model**: Data container (inherits from or uses PlayerData/GameState)
- **View**: UI display logic (MonoBehaviour on screen prefab)
- **Presenter**: Business logic, mediates between Model and View

### Performance Considerations
- Use object pooling for frequently created/destroyed elements
- Cache component references in Awake()
- Minimize allocations in Update()
- Use events instead of polling
- Throttle expensive operations

## 🐛 Known Issues

None currently. Phase 1 components are stable and tested.

## 📚 Documentation

For more information on the Enhanced UI Framework:
- [Main Documentation](../../Packages/com.yourstudio.enhanced-ui-framework/README.md)
- [API Reference](../../Packages/com.yourstudio.enhanced-ui-framework/Documentation~/API.md)

---

**Version**: 1.0.0
**Status**: Phase 1 Complete ✅
**Next**: Phase 2 - Screen Implementation 🚀
