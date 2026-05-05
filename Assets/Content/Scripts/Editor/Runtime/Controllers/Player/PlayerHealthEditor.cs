using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(PlayerHealth))]
    public class PlayerHealthEditor : InspectorEditor<PlayerHealth>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Player Health"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Properties.Draw("MaxHealth");
                    Properties.Draw("StartHealth");

                    EditorGUILayout.Space();
                    Rect healthRect = EditorGUILayout.GetControlRect();
                    float health = Application.isPlaying ? Target.EntityHealth : Target.StartHealth;
                    float healthPercent = health / Target.MaxHealth;
                    EditorGUI.ProgressBar(healthRect, healthPercent, $"Health ({health} HP)");
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Health Settings")))
                {
                    Properties.Draw("UseHearthbeat");
                    Properties.Draw("LowHealthPulse");
                    Properties.Draw("HealthFadeTime");
                }

                EditorGUILayout.Space();
                using(new EditorDrawing.BorderBoxScope(new GUIContent("Blood Overlay")))
                {
                    Properties.Draw("MinHealthFade");
                    Properties.Draw("BloodDuration");
                    Properties.Draw("BloodFadeInSpeed");
                    Properties.Draw("BloodFadeOutSpeed");
                    Properties.Draw("CloseEyesTime");
                    Properties.Draw("CloseEyesSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Fall Damage"), Properties["EnableFallDamage"]))
                {
                    Properties.Draw("FallDistance");
                    Properties.Draw("FallDamage");
                }

                EditorGUILayout.Space();
                using(new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Damage Sounds"), Properties["UseDamageSounds"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("UseDamageSounds")))
                    {
                        EditorGUI.indentLevel++;
                        Properties.Draw("DamageSounds");
                        EditorGUI.indentLevel--;
                        Properties.Draw("DamageVolume");
                    }
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Additional Settings")))
                {
                    Properties.Draw("IsInvisibleToEnemies");
                    Properties.Draw("IsInvisibleToAllies");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}