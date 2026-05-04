using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using Unity.Cinemachine;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(CutsceneTrigger))]
    public class CutsceneTriggerEditor : InspectorEditor<CutsceneTrigger>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Cutscene Trigger"), Target);
            EditorGUILayout.Space();

            CutsceneTrigger.CutsceneTypeEnum cutsceneType = (CutsceneTrigger.CutsceneTypeEnum)Properties["CutsceneType"].enumValueIndex;

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                Properties.Draw("TriggerType");
                Properties.Draw("CutsceneType");
                Properties.Draw("Cutscene");
                EditorGUILayout.EndVertical();

                if (cutsceneType == CutsceneTrigger.CutsceneTypeEnum.CameraCutscene)
                {
                    EditorGUILayout.Space();

                    using (new EditorDrawing.BorderBoxScope())
                    {
                        Properties.Draw("WaitForDialogue");
                    }
                    EditorGUILayout.Space();

                    using (new EditorDrawing.BorderBoxScope())
                    {
                        EditorGUILayout.LabelField("Blend Settings", EditorStyles.miniBoldLabel);
                        Properties.Draw("CutsceneCamera");
                        Properties.Draw("CutsceneFadeSpeed");
                    }
                }
                else
                {
                    EditorGUILayout.Space();
                    using (new EditorDrawing.BorderBoxScope())
                    {
                        Properties.Draw("CutscenePlayer");
                        Properties.Draw("WaitForDialogue");
                    }
                    EditorGUILayout.Space();

                    SerializedProperty blendDefinition = Properties["BlendDefinition"];
                    SerializedProperty style = blendDefinition.FindPropertyRelative("m_Style");
                    SerializedProperty time = blendDefinition.FindPropertyRelative("m_Time");
                    SerializedProperty curve = blendDefinition.FindPropertyRelative("m_CustomCurve");

                    CinemachineBlendDefinition.Styles blendStyle = (CinemachineBlendDefinition.Styles)style.enumValueIndex;

                    using (new EditorDrawing.BorderBoxScope())
                    {
                        EditorGUILayout.LabelField("Blend Definition", EditorStyles.miniBoldLabel);
                        Properties.Draw("BlendDefinition");

                        EditorGUILayout.Space();
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        EditorGUILayout.LabelField("Custom Blending", EditorStyles.miniBoldLabel);
                        Properties.Draw("CustomBlendAsset");
                        EditorGUILayout.EndVertical();
                    }

                    EditorGUILayout.Space();

                    using (new EditorDrawing.BorderBoxScope())
                    {
                        EditorGUILayout.LabelField("Blend Settings", EditorStyles.miniBoldLabel);

                        if (blendStyle != CinemachineBlendDefinition.Styles.Cut)
                        {
                            Properties.Draw("WaitForBlendIn");
                            Properties.Draw("BlendInOffset");
                            Properties.Draw("BlendOutTime");
                        }
                        else
                        {
                            Properties.Draw("CutEndTransform");
                            Properties.Draw("CutFadeInSpeed");
                            Properties.Draw("CutFadeOutSpeed");
                            Properties.Draw("DrawCutEndGizmos");
                        }
                    }
                }

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnCutsceneStart"], new GUIContent("Events")))
                {
                    Properties.Draw("OnCutsceneStart");
                    Properties.Draw("OnCutsceneEnd");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}