using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;
using UHFPS.Tools;
using System;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(EMFAnomaly))]
    public class EMFAnomalyEditor : InspectorEditor<EMFAnomaly>
    {
        private bool randomExpanded;

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("EMF Anomaly"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    Properties.Draw("Weight", new GUIContent("Spikes Weight"));

                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.FloatField(new GUIContent("Anomaly Weight"), Target.AnomalyWeight);
                    }
                }

                EditorGUILayout.Space();
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                    {
                        Properties.Draw("AnomalyDetection");
                        if (Target.AnomalyDetection == EMFAnomaly.AnomalyDetect.Event)
                            Properties.Draw("StartingWeight");

                        Properties.Draw("EMFSpikes", 1);
                    }

                    if(Target.AnomalyDetection == EMFAnomaly.AnomalyDetect.Once)
                    {
                        EditorGUILayout.HelpBox("The anomaly will be detected once during the detection time. After that, the anomaly disappears.", MessageType.Info);
                    }
                    else if (Target.AnomalyDetection == EMFAnomaly.AnomalyDetect.Random)
                    {
                        EditorGUILayout.HelpBox("The anomaly will be detected during the detection time. Then the anomaly disappears and reappears after the random reset time has elapsed.", MessageType.Info);
                    }
                    else if (Target.AnomalyDetection == EMFAnomaly.AnomalyDetect.Always)
                    {
                        EditorGUILayout.HelpBox("The anomaly is always detected and does not disappear.", MessageType.Info);
                    }
                    else if (Target.AnomalyDetection == EMFAnomaly.AnomalyDetect.Event)
                    {
                        EditorGUILayout.HelpBox("The anomaly will be shown or hidden by calling events from the script.", MessageType.Info);
                    }

                    EditorGUILayout.Space();
                    if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Randomize"), ref randomExpanded))
                    {
                        Properties.Draw("SpikesCount");
                        Properties.Draw("MilligaussRange");
                        Properties.Draw("ProbabilityRange");

                        if (GUILayout.Button("Random"))
                        {
                            Target.EMFSpikes = new EMFAnomaly.EMFSpike[Target.SpikesCount];
                            for (int i = 0; i < Target.SpikesCount; i++)
                            {
                                float milligauss = Target.MilligaussRange.Random();
                                milligauss = (float)Math.Round(milligauss, 2);
                                Target.EMFSpikes[i].Milligauss = milligauss;

                                float probability = Target.ProbabilityRange.Random();
                                probability = (float)Math.Round(probability, 2);
                                Target.EMFSpikes[i].Probability = probability;
                            }
                        }
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Spike Settings")))
                {
                    Properties.Draw("SpikeRate");
                    Properties.Draw("MinDistance");
                    Properties.Draw("NoiseAmount");
                    Properties.Draw("NoiseSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Timer Settings")))
                {
                    using (new EditorGUI.DisabledGroupScope(Target.AnomalyDetection == EMFAnomaly.AnomalyDetect.Always))
                    {
                        Properties.Draw("TimerStartRange");
                        Properties.Draw("WeightFadeSpeed");

                        EditorGUILayout.Space(1f);
                        Properties.Draw("EMFDetectionTime");
                        using (new EditorGUI.DisabledGroupScope(Target.AnomalyDetection == EMFAnomaly.AnomalyDetect.Once))
                        {
                            Properties.Draw("AnomalyResetTime");
                        }
                    }
                }

                EditorGUILayout.Space();
                if(EditorDrawing.BeginFoldoutBorderLayout(Properties["OnAnomalyStart"], new GUIContent("Events")))
                {
                    Properties.Draw("OnAnomalyStart");
                    Properties.Draw("OnAnomalyEnd");
                    Properties.Draw("OnOutOfRange");
                    Properties.Draw("OnTimerStart");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}