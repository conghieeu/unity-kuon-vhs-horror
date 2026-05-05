using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(KnifeItem))]
    public class KnifeItemEditor : PlayerItemEditor<KnifeItem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Knife Item"), Target);
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
                    Properties.Draw("RaycastDelay");
                    Properties.Draw("ShowAttackGizmos");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Damage Settings")))
                {
                    Properties.Draw("AttackDamage");
                    Properties.Draw("NextAttackDelay");
                    Properties.Draw("AttackTimeOffset");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Animation Settings")))
                {
                    Properties.Draw("DrawState");
                    Properties.Draw("HideState");
                    Properties.Draw("IdleState");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Slash"), EditorStyles.boldLabel);
                    Properties.Draw("SlashRState");
                    Properties.Draw("SlashLState");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Triggers"), EditorStyles.boldLabel);
                    Properties.Draw("AttackBool");
                    Properties.Draw("SlashTrigger");
                    Properties.Draw("HideTrigger");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Audio Settings")))
                {
                    Properties.Draw("KnifeDraw");
                    Properties.Draw("KnifeHide");
                    Properties.Draw("KnifeSlash");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}