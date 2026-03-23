using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(DialogueTrigger))]
    public class DialogueTriggerEditor : InspectorEditor<DialogueTrigger>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Dialogue Trigger"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Properties.Draw("TriggerType");
                    Properties.Draw("DialogueType");
                    Properties.Draw("DialogueContinue");
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Properties.Draw("Dialogue");
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                DialogueTrigger.DialogueTypeEnum dialogueType = (DialogueTrigger.DialogueTypeEnum)Properties["DialogueType"].enumValueIndex;

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Dialogue Properties")))
                {
                    if (dialogueType == DialogueTrigger.DialogueTypeEnum.Local)
                        Properties.Draw("DialogueAudio");

                    Properties.Draw("BinderName");
                }

                if (dialogueType == DialogueTrigger.DialogueTypeEnum.Local)
                {
                    EditorGUILayout.Space();
                    using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Ranged Dialogue"), Properties["RangedDialogue"]))
                    {
                        Properties.Draw("LocalDialogueRange");
                        Properties.Draw("ResetDialogueWhenOut");
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}