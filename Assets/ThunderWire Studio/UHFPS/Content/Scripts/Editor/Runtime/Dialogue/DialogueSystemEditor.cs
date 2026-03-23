using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(DialogueSystem))]
    public class DialogueSystemEditor : InspectorEditor<DialogueSystem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Dialogue System"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using (new EditorDrawing.BorderBoxScope())
                {
                    EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
                    Properties.Draw("AudioSource");
                    Properties.Draw("DialoguePanel");
                    Properties.Draw("DialogueText");
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                Properties.Draw("ShowNarrator");
                Properties.Draw("UseNarratorColors");
                Properties.Draw("SequenceWait");
                Properties.Draw("FadeTime");
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}