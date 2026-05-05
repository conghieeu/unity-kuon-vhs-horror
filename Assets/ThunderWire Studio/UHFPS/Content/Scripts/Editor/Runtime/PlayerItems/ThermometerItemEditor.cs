using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ThermometerItem))]
    public class ThermometerItemEditor : PlayerItemEditor<ThermometerItem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Thermometer Item"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.DrawBacking("ItemObject");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Display Settings")))
                {
                    Properties.Draw("Display");
                    Properties.Draw("EmissionKeyword");
                    EditorGUILayout.Space(1f);
                    Properties.Draw("DisplayCanvas");
                    Properties.Draw("Temperature");
                    Properties.Draw("DisplayFormat");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Base Temperature"), Properties["SetBaseTemp"]))
                {
                    Properties.Draw("BaseTemperature");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Raycast Settings")))
                {
                    Properties.Draw("RaycastMask");
                    Properties.Draw("RaycastDistance");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Temperature Settings")))
                {
                    Properties.Draw("TempGetInterval");
                    Properties.Draw("TempNoiseScale");
                    Properties.Draw("TempNoiseSpeed");
                    Properties.Draw("TempGainSpeed");
                    Properties.Draw("TempDropSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Animation Settings")))
                {
                    Properties.Draw("ThermometerDrawState");
                    Properties.Draw("ThermometerHideState");
                    Properties.Draw("ThermometerHideTrigger");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}