using UnityEngine;
using UnityEditor;
using System.IO;

namespace EnhancedUI.Editor.Tools
{
    /// <summary>
    /// Generator for creating screen scripts
    /// </summary>
    public class ScreenGenerator : EditorWindow
    {
        private string _screenName = "MyScreen";
        private ScreenType _screenType = ScreenType.Page;
        private bool _generatePresenter = true;
        private bool _generateViewState = true;
        private string _outputPath = "Assets/Scripts/UI";

        private enum ScreenType
        {
            Page,
            Modal,
            Sheet
        }

        [MenuItem("Tools/Enhanced UI/Generate Screen")]
        public static void ShowWindow()
        {
            var window = GetWindow<ScreenGenerator>("Generate Screen");
            window.minSize = new Vector2(400, 350);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Screen Generator", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "Generate boilerplate code for Page/Modal/Sheet screens with optional MVP support.",
                MessageType.Info);

            GUILayout.Space(10);

            // Screen settings
            _screenName = EditorGUILayout.TextField("Screen Name", _screenName);
            _screenType = (ScreenType)EditorGUILayout.EnumPopup("Screen Type", _screenType);

            GUILayout.Space(10);

            // MVP options
            GUILayout.Label("MVP Options", EditorStyles.boldLabel);
            _generatePresenter = EditorGUILayout.Toggle("Generate Presenter", _generatePresenter);
            _generateViewState = EditorGUILayout.Toggle("Generate ViewState", _generateViewState);

            GUILayout.Space(10);

            // Output
            GUILayout.Label("Output", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                _outputPath = EditorGUILayout.TextField("Output Path", _outputPath);
                if (GUILayout.Button("Browse", GUILayout.Width(70)))
                {
                    var path = EditorUtility.OpenFolderPanel("Select Output Folder", _outputPath, "");
                    if (!string.IsNullOrEmpty(path))
                    {
                        if (path.StartsWith(Application.dataPath))
                        {
                            _outputPath = "Assets" + path.Substring(Application.dataPath.Length);
                        }
                    }
                }
            }

            GUILayout.Space(20);

            // Generate button
            if (GUILayout.Button("Generate", GUILayout.Height(30)))
            {
                GenerateScripts();
            }
        }

        private void GenerateScripts()
        {
            if (string.IsNullOrWhiteSpace(_screenName))
            {
                EditorUtility.DisplayDialog("Error", "Screen name cannot be empty", "OK");
                return;
            }

            // Ensure directory exists
            if (!Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
            }

            // Generate Screen script
            GenerateScreenScript();

            // Generate Presenter if requested
            if (_generatePresenter)
            {
                GeneratePresenterScript();
            }

            // Generate ViewState if requested
            if (_generateViewState)
            {
                GenerateViewStateScript();
            }

            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("Success",
                $"Generated scripts for {_screenName} in {_outputPath}",
                "OK");
        }

        private void GenerateScreenScript()
        {
            var baseClass = _screenType.ToString();
            var viewInterface = _generatePresenter ? $", IView<{_screenName}ViewState>" : "";

            var code = $@"using UnityEngine;
using EnhancedUI;
{(_generatePresenter ? "using EnhancedUI.MVP;\n" : "")}
public class {_screenName} : {baseClass}{viewInterface}
{{
    {(_generateViewState ? $"[SerializeField] private {_screenName}ViewState _viewState;\n" : "")}
    {(_generatePresenter ? $@"
    public {_screenName}ViewState ViewState => _viewState;

    public void OnViewInitialized()
    {{
        Debug.Log(""{_screenName} initialized"");
    }}

    public void OnViewDestroyed()
    {{
        Debug.Log(""{_screenName} destroyed"");
    }}" : "")}
}}
";

            var filePath = Path.Combine(_outputPath, $"{_screenName}.cs");
            File.WriteAllText(filePath, code);

            Debug.Log($"Generated: {filePath}");
        }

        private void GeneratePresenterScript()
        {
            var presenterBase = $"{_screenType}PresenterBase";
            var viewInterface = $"I{_screenName}View";

            var code = $@"using System.Collections;
using EnhancedUI;
using EnhancedUI.MVP;
{(_screenType == ScreenType.Page ? "using EnhancedUI.Lifecycle;" : "")}

public interface {viewInterface} : IView<{_screenName}ViewState>
{{
    // Add view-specific methods here
}}

public class {_screenName}Presenter : {presenterBase}<{_screenName}, {viewInterface}, {_screenName}ViewState>
{{
    public {_screenName}Presenter({_screenName} screen) : base(screen)
    {{
    }}

#if EUI_UNITASK_SUPPORT
    public override async Cysharp.Threading.Tasks.UniTask OnViewLoaded()
    {{
        // Load data, initialize presenter logic
        await base.OnViewLoaded();
    }}
#else
    public override IEnumerator OnViewLoaded()
    {{
        // Load data, initialize presenter logic
        yield return base.OnViewLoaded();
    }}
#endif

    public override void DidPushEnter()
    {{
        base.DidPushEnter();
        // Handle screen enter
    }}
}}
";

            var filePath = Path.Combine(_outputPath, $"{_screenName}Presenter.cs");
            File.WriteAllText(filePath, code);

            Debug.Log($"Generated: {filePath}");
        }

        private void GenerateViewStateScript()
        {
            var code = $@"using System;
using EnhancedUI.MVP;

[Serializable]
public class {_screenName}ViewState : ViewStateBase
{{
    // Add view state properties here
    // Example:
    // public string Title {{ get; set; }}
    // public bool IsLoading {{ get; set; }}

    public void UpdateTitle(string title)
    {{
        // Update property and notify
        NotifyStateChanged();
    }}
}}
";

            var filePath = Path.Combine(_outputPath, $"{_screenName}ViewState.cs");
            File.WriteAllText(filePath, code);

            Debug.Log($"Generated: {filePath}");
        }
    }
}
