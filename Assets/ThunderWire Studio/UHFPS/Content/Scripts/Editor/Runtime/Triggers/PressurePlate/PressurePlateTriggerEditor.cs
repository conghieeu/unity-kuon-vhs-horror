using UHFPS.Editors;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;

namespace UHFPS.Runtime
{
    [CustomEditor(typeof(PressurePlateTrigger))]
    public class PressurePlateTriggerEditor : InspectorEditor<PressurePlateTrigger>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Pressure Plate Trigger"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                if (Application.isPlaying)
                {
                    Rect progressBarRect = EditorGUILayout.GetControlRect();
                    float weightPercent = Target.totalWeight / Target.TriggerWeight;
                    EditorGUI.ProgressBar(progressBarRect, weightPercent, $"Weight {Target.totalWeight}/{Target.TriggerWeight}");
                    EditorGUILayout.Space();
                }

                EditorGUILayout.BeginVertical(GUI.skin.box);
                Properties.Draw("WeightType");
                Properties.Draw("TriggerWeight");
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnWeightTrigger"], new GUIContent("Events")))
                {
                    Properties.Draw("OnWeightTrigger");
                    Properties.Draw("OnWeightChange");
                    Properties.Draw("OnWeightRelease");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}