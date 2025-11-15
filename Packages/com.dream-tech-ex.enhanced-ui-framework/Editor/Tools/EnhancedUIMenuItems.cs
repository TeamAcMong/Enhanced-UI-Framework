using UnityEngine;
using UnityEditor;
using EnhancedUI.Editor.Utilities;

namespace EnhancedUI.Editor.Tools
{
    /// <summary>
    /// Centralized menu items for Enhanced UI Framework
    /// </summary>
    public static class EnhancedUIMenuItems
    {
        // Menu priority constants
        private const int PRIORITY_MAIN = 0;
        private const int PRIORITY_SEPARATOR_1 = 50;
        private const int PRIORITY_DEBUG = 100;
        private const int PRIORITY_ANALYSIS = 200;
        private const int PRIORITY_SEPARATOR_2 = 300;
        private const int PRIORITY_HELP = 400;

        // GameObject menu priorities (after Unity's UI elements)
        private const int GAMEOBJECT_PRIORITY = 10;

        // Main Tools Menu Items are already defined in their respective classes:
        // - EnhancedUIControlCenter (priority 0)
        // - SetupWizard (defined in SetupWizard.cs)
        // - ScreenGenerator (defined in ScreenGenerator.cs)

        #region GameObject Menu - Create Containers

        [MenuItem("GameObject/Enhanced UI/Page Container", false, GAMEOBJECT_PRIORITY)]
        public static void CreatePageContainer(MenuCommand menuCommand)
        {
            CreateContainer<PageContainer>("PageContainer", menuCommand);
        }

        [MenuItem("GameObject/Enhanced UI/Modal Container", false, GAMEOBJECT_PRIORITY + 1)]
        public static void CreateModalContainer(MenuCommand menuCommand)
        {
            CreateContainer<ModalContainer>("ModalContainer", menuCommand);
        }

        [MenuItem("GameObject/Enhanced UI/Sheet Container", false, GAMEOBJECT_PRIORITY + 2)]
        public static void CreateSheetContainer(MenuCommand menuCommand)
        {
            CreateContainer<SheetContainer>("SheetContainer", menuCommand);
        }

        private static void CreateContainer<T>(string name, MenuCommand menuCommand) where T : MonoBehaviour, IUIContainer
        {
            // Get or create Canvas
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                // Create Canvas
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                // Create EventSystem
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(canvasGO, $"Create {name}");
                Undo.RegisterCreatedObjectUndo(eventSystemGO, $"Create {name}");
            }

            // Check if container already exists
            var existing = Object.FindObjectOfType<T>();
            if (existing != null)
            {
                EditorUtility.DisplayDialog(
                    "Container Already Exists",
                    $"A {name} already exists in the scene. Only one instance of each container type is allowed.",
                    "OK"
                );
                Selection.activeGameObject = (existing as MonoBehaviour).gameObject;
                return;
            }

            // Create container GameObject
            var go = new GameObject(name);
            go.transform.SetParent(canvas.transform, false);

            // Setup RectTransform to fill parent
            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add CanvasGroup for interaction control
            go.AddComponent<CanvasGroup>();

            // Add container component
            go.AddComponent<T>();

            // Register undo
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");

            // Select the created object
            Selection.activeGameObject = go;

            Debug.Log($"[Enhanced UI] Created {name} in the scene");
        }

        #endregion

        #region Tools Menu - Quick Setup

        [MenuItem("Tools/Enhanced UI/Setup All Containers", false, PRIORITY_MAIN + 10)]
        public static void SetupAllContainers()
        {
            // Get or create Canvas
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                // Create EventSystem
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

                Undo.RegisterCreatedObjectUndo(canvasGO, "Setup All Containers");
                Undo.RegisterCreatedObjectUndo(eventSystemGO, "Setup All Containers");

                Debug.Log("[Enhanced UI] Created Canvas and EventSystem");
            }

            int createdCount = 0;

            // Create PageContainer if it doesn't exist
            if (Object.FindObjectOfType<PageContainer>() == null)
            {
                CreateContainerForSetup<PageContainer>("PageContainer", canvas.transform);
                createdCount++;
            }

            // Create ModalContainer if it doesn't exist
            if (Object.FindObjectOfType<ModalContainer>() == null)
            {
                CreateContainerForSetup<ModalContainer>("ModalContainer", canvas.transform);
                createdCount++;
            }

            // Create SheetContainer if it doesn't exist
            if (Object.FindObjectOfType<SheetContainer>() == null)
            {
                CreateContainerForSetup<SheetContainer>("SheetContainer", canvas.transform);
                createdCount++;
            }

            if (createdCount > 0)
            {
                EditorUtility.DisplayDialog(
                    "Setup Complete",
                    $"Successfully created {createdCount} container(s) in the scene.\n\n" +
                    "All containers are ready to use!",
                    "OK"
                );
                Debug.Log($"[Enhanced UI] Setup complete. Created {createdCount} container(s)");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Setup Complete",
                    "All containers already exist in the scene.\n\nNo changes were made.",
                    "OK"
                );
            }
        }

        private static void CreateContainerForSetup<T>(string name, Transform parent) where T : MonoBehaviour, IUIContainer
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            // Setup RectTransform to fill parent
            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;

            // Add CanvasGroup for interaction control
            go.AddComponent<CanvasGroup>();

            // Add container component
            go.AddComponent<T>();

            // Register undo
            Undo.RegisterCreatedObjectUndo(go, "Setup All Containers");

            Debug.Log($"[Enhanced UI] Created {name}");
        }

        #endregion

        // Documentation
        [MenuItem("Tools/Enhanced UI/Documentation", false, PRIORITY_HELP)]
        public static void OpenDocumentation()
        {
            Application.OpenURL("https://github.com/yourstudio/enhanced-ui-framework");
        }

        // About
        [MenuItem("Tools/Enhanced UI/About", false, PRIORITY_HELP + 1)]
        public static void ShowAbout()
        {
            AboutWindow.ShowWindow();
        }

        // Settings shortcut
        [MenuItem("Tools/Enhanced UI/Settings", false, PRIORITY_HELP + 2)]
        public static void OpenSettings()
        {
            Selection.activeObject = EnhancedUISettings.Instance;
            EditorGUIUtility.PingObject(EnhancedUISettings.Instance);
        }
    }

    /// <summary>
    /// About window showing version and credits
    /// </summary>
    public class AboutWindow : EditorWindow
    {
        private const string VERSION = "2.0.0";
        private const string RELEASE_DATE = "2025";

        public static void ShowWindow()
        {
            var window = GetWindow<AboutWindow>(true, "About Enhanced UI Framework", true);
            window.minSize = new Vector2(400, 350);
            window.maxSize = new Vector2(400, 350);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(20);

            // Logo/Title
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField("Enhanced UI Framework", new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 20,
                    alignment = TextAnchor.MiddleCenter
                });

                GUILayout.Space(5);

                EditorGUILayout.LabelField($"Version {VERSION}", new GUIStyle(EditorStyles.label)
                {
                    alignment = TextAnchor.MiddleCenter
                });

                GUILayout.Space(10);

                EditorGUILayout.LabelField("Advanced UI Navigation System for Unity", new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    wordWrap = true
                });

                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(20);

            // Info box
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.LabelField("Features:", EditorStyles.boldLabel);

                DrawFeature("✓ Page, Modal & Sheet Navigation");
                DrawFeature("✓ Advanced Transition System");
                DrawFeature("✓ Asset Management (Resources, Addressables, Remote)");
                DrawFeature("✓ Object Pooling & Caching");
                DrawFeature("✓ Mobile Optimizations (SafeArea, BackButton)");
                DrawFeature("✓ MVP Pattern Support");
                DrawFeature("✓ Comprehensive Debug Tools");
                DrawFeature("✓ Performance Analysis");

                GUILayout.Space(10);

                EditorGUILayout.LabelField("Components:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"• {GetFileCount()} Total Files", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"• ~11,100+ Lines of Code", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"• Built for Unity 2021.3+", EditorStyles.miniLabel);
            }

            GUILayout.Space(10);

            // Credits
            using (new EditorGUILayout.VerticalScope(EnhancedUIEditorStyles.SectionBox))
            {
                EditorGUILayout.LabelField("Credits:", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Inspired by UnityScreenNavigator", EditorStyles.miniLabel);
                EditorGUILayout.LabelField($"© {RELEASE_DATE} YourStudio", EditorStyles.miniLabel);
            }

            GUILayout.Space(10);

            // Buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Documentation", GUILayout.Height(30)))
                {
                    Application.OpenURL("https://github.com/yourstudio/enhanced-ui-framework");
                }

                if (GUILayout.Button("Control Center", GUILayout.Height(30)))
                {
                    EnhancedUIControlCenter.ShowWindow();
                    Close();
                }
            }

            GUILayout.Space(10);

            // Footer
            EditorGUILayout.LabelField("Thank you for using Enhanced UI Framework!", new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                fontStyle = FontStyle.Italic
            });
        }

        private void DrawFeature(string feature)
        {
            EditorGUILayout.LabelField(feature, EditorStyles.miniLabel);
        }

        private int GetFileCount()
        {
            // Runtime: 49 + Editor: 4 (old) + New Editor: ~15 = ~68 files
            return 68;
        }
    }
}
