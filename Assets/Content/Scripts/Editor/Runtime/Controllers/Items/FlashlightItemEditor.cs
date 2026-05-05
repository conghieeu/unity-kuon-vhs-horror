using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(FlashlightItem))]
    public class FlashlightItemEditor : PlayerItemEditor<FlashlightItem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Flashlight Item"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.Draw("<ItemObject>k__BackingField");
                Properties.Draw("BatteryInventoryItem");
                Properties.Draw("FlashlightLight");
                Properties.Draw("LightIntensity");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Battery Settings")))
                {
                    Properties.Draw("InfiniteBattery");
                    Properties.Draw("BatteryLife");
                    Properties.Draw("BatteryPercentage");
                    Properties.Draw("BatteryLowPercent");
                    Properties.Draw("ReloadLightEnableOffset");
                    Properties.Draw("BatteryFullColor");
                    Properties.Draw("BatteryLowColor");

                    EditorGUILayout.Space();
                    float batteryEnergy = Application.isPlaying ? Target.batteryEnergy : Target.BatteryPercentage.Ratio();
                    int batteryPercent = Mathf.RoundToInt(batteryEnergy * 100);
                    Rect batteryPercentageRect = EditorGUILayout.GetControlRect();
                    EditorGUI.ProgressBar(batteryPercentageRect, batteryEnergy, $"Battery Energy ({batteryPercent}%)");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Animation Settings")))
                {
                    Properties.Draw("FlashlightDrawState");
                    Properties.Draw("FlashlightHideState");
                    Properties.Draw("FlashlightReloadState");
                    Properties.Draw("FlashlightIdleState");
                    Properties.Draw("FlashlightHideTrim");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);
                    Properties.Draw("FlashlightHideTrigger");
                    Properties.Draw("FlashlightReloadTrigger");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sound Settings")))
                {
                    Properties.Draw("FlashlightClickOn");
                    Properties.Draw("FlashlightClickOff");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}