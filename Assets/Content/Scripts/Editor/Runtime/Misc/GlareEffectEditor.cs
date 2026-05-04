using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(GlareEffect))]
    public class GlareEffectEditor : InspectorEditor<GlareEffect>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Glare Effect"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("ColorParam");
                Properties.Draw("GlareState");
                EditorGUILayout.Space();

                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Distance Scaling"), Properties["EnableDistanceScaling"]))
                {
                    Properties.Draw("ScaleDistance");
                    Properties.Draw("MinScaleDistance");
                }

                EditorGUILayout.Space(2f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Distance Fading")))
                {
                    Properties.Draw("EnableNearFading");
                    Properties.Draw("NearFarDistance");
                    Properties.Draw("BlendDistance");
                }

                EditorGUILayout.Space(2f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Effect Settings")))
                {
                    Properties.Draw("PulseScale");
                    Properties.Draw("MinWaitTime");
                    Properties.Draw("PulseSpeed");
                    Properties.Draw("RotateSpeed");
                    Properties.Draw("EnableRotation");
                }

            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}