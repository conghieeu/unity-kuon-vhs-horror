using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(AxeItem))]
    public class AxeItemEditor : PlayerItemEditor<AxeItem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Axe Item"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.DrawBacking("ItemObject");
                Properties.Draw("SurfaceDefinitionSet");
                Properties.Draw("SurfaceDetection");
                Properties.Draw("FleshTag");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Raycast Settings")))
                {
                    Properties.Draw("RaycastMask");
                    Properties.Draw("AttackAngle");
                    Properties.Draw("AttackRange");
                    Properties.Draw("RaycastCount");
                    Properties.Draw("AttackDelay");
                    Properties.Draw("RaycastDelay");
                    Properties.Draw("ShowAttackGizmos");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Damage Settings")))
                {
                    Properties.Draw("AttackDamage");
                    Properties.Draw("NextAttackTime");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Animation Settings")))
                {
                    Properties.Draw("DrawState");
                    Properties.Draw("HideState");
                    Properties.Draw("IdleState");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Triggers"), EditorStyles.boldLabel);
                    Properties.Draw("HideTrigger");
                    Properties.Draw("AttackTrigger");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Audio Settings")))
                {
                    Properties.Draw("AxeDraw");
                    Properties.Draw("AxeHide");
                    Properties.Draw("AxeSlash");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}