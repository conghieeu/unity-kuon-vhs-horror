using System.Linq;
using UnityEngine;
using UnityEditor;
using UHFPS.Tools;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(SafePuzzle))]
    public class SafePuzzleEditor : InspectorEditor<SafePuzzle>
    {
        private GUIStyle UnlockCodeStyle
        {
            get => new(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                fontSize = 25
            };
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Safe Puzzle"), Target);
            EditorGUILayout.Space();

            GUIStyle labelStyle = new GUIStyle(EditorStyles.miniBoldLabel);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            ColorUtility.TryParseHtmlString("#F7E987", out Color textColor);
            labelStyle.normal.textColor = textColor;

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    EditorGUILayout.LabelField("Unlock Code", labelStyle);
                    EditorGUILayout.Space();
                    DrawUnlockCode();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Safe Wheel")))
                {
                    Properties.Draw("WheelObject");
                    Properties.Draw("WheelRotation");
                    Properties.Draw("WheelDistance");
                    Properties.Draw("FocusLightIntensity");
                }

                EditorGUILayout.Space(1f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Unlock Animation")))
                {
                    Properties.Draw("Animator");
                    Properties.Draw("UnlockTrigger");
                    Properties.Draw("ResetTrigger");
                }

                EditorGUILayout.Space(1f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Solution Colors")))
                {
                    Properties.Draw("SolutionNormalColor");
                    Properties.Draw("SolutionCurrentColor");
                }

                EditorGUILayout.Space(1f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Puzzle Settings")))
                {
                    Properties.Draw("UnlockedLayer");
                    EditorGUI.indentLevel++;
                    Properties.Draw("ControlsContexts");
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["LoadCallEvent"], new GUIContent("Events")))
                {
                    Properties.Draw("LoadCallEvent");
                    EditorGUILayout.Space(1f);
                    Properties.Draw("OnUnlock");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawUnlockCode()
        {
            Rect rect = EditorGUILayout.GetControlRect(false, 80);

            GUI.BeginGroup(rect);
            {
                rect.height = 50f;
                float boxWidth = 60f;
                float totalWidth = boxWidth * 3 + 2 * 10f;

                Rect outlineRect = rect;
                outlineRect.height += 20f;
                outlineRect.width = totalWidth + 20f;
                outlineRect.x = (rect.width - (totalWidth + 20f)) / 2;
                outlineRect.y = 0f;
                GUI.Box(outlineRect, GUIContent.none, EditorStyles.helpBox);

                rect.y = 10f;
                rect.x = (rect.width - totalWidth) / 2;
                rect.width = boxWidth;

                Rect solution1 = rect;
                Rect solution2 = new(solution1.xMax + 10, rect.y, boxWidth, 50f);
                Rect solution3 = new(solution2.xMax + 10, rect.y, boxWidth, 50f);

                EditorGUI.DrawRect(solution1, Color.black.Alpha(0.5f));
                EditorGUI.DrawRect(solution2, Color.black.Alpha(0.5f));
                EditorGUI.DrawRect(solution3, Color.black.Alpha(0.5f));

                SerializedProperty code = Properties["UnlockCode"];
                string number1 = code.stringValue[0..2];
                string number2 = code.stringValue[2..4];
                string number3 = code.stringValue[4..6];

                EditorGUI.BeginChangeCheck();
                number1 = EditorGUI.TextField(solution1, number1, UnlockCodeStyle);
                number2 = EditorGUI.TextField(solution2, number2, UnlockCodeStyle);
                number3 = EditorGUI.TextField(solution3, number3, UnlockCodeStyle);
                if (EditorGUI.EndChangeCheck())
                {
                    if (number1.Length > 2)
                    {
                        number1 = number1[..2];
                        GUI.FocusControl(null);
                    }
                    if (number2.Length > 2)
                    {
                        number2 = number2[..2];
                        GUI.FocusControl(null);
                    }
                    if (number3.Length > 2)
                    {
                        number3 = number3[..2];
                        GUI.FocusControl(null);
                    }
                }

                if (number1.All(char.IsDigit))
                {
                    number1 = int.Parse(number1).ToString("00");
                    code.stringValue = number1 + code.stringValue[2..];
                }

                if (number2.All(char.IsDigit))
                {
                    number2 = int.Parse(number2).ToString("00");
                    code.stringValue = code.stringValue[..2] + number2 + code.stringValue[4..];
                }

                if (number3.All(char.IsDigit))
                {
                    number3 = int.Parse(number3).ToString("00");
                    code.stringValue = code.stringValue[..4] + number3;
                }
            }
            GUI.EndGroup();
        }
    }
}