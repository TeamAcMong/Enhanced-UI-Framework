using UnityEngine;
using UnityEditor;
using System.Linq;

namespace EnhancedUI.Editor.Validation
{
    /// <summary>
    /// Validation for container setup
    /// </summary>
    public static class ContainerValidator
    {
        [MenuItem("Tools/Enhanced UI/Validate Setup")]
        public static void ValidateSetup()
        {
            bool hasErrors = false;
            int warningCount = 0;

            // Check settings
            var settings = Resources.Load<EnhancedUISettings>("EnhancedUISettings");
            if (settings == null)
            {
                Debug.LogWarning("[Enhanced UI] Settings not found. Create one via Assets > Create > Enhanced UI > Settings");
                warningCount++;
            }

            // Check containers
            var pageContainers = Object.FindObjectsOfType<PageContainer>();
            var modalContainers = Object.FindObjectsOfType<ModalContainer>();
            var sheetContainers = Object.FindObjectsOfType<SheetContainer>();

            if (pageContainers.Length == 0 && modalContainers.Length == 0 && sheetContainers.Length == 0)
            {
                Debug.LogWarning("[Enhanced UI] No containers found in scene. Use Setup Wizard to create them.");
                warningCount++;
            }

            // Check for duplicate container names
            CheckDuplicateNames(pageContainers, "PageContainer", ref hasErrors);
            CheckDuplicateNames(modalContainers, "ModalContainer", ref hasErrors);
            CheckDuplicateNames(sheetContainers, "SheetContainer", ref hasErrors);

            // Check Canvas setup
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[Enhanced UI] No Canvas found in scene. Containers require a Canvas.");
                hasErrors = true;
            }
            else if (canvas.GetComponent<UnityEngine.UI.GraphicRaycaster>() == null)
            {
                Debug.LogWarning("[Enhanced UI] Canvas is missing GraphicRaycaster component.");
                warningCount++;
            }

            // Summary
            if (!hasErrors && warningCount == 0)
            {
                EditorUtility.DisplayDialog("Validation Complete",
                    "Enhanced UI Framework setup is valid!",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Validation Complete",
                    $"Found {(hasErrors ? "errors" : "warnings")}. Check console for details.",
                    "OK");
            }
        }

        private static void CheckDuplicateNames<T>(T[] containers, string typeName, ref bool hasErrors) where T : MonoBehaviour, IUIContainer
        {
            var duplicates = containers.GroupBy(c => c.Name).Where(g => g.Count() > 1).ToList();

            foreach (var group in duplicates)
            {
                Debug.LogError($"[Enhanced UI] Duplicate {typeName} names found: '{group.Key}'. Each container must have a unique name.");
                hasErrors = true;
            }
        }
    }
}
