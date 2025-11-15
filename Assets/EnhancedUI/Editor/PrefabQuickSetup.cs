using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace EnhancedUI.Demo.Editor
{
    /// <summary>
    /// Unity Editor utility to quickly create prefab structures
    /// Accessible via: Tools → Enhanced UI Demo → Quick Setup
    ///
    /// This tool automates creation of:
    /// - Basic screen structure
    /// - Component prefabs
    /// - UI element hierarchies
    /// </summary>
    public class PrefabQuickSetup : EditorWindow
    {
        private string screenName = "NewScreen";
        private bool includeTopBar = true;
        private bool includeBottomNav = false;
        private bool includeBackButton = true;

        [MenuItem("Tools/Enhanced UI Demo/Prefab Quick Setup")]
        public static void ShowWindow()
        {
            var window = GetWindow<PrefabQuickSetup>("Prefab Quick Setup");
            window.minSize = new Vector2(400, 500);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Enhanced UI Demo - Prefab Quick Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This tool helps you quickly create screen prefab structures. " +
                "Select options below and click 'Create Screen' to generate the hierarchy.",
                MessageType.Info
            );

            GUILayout.Space(10);

            // Screen settings
            GUILayout.Label("Screen Settings", EditorStyles.boldLabel);
            screenName = EditorGUILayout.TextField("Screen Name", screenName);
            includeTopBar = EditorGUILayout.Toggle("Include TopBar", includeTopBar);
            includeBottomNav = EditorGUILayout.Toggle("Include Bottom Navigation", includeBottomNav);
            includeBackButton = EditorGUILayout.Toggle("Include Back Button", includeBackButton);

            GUILayout.Space(20);

            // Quick create buttons
            GUILayout.Label("Quick Create", EditorStyles.boldLabel);

            if (GUILayout.Button("Create Basic Screen", GUILayout.Height(30)))
            {
                CreateBasicScreen();
            }

            if (GUILayout.Button("Create TopBar Prefab", GUILayout.Height(30)))
            {
                CreateTopBar();
            }

            if (GUILayout.Button("Create BottomNavigation Prefab", GUILayout.Height(30)))
            {
                CreateBottomNavigation();
            }

            if (GUILayout.Button("Create CurrencyDisplay Prefab", GUILayout.Height(30)))
            {
                CreateCurrencyDisplay();
            }

            if (GUILayout.Button("Create SideMenuButton Prefab", GUILayout.Height(30)))
            {
                CreateSideMenuButton();
            }

            GUILayout.Space(20);

            // Scene setup buttons
            GUILayout.Label("Scene Setup", EditorStyles.boldLabel);

            if (GUILayout.Button("Create Full Canvas Structure", GUILayout.Height(30)))
            {
                CreateFullCanvasStructure();
            }

            if (GUILayout.Button("Add GameState Manager", GUILayout.Height(30)))
            {
                AddGameStateManager();
            }

            if (GUILayout.Button("Add Navigation Manager", GUILayout.Height(30)))
            {
                AddNavigationManager();
            }
        }

        #region Screen Creation

        private void CreateBasicScreen()
        {
            // Create root panel
            var screenObj = CreateUIElement("Panel", screenName);
            var rectTransform = screenObj.GetComponent<RectTransform>();
            SetStretchAll(rectTransform);

            // Add background
            var bg = CreateUIElement("Image", "Background", screenObj.transform);
            SetStretchAll(bg.GetComponent<RectTransform>());
            var bgImage = bg.GetComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            // Add TopBar if enabled
            if (includeTopBar)
            {
                CreateTopBarInScreen(screenObj.transform);
            }

            // Add content area
            var content = CreateUIElement("Empty", "ContentArea", screenObj.transform);
            var contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, includeTopBar ? 0.9f : 1f);
            contentRect.offsetMin = new Vector2(20, includeBottomNav ? 120 : 20);
            contentRect.offsetMax = new Vector2(-20, -20);

            // Add BottomNavigation if enabled
            if (includeBottomNav)
            {
                CreateBottomNavigationInScreen(screenObj.transform);
            }

            // Add BackButton if enabled
            if (includeBackButton)
            {
                CreateBackButton(screenObj.transform);
            }

            // Select created object
            Selection.activeGameObject = screenObj;

            Debug.Log($"[PrefabQuickSetup] Created basic screen: {screenName}");
        }

        private void CreateTopBarInScreen(Transform parent)
        {
            var topBar = CreateUIElement("Panel", "TopBar", parent);
            var rect = topBar.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 1);
            rect.sizeDelta = new Vector2(0, 120);
            rect.anchoredPosition = Vector2.zero;

            // Background
            var bgImage = topBar.GetComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            // Player info (left)
            var playerInfo = CreateUIElement("Empty", "PlayerInfo", topBar.transform);
            var playerRect = playerInfo.GetComponent<RectTransform>();
            playerRect.anchorMin = new Vector2(0, 0.5f);
            playerRect.anchorMax = new Vector2(0, 0.5f);
            playerRect.pivot = new Vector2(0, 0.5f);
            playerRect.anchoredPosition = new Vector2(20, 0);
            playerRect.sizeDelta = new Vector2(300, 80);

            // Add horizontal layout
            var layout = playerInfo.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // Avatar
            var avatar = CreateUIElement("Image", "Avatar", playerInfo.transform);
            avatar.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 60);
            avatar.GetComponent<Image>().color = Color.gray;

            // Name text
            var nameText = CreateTextElement("PlayerNameText", playerInfo.transform);
            nameText.text = "Player Name";
            nameText.fontSize = 24;

            // Currencies (right)
            var currencies = CreateUIElement("Empty", "Currencies", topBar.transform);
            var currRect = currencies.GetComponent<RectTransform>();
            currRect.anchorMin = new Vector2(1, 0.5f);
            currRect.anchorMax = new Vector2(1, 0.5f);
            currRect.pivot = new Vector2(1, 0.5f);
            currRect.anchoredPosition = new Vector2(-20, 0);
            currRect.sizeDelta = new Vector2(400, 80);

            // Add horizontal layout for currencies
            var currLayout = currencies.AddComponent<HorizontalLayoutGroup>();
            currLayout.spacing = 15;
            currLayout.childAlignment = TextAnchor.MiddleRight;
        }

        private void CreateBottomNavigationInScreen(Transform parent)
        {
            var bottomNav = CreateUIElement("Panel", "BottomNavigation", parent);
            var rect = bottomNav.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0);
            rect.sizeDelta = new Vector2(0, 100);
            rect.anchoredPosition = Vector2.zero;

            // Background
            var bgImage = bottomNav.GetComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            // Add horizontal layout
            var layout = bottomNav.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10;
            layout.padding = new RectOffset(20, 20, 10, 10);
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            // Create 4 tab buttons
            string[] tabs = { "Home", "Play", "Battle", "Inventory" };
            foreach (var tabName in tabs)
            {
                var button = CreateButton(tabName + "Tab", bottomNav.transform);
                button.GetComponentInChildren<TextMeshProUGUI>().text = tabName;
            }
        }

        private void CreateBackButton(Transform parent)
        {
            var backBtn = CreateButton("BackButton", parent);
            var rect = backBtn.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);
            rect.sizeDelta = new Vector2(100, 60);

            var text = backBtn.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "← Back";
            text.fontSize = 20;
        }

        #endregion

        #region Component Prefabs

        private void CreateTopBar()
        {
            var topBar = CreateUIElement("Panel", "TopBar");
            var rect = topBar.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1080, 120);

            CreateTopBarInScreen(topBar.transform.parent);

            Selection.activeGameObject = topBar;
            Debug.Log("[PrefabQuickSetup] TopBar prefab created");
        }

        private void CreateBottomNavigation()
        {
            var bottomNav = CreateUIElement("Panel", "BottomNavigation");
            var rect = bottomNav.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(1080, 100);

            CreateBottomNavigationInScreen(bottomNav.transform.parent);

            Selection.activeGameObject = bottomNav;
            Debug.Log("[PrefabQuickSetup] BottomNavigation prefab created");
        }

        private void CreateCurrencyDisplay()
        {
            var display = CreateUIElement("Panel", "CurrencyDisplay");
            var rect = display.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(150, 50);

            // Add horizontal layout
            var layout = display.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8;
            layout.padding = new RectOffset(10, 10, 5, 5);
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = false;
            layout.childControlHeight = false;

            // Icon
            var icon = CreateUIElement("Image", "Icon", display.transform);
            icon.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
            icon.GetComponent<Image>().color = Color.yellow;

            // Amount text
            var amountText = CreateTextElement("AmountText", display.transform);
            amountText.text = "999,999";
            amountText.fontSize = 24;
            amountText.fontStyle = FontStyles.Bold;

            // Add CurrencyDisplay script
            display.AddComponent<Components.CurrencyDisplay>();

            Selection.activeGameObject = display;
            Debug.Log("[PrefabQuickSetup] CurrencyDisplay prefab created");
        }

        private void CreateSideMenuButton()
        {
            var button = CreateButton("SideMenuButton", null);
            var rect = button.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(80, 80);

            // Icon
            var icon = CreateUIElement("Image", "Icon", button.transform);
            var iconRect = icon.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.sizeDelta = new Vector2(50, 50);
            icon.GetComponent<Image>().color = Color.white;

            // Notification badge
            var badge = CreateUIElement("Image", "NotificationBadge", button.transform);
            var badgeRect = badge.GetComponent<RectTransform>();
            badgeRect.anchorMin = new Vector2(1, 1);
            badgeRect.anchorMax = new Vector2(1, 1);
            badgeRect.pivot = new Vector2(1, 1);
            badgeRect.anchoredPosition = new Vector2(0, 0);
            badgeRect.sizeDelta = new Vector2(24, 24);
            badge.GetComponent<Image>().color = Color.red;

            var countText = CreateTextElement("CountText", badge.transform);
            SetStretchAll(countText.GetComponent<RectTransform>());
            countText.text = "5";
            countText.fontSize = 14;
            countText.alignment = TextAlignmentOptions.Center;

            // Add SideMenuButton script
            button.AddComponent<Components.SideMenuButton>();

            Selection.activeGameObject = button;
            Debug.Log("[PrefabQuickSetup] SideMenuButton prefab created");
        }

        #endregion

        #region Scene Setup

        private void CreateFullCanvasStructure()
        {
            // Find or create canvas
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasObj = new GameObject("MainCanvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();

                // Configure CanvasScaler
                var scaler = canvas.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                scaler.matchWidthOrHeight = 0.5f;

                Debug.Log("[PrefabQuickSetup] Created MainCanvas");
            }

            // Create SafeArea
            var safeArea = CreateUIElement("Empty", "SafeArea", canvas.transform);
            SetStretchAll(safeArea.GetComponent<RectTransform>());
            safeArea.AddComponent<Utils.SafeAreaAdapter>();

            // Create PageContainer
            var pageContainer = CreateUIElement("Empty", "PageContainer", safeArea.transform);
            SetStretchAll(pageContainer.GetComponent<RectTransform>());
            pageContainer.AddComponent<CanvasGroup>();
            // Note: PageContainer script from UnityScreenNavigator needs to be added manually

            // Create ModalContainer
            var modalContainer = CreateUIElement("Empty", "ModalContainer", safeArea.transform);
            SetStretchAll(modalContainer.GetComponent<RectTransform>());
            modalContainer.AddComponent<CanvasGroup>();

            // Create modal backdrop
            var backdrop = CreateUIElement("Image", "ModalBackdrop", modalContainer.transform);
            SetStretchAll(backdrop.GetComponent<RectTransform>());
            var backdropImage = backdrop.GetComponent<Image>();
            backdropImage.color = new Color(0, 0, 0, 0.5f);
            backdrop.AddComponent<Button>();

            // Create SheetContainer
            var sheetContainer = CreateUIElement("Empty", "SheetContainer", safeArea.transform);
            SetStretchAll(sheetContainer.GetComponent<RectTransform>());

            Debug.Log("[PrefabQuickSetup] Full canvas structure created!");
            EditorUtility.DisplayDialog("Success", "Canvas structure created successfully!\n\nRemember to add UnityScreenNavigator components:\n- PageContainer\n- ModalContainer\n- SheetContainer", "OK");
        }

        private void AddGameStateManager()
        {
            var existing = FindObjectOfType<Models.GameState>();
            if (existing != null)
            {
                Debug.LogWarning("[PrefabQuickSetup] GameState already exists in scene");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            var go = new GameObject("GameStateManager");
            go.AddComponent<Models.GameState>();

            Debug.Log("[PrefabQuickSetup] GameState manager added");
            Selection.activeGameObject = go;
        }

        private void AddNavigationManager()
        {
            var existing = FindObjectOfType<NavigationManager>();
            if (existing != null)
            {
                Debug.LogWarning("[PrefabQuickSetup] NavigationManager already exists in scene");
                Selection.activeGameObject = existing.gameObject;
                return;
            }

            var go = new GameObject("NavigationManager");
            go.AddComponent<NavigationManager>();

            Debug.Log("[PrefabQuickSetup] NavigationManager added");
            Selection.activeGameObject = go;
        }

        #endregion

        #region Helper Methods

        private GameObject CreateUIElement(string type, string name, Transform parent = null)
        {
            GameObject go;

            if (type == "Empty")
            {
                go = new GameObject(name, typeof(RectTransform));
            }
            else if (type == "Panel" || type == "Image")
            {
                go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            }
            else
            {
                go = new GameObject(name, typeof(RectTransform));
            }

            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }
            else
            {
                // Try to parent to selected object
                if (Selection.activeTransform != null)
                {
                    go.transform.SetParent(Selection.activeTransform, false);
                }
            }

            return go;
        }

        private TextMeshProUGUI CreateTextElement(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);

            var text = go.GetComponent<TextMeshProUGUI>();
            text.color = Color.white;
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;

            return text;
        }

        private GameObject CreateButton(string name, Transform parent)
        {
            var button = CreateUIElement("Image", name, parent);
            button.AddComponent<Button>();

            // Button background
            var image = button.GetComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 1f, 1f);

            // Button text
            var textObj = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(button.transform, false);
            SetStretchAll(textObj.GetComponent<RectTransform>());

            var text = textObj.GetComponent<TextMeshProUGUI>();
            text.text = name;
            text.color = Color.white;
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;

            return button;
        }

        private void SetStretchAll(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        #endregion
    }
}
