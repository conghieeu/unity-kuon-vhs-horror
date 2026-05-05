using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(EMFMeterItem))]
    public class EMFMeterItemEditor : PlayerItemEditor<EMFMeterItem>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("EMF Meter Item"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.DrawBacking("ItemObject");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Indicator Settings")))
                {
                    Properties.Draw("Indicators", indent: 1);
                    Properties.Draw("EmissionKeyword");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Display Settings")))
                {
                    Properties.Draw("DisplayCanvas");
                    Properties.Draw("Display");
                    EditorGUILayout.Space(1f);
                    Properties.Draw("MilligaussText");
                    Properties.Draw("DisplayFormat");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Detection Settings")))
                {
                    Properties.Draw("DetectionMask");
                    Properties.Draw("DetectionRadius");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("More Settings", EditorStyles.boldLabel);
                    Properties.Draw("MaxMilligaussValue");
                    Properties.Draw("AnomalyDotRangeCompensation");
                    Properties.Draw("MinAnomalyDirection");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Update Settings", EditorStyles.boldLabel);
                    Properties.Draw("MilligaussUpdateSpeed");
                    Properties.Draw("DecimalPartUpdateSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Background Noise"), Properties["EnableNoise"]))
                {
                    Properties.Draw("BackgroundNoise");
                    Properties.Draw("NoiseAmount");
                    Properties.Draw("NoiseSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Reader Beep"), Properties["EneableReaderBeep"]))
                {
                    Properties.Draw("EneablePitchedBeep");
                    Properties.Draw("ReaderAudio");
                    Properties.Draw("ReaderStartLevel");
                    Properties.Draw("ReaderPitchLimits");
                    Properties.Draw("ReaderBeepSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Animation Settings")))
                {
                    Properties.Draw("EMFDrawState");
                    Properties.Draw("EMFHideState");
                    Properties.Draw("EMFHideTrigger");
                }

                EditorGUILayout.Space();
                Properties.Draw("ShowRadiusDebug");
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}