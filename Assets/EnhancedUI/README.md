# EnhancedUI Demo Components

This folder contains all demo code that depends on the **EnhancedUI Framework** package (`com.yourstudio.enhanced-ui-framework`).

## Folder Structure

```
EnhancedUI/
├── Animations/
│   └── TabSheetSlideAnimation.cs - Horizontal slide animation for tab sheets
├── Input/
│   └── (Future: SwipeDetector and input components)
└── UI/
    └── (Future: BottomTabBar and UI components)
```

## Important Notes

- **DO NOT** mix UnityScreenNavigator and EnhancedUI code
- All code in this folder uses **EnhancedUI** namespace
- TabSheetSlideAnimation uses EnhancedUI.Transition.TransitionAnimationBehaviour
- For old UnityScreenNavigator code, see Assets/UnityScreenNavigator/

## Migration Status

✅ TabSheetSlideAnimation - Migrated to EnhancedUI API
✅ Sheet classes (HomeSheet, BattleSheet, etc.) - Using EnhancedUI.Sheet
✅ DemoAutoSetup.cs - Updated to use EnhancedUI namespaces

## Package Dependencies

- EnhancedUI Framework (`com.yourstudio.enhanced-ui-framework`)
- UniTask (optional, for async operations)
- TextMeshPro (for UI text)
