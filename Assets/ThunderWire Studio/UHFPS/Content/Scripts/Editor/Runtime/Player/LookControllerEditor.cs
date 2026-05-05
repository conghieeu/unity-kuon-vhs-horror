using UnityEngine;
using UnityEditor;
using UHFPS.Editors;
using ThunderWire.Editors;

namespace UHFPS.Runtime
{
    [CustomEditor(typeof(LookController))]
    public class LookControllerEditor : InspectorEditor<LookController>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Look Controller"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Mouse Sensitivity")))
                {
                    Properties.Draw("SensitivityX");
                    Properties.Draw("SensitivityY");

                    EditorGUILayout.Space(2f);
                    EditorDrawing.Separator();
                    EditorGUILayout.Space(2f);

                    Properties.Draw("MultiplierX");
                    Properties.Draw("MultiplierY");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Look Smoothing"), Properties["SmoothLook"]))
                {
                    Properties.Draw("SmoothTime");
                    Properties.Draw("SmoothMultiplier");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Look Limits")))
                {
                    Properties.Draw("HorizontalLimits");
                    Properties.Draw("VerticalLimits");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Experimental")))
                {
                    Properties.Draw("PlayerForward");

                    if(Target.PlayerForward == LookController.ForwardStyle.RootForward)
                    {
                        EditorGUILayout.HelpBox("Player root transform will rotate depending on where the player is looking.", MessageType.Info);
                    }
                    else if(Target.PlayerForward == LookController.ForwardStyle.LookForward)
                    {
                        EditorGUILayout.HelpBox("Only the rotation of this transform will rotate depending on where the player is looking.", MessageType.Info);
                    }
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope())
                {
                    Properties.Draw("LockCursor");
                    Properties.Draw("LookOffset");

                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        Properties.Draw("LookRotation");
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}