using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(AnimationPlayEvent))]
    public class AnimationPlayEventEditor : InspectorEditor<AnimationPlayEvent>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Animation Play Event"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Properties.Draw("Animator");
                    if (!Target.UseOnlyState)
                        Properties.Draw("TriggerName");
                    Properties.Draw("StateName");
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("EndEventTimeOffset");
                    Properties.Draw("UseOnlyState");
                    Properties.Draw("PlayMoreTimes");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Events")))
                {
                    Properties.Draw("OnAnimationStart");
                    Properties.Draw("OnAnimationEnd");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}