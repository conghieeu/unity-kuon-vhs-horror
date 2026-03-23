using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(DialogueBinder))]
    public class DialogueBinderEditor : InspectorEditor<DialogueBinder>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Dialogue Binder"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnDialogueStart"], new GUIContent("Events")))
                {
                    Properties.Draw("OnDialogueStart");
                    Properties.Draw("OnSubtitle");
                    Properties.Draw("OnSubtitleFinish");
                    Properties.Draw("OnDialogueEnd");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space();
                if(GUILayout.Button("Next Dialogue", GUILayout.Height(22f)))
                {
                    Target.NextDialogue();
                }

                if (GUILayout.Button("Stop Dialogue", GUILayout.Height(22f)))
                {
                    Target.StopDialogue();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}