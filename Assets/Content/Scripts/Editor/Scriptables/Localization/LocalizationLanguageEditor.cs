using UnityEngine;
using UnityEditor;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LocalizationLanguage))]
    public class LocalizationLanguageEditor : InspectorEditor<LocalizationLanguage>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Localization Language"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.HelpBox("You can edit this language in the Game Localization Table Editor window.", MessageType.Info);
                EditorGUILayout.Space();

                Properties.Draw("LanguageName");
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    int entries = Target.Strings.Count;
                    EditorGUILayout.TextField("Strings", entries.ToString());
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}