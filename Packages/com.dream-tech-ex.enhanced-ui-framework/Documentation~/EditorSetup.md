# Unity Editor Setup Guide
## Complete Step-by-Step Integration Guide

**Time Required**: 6-9 hours
**Difficulty**: Intermediate
**Prerequisites**: Unity 2021.3+ with TextMeshPro installed

---

## 📋 TABLE OF CONTENTS

1. [Scene Setup](#1-scene-setup)
2. [Container Configuration](#2-container-configuration)
3. [Prefab Creation](#3-prefab-creation)
4. [Navigation Setup](#4-navigation-setup)
5. [Testing & Debugging](#5-testing--debugging)

---

## 1. SCENE SETUP (1-2 hours)

### 1.1 Create Demo Scene

1. **Create new scene**: `File → New Scene → 2D`
2. **Save scene**: `Assets/Demo/Scenes/DemoScene.unity`

### 1.2 Setup Canvas

1. **Create Canvas**:
   - Right-click in Hierarchy → `UI → Canvas`
   - Rename to `MainCanvas`

2. **Configure Canvas**:
   ```
   Canvas Component:
   - Render Mode: Screen Space - Overlay
   - Pixel Perfect: ✓ (checked)
   - Sort Order: 0

   Canvas Scaler Component:
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1080 x 1920 (portrait)
   - Screen Match Mode: Match Width Or Height
   - Match: 0.5
   - Reference Pixels Per Unit: 100
   ```

3. **Add SafeAreaAdapter**:
   - Select `MainCanvas`
   - Add Component → `SafeAreaAdapter`
   - Settings:
     - Adapt On Enable: ✓
     - Adapt On Orientation Change: ✓
     - Adapt All Edges: ✓

### 1.3 Create Container Structure

Create this hierarchy under `MainCanvas`:

```
MainCanvas
├── SafeArea (RectTransform - fills parent)
│   ├── PageContainer (Add Component: PageContainer)
│   │   └── [Pages will be added here]
│   ├── ModalContainer (Add Component: ModalContainer)
│   │   └── ModalBackdrop
│   │       └── [Modals will be added here]
│   └── SheetContainer (Add Component: SheetContainer)
│       └── [Sheets will be added here]
└── GameStateManager (Add Component: GameState)
```

### 1.4 Configure SafeArea

1. **Select SafeArea GameObject**:
   - Add Component → `RectTransform`
   - Anchors: Stretch both (0,0,1,1)
   - Left/Right/Top/Bottom: 0

---

## 2. CONTAINER CONFIGURATION (1 hour)

### 2.1 PageContainer Setup

1. **Select PageContainer**:
   - Add Component → `PageContainer` (from UnityScreenNavigator)
   - RectTransform: Stretch all (fills parent)
   - CanvasGroup: Add if not present

2. **Configure PageContainer Settings**:
   ```
   PageContainer:
   - Enable Interaction In Transition: false
   - Control Interactability: true
   ```

### 2.2 ModalContainer Setup

1. **Select ModalContainer**:
   - Add Component → `ModalContainer`
   - RectTransform: Stretch all
   - CanvasGroup: Add if not present

2. **Create ModalBackdrop**:
   - Create child: Right-click ModalContainer → `UI → Image`
   - Rename to `ModalBackdrop`
   - Component settings:
     ```
     RectTransform: Stretch all (fills parent)
     Image:
       - Color: Black (0,0,0,128) - semi-transparent
       - Raycast Target: ✓
     Button: Add Component
       - Transition: None
       - OnClick: Close top modal
     ```

### 2.3 SheetContainer Setup

1. **Select SheetContainer**:
   - Add Component → `SheetContainer`
   - RectTransform: Stretch all

---

## 3. PREFAB CREATION (2-3 hours)

### 3.1 Create Prefab Folders

Create folder structure:
```
Assets/Demo/Prefabs/
├── Screens/
│   ├── Home/
│   ├── LevelSelection/
│   ├── Gameplay/
│   ├── Battle/
│   ├── Shop/
│   ├── Inventory/
│   └── RPGStage/
├── Modals/
│   └── Settings/
├── Components/
│   ├── TopBar/
│   ├── BottomNavigation/
│   ├── CurrencyDisplay/
│   ├── SideMenuButton/
│   ├── LevelButton/
│   └── LoadingIndicator/
└── Cards/
    ├── ShopItemCard/
    ├── InventoryItemCard/
    └── CharacterCard/
```

### 3.2 HomeScreen Prefab (Example)

**Create HomeScreen Prefab**:

1. **In Hierarchy**, under `PageContainer`:
   - Right-click → `UI → Panel`
   - Rename to `HomeScreen`

2. **Add Components**:
   - Add Component → `HomeScreen` (script)
   - Add Component → `HomeScreenView` (script)
   - CanvasGroup (if not present)

3. **RectTransform Settings**:
   - Anchors: Stretch all (0,0,1,1)
   - Left/Right/Top/Bottom: 0

4. **Create Child UI Elements**:

```
HomeScreen
├── Background (Image - full screen, color gradient)
├── TopBar (Prefab instance or create manually)
│   ├── PlayerInfo
│   │   ├── AvatarImage
│   │   ├── PlayerNameText (TMP)
│   │   └── LevelText (TMP)
│   └── Currencies
│       ├── GoldDisplay
│       ├── GemsDisplay
│       └── EnergyDisplay
├── ContentArea
│   ├── WelcomeMessageText (TMP)
│   ├── PlayButton (Button)
│   │   └── PlayButtonText (TMP)
│   ├── DailyRewardPanel
│   │   ├── TitleText (TMP)
│   │   ├── DayNumberText (TMP)
│   │   ├── StreakText (TMP)
│   │   └── ClaimButton (Button)
│   └── EventBanner
│       ├── EventTitleText (TMP)
│       ├── EventProgressSlider (Slider)
│       └── EventProgressText (TMP)
├── SideMenu (Vertical Layout Group)
│   ├── MailButton (SideMenuButton)
│   ├── ShopButton (SideMenuButton)
│   ├── EventsButton (SideMenuButton)
│   ├── FriendsButton (SideMenuButton)
│   └── GuildButton (SideMenuButton)
├── BottomNavigation (Prefab instance)
│   ├── HomeTab (Button)
│   ├── PlayTab (Button)
│   ├── BattleTab (Button)
│   └── InventoryTab (Button)
└── LoadingIndicator (Prefab instance)
```

5. **Link UI References** in `HomeScreenView`:
   - Drag each UI element to corresponding field in Inspector

6. **Save as Prefab**:
   - Drag `HomeScreen` to `Assets/Demo/Prefabs/Screens/Home/`
   - Rename to `prefab_home_screen.prefab`

### 3.3 Quick Prefab Creation for All Screens

**For each screen**, follow similar pattern:

#### **LevelSelectionScreen**:
```
LevelSelectionScreen
├── Background
├── TopBar (shared prefab)
├── ChapterSelector (Dropdown or Horizontal Scroll)
├── LevelGrid (Grid Layout Group)
│   └── [LevelButton prefabs x20]
├── ProgressBar (Kingdom Pass)
└── BackButton
```

#### **GameplayScreen**:
```
GameplayScreen
├── Background
├── GameplayHUD
│   ├── TimerText (TMP)
│   ├── WaveText (TMP)
│   ├── ResourceDisplays (Wood, Stone, Iron)
│   └── PauseButton
├── UpgradeSlots (3 slots with buttons)
├── PauseMenuPanel (initially disabled)
├── VictoryPanel (initially disabled)
└── DefeatPanel (initially disabled)
```

#### **ShopScreen**:
```
ShopScreen
├── Background
├── TopBar (shared prefab)
├── TabNavigation (Horizontal Layout)
│   ├── GemsTabButton
│   ├── GoldTabButton
│   └── SpecialTabButton
├── PlayerCurrency
│   ├── GemsText (TMP)
│   └── GoldText (TMP)
├── ShopItemsGrid (Grid Layout Group)
│   └── [ShopItemCard prefabs - instantiated at runtime]
├── LoadingIndicator
└── BackButton
```

#### **InventoryScreen**:
```
InventoryScreen
├── Background
├── TopBar (shared prefab)
├── FilterButtons (Horizontal Layout)
│   ├── AllButton
│   ├── WeaponsButton
│   ├── ArmorButton
│   ├── ConsumablesButton
│   ├── MaterialsButton
│   └── SpecialButton
├── SortDropdown (TMP Dropdown)
├── InventoryGrid (Grid Layout Group)
│   └── [ItemCard prefabs - instantiated at runtime]
├── ItemDetailPanel (initially disabled)
│   ├── ItemNameText (TMP)
│   ├── ItemDescriptionText (TMP)
│   ├── ItemStatsText (TMP)
│   ├── EquipButton
│   ├── UseButton
│   └── CloseButton
└── BackButton
```

#### **SettingsModal**:
```
SettingsModal
├── ModalBackground (semi-transparent)
├── ContentPanel
│   ├── TitleText (TMP)
│   ├── TabNavigation
│   │   ├── AudioTabButton
│   │   ├── NotificationsTabButton
│   │   ├── GameplayTabButton
│   │   └── AccountTabButton
│   ├── AudioPanel
│   │   ├── MusicToggle
│   │   ├── MusicVolumeSlider
│   │   ├── SoundEffectsToggle
│   │   └── SoundEffectsVolumeSlider
│   ├── NotificationsPanel (initially disabled)
│   ├── GameplayPanel (initially disabled)
│   ├── AccountPanel (initially disabled)
│   ├── ResetButton
│   └── CloseButton
```

#### **RPGStageScreen** (Landscape):
```
RPGStageScreen
├── Background (landscape-oriented image)
├── LeftPanel (Boss Display)
│   ├── BossNameText (TMP)
│   ├── BossLevelText (TMP)
│   ├── BossHPSlider
│   ├── BossStatsText (TMP)
│   └── AbilitiesList (Vertical Layout)
├── CenterPanel (Character Roster)
│   ├── CharacterGridScrollView
│   │   └── CharacterGrid (Grid Layout - 2x3)
│   │       └── [CharacterCard prefabs x6]
├── RightPanel (Party & Actions)
│   ├── PartyTitleText (TMP)
│   ├── PartySlots (4 slots)
│   ├── AutoSelectButton
│   ├── ClearPartyButton
│   └── StartBattleButton
└── TopBar (Stage Info)
    ├── StageNumberText (TMP)
    ├── StageNameText (TMP)
    └── EnergyCostText (TMP)
```

### 3.4 Component Prefabs

**Create reusable component prefabs**:

#### **TopBar Prefab**:
```
TopBar
├── Background (Image)
├── PlayerInfo (Horizontal Layout)
│   ├── AvatarImage (Image)
│   ├── PlayerNameText (TMP)
│   └── LevelText (TMP)
└── CurrenciesPanel (Horizontal Layout)
    ├── GoldDisplay (CurrencyDisplay prefab)
    ├── GemsDisplay (CurrencyDisplay prefab)
    └── EnergyDisplay (CurrencyDisplay prefab)
```

#### **CurrencyDisplay Prefab**:
```
CurrencyDisplay (Add CurrencyDisplay script)
├── Icon (Image)
└── AmountText (TMP)
```

#### **SideMenuButton Prefab**:
```
SideMenuButton (Add SideMenuButton script + Button)
├── Background (Image)
├── Icon (Image)
├── LabelText (TMP)
└── NotificationBadge (Image)
    └── CountText (TMP)
```

#### **LevelButton Prefab**:
```
LevelButton (Add LevelButton script + Button)
├── Background (Image)
├── LevelNumberText (TMP)
├── StarsPanel (Horizontal Layout)
│   ├── Star1 (Image)
│   ├── Star2 (Image)
│   └── Star3 (Image)
└── LockIcon (Image)
```

---

## 4. NAVIGATION SETUP (1 hour)

### 4.1 Create DemoBootstrap Script

Create `Assets/Demo/Scripts/DemoBootstrap.cs`:

```csharp
using UnityEngine;
using System.Collections;
using UnityScreenNavigator.Runtime.Core.Page;
using UnityScreenNavigator.Runtime.Core.Modal;
using EnhancedUI.Demo.Screens.Home;

namespace EnhancedUI.Demo
{
    /// <summary>
    /// Bootstrap script to initialize demo and load first screen
    /// Attach to a GameObject in the scene
    /// </summary>
    public class DemoBootstrap : MonoBehaviour
    {
        [Header("Containers")]
        [SerializeField] private PageContainer pageContainer;
        [SerializeField] private ModalContainer modalContainer;

        [Header("Screen Prefabs")]
        [SerializeField] private GameObject homeScreenPrefab;
        [SerializeField] private GameObject levelSelectionScreenPrefab;
        [SerializeField] private GameObject gameplayScreenPrefab;
        [SerializeField] private GameObject battleArenaScreenPrefab;
        [SerializeField] private GameObject shopScreenPrefab;
        [SerializeField] private GameObject inventoryScreenPrefab;
        [SerializeField] private GameObject rpgStageScreenPrefab;

        [Header("Modal Prefabs")]
        [SerializeField] private GameObject settingsModalPrefab;

        private IEnumerator Start()
        {
            Debug.Log("[DemoBootstrap] Starting demo initialization...");

            // Register screen prefabs
            RegisterScreens();

            // Initialize game state
            InitializeGameState();

            // Load home screen
            yield return LoadHomeScreen();

            Debug.Log("[DemoBootstrap] Demo initialization complete!");
        }

        private void RegisterScreens()
        {
            // In a real implementation, you would register prefabs with containers
            // For now, we'll just log
            Debug.Log("[DemoBootstrap] Screen prefabs registered");
        }

        private void InitializeGameState()
        {
            // Game state is a singleton, it will auto-initialize
            var gameState = Models.GameState.Instance;
            Debug.Log($"[DemoBootstrap] Game state initialized - Player: {gameState.PlayerData.playerName}");
        }

        private IEnumerator LoadHomeScreen()
        {
            if (pageContainer != null && homeScreenPrefab != null)
            {
                // Instantiate home screen
                var homeScreenObj = Instantiate(homeScreenPrefab, pageContainer.transform);
                var homeScreen = homeScreenObj.GetComponent<HomeScreen>();

                if (homeScreen != null)
                {
                    yield return homeScreen.Initialize();
                    Debug.Log("[DemoBootstrap] Home screen loaded");
                }
            }
            else
            {
                Debug.LogError("[DemoBootstrap] PageContainer or HomeScreen prefab not assigned!");
            }
        }
    }
}
```

### 4.2 Setup Navigation Manager

Create `Assets/Demo/Scripts/NavigationManager.cs`:

```csharp
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace EnhancedUI.Demo
{
    /// <summary>
    /// Centralized navigation manager for the demo
    /// Singleton pattern for easy access from any screen
    /// </summary>
    public class NavigationManager : MonoBehaviour
    {
        private static NavigationManager _instance;
        public static NavigationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<NavigationManager>();
                }
                return _instance;
            }
        }

        [Header("Containers")]
        [SerializeField] private PageContainer pageContainer;
        [SerializeField] private ModalContainer modalContainer;

        [Header("Screen Resource Paths")]
        [SerializeField] private string homeScreenPath = "Prefabs/Screens/Home/prefab_home_screen";
        [SerializeField] private string levelSelectionPath = "Prefabs/Screens/LevelSelection/prefab_level_selection_screen";
        [SerializeField] private string gameplayScreenPath = "Prefabs/Screens/Gameplay/prefab_gameplay_screen";
        [SerializeField] private string battleArenaPath = "Prefabs/Screens/Battle/prefab_battle_arena_screen";
        [SerializeField] private string shopScreenPath = "Prefabs/Screens/Shop/prefab_shop_screen";
        [SerializeField] private string inventoryScreenPath = "Prefabs/Screens/Inventory/prefab_inventory_screen";
        [SerializeField] private string rpgStageScreenPath = "Prefabs/Screens/RPGStage/prefab_rpg_stage_screen";
        [SerializeField] private string settingsModalPath = "Prefabs/Modals/Settings/prefab_settings_modal";

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        // Navigation methods
        public void GoToHome() => LoadScreen(homeScreenPath);
        public void GoToLevelSelection() => LoadScreen(levelSelectionPath);
        public void GoToGameplay(int levelId) => LoadScreen(gameplayScreenPath);
        public void GoToBattleArena() => LoadScreen(battleArenaPath);
        public void GoToShop() => LoadScreen(shopScreenPath);
        public void GoToInventory() => LoadScreen(inventoryScreenPath);
        public void GoToRPGStage() => LoadScreen(rpgStageScreenPath);
        public void ShowSettings() => ShowModal(settingsModalPath);

        private void LoadScreen(string resourcePath)
        {
            Debug.Log($"[NavigationManager] Loading screen: {resourcePath}");
            // In real implementation, use pageContainer.Push() with resource path
        }

        private void ShowModal(string resourcePath)
        {
            Debug.Log($"[NavigationManager] Showing modal: {resourcePath}");
            // In real implementation, use modalContainer.Push() with resource path
        }

        public void GoBack()
        {
            Debug.Log("[NavigationManager] Going back");
            // In real implementation, use pageContainer.Pop()
        }
    }
}
```

### 4.3 Connect Navigation

In each screen's presenter, replace navigation stubs with:

```csharp
// Example in HomeScreenPresenter:
private void HandlePlayPressed()
{
    NavigationManager.Instance?.GoToLevelSelection();
}

private void HandleShopPressed()
{
    NavigationManager.Instance?.GoToShop();
}
```

---

## 5. TESTING & DEBUGGING (1 hour)

### 5.1 Create Test Scene

1. **Enter Play Mode**
2. **Check console** for initialization logs
3. **Test navigation flow**:
   - Home → Level Selection
   - Home → Shop
   - Home → Settings Modal
   - Back navigation

### 5.2 Debug Common Issues

**Issue**: Screens don't show
- Check PageContainer has correct RectTransform (stretch all)
- Verify CanvasGroup is present
- Check screen prefab is assigned in DemoBootstrap

**Issue**: Buttons don't respond
- Verify EventSystem exists in scene
- Check CanvasGroup "Interactable" is checked
- Verify button's "Interactable" is checked

**Issue**: Text shows as boxes
- Import TextMeshPro essentials: `Window → TextMeshPro → Import TMP Essential Resources`

**Issue**: Safe area not working
- Check SafeAreaAdapter is on correct GameObject
- Verify it's calling ApplySafeArea in Update
- Test on device or simulator with notch

### 5.3 Performance Testing

1. **Enable Profiler**: `Window → Analysis → Profiler`
2. **Check for leaks**: Watch memory usage when navigating
3. **Test transitions**: Verify smooth animations between screens
4. **Test on device**: Build and test on actual mobile device

---

## 📱 QUICK START CHECKLIST

- [ ] Scene created with Canvas
- [ ] Canvas Scaler configured (1080x1920)
- [ ] SafeArea configured
- [ ] PageContainer added
- [ ] ModalContainer added
- [ ] GameState singleton in scene
- [ ] HomeScreen prefab created
- [ ] All UI references linked
- [ ] DemoBootstrap script added
- [ ] HomeScreen loads in Play Mode
- [ ] Navigation works between screens
- [ ] Settings modal opens/closes
- [ ] Tested on device

---

## 🎨 STYLING TIPS

### Color Palette (Example)
```
Primary: #4A90E2 (Blue)
Secondary: #50C878 (Green)
Accent: #FF6B6B (Red)
Background: #1E1E2E (Dark Blue)
Surface: #2D2D44 (Medium Dark)
Text: #FFFFFF (White)
Text Secondary: #B0B0C0 (Light Gray)
```

### Typography
```
Headers: Bold, Size 48-72
Body: Regular, Size 24-36
Buttons: Bold, Size 28-32
Small Text: Regular, Size 18-20
```

### Spacing
```
Padding: 20-40 units
Margins: 10-20 units
Button Height: 80-100 units
Icon Size: 60-80 units
```

---

**Next Steps**: Follow this guide step-by-step to integrate all screens into Unity Editor!
