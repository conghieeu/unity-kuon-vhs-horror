using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(KnifeItemOld))]
    public class KnifeItemOldEditor : PlayerItemEditor<KnifeItemOld>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Knife Item (OLD)"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.Draw("<ItemObject>k__BackingField");
                Properties.Draw("SurfaceDefinitionSet");
                Properties.Draw("SurfaceDetection");
                Properties.Draw("FleshTag");

                EditorGUILayout.Space();
                using(new EditorDrawing.BorderBoxScope(new GUIContent("Knife Settings")))
                {
                    Properties.Draw("RaycastMask");
                    Properties.Draw("AttackDistance");
                    Properties.Draw("AttackDamage");
                    Properties.Draw("AttackWait");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Knife Animations")))
                {
                    Properties.Draw("KnifeDrawState");
                    Properties.Draw("KnifeHideState");
                    Properties.Draw("KnifeIdleState");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Triggers"), EditorStyles.boldLabel);
                    Properties.Draw("HideTrigger");
                    Properties.Draw("AttackTrigger");
                    Properties.Draw("AttackTypeTrigger");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField(new GUIContent("Settings"), EditorStyles.boldLabel);
                    Properties.DrawArray("SlashTypes");
                    Properties.Draw("StabIndex");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Knife Sounds")))
                {
                    Properties.DrawArray("FleshImpact");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Whoosh", EditorStyles.boldLabel);

                    Properties.DrawArray("SlashWhoosh");
                    Properties.DrawArray("StabWhoosh");

                    EditorGUILayout.Space();

                    if(EditorDrawing.BeginFoldoutBorderLayout(Properties["SlashWhoosh"], new GUIContent("Flesh Hit")))
                    {
                        Properties.DrawArray("FleshSlash");
                        Properties.DrawArray("FleshStab");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["StabWhoosh"], new GUIContent("Volumes")))
                    {
                        Properties.Draw("DefaultSlashVolume");
                        Properties.Draw("DefaultStabVolume");
                        Properties.Draw("FleshSlashVolume");
                        Properties.Draw("FleshStabVolume");
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}