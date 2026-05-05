using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UHFPS.Scriptable;
using ThunderWire.Editors;
using UHFPS.Tools;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(GameLocaizationTable))]
    public class GameLocaizationTableEditor : InspectorEditor<GameLocaizationTable>
    {
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            var asset = obj as GameLocaizationTable;
            if (asset == null) return false;

            OpenLocalizationEditor(asset);
            return true;
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Game Localization Table"), Target);
            EditorGUILayout.Space();

            using(new EditorDrawing.BorderBoxScope(new GUIContent("Languages"), roundedBox: false))
            {
                if(Target.Languages.Count > 0)
                {
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        foreach (var lang in Target.Languages)
                        {
                            string name = lang.LanguageName.Or("Unknown");
                            EditorGUILayout.ObjectField(new GUIContent(name), lang, typeof(LocalizationLanguage), false);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("There are currently no languages available, open the localization editor and add new languages.", MessageType.Info);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                {
                    if (GUILayout.Button("Open Localization Editor", GUILayout.Width(180f), GUILayout.Height(25)))
                    {
                        OpenLocalizationEditor(Target);
                    }
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        private static void OpenLocalizationEditor(GameLocaizationTable asset)
        {
            EditorWindow window = EditorWindow.GetWindow<LocalizationTableWindow>(true, "Localization Editor", true);

            Vector2 windowSize = new(1000, 500);
            window.minSize = windowSize;
            (window as LocalizationTableWindow).Show(asset);
        }
    }
}