using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEditor.ShortcutManagement;
using UnityEngine;

public class CreateScriptHotkey
{
    // Ctrl+Alt+Shift+X on Windows / Cmd+Alt+Shift+X on macOS
    [Shortcut("Assets/Create Empty C# Script", KeyCode.X, ShortcutModifiers.Action | ShortcutModifiers.Alt | ShortcutModifiers.Shift)]
    private static void CreateEmptyScript()
    {
        string folderPath = GetActiveProjectFolder();
        string targetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(folderPath, "NewScript.cs"));

        Texture2D icon = EditorGUIUtility.IconContent("cs Script Icon").image as Texture2D;

        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
            0,
            ScriptableObject.CreateInstance<CreateScriptEndNameEditAction>(),
            targetPath,
            icon,
            null
        );
    }

    private static string GetActiveProjectFolder()
    {
        // This internal Unity method matches the Project window's current folder behavior
        MethodInfo method = typeof(ProjectWindowUtil).GetMethod(
            "GetActiveFolderPath",
            BindingFlags.Static | BindingFlags.NonPublic
        );

        if (method != null)
        {
            return (string)method.Invoke(null, null);
        }

        // Fallback if Unity changes internals
        Object selected = Selection.activeObject;
        if (selected != null)
        {
            string path = AssetDatabase.GetAssetPath(selected);
            if (AssetDatabase.IsValidFolder(path))
                return path;

            if (!string.IsNullOrEmpty(path))
                return Path.GetDirectoryName(path)?.Replace("\\", "/") ?? "Assets";
        }

        return "Assets";
    }

    private class CreateScriptEndNameEditAction : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            string className = Path.GetFileNameWithoutExtension(pathName);
            string scriptContents =
$@"using UnityEngine;

public class {MakeValidClassName(className)} : MonoBehaviour
{{
    
}}";

            File.WriteAllText(pathName, scriptContents);
            AssetDatabase.ImportAsset(pathName);
            AssetDatabase.Refresh();

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(pathName);
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }

        private static string MakeValidClassName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "NewScript";

            // Basic sanitization so the class name is valid C#
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            if (!char.IsLetter(fileName[0]) && fileName[0] != '_')
                sb.Append('_');

            foreach (char c in fileName)
            {
                sb.Append(char.IsLetterOrDigit(c) || c == '_' ? c : '_');
            }

            return sb.ToString();
        }
    }
}
