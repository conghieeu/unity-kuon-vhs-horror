using ThunderWire.Editors;
using UHFPS.Runtime;
using UnityEditor;
using UnityEngine;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(SafeWheel))]
    public class SafeWheelEditor : InspectorEditor<SafeWheel>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Safe Wheel"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("Wheel");
                Properties.Draw("WheelRotateAxis");

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Wheel Settings")))
                {
                    Properties.Draw("MaxWheelNumber");
                    Properties.Draw("FastRotateWaitTime");
                    Properties.Draw("FastRotateNumberTime");
                    Properties.Draw("RotateSmoothing");
                    Properties.Draw("UnlockWaitTime");
                }

                EditorGUILayout.Space(1f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sound Settings")))
                {
                    Properties.Draw("AudioSource");
                    Properties.Draw("DialTurn");
                    Properties.Draw("SafeUnlock");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}