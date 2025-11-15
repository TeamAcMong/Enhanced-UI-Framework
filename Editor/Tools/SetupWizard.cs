using UnityEngine;
using UnityEditor;

namespace EnhancedUI.Editor.Tools
{
    /// <summary>
    /// Setup wizard for initial Enhanced UI Framework configuration
    /// </summary>
    public class SetupWizard : EditorWindow
    {
        private string _settingsPath = "Assets/Resources/EnhancedUISettings.asset";
        private bool _createSampleContainers = true;
        private bool _setupBackButton = true;
        private bool _setupSafeArea = true;

        [MenuItem("Tools/Enhanced UI/Setup Wizard")]
        public static void ShowWindow()
        {
            var window = GetWindow<SetupWizard>("Enhanced UI Setup");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Enhanced UI Framework Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This wizard will help you set up the Enhanced UI Framework in your project.",
                MessageType.Info);

            GUILayout.Space(10);

            // Settings
            GUILayout.Label("Settings", EditorStyles.boldLabel);
            _settingsPath = EditorGUILayout.TextField("Settings Path", _settingsPath);

            GUILayout.Space(10);

            // Options
            GUILayout.Label("Setup Options", EditorStyles.boldLabel);
            _createSampleContainers = EditorGUILayout.Toggle("Create Sample Containers", _createSampleContainers);
            _setupBackButton = EditorGUILayout.Toggle("Setup Back Button Handler", _setupBackButton);
            _setupSafeArea = EditorGUILayout.Toggle("Enable Safe Area", _setupSafeArea);

            GUILayout.Space(20);

            // Buttons
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Setup", GUILayout.Height(30)))
                {
                    PerformSetup();
                }

                if (GUILayout.Button("Cancel", GUILayout.Height(30)))
                {
                    Close();
                }
            }
        }

        private void PerformSetup()
        {
            // Create settings
            CreateSettings();

            // Create containers if requested
            if (_createSampleContainers)
            {
                CreateSampleContainers();
            }

            // Setup back button
            if (_setupBackButton)
            {
                SetupBackButtonHandler();
            }

            EditorUtility.DisplayDialog("Setup Complete",
                "Enhanced UI Framework has been set up successfully!",
                "OK");

            Close();
        }

        private void CreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<EnhancedUISettings>(_settingsPath);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<EnhancedUISettings>();

                // Ensure directory exists
                var directory = System.IO.Path.GetDirectoryName(_settingsPath);
                if (!AssetDatabase.IsValidFolder(directory))
                {
                    var folders = directory.Split('/');
                    var currentPath = folders[0];
                    for (int i = 1; i < folders.Length; i++)
                    {
                        var newPath = currentPath + "/" + folders[i];
                        if (!AssetDatabase.IsValidFolder(newPath))
                        {
                            AssetDatabase.CreateFolder(currentPath, folders[i]);
                        }
                        currentPath = newPath;
                    }
                }

                AssetDatabase.CreateAsset(settings, _settingsPath);

                // Apply initial settings
                settings.enableSafeArea = _setupSafeArea;
                settings.enableBackButton = _setupBackButton;

                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();

                Debug.Log($"Created Enhanced UI Settings at: {_settingsPath}");
            }
        }

        private void CreateSampleContainers()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

                Debug.Log("Created Canvas");
            }

            // Create Page Container
            CreateContainer<PageContainer>("PageContainer", canvas.transform);

            // Create Modal Container
            CreateContainer<ModalContainer>("ModalContainer", canvas.transform);

            // Create Sheet Container
            CreateContainer<SheetContainer>("SheetContainer", canvas.transform);

            Debug.Log("Created sample containers");
        }

        private void CreateContainer<T>(string name, Transform parent) where T : MonoBehaviour, IUIContainer
        {
            var existing = FindObjectOfType<T>();
            if (existing != null)
            {
                Debug.Log($"{name} already exists");
                return;
            }

            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;

            go.AddComponent<CanvasGroup>();
            go.AddComponent<T>();

            Debug.Log($"Created {name}");
        }

        private void SetupBackButtonHandler()
        {
            var existing = FindObjectOfType<Platform.BackButton.BackButtonHandler>();
            if (existing == null)
            {
                var go = new GameObject("[Back Button Handler]");
                go.AddComponent<Platform.BackButton.BackButtonHandler>();
                Debug.Log("Created Back Button Handler");
            }
        }
    }
}
