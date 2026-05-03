using System.Linq;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(KeypadPuzzle))]
    public class KeypadPuzzleEditor : PuzzleEditor<KeypadPuzzle>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Keypad Puzzle"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.Draw("DisplayTextMesh");

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                DrawAccessCode();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    Properties.Draw("UseInteract");
                    EditorGUILayout.HelpBox("Use normal interaction instead of changing the player's view to puzzle mode and focusing on the keypad.", MessageType.Info);
                    EditorGUILayout.EndVertical();

                    Properties.Draw("MaxCodeLength");
                    Properties.Draw("AccessUpdateWaitTime");
                    if (Properties.BoolValue("UseInteract"))
                    {
                        Properties.Draw("SleepWaitTime");
                    }

                    EditorGUILayout.Space(2f);
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["GrantedText"], new GUIContent("Text Settings")))
                    {
                        Properties.Draw("GrantedText");
                        Properties.Draw("DeniedText");
                        Properties.Draw("TextFontSize");
                        Properties.Draw("CodeFontSize");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["DefaultColor"], new GUIContent("Color Settings")))
                    {
                        Properties.Draw("DefaultColor");
                        Properties.Draw("GrantedColor");
                        Properties.Draw("DeniedColor");
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Keypad Lights"), Properties["UseLights"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("UseLights")))
                    {
                        Properties.Draw("KeypadLight");
                        Properties.Draw("GrantedLightColor");
                        Properties.Draw("DeniedLightColor");
                    }
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Keypad Emission"), Properties["UseEmission"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("UseEmission")))
                    {
                        Properties.Draw("KeypadRenderer");
                        Properties.Draw("EmissionKeyword");
                    }
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sounds")))
                {
                    Properties.Draw("AudioSource");
                    Properties.Draw("ButtonPressSound");
                    Properties.Draw("AccessGrantedSound");
                    Properties.Draw("AccessDeniedSound");
                }

                EditorGUILayout.Space(2f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnAccessGranted"], new GUIContent("Events")))
                {
                    Properties.Draw("OnAccessGranted");
                    Properties.Draw("OnAccessDenied");
                    Properties.Draw("OnButtonPressed");
                    Properties.Draw("OnDisplayUpdate");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAccessCode()
        {
            SerializedProperty accessCode = Properties["AccessCode"];
            EditorGUILayout.PropertyField(accessCode);
            string cleanCode = new(accessCode.stringValue.Where(char.IsDigit).ToArray());
            accessCode.stringValue = cleanCode;
        }
    }
}