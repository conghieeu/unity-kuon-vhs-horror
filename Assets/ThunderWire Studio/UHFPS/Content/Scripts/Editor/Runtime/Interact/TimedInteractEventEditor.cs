using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(TimedInteractEvent)), CanEditMultipleObjects]
    public class TimedInteractEventEditor : InspectorEditor<TimedInteractEvent>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Timed Interact Event"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Interact Settings")))
                {
                    Properties.DrawBacking("InteractTime");
                    Properties.Draw("InteractOnce");
                    Properties.Draw("UseResetInteract");

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    if(Properties.DrawGetBool("RequireInventoryItem"))
                        Properties.Draw("RequiredItem");
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    if (Properties.DrawGetBool("ShowRequireItemHint"))
                    {
                        Properties.Draw("HintMessageTime");
                        Properties.Draw("HintMessage");
                    }
                    EditorGUILayout.EndVertical();
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sounds")))
                {
                    Properties.Draw("InteractSound");
                    if (Target.UseResetInteract)
                        Properties.Draw("ResetSound");
                }

                EditorGUILayout.Space();

                Properties.Draw("OnInteract");

                if(Target.UseResetInteract)
                    Properties.Draw("OnReset");
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}