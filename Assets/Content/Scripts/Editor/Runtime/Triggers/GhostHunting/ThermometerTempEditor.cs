using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;
using UHFPS.Tools;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ThermometerTemp))]
    public class ThermometerTempEditor : InspectorEditor<ThermometerTemp>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Thermometer Temp"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("TemperatureType");
                if (Target.IsBaseTrigger)
                    Properties.Draw("ChangeType");

                if (Target.TemperatureType == ThermometerTemp.TempType.Trigger)
                    Properties.Draw("TriggerType");

                Properties.Draw("ThermometerItem");

                if(Target.IsBaseTrigger)
                {
                    if (Target.ChangeType == ThermometerTemp.TempChangeType.SetBase)
                    {
                        EditorGUILayout.Space();
                        using (new EditorDrawing.BorderBoxScope())
                        {
                            DrawTemperature();
                            EditorGUILayout.Space(1f);
                            DrawRandomizer();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.Space();
                    using (new EditorDrawing.BorderBoxScope())
                    {
                        DrawTemperature();
                        EditorGUILayout.Space(1f);
                        DrawRandomizer();
                    }
                }

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnSetTemp"], new GUIContent("Events")))
                {
                    Properties.Draw("OnSetTemp");
                    Properties.Draw("OnResetTemp");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            if (Target.IsBaseTrigger && Target.ChangeType == ThermometerTemp.TempChangeType.SetBase)
            {
                float fahrenheit = Target.Temperature * 1.8f + 32f;
                EditorGUILayout.HelpBox($"Temperature: {Target.Temperature}°C / {fahrenheit}°F", MessageType.None);
            }

            if (Target.TemperatureType == ThermometerTemp.TempType.Base)
            {
                EditorGUILayout.HelpBox("The temperature will be set as the base temperature at the start.", MessageType.Info);
            }
            else if (Target.TemperatureType == ThermometerTemp.TempType.Trigger)
            {
                EditorGUILayout.HelpBox("The temperature will be set when going through the trigger. You can set the temperature change type to set or reset.", MessageType.Info);
            }
            else if (Target.TemperatureType == ThermometerTemp.TempType.Raycast)
            {
                EditorGUILayout.HelpBox("The temperature will be set by raycasting to the collider with this script attached.", MessageType.Info);
            }
            else if (Target.TemperatureType == ThermometerTemp.TempType.Event)
            {
                EditorGUILayout.HelpBox("The temperature will be set by calling the set or reset functions in this script. You can set the temperature change type to set or reset.", MessageType.Info);
            }
        }

        private void DrawTemperature()
        {
            Rect tempRect = EditorGUILayout.GetControlRect();
            tempRect = EditorGUI.PrefixLabel(tempRect, new GUIContent("Temperature"));
            EditorGUI.PropertyField(tempRect, Properties["Temperature"], GUIContent.none);

            Rect celsiusRect = tempRect;
            celsiusRect.xMin = tempRect.xMax - EditorGUIUtility.singleLineHeight;
            EditorGUI.LabelField(celsiusRect, new GUIContent("°C"));
        }

        private void DrawRandomizer()
        {
            if (EditorDrawing.BeginFoldoutBorderLayout(Properties["RandomTempScale"], new GUIContent("Randomize")))
            {
                Properties.Draw("RandomTempScale");

                if (GUILayout.Button("Random"))
                {
                    float random = Target.RandomTempScale.Random();
                    random = (float)System.Math.Round(random, 2);
                    Target.Temperature = random;
                }

                EditorDrawing.EndBorderHeaderLayout();
            }
        }
    }
}