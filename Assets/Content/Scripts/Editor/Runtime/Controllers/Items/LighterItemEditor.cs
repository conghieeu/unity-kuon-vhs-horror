using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LighterItem))]
    public class LighterItemEditor : PlayerItemEditor<LighterItem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Lighter Item"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.Draw("<ItemObject>k__BackingField");
                Properties.Draw("FlameLight");
                Properties.Draw("SparkLight");
                Properties.Draw("SparkParticle");
                Properties.Draw("FlameRenderer");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Lighter Settings")))
                {
                    Properties.Draw("SparkLightTime");
                    Properties.Draw("FlameIgniteProbability");
                    Properties.Draw("FlameExtinguishProbability");
                    Properties.Draw("FlameExtinguishTimeRange");
                    Properties.Draw("EnableFlameExtinguishing");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Light Intensity", EditorStyles.boldLabel);
                    Properties.Draw("FlameFlickerLimits");
                    Properties.Draw("FlameFlickerSpeed");
                    Properties.Draw("FlameLightIntensity");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Animation Settings")))
                {
                    Properties.Draw("LighterDrawState");
                    Properties.Draw("LighterHideState");
                    Properties.Draw("LighterIgniteStartState");
                    Properties.Draw("LighterIgniteSparkState");
                    Properties.Draw("LighterIgniteHoldState");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);
                    Properties.Draw("LighterHideTrigger");
                    Properties.Draw("LighterSparkTrigger");
                    Properties.Draw("LighterHoldTrigger");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sound Settings")))
                {
                    Properties.Draw("LighterFlick");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}