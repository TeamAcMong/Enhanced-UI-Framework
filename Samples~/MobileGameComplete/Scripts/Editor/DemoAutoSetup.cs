#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using EnhancedUI;
using EnhancedUI.Transition;
using EnhancedUI.Demo.Input;
using EnhancedUI.Demo.UI;
using EnhancedUI.Demo.Animations;
using EnhancedUI.Demo.Screens.HomeContainer;
using EnhancedUI.Demo.Screens.Home;
using EnhancedUI.Demo.Screens.Battle;
using EnhancedUI.Demo.Screens.Inventory;
using EnhancedUI.Demo.Screens.Shop;

namespace EnhancedUI.Demo.Editor
{
    /// <summary>
    /// Auto-setup tool for Enhanced UI Framework Demo
    /// Creates a complete demo scene with one click - includes full tab navigation UI
    /// </summary>
    public static class DemoAutoSetup
    {
        private const string MENU_PATH = "Tools/Enhanced UI Demo/";
        private const string DEMO_SCENE_PATH = "Assets/Demo/Scenes/DemoScene.unity";

        [MenuItem(MENU_PATH + "Create Full Demo Scene (One-Click)", priority = 1)]
        public static void CreateFullDemoScene()
        {
            if (!EditorUtility.DisplayDialog(
                "Create Full Demo Scene",
                "This will create a COMPLETE demo scene with all UI components:\n\n" +
                "• Canvas with PageContainer and ModalContainer\n" +
                "• HomeContainerPage with SheetContainer\n" +
                "• Bottom Tab Bar with 4 functional buttons\n" +
                "• Swipe Detector for horizontal navigation\n" +
                "• EventSystem, NavigationManager, DemoBootstrap\n" +
                "• GameState singleton\n\n" +
                "Everything will be functional and ready to use!\n\n" +
                "Continue?",
                "Create",
                "Cancel"))
            {
                return;
            }

            Debug.Log("[DemoAutoSetup] Creating FULL demo scene with complete UI...");

            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // Remove default Main Camera (we'll add it later)
            var mainCamera = GameObject.Find("Main Camera");
            if (mainCamera != null)
            {
                Object.DestroyImmediate(mainCamera);
            }

            // Create EventSystem
            CreateEventSystem();

            // Create Canvas
            var canvas = CreateCanvas();

            // Create PageContainer
            var pageContainer = CreatePageContainer(canvas.transform);

            // Create ModalContainer
            var modalContainer = CreateModalContainer(canvas.transform);

            // Create NavigationManager
            var navigationManager = CreateNavigationManager(pageContainer, modalContainer);

            // Create DemoBootstrap
            var demoBootstrap = CreateDemoBootstrap(pageContainer, modalContainer);

            // Create GameState singleton holder
            CreateGameStateHolder();

            // Create Main Camera
            CreateUICamera();

            // Create HomeContainerPage template prefab
            Debug.Log("[DemoAutoSetup] Creating HomeContainerPage prefab template...");
            CreateHomeContainerPagePrefab();

            // Create all sheet prefabs with animations
            Debug.Log("[DemoAutoSetup] Creating sheet prefabs with animations...");
            CreateAllSheetPrefabs();

            // Save scene
            EditorSceneManager.SaveScene(scene, DEMO_SCENE_PATH);

            Debug.Log("[DemoAutoSetup] ✓ FULL demo scene created successfully!");
            Debug.Log("[DemoAutoSetup] Scene saved to: " + DEMO_SCENE_PATH);
            Debug.Log("[DemoAutoSetup] Prefabs created:");
            Debug.Log("  ✓ HomeContainer.prefab (with BottomTabBar + SheetSwipePager)");
            Debug.Log("  ✓ HomeSheet.prefab (with slide animation)");
            Debug.Log("  ✓ BattleSheet.prefab (with slide animation)");
            Debug.Log("  ✓ InventorySheet.prefab (with slide animation)");
            Debug.Log("  ✓ ShopSheet.prefab (with slide animation)");
            Debug.Log("[DemoAutoSetup] Press Play to test horizontal tab navigation!");

            EditorUtility.DisplayDialog(
                "Full Demo Scene Created!",
                "COMPLETE demo scene created successfully!\n\n" +
                "Scene: " + DEMO_SCENE_PATH + "\n\n" +
                "Prefabs created in Assets/Resources/:\n" +
                "✓ HomeContainer.prefab\n" +
                "✓ HomeSheet.prefab (with animations)\n" +
                "✓ BattleSheet.prefab (with animations)\n" +
                "✓ InventorySheet.prefab (with animations)\n" +
                "✓ ShopSheet.prefab (with animations)\n\n" +
                "Everything is ready!\n" +
                "Press Play to test swipe navigation!",
                "OK");
        }

        [MenuItem(MENU_PATH + "Setup Current Scene", priority = 2)]
        public static void SetupCurrentScene()
        {
            if (!EditorUtility.DisplayDialog(
                "Setup Current Scene",
                "This will add full demo components to the current scene.\n\n" +
                "Are you sure you want to continue?",
                "Setup",
                "Cancel"))
            {
                return;
            }

            Debug.Log("[DemoAutoSetup] Setting up current scene...");

            // Check for existing components
            var existingCanvas = Object.FindObjectOfType<Canvas>();
            var existingPageContainer = Object.FindObjectOfType<PageContainer>();
            var existingModalContainer = Object.FindObjectOfType<ModalContainer>();

            Canvas canvas;
            if (existingCanvas != null)
            {
                canvas = existingCanvas;
                Debug.Log("[DemoAutoSetup] Using existing Canvas");
            }
            else
            {
                canvas = CreateCanvas();
            }

            PageContainer pageContainer;
            if (existingPageContainer != null)
            {
                pageContainer = existingPageContainer;
                Debug.Log("[DemoAutoSetup] Using existing PageContainer");
            }
            else
            {
                pageContainer = CreatePageContainer(canvas.transform);
            }

            ModalContainer modalContainer;
            if (existingModalContainer != null)
            {
                modalContainer = existingModalContainer;
                Debug.Log("[DemoAutoSetup] Using existing ModalContainer");
            }
            else
            {
                modalContainer = CreateModalContainer(canvas.transform);
            }

            // Create/Update NavigationManager
            var navigationManager = Object.FindObjectOfType<NavigationManager>();
            if (navigationManager == null)
            {
                navigationManager = CreateNavigationManager(pageContainer, modalContainer);
            }
            else
            {
                Debug.Log("[DemoAutoSetup] Using existing NavigationManager");
            }

            // Create/Update DemoBootstrap
            var demoBootstrap = Object.FindObjectOfType<DemoBootstrap>();
            if (demoBootstrap == null)
            {
                demoBootstrap = CreateDemoBootstrap(pageContainer, modalContainer);
            }
            else
            {
                Debug.Log("[DemoAutoSetup] Using existing DemoBootstrap");
            }

            // Create EventSystem if needed
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                CreateEventSystem();
            }

            // Create GameState holder if needed
            if (GameObject.Find("GameState") == null)
            {
                CreateGameStateHolder();
            }

            Debug.Log("[DemoAutoSetup] Current scene setup complete!");

            EditorUtility.DisplayDialog(
                "Setup Complete!",
                "Current scene has been setup with full demo components.\n\n" +
                "Press Play to run the demo.",
                "OK");
        }

        [MenuItem(MENU_PATH + "Validate Setup", priority = 20)]
        public static void ValidateSetup()
        {
            Debug.Log("=== Enhanced UI Demo Setup Validation ===");

            var report = new System.Text.StringBuilder();
            bool isValid = true;

            // Check Canvas
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                report.AppendLine("✓ Canvas found");
            }
            else
            {
                report.AppendLine("✗ Canvas NOT found");
                isValid = false;
            }

            // Check PageContainer
            var pageContainer = Object.FindObjectOfType<PageContainer>();
            if (pageContainer != null)
            {
                report.AppendLine("✓ PageContainer found");
            }
            else
            {
                report.AppendLine("✗ PageContainer NOT found");
                isValid = false;
            }

            // Check ModalContainer
            var modalContainer = Object.FindObjectOfType<ModalContainer>();
            if (modalContainer != null)
            {
                report.AppendLine("✓ ModalContainer found");
            }
            else
            {
                report.AppendLine("✗ ModalContainer NOT found");
                isValid = false;
            }

            // Check NavigationManager
            var navigationManager = Object.FindObjectOfType<NavigationManager>();
            if (navigationManager != null)
            {
                report.AppendLine("✓ NavigationManager found");
            }
            else
            {
                report.AppendLine("✗ NavigationManager NOT found");
                isValid = false;
            }

            // Check DemoBootstrap
            var demoBootstrap = Object.FindObjectOfType<DemoBootstrap>();
            if (demoBootstrap != null)
            {
                report.AppendLine("✓ DemoBootstrap found");
            }
            else
            {
                report.AppendLine("✗ DemoBootstrap NOT found");
                isValid = false;
            }

            // Check EventSystem
            var eventSystem = Object.FindObjectOfType<EventSystem>();
            if (eventSystem != null)
            {
                report.AppendLine("✓ EventSystem found");
            }
            else
            {
                report.AppendLine("✗ EventSystem NOT found");
                isValid = false;
            }

            report.AppendLine();
            report.AppendLine(isValid ? "✓ Setup is VALID - Ready to run!" : "✗ Setup is INCOMPLETE - Run auto-setup");
            report.AppendLine("=========================================");

            Debug.Log(report.ToString());

            EditorUtility.DisplayDialog(
                "Setup Validation",
                report.ToString(),
                "OK");
        }

        #region Helper Methods - Core Components

        private static void CreateEventSystem()
        {
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            Debug.Log("[DemoAutoSetup] Created EventSystem");
        }

        private static Canvas CreateCanvas()
        {
            var canvasGO = new GameObject("Canvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var canvasScaler = canvasGO.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080, 1920); // Portrait mobile
            canvasScaler.matchWidthOrHeight = 0.5f;
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            canvasGO.AddComponent<GraphicRaycaster>();

            Debug.Log("[DemoAutoSetup] Created Canvas");
            return canvas;
        }

        private static PageContainer CreatePageContainer(Transform parent)
        {
            var pageContainerGO = new GameObject("PageContainer");
            pageContainerGO.transform.SetParent(parent, false);

            var rectTransform = pageContainerGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add CanvasGroup for interaction control
            pageContainerGO.AddComponent<CanvasGroup>();

            var pageContainer = pageContainerGO.AddComponent<PageContainer>();

            Debug.Log("[DemoAutoSetup] Created PageContainer");
            return pageContainer;
        }

        private static ModalContainer CreateModalContainer(Transform parent)
        {
            var modalContainerGO = new GameObject("ModalContainer");
            modalContainerGO.transform.SetParent(parent, false);

            var rectTransform = modalContainerGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add CanvasGroup for interaction control
            modalContainerGO.AddComponent<CanvasGroup>();

            var modalContainer = modalContainerGO.AddComponent<ModalContainer>();

            Debug.Log("[DemoAutoSetup] Created ModalContainer");
            return modalContainer;
        }

        private static NavigationManager CreateNavigationManager(PageContainer pageContainer, ModalContainer modalContainer)
        {
            var navigationManagerGO = new GameObject("NavigationManager");
            var navigationManager = navigationManagerGO.AddComponent<NavigationManager>();

            // Use reflection to set private serialized fields
            var pageContainerField = typeof(NavigationManager).GetField("pageContainer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var modalContainerField = typeof(NavigationManager).GetField("modalContainer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (pageContainerField != null)
            {
                pageContainerField.SetValue(navigationManager, pageContainer);
            }

            if (modalContainerField != null)
            {
                modalContainerField.SetValue(navigationManager, modalContainer);
            }

            EditorUtility.SetDirty(navigationManager);

            Debug.Log("[DemoAutoSetup] Created NavigationManager");
            return navigationManager;
        }

        private static DemoBootstrap CreateDemoBootstrap(PageContainer pageContainer, ModalContainer modalContainer)
        {
            var demoBootstrapGO = new GameObject("DemoBootstrap");
            var demoBootstrap = demoBootstrapGO.AddComponent<DemoBootstrap>();

            // Use reflection to set private serialized fields
            var pageContainerField = typeof(DemoBootstrap).GetField("pageContainer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var modalContainerField = typeof(DemoBootstrap).GetField("modalContainer",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (pageContainerField != null)
            {
                pageContainerField.SetValue(demoBootstrap, pageContainer);
            }

            if (modalContainerField != null)
            {
                modalContainerField.SetValue(demoBootstrap, modalContainer);
            }

            EditorUtility.SetDirty(demoBootstrap);

            Debug.Log("[DemoAutoSetup] Created DemoBootstrap");
            return demoBootstrap;
        }

        private static void CreateGameStateHolder()
        {
            var gameStateGO = new GameObject("GameState");

            // GameState will auto-initialize as a singleton
            // Just create an empty GameObject as a placeholder/marker

            Debug.Log("[DemoAutoSetup] Created GameState placeholder (singleton will auto-initialize)");
        }

        private static Camera CreateUICamera()
        {
            var cameraGO = new GameObject("Main Camera");
            var camera = cameraGO.AddComponent<Camera>();

            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            camera.orthographic = false;
            camera.fieldOfView = 60f;
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;

            cameraGO.tag = "MainCamera";
            cameraGO.AddComponent<AudioListener>();

            Debug.Log("[DemoAutoSetup] Created Main Camera");
            return camera;
        }

        #endregion

        [MenuItem(MENU_PATH + "Create HomeContainerPage Prefab", priority = 10)]
        public static void CreateHomeContainerPagePrefabMenu()
        {
            if (!EditorUtility.DisplayDialog(
                "Create HomeContainerPage Prefab",
                "This will create HomeContainerPage prefab with:\n\n" +
                "• SheetContainer for 4 tabs\n" +
                "• Bottom Tab Bar with 4 buttons\n" +
                "• Swipe Detector\n\n" +
                "Prefab will be saved to:\n" +
                "Assets/Resources/HomeContainer.prefab\n\n" +
                "Continue?",
                "Create",
                "Cancel"))
            {
                return;
            }

            CreateHomeContainerPagePrefab();

            EditorUtility.DisplayDialog(
                "Prefab Created!",
                "HomeContainerPage prefab created successfully!\n\n" +
                "Location: Assets/Resources/HomeContainer.prefab\n\n" +
                "You can now customize it in the Editor or press Play to test!",
                "OK");
        }

        [MenuItem(MENU_PATH + "Create Sheet Prefabs with Animations", priority = 11)]
        public static void CreateSheetPrefabsMenu()
        {
            if (!EditorUtility.DisplayDialog(
                "Create Sheet Prefabs",
                "This will create 4 sheet prefabs with horizontal slide animations:\n\n" +
                "• HomeSheet.prefab (Tab 0)\n" +
                "• BattleSheet.prefab (Tab 1)\n" +
                "• InventorySheet.prefab (Tab 2)\n" +
                "• ShopSheet.prefab (Tab 3)\n\n" +
                "Each prefab will have:\n" +
                "- TabSheetSlideAnimation for enter/exit\n" +
                "- Basic placeholder UI\n\n" +
                "Prefabs will be saved to:\n" +
                "Assets/Resources/\n\n" +
                "Continue?",
                "Create",
                "Cancel"))
            {
                return;
            }

            CreateAllSheetPrefabs();

            EditorUtility.DisplayDialog(
                "Sheet Prefabs Created!",
                "4 sheet prefabs created successfully!\n\n" +
                "Locations:\n" +
                "- Assets/Resources/HomeSheet.prefab\n" +
                "- Assets/Resources/BattleSheet.prefab\n" +
                "- Assets/Resources/InventorySheet.prefab\n" +
                "- Assets/Resources/ShopSheet.prefab\n\n" +
                "Each prefab has horizontal slide animations configured!\n" +
                "Press Play to test swipe navigation!",
                "OK");
        }

        [MenuItem(MENU_PATH + "Create ALL Prefabs (Full Setup)", priority = 15)]
        public static void CreateAllPrefabsMenu()
        {
            if (!EditorUtility.DisplayDialog(
                "Create ALL Prefabs",
                "This will create:\n\n" +
                "1. HomeContainerPage prefab\n" +
                "2. All 4 sheet prefabs with animations\n\n" +
                "Everything you need for horizontal tab navigation!\n\n" +
                "Continue?",
                "Create All",
                "Cancel"))
            {
                return;
            }

            Debug.Log("[DemoAutoSetup] Creating ALL prefabs...");

            CreateHomeContainerPagePrefab();
            CreateAllSheetPrefabs();

            EditorUtility.DisplayDialog(
                "All Prefabs Created!",
                "Complete prefab setup finished!\n\n" +
                "Created:\n" +
                "✓ HomeContainer.prefab\n" +
                "✓ HomeSheet.prefab\n" +
                "✓ BattleSheet.prefab\n" +
                "✓ InventorySheet.prefab\n" +
                "✓ ShopSheet.prefab\n\n" +
                "All prefabs have horizontal slide animations!\n" +
                "Press Play to test!",
                "OK");
        }

        [MenuItem(MENU_PATH + "Help: How to Configure Animations Manually", priority = 100)]
        public static void ShowAnimationSetupGuide()
        {
            EditorUtility.DisplayDialog(
                "How to Configure Sheet Animations Manually",
                "To manually configure slide animations on a Sheet prefab:\n\n" +
                "1. Select your Sheet prefab (e.g., HomeSheet.prefab)\n\n" +
                "2. Add TabSheetSlideAnimation components:\n" +
                "   - Add one for ENTER (set 'Is Enter Animation' = true)\n" +
                "   - Add one for EXIT (set 'Is Enter Animation' = false)\n\n" +
                "3. Configure Push Enter Animation Container:\n" +
                "   - Expand 'Push Enter Animation Container'\n" +
                "   - Click '+' to add entry\n" +
                "   - Leave 'Partner Screen Identifier' empty (default)\n" +
                "   - Drag ENTER TabSheetSlideAnimation to 'Animation Behaviour'\n\n" +
                "4. Configure Push Exit Animation Container:\n" +
                "   - Expand 'Push Exit Animation Container'\n" +
                "   - Click '+' to add entry\n" +
                "   - Leave 'Partner Screen Identifier' empty\n" +
                "   - Drag EXIT TabSheetSlideAnimation to 'Animation Behaviour'\n\n" +
                "5. Save the prefab\n\n" +
                "Tip: Use the auto-setup tools to generate prefabs with animations already configured!",
                "Got it!");
        }

        [MenuItem(MENU_PATH + "Debug: Verify Sheet Animation Setup", priority = 101)]
        public static void VerifySheetAnimationSetup()
        {
            var selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                EditorUtility.DisplayDialog(
                    "No Selection",
                    "Please select a Sheet prefab or GameObject in the hierarchy to verify its animation setup.",
                    "OK");
                return;
            }

            var sheet = selectedObject.GetComponent<Sheet>();
            if (sheet == null)
            {
                EditorUtility.DisplayDialog(
                    "Not a Sheet",
                    $"{selectedObject.name} does not have a Sheet component.\n\n" +
                    "Please select a Sheet prefab (HomeSheet, BattleSheet, etc.)",
                    "OK");
                return;
            }

            // Use SerializedObject to check animation setup
            var serializedSheet = new SerializedObject(sheet);
            var pushEnterContainer = serializedSheet.FindProperty("pushEnterAnimationContainer");
            var pushExitContainer = serializedSheet.FindProperty("pushExitAnimationContainer");

            string report = $"=== Animation Setup for {selectedObject.name} ===\n\n";

            // Check TabSheetSlideAnimation components
            var animations = selectedObject.GetComponents<TabSheetSlideAnimation>();
            report += $"TabSheetSlideAnimation components: {animations.Length}\n";
            foreach (var anim in animations)
            {
                var so = new SerializedObject(anim);
                bool isEnter = so.FindProperty("isEnterAnimation").boolValue;
                float duration = so.FindProperty("duration").floatValue;
                report += $"  - {(isEnter ? "ENTER" : "EXIT")} animation (duration: {duration}s)\n";
            }
            report += "\n";

            // Check push enter container
            var enterEntries = pushEnterContainer.FindPropertyRelative("entries");
            report += $"Push Enter Animation Container:\n";
            report += $"  Entries: {enterEntries.arraySize}\n";
            if (enterEntries.arraySize > 0)
            {
                for (int i = 0; i < enterEntries.arraySize; i++)
                {
                    var entry = enterEntries.GetArrayElementAtIndex(i);
                    var animBehaviour = entry.FindPropertyRelative("animationBehaviour").objectReferenceValue;
                    report += $"  [{i}] Animation Behaviour: {(animBehaviour != null ? animBehaviour.name : "NULL")}\n";
                }
            }
            report += "\n";

            // Check push exit container
            var exitEntries = pushExitContainer.FindPropertyRelative("entries");
            report += $"Push Exit Animation Container:\n";
            report += $"  Entries: {exitEntries.arraySize}\n";
            if (exitEntries.arraySize > 0)
            {
                for (int i = 0; i < exitEntries.arraySize; i++)
                {
                    var entry = exitEntries.GetArrayElementAtIndex(i);
                    var animBehaviour = entry.FindPropertyRelative("animationBehaviour").objectReferenceValue;
                    report += $"  [{i}] Animation Behaviour: {(animBehaviour != null ? animBehaviour.name : "NULL")}\n";
                }
            }

            // Check if setup is valid
            bool isValid = animations.Length >= 2 &&
                          enterEntries.arraySize > 0 &&
                          exitEntries.arraySize > 0;

            report += "\n";
            report += $"Status: {(isValid ? "✓ VALID" : "✗ INVALID - Missing animations or container entries")}";

            Debug.Log(report);

            EditorUtility.DisplayDialog(
                "Animation Setup Verification",
                report,
                "OK");
        }

        #region Helper Methods - Tab Navigation UI (NEW)

        /// <summary>
        /// Create SheetContainer GameObject with proper setup
        /// </summary>
        private static SheetContainer CreateSheetContainer(Transform parent)
        {
            var sheetContainerGO = new GameObject("SheetContainer");
            sheetContainerGO.transform.SetParent(parent, false);

            var rectTransform = sheetContainerGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = new Vector2(0, -100); // Leave space for bottom tab bar
            rectTransform.anchoredPosition = new Vector2(0, 50); // Center vertically with space below

            // Add CanvasGroup for interaction control
            sheetContainerGO.AddComponent<CanvasGroup>();

            var sheetContainer = sheetContainerGO.AddComponent<SheetContainer>();

            Debug.Log("[DemoAutoSetup] Created SheetContainer");
            return sheetContainer;
        }

        /// <summary>
        /// Create Bottom Tab Bar with 4 functional buttons
        /// </summary>
        private static BottomTabBar CreateBottomTabBar(Transform parent, SheetContainer sheetContainer)
        {
            // Create main BottomTabBar GameObject
            var tabBarGO = new GameObject("BottomTabBar");
            tabBarGO.transform.SetParent(parent, false);

            var tabBarRT = tabBarGO.AddComponent<RectTransform>();
            tabBarRT.anchorMin = new Vector2(0, 0);
            tabBarRT.anchorMax = new Vector2(1, 0);
            tabBarRT.pivot = new Vector2(0.5f, 0);
            tabBarRT.anchoredPosition = Vector2.zero;
            tabBarRT.sizeDelta = new Vector2(0, 100); // 100px height

            // Add background
            var bgImage = tabBarGO.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Dark gray background

            // Add HorizontalLayoutGroup for auto-layout
            var layoutGroup = tabBarGO.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 10f;
            layoutGroup.padding = new RectOffset(20, 20, 10, 10);
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;

            // Create 4 buttons
            var homeButton = CreateTabButton(tabBarGO.transform, "Home", new Color(0.3f, 0.5f, 1f)); // Blue
            var battleButton = CreateTabButton(tabBarGO.transform, "Battle", new Color(1f, 0.3f, 0.3f)); // Red
            var inventoryButton = CreateTabButton(tabBarGO.transform, "Inventory", new Color(0.3f, 1f, 0.5f)); // Green
            var shopButton = CreateTabButton(tabBarGO.transform, "Shop", new Color(1f, 0.8f, 0.2f)); // Yellow

            // Add BottomTabBar component
            var bottomTabBar = tabBarGO.AddComponent<BottomTabBar>();

            // Use reflection to set private serialized fields
            SetPrivateField(bottomTabBar, "sheetContainer", sheetContainer);
            SetPrivateField(bottomTabBar, "homeButton", homeButton);
            SetPrivateField(bottomTabBar, "battleButton", battleButton);
            SetPrivateField(bottomTabBar, "inventoryButton", inventoryButton);
            SetPrivateField(bottomTabBar, "shopButton", shopButton);
            SetPrivateField(bottomTabBar, "updateVisualStates", true);

            EditorUtility.SetDirty(bottomTabBar);

            Debug.Log("[DemoAutoSetup] Created BottomTabBar with 4 buttons");
            return bottomTabBar;
        }

        /// <summary>
        /// Create a single tab button with text label
        /// </summary>
        private static Button CreateTabButton(Transform parent, string label, Color color)
        {
            var buttonGO = new GameObject(label + "Button");
            buttonGO.transform.SetParent(parent, false);

            var buttonRT = buttonGO.AddComponent<RectTransform>();
            buttonRT.sizeDelta = new Vector2(200, 80);

            // Add Image for background
            var buttonImage = buttonGO.AddComponent<Image>();
            buttonImage.color = color;
            buttonImage.type = Image.Type.Sliced;

            // Add Button component
            var button = buttonGO.AddComponent<Button>();
            button.targetGraphic = buttonImage;

            // Create Text child
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);

            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.sizeDelta = Vector2.zero;
            textRT.anchoredPosition = Vector2.zero;

            var textComponent = textGO.AddComponent<TextMeshProUGUI>();
            textComponent.text = label;
            textComponent.fontSize = 24;
            textComponent.color = Color.white;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.fontStyle = FontStyles.Bold;

            Debug.Log($"[DemoAutoSetup] Created {label} button");
            return button;
        }

        /// <summary>
        /// Create SheetSwipePager for drag-to-follow + snap-to-page tab
        /// navigation. The pager is parented to the SheetContainer's
        /// GameObject so its (auto-added) transparent Image covers the
        /// swipe area exactly.
        /// </summary>
        private static SheetSwipePager CreateSwipePager(SheetContainer sheetContainer)
        {
            // Place the pager on the SheetContainer itself: it adds a
            // transparent Image so the EventSystem can raycast drag events.
            var pager = sheetContainer.gameObject.AddComponent<SheetSwipePager>();

            SetPrivateField(pager, "sheetContainer", sheetContainer);

            // Default tab strip — keep in sync with HomeContainerPage.
            var tabSheetIds = new System.Collections.Generic.List<string>
            {
                ScreenKeys.HomeSheet,
                ScreenKeys.BattleSheet,
                ScreenKeys.InventorySheet,
                ScreenKeys.ShopSheet
            };
            SetPrivateField(pager, "tabSheetIds", tabSheetIds);

            // Sensible defaults for portrait mobile lobby (matches doc).
            SetPrivateField(pager, "followRatio", 1f);
            SetPrivateField(pager, "edgeResistance", 0.4f);
            SetPrivateField(pager, "axisLockThreshold", 10f);
            SetPrivateField(pager, "positionThreshold", 0.25f);
            SetPrivateField(pager, "velocityThreshold", 800f);
            SetPrivateField(pager, "snapDuration", 0.3f);
            SetPrivateField(pager, "snapEase", EnhancedUI.Transition.EaseType.QuarticEaseOut);
            SetPrivateField(pager, "allowFlickSkip", false);
            SetPrivateField(pager, "disableDuringFrameworkTransition", true);
            SetPrivateField(pager, "wrapAround", false);

            EditorUtility.SetDirty(pager);

            Debug.Log("[DemoAutoSetup] Created SheetSwipePager on SheetContainer");
            return pager;
        }

        /// <summary>
        /// Helper to set private serialized field via reflection
        /// </summary>
        private static void SetPrivateField(object obj, string fieldName, object value)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);

            if (field != null)
            {
                field.SetValue(obj, value);
            }
            else
            {
                Debug.LogWarning($"[DemoAutoSetup] Field '{fieldName}' not found on {obj.GetType().Name}");
            }
        }

        /// <summary>
        /// Helper to get private serialized field via reflection
        /// </summary>
        private static object GetPrivateField(object obj, string fieldName)
        {
            var field = obj.GetType().GetField(fieldName,
                System.Reflection.BindingFlags.NonPublic |
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);

            if (field != null)
            {
                return field.GetValue(obj);
            }
            else
            {
                Debug.LogWarning($"[DemoAutoSetup] Field '{fieldName}' not found on {obj.GetType().Name}");
                return null;
            }
        }

        /// <summary>
        /// Configure sheet animations using SerializedObject (proper Editor way)
        /// This ensures the changes are properly serialized to the prefab
        /// </summary>
        private static void ConfigureSheetAnimations(Sheet sheetComponent, TabSheetSlideAnimation enterAnim, TabSheetSlideAnimation exitAnim)
        {
            // Use SerializedObject to modify private serialized fields
            var serializedSheet = new SerializedObject(sheetComponent);

            // Find the animation container properties
            var pushEnterContainerProp = serializedSheet.FindProperty("pushEnterAnimationContainer");
            var pushExitContainerProp = serializedSheet.FindProperty("pushExitAnimationContainer");

            if (pushEnterContainerProp != null && pushExitContainerProp != null)
            {
                // Add entry for enter animation
                var enterEntriesProp = pushEnterContainerProp.FindPropertyRelative("entries");
                if (enterEntriesProp != null && enterEntriesProp.isArray)
                {
                    enterEntriesProp.ClearArray();
                    enterEntriesProp.InsertArrayElementAtIndex(0);
                    var enterEntryProp = enterEntriesProp.GetArrayElementAtIndex(0);

                    enterEntryProp.FindPropertyRelative("partnerScreenIdentifier").stringValue = "";
                    enterEntryProp.FindPropertyRelative("animationObject").objectReferenceValue = null;
                    enterEntryProp.FindPropertyRelative("animationBehaviour").objectReferenceValue = enterAnim;
                    enterEntryProp.FindPropertyRelative("isRegex").boolValue = false;

                    Debug.Log($"[DemoAutoSetup] ✓ Configured push enter animation: {enterAnim.name}");
                }

                // Add entry for exit animation
                var exitEntriesProp = pushExitContainerProp.FindPropertyRelative("entries");
                if (exitEntriesProp != null && exitEntriesProp.isArray)
                {
                    exitEntriesProp.ClearArray();
                    exitEntriesProp.InsertArrayElementAtIndex(0);
                    var exitEntryProp = exitEntriesProp.GetArrayElementAtIndex(0);

                    exitEntryProp.FindPropertyRelative("partnerScreenIdentifier").stringValue = "";
                    exitEntryProp.FindPropertyRelative("animationObject").objectReferenceValue = null;
                    exitEntryProp.FindPropertyRelative("animationBehaviour").objectReferenceValue = exitAnim;
                    exitEntryProp.FindPropertyRelative("isRegex").boolValue = false;

                    Debug.Log($"[DemoAutoSetup] ✓ Configured push exit animation: {exitAnim.name}");
                }

                // Apply changes
                serializedSheet.ApplyModifiedProperties();
            }
            else
            {
                Debug.LogWarning("[DemoAutoSetup] Could not find animation container properties!");
            }
        }

        /// <summary>
        /// Create all 4 sheet prefabs with animations
        /// </summary>
        private static void CreateAllSheetPrefabs()
        {
            Debug.Log("[DemoAutoSetup] Creating all sheet prefabs with animations...");

            // Ensure Resources folder exists
            string resourcesPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                Debug.Log("[DemoAutoSetup] Created Resources folder");
            }

            // Create each sheet prefab
            CreateSheetPrefab("HomeSheet", 0, new Color(0.3f, 0.5f, 1f, 1f)); // Blue
            CreateSheetPrefab("BattleSheet", 1, new Color(1f, 0.3f, 0.3f, 1f)); // Red
            CreateSheetPrefab("InventorySheet", 2, new Color(0.3f, 1f, 0.5f, 1f)); // Green
            CreateSheetPrefab("ShopSheet", 3, new Color(1f, 0.8f, 0.2f, 1f)); // Yellow

            Debug.Log("[DemoAutoSetup] ✓ All sheet prefabs created with animations!");
        }

        /// <summary>
        /// Create a single sheet prefab with animations
        /// </summary>
        private static void CreateSheetPrefab(string sheetName, int tabIndex, Color backgroundColor)
        {
            Debug.Log($"[DemoAutoSetup] Creating {sheetName} prefab...");

            // Create GameObject
            var sheetGO = new GameObject(sheetName);

            // Add RectTransform (full-screen)
            var rectTransform = sheetGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add CanvasGroup
            var canvasGroup = sheetGO.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            // Add background image
            var bgImage = sheetGO.AddComponent<Image>();
            bgImage.color = backgroundColor;

            // Add Sheet component based on name
            Sheet sheetComponent = null;
            switch (sheetName)
            {
                case "HomeSheet":
                    sheetComponent = sheetGO.AddComponent<HomeSheet>();
                    break;
                case "BattleSheet":
                    sheetComponent = sheetGO.AddComponent<BattleSheet>();
                    break;
                case "InventorySheet":
                    sheetComponent = sheetGO.AddComponent<InventorySheet>();
                    break;
                case "ShopSheet":
                    sheetComponent = sheetGO.AddComponent<ShopSheet>();
                    break;
            }

            if (sheetComponent == null)
            {
                Debug.LogError($"[DemoAutoSetup] Failed to add Sheet component for {sheetName}");
                Object.DestroyImmediate(sheetGO);
                return;
            }

            // Set identifier
            SetPrivateField(sheetComponent, "_identifier", sheetName);

            // Add Enter Animation
            var enterAnim = sheetGO.AddComponent<TabSheetSlideAnimation>();
            SetPrivateField(enterAnim, "isEnterAnimation", true);
            SetPrivateField(enterAnim, "duration", 0.3f);
            SetPrivateField(enterAnim, "easeType", EnhancedUI.Transition.EaseType.QuadraticEaseOut);

            // Add Exit Animation
            var exitAnim = sheetGO.AddComponent<TabSheetSlideAnimation>();
            SetPrivateField(exitAnim, "isEnterAnimation", false);
            SetPrivateField(exitAnim, "duration", 0.3f);
            SetPrivateField(exitAnim, "easeType", EnhancedUI.Transition.EaseType.QuadraticEaseOut);

            // Configure animation containers using SerializedObject (proper way for Editor)
            ConfigureSheetAnimations(sheetComponent, enterAnim, exitAnim);

            // Create placeholder UI (Title text)
            CreateSheetPlaceholderUI(sheetGO.transform, sheetName);

            EditorUtility.SetDirty(sheetComponent);

            // Save as prefab
            string prefabPath = $"Assets/Resources/{sheetName}.prefab";

            // Delete existing prefab if it exists
            if (System.IO.File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
            }

            // Create new prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(sheetGO, prefabPath);

            if (prefab != null)
            {
                Debug.Log($"[DemoAutoSetup] ✓ Created {sheetName}.prefab with animations at: {prefabPath}");
            }
            else
            {
                Debug.LogError($"[DemoAutoSetup] Failed to create {sheetName}.prefab!");
            }

            // Destroy temporary GameObject
            Object.DestroyImmediate(sheetGO);
        }

        /// <summary>
        /// Create placeholder UI for sheet (title text)
        /// </summary>
        private static void CreateSheetPlaceholderUI(Transform parent, string sheetName)
        {
            // Create title text
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(parent, false);

            var titleRT = titleGO.AddComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0.5f, 0.5f);
            titleRT.anchorMax = new Vector2(0.5f, 0.5f);
            titleRT.pivot = new Vector2(0.5f, 0.5f);
            titleRT.anchoredPosition = Vector2.zero;
            titleRT.sizeDelta = new Vector2(600, 100);

            var titleText = titleGO.AddComponent<TextMeshProUGUI>();
            titleText.text = sheetName.Replace("Sheet", " Tab");
            titleText.fontSize = 48;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Add shadow for better visibility
            var shadow = titleGO.AddComponent<UnityEngine.UI.Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.5f);
            shadow.effectDistance = new Vector2(3, -3);
        }

        /// <summary>
        /// Create complete HomeContainerPage prefab with full UI hierarchy
        /// </summary>
        private static void CreateHomeContainerPagePrefab()
        {
            // Create temporary GameObject for prefab
            var homeContainerGO = new GameObject("HomeContainer");

            // Add RectTransform (required for UI)
            var rectTransform = homeContainerGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add CanvasGroup
            homeContainerGO.AddComponent<CanvasGroup>();

            // Add HomeContainerPage component
            var homeContainerPage = homeContainerGO.AddComponent<HomeContainerPage>();

            // Create SheetContainer child
            var sheetContainer = CreateSheetContainer(homeContainerGO.transform);

            // Create SheetSwipePager (attaches to SheetContainer GO and
            // adds a transparent Image as raycast target).
            var swipePager = CreateSwipePager(sheetContainer);

            // Create BottomTabBar with buttons, wired to the pager so
            // click navigation matches swipe visuals.
            var bottomTabBar = CreateBottomTabBar(homeContainerGO.transform, sheetContainer);
            SetPrivateField(bottomTabBar, "swipePager", swipePager);
            EditorUtility.SetDirty(bottomTabBar);

            // Use reflection to set tabContainer reference on HomeContainerPage
            SetPrivateField(homeContainerPage, "tabContainer", sheetContainer);
            SetPrivateField(homeContainerPage, "initialTabSheetId", ScreenKeys.HomeSheet);
            SetPrivateField(homeContainerPage, "playInitialAnimation", false);

            EditorUtility.SetDirty(homeContainerPage);

            // Ensure Resources folder exists
            string resourcesPath = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
                Debug.Log("[DemoAutoSetup] Created Resources folder");
            }

            // Save as prefab
            string prefabPath = resourcesPath + "/HomeContainer.prefab";

            // Delete existing prefab if it exists
            if (System.IO.File.Exists(prefabPath))
            {
                AssetDatabase.DeleteAsset(prefabPath);
                Debug.Log("[DemoAutoSetup] Deleted existing HomeContainer.prefab");
            }

            // Create new prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(homeContainerGO, prefabPath);

            if (prefab != null)
            {
                Debug.Log($"[DemoAutoSetup] ✓ Created HomeContainer.prefab at: {prefabPath}");
                Debug.Log("[DemoAutoSetup] Prefab contains:");
                Debug.Log("  - HomeContainerPage component");
                Debug.Log("  - SheetContainer for 4 tabs");
                Debug.Log("  - BottomTabBar with 4 buttons (Home, Battle, Inventory, Shop)");
                Debug.Log("  - SheetSwipePager (drag-to-follow + snap) on the SheetContainer");
            }
            else
            {
                Debug.LogError("[DemoAutoSetup] Failed to create HomeContainer.prefab!");
            }

            // Destroy temporary GameObject
            Object.DestroyImmediate(homeContainerGO);

            // Refresh AssetDatabase
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the prefab in Project window
            Selection.activeObject = prefab;
            EditorGUIUtility.PingObject(prefab);
        }

        #endregion
    }
}
#endif
