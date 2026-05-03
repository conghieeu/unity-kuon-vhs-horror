using System;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;
using Object = UnityEngine.Object;
using UHFPS.Scriptable;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(MotionController))]
    public class MotionControllerEditor : InspectorEditor<MotionController>
    {
        private MotionListHelper motionListHelper;

        public override void OnEnable()
        {
            base.OnEnable();

            MotionPreset preset = Target.MotionPreset;
            motionListHelper = new(preset);
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Motion Controller"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    EditorGUI.BeginChangeCheck();
                    Properties.Draw("MotionPreset");
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        Object obj = Properties["MotionPreset"].objectReferenceValue;
                        motionListHelper.UpdatePreset((MotionPreset)obj);
                    }
                }

                Properties.Draw("HandsMotionTransform");
                Properties.Draw("HeadMotionTransform");

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Motion Settings")))
                {
                    Properties.Draw("MotionSuppress");
                    Properties.Draw("MotionSuppressSpeed");
                    Properties.Draw("MotionResetSpeed");
                }

                EditorGUILayout.Space();
                MotionPreset presetInstance = Target.MotionBlender.Instance;
                motionListHelper.DrawMotionsList(presetInstance);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}