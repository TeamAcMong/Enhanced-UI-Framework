# MVP Pattern Implementation Guide

This guide explains how to implement screens using the MVP (Model-View-Presenter) pattern in the Enhanced UI Framework demo.

## 🎯 Why MVP?

The MVP pattern provides:
- **Separation of Concerns**: Business logic separate from UI
- **Testability**: Presenters can be unit tested without Unity
- **Maintainability**: Changes to UI don't affect business logic
- **Reusability**: Presenters can be reused with different views

## 📐 Architecture Overview

```
┌─────────────────────────────────────────────────┐
│                   GameState                     │
│  (Singleton - Global game state & data)         │
└─────────────────────────────────────────────────┘
                        ▲
                        │
                        ├──────────────┬──────────────┐
                        │              │              │
┌───────────────┐  ┌────────────┐  ┌──────────┐  ┌──────────┐
│     Model     │  │    View    │  │Presenter │  │  Screen  │
│ (Data/State)  │◄─┤ (UI Logic) │◄─┤(Business)│◄─┤(Enhanced │
│               │  │            │  │  Logic   │  │    UI)   │
└───────────────┘  └────────────┘  └──────────┘  └──────────┘
```

## 📋 File Structure for Each Screen

```
Assets/Demo/Screens/ExampleScreen/
├── ExampleScreenModel.cs          # Data container
├── ExampleScreenView.cs           # UI logic
├── ExampleScreenPresenter.cs      # Business logic
└── prefab_example_screen.prefab   # Prefab with View attached
```

## 🔨 Implementation Steps

### Step 1: Create the Model

The Model is a simple data container (POCO - Plain Old C# Object).

```csharp
using System;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Example
{
    /// <summary>
    /// Model for Example Screen - contains all data needed by the screen
    /// </summary>
    [Serializable]
    public class ExampleScreenModel
    {
        // Screen-specific data
        public string title;
        public int itemCount;
        public bool isSpecialEventActive;

        // References to global state (if needed)
        public PlayerData playerData;

        // Constructor
        public ExampleScreenModel()
        {
            title = "Example Screen";
            itemCount = 0;
            isSpecialEventActive = false;
        }

        // Helper methods for data manipulation
        public void IncrementItemCount()
        {
            itemCount++;
        }

        public bool HasItems()
        {
            return itemCount > 0;
        }
    }
}
```

### Step 2: Create the View

The View is a MonoBehaviour that handles UI display and user input.

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EnhancedUI.Demo.Components;

namespace EnhancedUI.Demo.Screens.Example
{
    /// <summary>
    /// View for Example Screen - handles UI display and user input
    /// </summary>
    public class ExampleScreenView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TopBar topBar;
        [SerializeField] private BottomNavigation bottomNav;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private Button actionButton;
        [SerializeField] private GameObject specialEventBanner;

        private ExampleScreenPresenter _presenter;

        private void Awake()
        {
            // Setup button listeners
            if (actionButton != null)
                actionButton.onClick.AddListener(OnActionButtonClicked);
        }

        private void OnDestroy()
        {
            // Cleanup
            if (actionButton != null)
                actionButton.onClick.RemoveListener(OnActionButtonClicked);
        }

        /// <summary>
        /// Set the presenter (called by Screen)
        /// </summary>
        public void SetPresenter(ExampleScreenPresenter presenter)
        {
            _presenter = presenter;
        }

        /// <summary>
        /// Update the view with model data
        /// </summary>
        public void UpdateView(ExampleScreenModel model)
        {
            if (titleText != null)
                titleText.text = model.title;

            if (itemCountText != null)
                itemCountText.text = $"Items: {model.itemCount}";

            if (specialEventBanner != null)
                specialEventBanner.SetActive(model.isSpecialEventActive);

            // Update action button interactability
            if (actionButton != null)
                actionButton.interactable = model.HasItems();
        }

        /// <summary>
        /// Show the view (called when screen enters)
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);

            // Update top bar
            if (topBar != null)
                topBar.UpdatePlayerInfo();
        }

        /// <summary>
        /// Hide the view (called when screen exits)
        /// </summary>
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Handle user input - delegate to presenter
        /// </summary>
        private void OnActionButtonClicked()
        {
            _presenter?.OnActionButtonPressed();
        }
    }
}
```

### Step 3: Create the Presenter

The Presenter contains business logic and mediates between Model and View.

```csharp
using UnityEngine;
using EnhancedUI.Demo.Models;

namespace EnhancedUI.Demo.Screens.Example
{
    /// <summary>
    /// Presenter for Example Screen - contains business logic
    /// </summary>
    public class ExampleScreenPresenter
    {
        private ExampleScreenModel _model;
        private ExampleScreenView _view;

        /// <summary>
        /// Constructor
        /// </summary>
        public ExampleScreenPresenter(ExampleScreenView view)
        {
            _view = view;
            _model = new ExampleScreenModel();

            // Get player data from GameState
            if (GameState.Instance != null)
            {
                _model.playerData = GameState.Instance.PlayerData;
            }

            // Initialize
            Initialize();
        }

        /// <summary>
        /// Initialize presenter
        /// </summary>
        private void Initialize()
        {
            // Load data, setup listeners, etc.
            LoadScreenData();

            // Subscribe to events
            if (GameState.Instance != null)
            {
                GameState.Instance.OnCurrencyChanged += OnCurrencyChanged;
            }

            // Update view
            UpdateView();
        }

        /// <summary>
        /// Cleanup presenter
        /// </summary>
        public void Cleanup()
        {
            // Unsubscribe from events
            if (GameState.Instance != null)
            {
                GameState.Instance.OnCurrencyChanged -= OnCurrencyChanged;
            }
        }

        /// <summary>
        /// Load screen-specific data
        /// </summary>
        private void LoadScreenData()
        {
            // Example: Load from PlayerPrefs, server, etc.
            _model.itemCount = PlayerPrefs.GetInt("ExampleScreen_ItemCount", 0);
            _model.isSpecialEventActive = CheckIfSpecialEventActive();
        }

        /// <summary>
        /// Save screen data
        /// </summary>
        private void SaveScreenData()
        {
            PlayerPrefs.SetInt("ExampleScreen_ItemCount", _model.itemCount);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Update the view with current model state
        /// </summary>
        private void UpdateView()
        {
            _view.UpdateView(_model);
        }

        /// <summary>
        /// Handle action button press (Business Logic)
        /// </summary>
        public void OnActionButtonPressed()
        {
            if (!_model.HasItems())
            {
                Debug.LogWarning("[ExamplePresenter] No items to process");
                return;
            }

            // Business logic
            ProcessAction();

            // Update view
            UpdateView();
        }

        /// <summary>
        /// Process the action (example business logic)
        /// </summary>
        private void ProcessAction()
        {
            // Example: Consume item and give reward
            _model.IncrementItemCount();

            if (GameState.Instance != null)
            {
                GameState.Instance.AddCurrency(CurrencyType.Gold, 100);
            }

            SaveScreenData();

            Debug.Log($"[ExamplePresenter] Action processed. Items: {_model.itemCount}");
        }

        /// <summary>
        /// Check if special event is active
        /// </summary>
        private bool CheckIfSpecialEventActive()
        {
            // Example: Check date, server data, etc.
            return System.DateTime.Now.Hour >= 18; // Active after 6 PM
        }

        /// <summary>
        /// Currency changed event handler
        /// </summary>
        private void OnCurrencyChanged(CurrencyType type, int oldValue, int newValue)
        {
            // React to currency changes if needed
            Debug.Log($"[ExamplePresenter] Currency changed: {type} {oldValue} -> {newValue}");
            UpdateView();
        }
    }
}
```

### Step 4: Create the Screen (Enhanced UI)

The Screen class connects to the Enhanced UI Framework.

```csharp
using System.Collections;
using UnityEngine;

namespace EnhancedUI.Demo.Screens.Example
{
    /// <summary>
    /// Example Screen - inherits from Page (or Modal, Sheet)
    /// Connects MVP pattern with Enhanced UI Framework
    /// </summary>
    public class ExampleScreen : Page
    {
        private ExampleScreenView _view;
        private ExampleScreenPresenter _presenter;

        public override IEnumerator Initialize()
        {
            Debug.Log("[ExampleScreen] Initialize");

            // Get view component
            _view = GetComponent<ExampleScreenView>();

            if (_view == null)
            {
                Debug.LogError("[ExampleScreen] View component not found!");
                yield break;
            }

            // Create presenter
            _presenter = new ExampleScreenPresenter(_view);

            // Connect view to presenter
            _view.SetPresenter(_presenter);

            // Base initialization
            yield return base.Initialize();
        }

        public override IEnumerator Cleanup()
        {
            Debug.Log("[ExampleScreen] Cleanup");

            // Cleanup presenter
            _presenter?.Cleanup();

            // Base cleanup
            yield return base.Cleanup();
        }

        private void OnEnable()
        {
            // Show view when screen becomes active
            _view?.Show();
        }

        private void OnDisable()
        {
            // Hide view when screen becomes inactive
            _view?.Hide();
        }
    }
}
```

## 🎯 Best Practices

### Model
- ✅ Keep it simple - just data
- ✅ Use [Serializable] for easy inspection
- ✅ Add helper methods for data validation
- ❌ No Unity dependencies (no MonoBehaviour, Transform, etc.)
- ❌ No business logic (that goes in Presenter)

### View
- ✅ Only handle UI updates and user input
- ✅ Delegate all logic to Presenter
- ✅ Cache component references in Awake()
- ✅ Clean up listeners in OnDestroy()
- ❌ No business logic
- ❌ Don't access GameState directly (use Presenter)

### Presenter
- ✅ Contains all business logic
- ✅ Mediates between Model and View
- ✅ Handles events and state changes
- ✅ Can be unit tested without Unity
- ❌ No direct UI manipulation (use View methods)
- ❌ No MonoBehaviour dependencies

### Screen (Enhanced UI)
- ✅ Minimal code - just connects MVP to framework
- ✅ Creates Presenter in Initialize()
- ✅ Cleans up in Cleanup()
- ❌ No business logic (delegate to Presenter)

## 📊 Data Flow

```
User Input → View → Presenter → Model → Presenter → View → UI Update
     ↑                                                         │
     └─────────────────────────────────────────────────────────┘
```

## 🧪 Testing

### Unit Testing the Presenter
```csharp
[Test]
public void TestActionButton_WithItems_ShouldIncreaseGold()
{
    // Arrange
    var mockView = new MockExampleScreenView();
    var presenter = new ExampleScreenPresenter(mockView);

    // Act
    presenter.OnActionButtonPressed();

    // Assert
    Assert.AreEqual(100, GameState.Instance.GetCurrency(CurrencyType.Gold));
}
```

## 📚 Additional Resources

- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [MVP Pattern](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93presenter)
- [SOLID Principles](https://en.wikipedia.org/wiki/SOLID)

---

**Remember**: Keep it simple, separate concerns, and let each component do one thing well!
