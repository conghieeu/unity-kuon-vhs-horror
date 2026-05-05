using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LanternItem))]
    public class LanternItemEditor : PlayerItemEditor<LanternItem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Lantern Item"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.Draw("<ItemObject>k__BackingField");
                Properties.Draw("FuelInventoryItem");
                Properties.Draw("HandleBone");
                Properties.Draw("LanternLight");
                Properties.Draw("LanternFlame");
                Properties.Draw("HandleLimits");
                Properties.Draw("HandleAxis");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Lantern Settings")))
                {
                    Properties.Draw("HandleGravityTime");
                    Properties.Draw("HandleForwardAngle");
                    Properties.Draw("FlameChangeSpeed");
                    Properties.Draw("FlameLightIntensity");
                    Properties.Draw("FlameAlphaFadeStart");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Light Intensity", EditorStyles.boldLabel);
                    Properties.Draw("FlameFlickerLimits");
                    Properties.Draw("FlameFlickerSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Handle Variation")))
                {
                    EditorDrawing.DrawClassBorderFoldout(Properties["HandleIdleVariation"], new GUIContent("Handle Idle Variation"));
                    EditorDrawing.DrawClassBorderFoldout(Properties["HandleWalkVariation"], new GUIContent("Handle Walk Variation"));
                    Properties.Draw("VariationBlendTime");
                    Properties.Draw("UseHandleVariation");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Fuel Settings")))
                {
                    Properties.Draw("InfiniteFuel");
                    Properties.Draw("FuelReloadTime");
                    Properties.Draw("FuelLife");
                    Properties.Draw("FuelPercentage");

                    EditorGUILayout.Space();
                    float fuel = Application.isPlaying ? Target.lanternFuel : Target.FuelPercentage.Ratio();
                    int fuelPercent = Mathf.RoundToInt(fuel * 100);
                    Rect fuelPercentageRect = EditorGUILayout.GetControlRect();
                    EditorGUI.ProgressBar(fuelPercentageRect, fuel, $"Lantern Fuel ({fuelPercent}%)");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Animation Settings")))
                {
                    Properties.Draw("LanternDrawState");
                    Properties.Draw("LanternHideState");
                    Properties.Draw("LanternReloadStartState");
                    Properties.Draw("LanternReloadEndState");
                    Properties.Draw("LanternIdleState");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);
                    Properties.Draw("LanternHideTrigger");
                    Properties.Draw("LanternReloadTrigger");
                    Properties.Draw("LanternReloadEndTrigger");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sound Settings")))
                {
                    Properties.Draw("LanternDraw");
                    Properties.Draw("LanternHide");
                    Properties.Draw("LanternReload");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}