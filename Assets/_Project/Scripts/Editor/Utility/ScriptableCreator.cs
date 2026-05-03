using System.IO;
using UnityEngine;
using UnityEditor;

namespace UHFPS.Editors
{
    public class ScriptableCreator : Editor
    {
        public const string ROOT_PATH = "UHFPS";
        public const string TOOLS_PATH = "Tools/" + ROOT_PATH;
        public const string TEMPLATES_PATH = "C# UHFPS Templates";

        public static string TemplatesPath
        {
            get
            {
                string projectPath = FindAssetPath(ROOT_PATH);
                if (projectPath == "Assets")
                    projectPath = Path.Combine(projectPath, ROOT_PATH);

                return Path.Combine(projectPath, "ScriptTemplates");
            }
        }

        public static string ScriptablesPath
        {
            get
            {
                string projectPath = FindAssetPath(ROOT_PATH);
                if (projectPath == "Assets")
                    projectPath = Path.Combine(projectPath, ROOT_PATH);

                return Path.Combine(projectPath, "Scriptables");
            }
        }

        public static string GameScriptablesPath
        {
            get
            {
                string scriptablesGamePath = Path.Combine(ScriptablesPath, "Game");

                if (!Directory.Exists(scriptablesGamePath))
                {
                    Directory.CreateDirectory(scriptablesGamePath);
                    AssetDatabase.Refresh();
                }

                return scriptablesGamePath;
            }
        }

        public static string FindAssetPath(string searchPattern)
        {
            string[] result = AssetDatabase.FindAssets(searchPattern);

            if (result.Length > 0)
                return AssetDatabase.GUIDToAssetPath(result[0]);

            return "Assets";
        }

        public static T CreateAssetFile<T>(string AssetName) where T : ScriptableObject
        {
            var asset = CreateInstance<T>();
            ProjectWindowUtil.CreateAsset(asset, Path.Combine(GameScriptablesPath, $"New {AssetName}.asset"));
            return asset;
        }

        [MenuItem("Assets/Create/" + TEMPLATES_PATH + "/Editor Script", false, 120)]
        static void CreateEditorScript()
        {
            string templatePath = Path.Combine(TemplatesPath, "EditorScript_Template.txt").Replace("\\", "/");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "New EditorScript.cs");
        }

        [MenuItem("Assets/Create/" + TEMPLATES_PATH + "/PlayerState Script", false, 120)]
        static void CreatePlayerStateScript()
        {
            string templatePath = Path.Combine(TemplatesPath, "PlayerState_Template.txt").Replace("\\", "/");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "New PlayerState.cs");
        }

        [MenuItem("Assets/Create/" + TEMPLATES_PATH + "/AIState Script", false, 120)]
        static void CreateAIStateScript()
        {
            string templatePath = Path.Combine(TemplatesPath, "AIState_Template.txt").Replace("\\", "/");
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(templatePath, "New AIState.cs");
        }
    }
}