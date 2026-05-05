using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(CandleItem))]
    public class CandleItemEditor : PlayerItemEditor<CandleItem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Candle Item"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.Draw("<ItemObject>k__BackingField");
                Properties.Draw("FlameLight");
                Properties.Draw("FlameRenderer");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Candle Settings")))
                {
                    Properties.Draw("NormalLightMultiplier");
                    Properties.Draw("FocusLightMultiplier");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Light Intensity", EditorStyles.boldLabel);
                    Properties.Draw("FlameLightIntensity");
                    Properties.Draw("FlameIntensityChangeSpeed");
                    Properties.Draw("FlameFlickerLimits");
                    Properties.Draw("FlameFlickerSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Animation Settings")))
                {
                    Properties.Draw("CandleDrawState");
                    Properties.Draw("CandleHideState");
                    Properties.Draw("CandleFocusState");
                    Properties.Draw("CandleUnfocusState");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);
                    Properties.Draw("CandleFocusTrigger");
                    Properties.Draw("CandleBlowTrigger");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sound Settings")))
                {
                    Properties.Draw("FlameBlow");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}