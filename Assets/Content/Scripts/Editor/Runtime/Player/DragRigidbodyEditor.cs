using UnityEngine;
using UnityEditor;
using UHFPS.Editors;
using ThunderWire.Editors;

namespace UHFPS.Runtime
{
    [CustomEditor(typeof(DragRigidbody))]
    public class DragRigidbodyEditor : InspectorEditor<DragRigidbody>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Drag Rigidbody"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("HoldType");
                Properties.Draw("DragType");

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["DragType"], new GUIContent("Controls Settings")))
                {
                    EditorGUI.indentLevel++;
                    Properties.Draw("ControlsContexts");
                    EditorGUI.indentLevel--;
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["ShowGrabReticle"], new GUIContent("Reticle Settings")))
                {
                    Properties.Draw("ShowGrabReticle");
                    EditorGUI.indentLevel++;
                    Properties.Draw("GrabHand");
                    Properties.Draw("HoldHand");
                    EditorGUI.indentLevel--;
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["Interpolate"], new GUIContent("Rigidbody Settings")))
                {
                    Properties.Draw("Interpolate");
                    Properties.Draw("CollisionDetection");
                    Properties.Draw("FreezeRotation");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["DragStrength"], new GUIContent("General Settings")))
                {
                    Properties.Draw("DragStrength");
                    Properties.Draw("ThrowStrength");
                    Properties.Draw("RotateSpeed");
                    Properties.Draw("ZoomSpeed");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["HitpointOffset"], new GUIContent("Features")))
                {
                    Properties.Draw("HitpointOffset");
                    Properties.Draw("PlayerCollision");
                    Properties.Draw("ObjectZooming");
                    Properties.Draw("ObjectRotating");
                    Properties.Draw("ObjectThrowing");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}