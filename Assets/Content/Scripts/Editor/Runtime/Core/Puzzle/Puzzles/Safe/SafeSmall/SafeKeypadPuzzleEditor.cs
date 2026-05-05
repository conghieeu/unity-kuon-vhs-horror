using System.Linq;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(SafeKeypadPuzzle))]
    public class SafeKeypadPuzzleEditor : PuzzleEditor<SafeKeypadPuzzle>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Safe Keypad Puzzle"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                DrawAccessCode();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                using(new EditorDrawing.BorderBoxScope(new GUIContent("Unlock Animation")))
                {
                    Properties.Draw("Animator");
                    Properties.Draw("UnlockTrigger");
                    Properties.Draw("ResetTrigger");
                }

                EditorGUILayout.Space(1f);

                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Indicators"), Properties["UseIndicators"]))
                {
                    EditorGUI.indentLevel++;
                    Properties.Draw("Indicators");
                    EditorGUI.indentLevel--;
                    Properties.Draw("EmissionKeyword");
                    Properties.Draw("EmissionColor");
                    Properties.Draw("DefaultLightColor");
                    Properties.Draw("EnterLightColor");
                }

                EditorGUILayout.Space(1f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Unlock Sounds")))
                {
                    Properties.Draw("ButtonPressSound");
                    Properties.Draw("AccessGrantedSound");
                    Properties.Draw("AccessDeniedSound");
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["LoadCallEvent"], new GUIContent("Events")))
                {
                    Properties.Draw("LoadCallEvent");
                    EditorGUILayout.Space(1f);
                    Properties.Draw("OnAccessGranted");
                    Properties.Draw("OnAccessDenied");
                    Properties.Draw("OnButtonPressed");
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