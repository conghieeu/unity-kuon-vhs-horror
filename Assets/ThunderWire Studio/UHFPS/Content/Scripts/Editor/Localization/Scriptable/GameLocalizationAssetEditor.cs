using UnityEngine;
using UnityEditor;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(GameLocalizationAsset))]
    public class GameLocalizationAssetEditor : InspectorEditor<GameLocalizationAsset>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Game Localization Asset"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.HelpBox("Game Localization Asset have been moved to the new Localization Language Asset. You can import old language data into the new localization table in the Game Localization Table window. Find the GameLocalizationTable and click the Import OLD button.", MessageType.Warning);
                EditorGUILayout.Space();

                Properties.Draw("LanguageName");

                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(GUI.skin.box);
                Properties.Draw("Localizations");
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}