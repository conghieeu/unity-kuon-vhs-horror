using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(UIButton)), CanEditMultipleObjects]
    public class UIButtonEditor : InspectorEditor<UIButton>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("UI Button"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("ButtonImage");
                Properties.Draw("ButtonText");

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("Interactable");
                    Properties.Draw("AutoDeselectOther");

                    EditorGUILayout.Space();
                    using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Use Fade"), Properties["UseFade"], roundedBox: false))
                    {
                        Properties.Draw("FadeSpeed");
                    }

                    EditorGUILayout.Space(1f);
                    using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Pulsating"), Properties["Pulsating"], roundedBox: false))
                    {
                        Properties.Draw("PulseColor");
                        Properties.Draw("PulseSpeed");
                        Properties.Draw("PulseBlend");
                    }
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Button Colors")))
                {
                    Properties.Draw("ButtonNormal");
                    Properties.Draw("ButtonHover");
                    Properties.Draw("ButtonPressed");
                    Properties.Draw("ButtonSelected");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Text Colors")))
                {
                    Properties.Draw("TextNormal");
                    Properties.Draw("TextHover");
                    Properties.Draw("TextPressed");
                    Properties.Draw("TextSelected");
                }

                EditorGUILayout.Space();

                Properties.Draw("OnClick");
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}