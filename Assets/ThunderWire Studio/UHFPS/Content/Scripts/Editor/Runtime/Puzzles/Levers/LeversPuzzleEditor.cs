using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Tools;
using ThunderWire.Editors;
using Random = UnityEngine.Random;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LeversPuzzle))]
    public class LeversPuzzleEditor : InspectorEditor<LeversPuzzle>
    {
        private int LeversCount => Properties["Levers"].arraySize;
        private Vector2 SingleLeverSize => new Vector2(40, 60);
        private int MaxLevers => 6;
        private float LeversSpacing => 5f;
        private float LeftRightPadding => 10f;
        private float LeversPanelSize => (LeftRightPadding * 2) + (MaxLevers * SingleLeverSize.x) + ((MaxLevers - 1) * LeversSpacing);

        private Texture2D LeverOff => Resources.Load<Texture2D>("EditorIcons/lever_off");
        private Texture2D LeverOn => Resources.Load<Texture2D>("EditorIcons/lever_on");

        private PropertyCollection leversOrderProperties;
        private PropertyCollection leversStateProperties;
        private PropertyCollection leversChainProperties;

        private LeversPuzzle.PuzzleType puzzleTypeEnum;
        private int selectedLeverIndex = -1;
        private int mouseDownIndex = -1;
        private bool mouseDown;

        public override void OnEnable()
        {
            base.OnEnable();
            mouseDown = false;
            mouseDownIndex = -1;
            selectedLeverIndex = -1;

            leversOrderProperties = EditorDrawing.GetAllProperties(Properties["LeversOrder"]);
            leversStateProperties = EditorDrawing.GetAllProperties(Properties["LeversState"]);
            leversChainProperties = EditorDrawing.GetAllProperties(Properties["LeversChain"]);
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Levers Puzzle"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                DrawLeversPuzzleTypeGroup();
                EditorGUILayout.Space();
                EditorDrawing.Separator();
                EditorGUILayout.Space();

                DrawLeversList();
                EditorGUILayout.Space(2f);

                DrawLeverSetup();
                EditorGUILayout.Space(2f);

                puzzleTypeEnum = (LeversPuzzle.PuzzleType)Properties["LeversPuzzleType"].enumValueIndex;
                switch (puzzleTypeEnum)
                {
                    case LeversPuzzle.PuzzleType.LeversOrder:
                        DrawLeversOrderProperties();
                        break;
                    case LeversPuzzle.PuzzleType.LeversState:
                        DrawLeversStateProperties();
                        break;
                    case LeversPuzzle.PuzzleType.LeversChain:
                        DrawLeversChainProperties();
                        break;
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Lever Settings")))
                {
                    Properties.Draw("LeverSwitchSpeed");
                }

                EditorGUILayout.Space(2f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnLeversCorrect"], new GUIContent("Events")))
                {
                    Properties.Draw("OnLeversCorrect");
                    Properties.Draw("OnLeversWrong");
                    Properties.Draw("OnLeverChanged");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLeversPuzzleTypeGroup()
        {
            GUIContent[] toolbarContent = {
                new GUIContent(Resources.Load<Texture>("EditorIcons/levers_order"), "Levers Order"),
                new GUIContent(Resources.Load<Texture>("EditorIcons/levers_state"), "Levers State"),
                new GUIContent(Resources.Load<Texture>("EditorIcons/levers_chain"), "Levers Chain"),
            };

            using (new EditorDrawing.IconSizeScope(25))
            {
                GUIStyle toolbarButtons = new GUIStyle(GUI.skin.button);
                toolbarButtons.fixedHeight = 0;
                toolbarButtons.fixedWidth = 50;

                Rect toolbarRect = EditorGUILayout.GetControlRect(false, 30);
                toolbarRect.width = toolbarButtons.fixedWidth * toolbarContent.Length;
                toolbarRect.x = EditorGUIUtility.currentViewWidth / 2 - toolbarRect.width / 2 + 7f;

                EditorGUI.BeginChangeCheck();
                SerializedProperty puzzleType = Properties["LeversPuzzleType"];
                puzzleType.enumValueIndex = GUI.Toolbar(toolbarRect, puzzleType.enumValueIndex, toolbarContent, toolbarButtons);
                if (EditorGUI.EndChangeCheck()) OnLeversPuzzleTypeChanged();
            }
        }
    
        private void DrawLeversList()
        {
            SerializedProperty levers = Properties["Levers"];
            if (EditorDrawing.BeginFoldoutBorderLayout(levers, new GUIContent("Levers List")))
            {
                EditorGUI.BeginChangeCheck();
                levers.arraySize = EditorGUILayout.IntSlider(new GUIContent("Levers Count"), levers.arraySize, 0, MaxLevers);
                if (EditorGUI.EndChangeCheck()) OnLeversCountChanged();

                for (int i = 0; i < levers.arraySize; i++)
                {
                    SerializedProperty leverElement = levers.GetArrayElementAtIndex(i);
                    EditorGUILayout.PropertyField(leverElement, new GUIContent("Lever " + i));
                }
                EditorDrawing.EndBorderHeaderLayout();
            }
        }

        private void DrawLeverSetup()
        {
            using (new EditorDrawing.BorderBoxScope(new GUIContent("Levers Puzzle Setup")))
            {
                Rect leversPanelRect = GUILayoutUtility.GetRect(LeversPanelSize, 100f);
                Rect maskRect = leversPanelRect;

                leversPanelRect.y = 0f;
                leversPanelRect.x = (leversPanelRect.width / 2) - (LeversPanelSize / 2);
                leversPanelRect.width = LeversPanelSize;

                GUI.BeginGroup(maskRect);
                DrawLevers(leversPanelRect);
                GUI.EndGroup();

                EditorGUILayout.Space();
                if (Target.Levers.Any(x => x == null))
                {
                    EditorGUILayout.HelpBox("Some lever references are not assigned. Please assign lever references first to make the levers clickable.", MessageType.Error);
                }
            }
        }

        private void DrawLeversOrderProperties()
        {
            SerializedProperty leversOrder = leversOrderProperties["LeversOrder"];

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Levers Order")))
            {
                EditorGUILayout.HelpBox("Define the order in which the levers interact by clicking on the levers at the top. Changing the number of levers will clear the currently defined order.", MessageType.Info);
                EditorGUILayout.Space(2f);

                EditorGUILayout.TextField(new GUIContent("Levers Order"), leversOrder.stringValue);

                if (GUILayout.Button("Randomize", GUILayout.Height(23f)))
                {
                    leversOrder.stringValue = "";
                    for (int i = 0; i < LeversCount; i++)
                    {
                        int random = Random.Range(0, LeversCount);
                        leversOrder.stringValue += random.ToString();
                    }
                }

                if (GUILayout.Button("Reset", GUILayout.Height(23f)))
                {
                    leversOrder.stringValue = "";
                }
            }
        }

        private void DrawLeversStateProperties()
        {
            SerializedProperty leverStates = leversStateProperties["LeverStates"];

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Levers State")))
            {
                EditorGUILayout.HelpBox("Define the state of the levers, which is correct, by clicking on the levers at the top. Changing the number of levers will clear the currently defined levers state.", MessageType.Info);
                EditorGUILayout.Space(2f);

                if (GUILayout.Button("Randomize", GUILayout.Height(23f)))
                {
                    for (int i = 0; i < LeversCount; i++)
                    {
                        SerializedProperty lever = leverStates.GetArrayElementAtIndex(i);
                        lever.boolValue = Random.Range(0, 2) != 0;
                    }
                }

                if (GUILayout.Button("Reset", GUILayout.Height(23f)))
                {
                    for (int i = 0; i < LeversCount; i++)
                    {
                        SerializedProperty lever = leverStates.GetArrayElementAtIndex(i);
                        lever.boolValue = false;
                    }
                }
            }
        }

        private void DrawLeversChainProperties()
        {
            SerializedProperty leversChains = leversChainProperties["LeversChains"];

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Levers Chain")))
            {
                EditorGUILayout.HelpBox("Define the levers chain reaction by selecting the first lever and clicking on the other levers at the top. Changing the number of levers will clear the currently defined levers state.", MessageType.Info);
                EditorGUILayout.Space(2f);

                if (Target.LeversChain.LeversChains.Any(x => x.ChainIndex.Count > 0))
                {
                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                    {
                        for (int i = 0; i < LeversCount; i++)
                        {
                            var leverChain = Target.LeversChain.LeversChains[i];
                            if (leverChain.ChainIndex.Count > 0)
                            {
                                EditorGUILayout.LabelField($"<b>[Lever {i}]</b>: {string.Join(", ", leverChain.ChainIndex)}", EditorDrawing.Styles.RichLabel);
                            }
                        }
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(2f);
                }

                SerializedProperty maxLeverReactions = leversChainProperties["MaxLeverReactions"];
                SerializedProperty maxReactiveLevers = leversChainProperties["MaxReactiveLevers"];

                EditorGUILayout.IntSlider(maxLeverReactions, 1, LeversCount - 1);
                EditorGUILayout.IntSlider(maxReactiveLevers, 1, LeversCount - 1);
                EditorGUILayout.Space(2f);

                if (GUILayout.Button("Randomize", GUILayout.Height(23f)))
                {
                    leversChains.arraySize = 0;
                    leversChains.arraySize = LeversCount;
                    List<int> randomLever = new();

                    for (int i = 0; i < maxReactiveLevers.intValue; i++)
                    {
                        int leverIndex = GameTools.RandomExclude(0, LeversCount, randomLever.ToArray());
                        randomLever.Add(leverIndex);

                        SerializedProperty lever = leversChains.GetArrayElementAtIndex(leverIndex);
                        SerializedProperty chainIndex = lever.FindPropertyRelative("ChainIndex");

                        int chainsCount = Random.Range(1, maxLeverReactions.intValue + 1);
                        chainIndex.arraySize = chainsCount;

                        for (int j = 0; j < chainsCount; j++)
                        {
                            List<int> current = new();
                            for (int k = 0; k < j; k++)
                            {
                                SerializedProperty currChain = chainIndex.GetArrayElementAtIndex(k);
                                current.Add(currChain.intValue);
                            }

                            SerializedProperty chain = chainIndex.GetArrayElementAtIndex(j);
                            chain.intValue = GameTools.RandomExcludeUnique(0, LeversCount, new int[] { leverIndex }, current.Distinct().ToArray());
                        }
                    }

                    serializedObject.ApplyModifiedProperties();
                    foreach (var lever in Target.LeversChain.LeversChains)
                    {
                        lever.ChainIndex = lever.ChainIndex.OrderBy(x => x).ToList();
                    }

                    mouseDown = false;
                    mouseDownIndex = -1;
                    selectedLeverIndex = -1;
                }

                if (GUILayout.Button("Reset", GUILayout.Height(23f)))
                {
                    leversChains.arraySize = 0;
                    leversChains.arraySize = LeversCount;

                    mouseDown = false;
                    mouseDownIndex = -1;
                    selectedLeverIndex = -1;
                }
            }
        }

        private void DrawLevers(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            float Y = rect.height / 2 - SingleLeverSize.y / 2 - 10f;
            float X = (rect.width / 2) - ((LeftRightPadding * 2) + (LeversCount * SingleLeverSize.x) + ((LeversCount - 1) * LeversSpacing)) / 2;

            GUI.BeginGroup(rect);
            {
                if (LeversCount > 0)
                {
                    for (int x = 0; x < LeversCount; x++)
                    {
                        Vector2 leverPos = new Vector2(X + LeftRightPadding + (x * SingleLeverSize.x) + x * LeversSpacing, Y);
                        DrawLever(new Rect(leverPos, SingleLeverSize), x);
                    }
                }
                else
                {
                    GUIContent labelText = new GUIContent("Change the Levers Count");
                    Vector2 labelSize = EditorStyles.label.CalcSize(labelText);
                    float xPos = (rect.width / 2) - (labelSize.x / 2);
                    EditorGUI.LabelField(new Rect(xPos, 0, labelSize.x, rect.height), labelText);
                }
            }
            GUI.EndGroup();
            Repaint();
        }

        private void DrawLever(Rect rect, int index)
        {
            SerializedProperty lever = Properties["Levers"].GetArrayElementAtIndex(index);
            bool leverState = false;
            bool onHover = false;

            Color backgroundColor = Color.black.Alpha(0.5f);
            Event e = Event.current;

            if (lever.objectReferenceValue != null && rect.Contains(e.mousePosition))
            {
                backgroundColor = Color.white.Alpha(0.35f);
                onHover = true;

                if (!mouseDown && e.type == EventType.MouseDown && e.button == 0)
                {
                    mouseDown = true;
                    OnLeverMouseDown(index);
                }
                else if(mouseDown && e.type == EventType.MouseUp)
                {
                    mouseDown = false;
                    mouseDownIndex = -1;
                }

                if(mouseDown) backgroundColor = Color.black.Alpha(0.5f);
            }

            leverState = OnDrawLever(rect, index, leverState, backgroundColor, onHover);

            Vector2 labelOffset = new Vector2(0, SingleLeverSize.y + LeversSpacing);
            Rect labelPos = new Rect(rect.position + labelOffset, new Vector2(SingleLeverSize.x, 15f));
            Color labelColor = Color.black.Alpha(0.5f);

            // draw lever label
            EditorGUI.DrawRect(labelPos, labelColor);
            GUI.Label(labelPos, index.ToString(), EditorDrawing.CenterStyle(EditorStyles.miniBoldLabel));

            // draw lever texture
            Texture2D leverStateTex = leverState ? LeverOn : LeverOff;
            EditorDrawing.DrawTransparentTexture(rect, leverStateTex);
        }

        private bool OnDrawLever(Rect rect, int index, bool leverState, Color backgroundColor, bool onHover)
        {
            if(puzzleTypeEnum == LeversPuzzle.PuzzleType.LeversChain)
            {
                if(selectedLeverIndex >= 0 && !(mouseDownIndex == index && mouseDown))
                {
                    if (selectedLeverIndex == index || onHover)
                    {
                         backgroundColor = Color.white.Alpha(0.35f);
                    }
                    else
                    {
                        var leversChain = Target.LeversChain.LeversChains[selectedLeverIndex];
                        if (leversChain.ChainIndex.Contains(index))
                            backgroundColor = Color.cyan.Alpha(0.35f);
                    }
                }
            }

            EditorGUI.DrawRect(rect, backgroundColor);

            if (puzzleTypeEnum == LeversPuzzle.PuzzleType.LeversState)
            {
                leverState = Target.LeversState.LeverStates[index];

                Rect stateRect = rect;
                stateRect.height = 2f;
                stateRect.xMin += 5f;
                stateRect.xMax -= 5f;
                stateRect.y = rect.yMax - 4f;

                Color stateColor = Color.red.Alpha(0.6f);
                if (leverState) stateColor = Color.green.Alpha(0.6f);

                EditorGUI.DrawRect(stateRect, stateColor);
            }

            return leverState;
        }

        private void OnLeversCountChanged()
        {
            leversOrderProperties["LeversOrder"].stringValue = "";
            leversStateProperties["LeverStates"].arraySize = LeversCount;
            leversChainProperties["LeversChains"].arraySize = LeversCount;
            serializedObject.ApplyModifiedProperties();

            mouseDown = false;
            mouseDownIndex = -1;
            selectedLeverIndex = -1;
        }

        private void OnLeversPuzzleTypeChanged()
        {
            mouseDown = false;
            mouseDownIndex = -1;
            selectedLeverIndex = -1;
        }

        private void OnLeverMouseDown(int index)
        {
            mouseDownIndex = index;

            if (puzzleTypeEnum == LeversPuzzle.PuzzleType.LeversOrder)
            {
                if(Target.LeversOrder.LeversOrder.Length < LeversCount)
                    Target.LeversOrder.LeversOrder += index.ToString();
            }
            else if(puzzleTypeEnum == LeversPuzzle.PuzzleType.LeversState)
            {
                var leverState = Target.LeversState.LeverStates[index];
                Target.LeversState.LeverStates[index] = !leverState;
            }
            else if(puzzleTypeEnum == LeversPuzzle.PuzzleType.LeversChain)
            {
                if(selectedLeverIndex == index)
                {
                    selectedLeverIndex = -1;
                    return;
                }

                if (selectedLeverIndex < 0)
                {
                    selectedLeverIndex = index;
                }
                else
                {
                    var leversChain = Target.LeversChain.LeversChains[selectedLeverIndex];
                    leversChain.ChainIndex = leversChain.ChainIndex.Contains(index)
                        ? leversChain.ChainIndex.Except(new int[] { index }).ToList()
                        : leversChain.ChainIndex.Concat(new int[] { index }).ToList();
                }
            }
        }
    }
}