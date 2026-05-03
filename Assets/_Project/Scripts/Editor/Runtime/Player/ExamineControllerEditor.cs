using UnityEngine;
using UnityEditor;
using UHFPS.Editors;
using ThunderWire.Editors;

namespace UHFPS.Runtime
{
    [CustomEditor(typeof(ExamineController))]
    public class ExamineControllerEditor : InspectorEditor<ExamineController>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Examine Controller"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("FocusCullLayes");
                Properties.Draw("FocusLayer");
                Properties.Draw("FocusRenderingLayer");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope())
                {
                    Properties.Draw("ExamineLight");
                    Properties.Draw("HotspotPrefab");
                }

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["HotspotPrefab"], new GUIContent("Controls Settings")))
                {
                    EditorGUI.indentLevel++;
                    Properties.Draw("ControlPutBack");
                    Properties.Draw("ControlRead");
                    Properties.Draw("ControlTake");
                    Properties.Draw("ControlRotate");
                    Properties.Draw("ControlZoom");
                    EditorGUI.indentLevel--;
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["RotateTime"], new GUIContent("General Settings")))
                {
                    Properties.Draw("RotateTime");
                    Properties.Draw("RotateMultiplier");
                    Properties.Draw("ZoomMultiplier");
                    Properties.Draw("TimeToExamine");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["DropOffset"], new GUIContent("Offset Settings")))
                {
                    Properties.Draw("DropOffset");
                    Properties.Draw("InventoryOffset");
                    Properties.Draw("ShowLabels");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["PickUpCurve"], new GUIContent("Pickup Settings")))
                {
                    Properties.Draw("PickUpCurve");
                    Properties.Draw("PickUpCurveMultiplier");
                    Properties.Draw("PickUpTime");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["PickUpTime"], new GUIContent("Put Settings")))
                {
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["PutPositionCurve"], new GUIContent("Position Curve")))
                    {
                        Properties.Draw("PutPositionCurve");
                        Properties.Draw("PutPositionCurveMultiplier");
                        Properties.Draw("PutPositionCurveTime");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["PutRotationCurve"], new GUIContent("Rotation Curve")))
                    {
                        Properties.Draw("PutRotationCurve");
                        Properties.Draw("PutRotationCurveMultiplier");
                        Properties.Draw("PutRotationCurveTime");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["ExamineHintSound"], new GUIContent("Sound Settings")))
                {
                    Properties.Draw("ExamineHintSound");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}