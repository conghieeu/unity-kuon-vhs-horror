using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(CameraItem))]
    public class CameraItemEditor : PlayerItemEditor<CameraItem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Camera Item"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.Draw("<ItemObject>k__BackingField");
                Properties.Draw("BatteryInventoryItem");
                Properties.Draw("CameraAudio");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Night Vision Settings")))
                {
                    Properties.Draw("CameraLight");
                    Properties.Draw("NVComponent");
                    Properties.Draw("NoNVDrainBattery");
                    Properties.Draw("InitialNVState");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Battery Settings")))
                {
                    Properties.Draw("BatteryPercentage");
                    Properties.Draw("BatteryLowPercent");
                    Properties.Draw("HighBatteryDrainSpeed");
                    if(Properties.BoolValue("NoNVDrainBattery"))
                        Properties.Draw("LowBatteryDrainSpeed");
                    Properties.Draw("LightIntensity");
                    Properties.Draw("BatteryFullColor");
                    Properties.Draw("BatteryLowColor");

                    EditorGUILayout.Space();
                    Rect batteryPercentageRect = EditorGUILayout.GetControlRect();
                    float batteryEnergy = Application.isPlaying ? Target.batteryEnergy : Target.BatteryPercentage.Ratio();
                    int batteryPercent = Mathf.RoundToInt(batteryEnergy * 100);
                    EditorGUI.ProgressBar(batteryPercentageRect, batteryEnergy, $"Battery Energy ({batteryPercent}%)");
                }

                EditorGUILayout.Space(1f);
                if(EditorDrawing.BeginFoldoutBorderLayout(Properties["LightZoomRange"], new GUIContent("Zoom Settings")))
                {
                    Properties.Draw("LightZoomRange");
                    Properties.Draw("CameraZoomFOV");
                    Properties.Draw("CameraZoomSpeed");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["EnableChannels"], new GUIContent("Channels Settings")))
                {
                    Properties.Draw("EnableChannels");
                    Properties.Draw("SampleDataLength");
                    Properties.Draw("FrameDelay");
                    Properties.Draw("MaxRMSValue");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["CameraShow"], new GUIContent("Animation Settings")))
                {
                    Properties.Draw("CameraShow");
                    Properties.Draw("CameraHide");
                    Properties.Draw("CameraReload");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["CameraShowFadeOffset"], new GUIContent("Fade Settings")))
                {
                    Properties.Draw("CameraShowFadeOffset");
                    Properties.Draw("CameraHideFadeOffset");
                    Properties.Draw("CameraShowFadeSpeed");
                    Properties.Draw("CameraHideFadeSpeed");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["CameraEquip"], new GUIContent("Sound Settings")))
                {
                    Properties.Draw("CameraEquip");
                    Properties.Draw("CameraUnequip");
                    Properties.Draw("CameraZoomIn");
                    Properties.Draw("CameraZoomOut");
                    Properties.Draw("CameraNVSwitch");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}