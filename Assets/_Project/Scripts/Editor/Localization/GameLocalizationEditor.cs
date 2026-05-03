using UnityEditor.Build;
using UnityEditor;
using UnityEngine;
using UHFPS.Runtime;
using UHFPS.Tools;
using ThunderWire.Editors;
using System.Linq;
using System;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(GameLocalization))]
    public class GameLocalizationEditor : InspectorEditor<GameLocalization>
    {
        private const string LOCALIZATION_SYMBOL = "UHFPS_LOCALIZATION";

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Game Localization"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                string[] languages = new string[0];
                if (Target.LocalizationTable != null)
                {
                    languages = Target.LocalizationTable.Languages
                        .Where(x => x != null)
                        .Select((x, i) => x.LanguageName.Or("Unknown Language " + i))
                        .ToArray();
                }

                Properties.Draw("LocalizationTable");
                DrawDefaultLanguageSelector(languages);
                Properties.Draw("ShowWarnings");

                if (Application.isPlaying)
                {
                    EditorGUILayout.Space();
                    using (new EditorGUI.DisabledGroupScope(languages.Length <= 0))
                    {
                        if (GUILayout.Button("Set Language", GUILayout.Height(25f)))
                        {
                            Target.ChangeLanguage(Target.DefaultLanguage);
                            string name = languages[Target.DefaultLanguage];
                            Debug.Log("Language set to " + name);
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();

            if (!Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("To enable or disable UHFPS localization, click the button below. A scripting symbol will automatically be included in the player settings to allow you to use Game Localization.", MessageType.Info);
                EditorGUILayout.Space(1f);

                string toggleText = CheckActivation() ? "Disable" : "Enable";
                if (GUILayout.Button($"{toggleText} GLoc Localization", GUILayout.Height(25f)))
                {
                    ToggleScriptingSymbol();
                }
            }
        }

        private void DrawDefaultLanguageSelector(string[] languages)
        {
            if (Target.LocalizationTable != null && languages.Length > 0)
            {
                string selected = languages.Length > 0 && Target.DefaultLanguage >= 0
                    ? languages[Target.DefaultLanguage]
                    : string.Empty;

                EditorDrawing.DrawStringSelectPopup(new GUIContent("Default Language"), new GUIContent("Language"), languages, selected, (lang) =>
                {
                    int index = Array.FindIndex(languages, x => lang == x);
                    Properties["DefaultLanguage"].intValue = index;
                    serializedObject.ApplyModifiedProperties();
                });
            }
            else
            {
                Properties["DefaultLanguage"].intValue = 0;
            }
        }

        private bool CheckActivation()
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);
            string defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            return defines.Contains(LOCALIZATION_SYMBOL);
        }

        private void ToggleScriptingSymbol()
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(buildTargetGroup);

            string defines = PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget);
            string[] definesParts = defines.Split(';');

            if (defines.Contains(LOCALIZATION_SYMBOL))
                definesParts = definesParts.Except(new[] { LOCALIZATION_SYMBOL }).ToArray();
            else
                definesParts = definesParts.Concat(new[] { LOCALIZATION_SYMBOL }).ToArray();

            defines = string.Join(";", definesParts);
            PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget, defines);
        }
    }
}