using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(InteractableLight))]
    public class InteractableLightEditor : InspectorEditor<InteractableLight>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Properties.Draw("IsSwitchedOn");
                    if (Properties.DrawGetBool("UseEnergy"))
                        Properties.DrawBacking("ConsumeWattage");
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Light Settings")))
                {
                    EditorGUI.indentLevel++;
                    Properties.Draw("LightComponents");
                    EditorGUI.indentLevel--;

                    if (Properties.DrawGetBool("SmoothLight"))
                        Properties.Draw("SmoothDuration");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Emission Settings")))
                {
                    Properties.Draw("LightMaterial");

                    EditorGUILayout.Space(2f);
                    Properties.Draw("EnableEmission");
                    Properties.Draw("EmissionKeyword");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sound Settings")))
                {
                    Properties.Draw("LightSwitchOn");
                    Properties.Draw("LightSwitchOff");
                }

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnLightOn"], new GUIContent("Events")))
                {
                    Properties.Draw("OnLightOn");
                    Properties.Draw("OnLightOff");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}