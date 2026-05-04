using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(GeneratorFuelTank))]
    public class GeneratorFuelTankEditor : InspectorEditor<GeneratorFuelTank>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Generator Fuel Tank"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Properties.Draw("Generator");
                    Properties.Draw("FuelItem");
                    Properties.Draw("FuelProperty");
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Refuel Settings")))
                {
                    Properties.Draw("MinRefuelLiters");
                    Properties.Draw("RefuelTime");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Message Settings")))
                {
                    Properties.Draw("MessageTime");
                    Properties.Draw("NotRequiredMessage");
                    Properties.Draw("NoCanistersMessage");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sound Settings")))
                {
                    Properties.Draw("AudioSource");
                    Properties.Draw("RefuelSound");
                    Properties.Draw("FadeTime");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}