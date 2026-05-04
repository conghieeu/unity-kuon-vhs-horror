using System;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(PowerGenerator))]
    public class PowerGeneratorEditor : InspectorEditor<PowerGenerator>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Power Generator"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Rect fuelRect = EditorGUILayout.GetControlRect();
                float fuel = Target.CurrentFuelLiters / Target.MaxFuelLiters;

                float currentLiters = (float)Math.Round(Target.CurrentFuelLiters, 2);
                EditorGUI.ProgressBar(fuelRect, fuel, $"Fuel liters ({currentLiters}/{Target.MaxFuelLiters})");

                float consumptionRate = (float)Math.Round(Target.fuelConsumptionRate, 2);
                EditorGUILayout.LabelField("Fuel Consumption Rate (L/h): " + consumptionRate, EditorStyles.miniBoldLabel);
                EditorGUILayout.LabelField("Runtime Consumers: " + Target.RuntimeConsumers, EditorStyles.miniBoldLabel);

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Properties.Draw("MaxFuelLiters");
                    Properties.Draw("CurrentFuelLiters");
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Generator References")))
                {
                    Properties.Draw("Switcher");
                    Properties.Draw("ExhaustParticles");
                    Properties.Draw("FuelStatus");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Generator Settings")))
                {
                    Properties.Draw("GeneratorEfficiency");
                    Properties.Draw("FuelCalorificValue");
                    Properties.Draw("MotorFuelDrainPerHour");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Power Consumers")))
                {
                    EditorGUI.indentLevel++;
                    Properties.Draw("PowerConsumers");
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Generator Sounds")))
                {
                    Properties.Draw("AudioSourceA");
                    Properties.Draw("AudioSourceB");
                    Properties.Draw("BlendTime");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Sounds", EditorStyles.miniBoldLabel);
                    Properties.Draw("MotorLoop");
                    Properties.Draw("MotorStart");
                    Properties.Draw("MotorEnd");
                }

                EditorGUILayout.Space();
                if(EditorDrawing.BeginFoldoutBorderLayout(Properties["OnGeneratorStart"], new GUIContent("Events")))
                {
                    Properties.Draw("OnGeneratorStart");
                    Properties.Draw("OnGeneratorEnd");
                    Properties.Draw("OnOutOfFuel");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}