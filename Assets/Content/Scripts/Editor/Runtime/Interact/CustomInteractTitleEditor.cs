using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(CustomInteractTitle)), CanEditMultipleObjects]
    public class CustomInteractTitleEditor : InspectorEditor<CustomInteractTitle>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Custom Interact Title"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("OverrideTitle");
                Properties.Draw("OverrideUseTitle");
                Properties.Draw("OverrideExamineTitle");

                EditorGUILayout.Space();
                EditorDrawing.Separator();
                EditorGUILayout.Space();

                bool overrideTitle = Properties.BoolValue("OverrideTitle");
                bool overrideUseTitle = Properties.BoolValue("OverrideUseTitle");
                bool overrideExamineTitle = Properties.BoolValue("OverrideExamineTitle");

                if(!overrideTitle && !overrideUseTitle && !overrideExamineTitle)
                {
                    EditorGUILayout.HelpBox("Nothing will be overridden. The interactive message will not be overridden.", MessageType.Info);
                }
                else
                {
                    if (overrideTitle)
                    {
                        using (new EditorDrawing.BorderBoxScope(new GUIContent("Override Title")))
                        {
                            if (!Properties.DrawGetBool("UseTitleDynamic", new GUIContent("Is Dynamic")))
                            {
                                Properties.Draw("Title");
                            }
                            else
                            {
                                EditorGUILayout.Space();
                                Properties.Draw("DynamicTitle");
                                EditorGUILayout.Space(2f);
                                Properties.Draw("TrueTitle");
                                Properties.Draw("FalseTitle");
                            }

                            if (Application.isPlaying)
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.TextField("Result", Target.Title);
                                }
                            }
                        }

                        if (overrideUseTitle || overrideExamineTitle) EditorGUILayout.Space();
                    }

                    if (overrideUseTitle)
                    {
                        using (new EditorDrawing.BorderBoxScope(new GUIContent("Override Use Title")))
                        {
                            if (!Properties.DrawGetBool("UseUseTitleDynamic", new GUIContent("Is Dynamic")))
                            {
                                Properties.Draw("UseTitle");
                            }
                            else
                            {
                                EditorGUILayout.Space();
                                Properties.Draw("DynamicUseTitle");
                                EditorGUILayout.Space(2f);
                                Properties.Draw("TrueUseTitle");
                                Properties.Draw("FalseUseTitle");
                            }

                            if (Application.isPlaying)
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.TextField("Result", Target.UseTitle);
                                }
                            }
                        }

                        if (overrideExamineTitle) EditorGUILayout.Space();
                    }

                    if (overrideExamineTitle)
                    {
                        using (new EditorDrawing.BorderBoxScope(new GUIContent("Override Examine Title")))
                        {
                            if (!Properties.DrawGetBool("UseExamineTitleDynamic", new GUIContent("Is Dynamic")))
                            {
                                Properties.Draw("ExamineTitle");
                            }
                            else
                            {
                                EditorGUILayout.Space();
                                Properties.Draw("DynamicExamineTitle");
                                EditorGUILayout.Space(2f);
                                Properties.Draw("TrueExamineTitle");
                                Properties.Draw("FalseExamineTitle");
                            }

                            if (Application.isPlaying)
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.TextField("Result", Target.ExamineTitle);
                                }
                            }
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}