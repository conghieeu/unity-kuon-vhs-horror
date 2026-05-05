using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(FootstepsSystem))]
    public class FootstepsSystemEditor : InspectorEditor<FootstepsSystem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Footsteps System"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("SurfaceDefinitionSet");
                Properties.Draw("FootstepStyle");
                Properties.Draw("SurfaceDetection");
                Properties.Draw("FootstepsMask");

                EditorGUILayout.Space();
                using(new EditorDrawing.BorderBoxScope(new GUIContent("Footstep Settings")))
                {
                    Properties.Draw("StepPlayerVelocity");
                    Properties.Draw("JumpStepAirTime");
                }

                if (Target.FootstepStyle != FootstepsSystem.FootstepStyleEnum.Animation)
                {
                    EditorGUILayout.Space();
                    using (new EditorDrawing.BorderBoxScope(new GUIContent("Footstep Timing")))
                    {
                        if (Target.FootstepStyle == FootstepsSystem.FootstepStyleEnum.Timed)
                        {
                            Properties.Draw("WalkStepTime");
                            Properties.Draw("RunStepTime");
                        }
                        else if (Target.FootstepStyle == FootstepsSystem.FootstepStyleEnum.HeadBob)
                        {
                            Properties.Draw("HeadBobStepWave");
                        }

                        Properties.Draw("LandStepTime");
                    }
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Footstep Volume")))
                {
                    Properties.Draw("WalkingVolume");
                    Properties.Draw("RunningVolume");
                    Properties.Draw("LandVolume");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}