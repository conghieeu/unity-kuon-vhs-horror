using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Tools;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(PadlockPuzzle))]
    public class PadlockPuzzleEditor : PuzzleEditor<PadlockPuzzle>
    {
        private int DigitsCount => Properties["PadlockDigits"].arraySize;
        private int MaxPadlockDigits => 5;
        private Vector2 DigitSize => new Vector2(40, 60);
        private float DigitsSpacing => 5f;
        private float LeftRightPadding => 10f;
        private float DigitsPanelSize => (LeftRightPadding * 2) + (MaxPadlockDigits * DigitSize.x) + ((MaxPadlockDigits - 1) * DigitsSpacing);

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Padlock Puzzle"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                PadlockPuzzle.PadlockTypeEnum padlockType = (PadlockPuzzle.PadlockTypeEnum)Properties["PadlockType"].enumValueIndex;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                Properties.Draw("PadlockType");
                Properties.Draw("UseInteract");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(2f);
                if (padlockType == PadlockPuzzle.PadlockTypeEnum.NumberPadlock)
                {
                    DrawDigitsList();
                    EditorGUILayout.Space(2f);

                    DrawPadlockSetup();
                    EditorGUILayout.Space(2f);
                }
                else
                {
                    using (new EditorDrawing.BorderBoxScope(new GUIContent("Key Padlock Setup")))
                    {
                        Properties.Draw("UnlockKeyItem");
                    }
                    EditorGUILayout.Space(2f);
                }

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Animation Settings")))
                {
                    Properties.Draw("Animator");
                    Properties.Draw("UnlockAnimation");
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sound Settings")))
                {
                    Properties.Draw("UnlockSound");
                }

                EditorGUILayout.Space(2f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnPadlockUnlock"], new GUIContent("Events")))
                {
                    Properties.Draw("OnPadlockUnlock");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDigitsList()
        {
            SerializedProperty digits = Properties["PadlockDigits"];
            if (EditorDrawing.BeginFoldoutBorderLayout(digits, new GUIContent("Padlock Digits List")))
            {
                digits.arraySize = EditorGUILayout.IntSlider(new GUIContent("Digits Count"), digits.arraySize, 1, MaxPadlockDigits);
                for (int i = 0; i < digits.arraySize; i++)
                {
                    SerializedProperty digitElement = digits.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(digitElement, new GUIContent("Digit " + i));
                }
                EditorDrawing.EndBorderHeaderLayout();
            }
        }

        private void DrawPadlockSetup()
        {
            using (new EditorDrawing.BorderBoxScope(new GUIContent("Number Padlock Setup")))
            {
                Rect digitsPanelRect = GUILayoutUtility.GetRect(DigitsPanelSize, 80f);
                Rect maskRect = digitsPanelRect;

                digitsPanelRect.y = 0f;
                digitsPanelRect.x = (digitsPanelRect.width / 2) - (DigitsPanelSize / 2);
                digitsPanelRect.width = DigitsPanelSize;

                GUI.BeginGroup(maskRect);
                DrawDigits(digitsPanelRect);
                GUI.EndGroup();

                if (Target.PadlockDigits.Any(x => x == null))
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox("Some lever references are not assigned. Please assign lever references first to make the levers clickable.", MessageType.Error);
                }
            }
        }

        private void DrawDigits(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            float Y = rect.height / 2 - DigitSize.y / 2;
            float X = (rect.width / 2) - ((LeftRightPadding * 2) + (DigitsCount * DigitSize.x) + ((DigitsCount - 1) * DigitsSpacing)) / 2;

            GUI.BeginGroup(rect);
            for (int x = 0; x < DigitsCount; x++)
            {
                Vector2 digitPos = new Vector2(X + LeftRightPadding + (x * DigitSize.x) + x * DigitsSpacing, Y);
                DrawDigit(new Rect(digitPos, DigitSize), x);
            }
            GUI.EndGroup();
            Repaint();
        }

        private void DrawDigit(Rect rect, int digitIndex)
        {
            Color labelColor = Color.black.Alpha(0.5f);
            EditorGUI.DrawRect(rect, labelColor);

            GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.alignment = TextAnchor.MiddleCenter;
            labelStyle.fontStyle = FontStyle.Bold;
            labelStyle.fontSize = 25;

            string digit = Target.UnlockCode[digitIndex].ToString();
            string newDigit = EditorGUI.TextField(rect, digit, labelStyle);
            if(newDigit.Length == 1 && char.IsDigit(newDigit[0]))
            {
                StringBuilder unlockCodeBuilder = new(Target.UnlockCode);
                unlockCodeBuilder.Length = DigitsCount;
                unlockCodeBuilder[digitIndex] = newDigit[0];
                Target.UnlockCode = unlockCodeBuilder.ToString();
            }
        }
    }
}