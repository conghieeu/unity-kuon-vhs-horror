using System.Linq;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Tools;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ElectricalCircuitComponent))]
    public class ElectricalCircuitComponentEditor : Editor
    {
        public ElectricalCircuitComponent Target;
        public PropertyCollection Properties;

        private int selectedFlowIndex = 0;

        private void OnEnable()
        {
            Target = target as ElectricalCircuitComponent;
            Properties = EditorDrawing.GetAllProperties(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Electrical Circuit Component"), target);
            EditorGUILayout.Space();

            serializedObject.Update();

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Component Base")))
            {
                Properties.Draw("ComponentIcon");
                Properties.Draw("ComponentMesh");
                Properties.Draw("ComponentUp");
            }

            EditorGUILayout.Space();
            using (new EditorDrawing.BorderBoxScope(new GUIContent("Component Builder")))
            {
                float componentSize = 120f;
                Rect componentRect = GUILayoutUtility.GetRect(componentSize, componentSize);
                Rect maskRect = componentRect;
                componentRect.y = 0f;
                componentRect.x = (componentRect.width / 2) - (componentSize / 2);
                componentRect.width = componentSize;

                GUI.BeginGroup(maskRect);
                DrawComponentPreview(componentRect);
                GUI.EndGroup();

                EditorGUILayout.Space(6f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Flow Directions")))
                {
                    for (int i = 0; i < Target.FlowDirections.Count; i++)
                    {
                        SerializedProperty flowDirection = Properties["FlowDirections"].GetArrayElementAtIndex(i);

                        string label = ElectricalCircuitPuzzleEditor.ALPHA[i].ToString();
                        GUIContent headerLabel = new GUIContent($"Flow Label: [{label}]");

                        if (EditorDrawing.BeginFoldoutBorderLayout(flowDirection, headerLabel, out Rect headerRect, 70f, 21f))
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(flowDirection.FindPropertyRelative("FlowRenderer"));
                            EditorGUI.indentLevel--;

                            if (Application.isPlaying && i <= Target.PowerFlows.Length)
                            {
                                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.TextField("Power Flows", string.Join(", ", Target.PowerFlows[i].PowerFlows));
                                }
                            }

                            EditorDrawing.EndBorderHeaderLayout();
                        }

                        Rect removeRect = headerRect;
                        removeRect.xMin = removeRect.xMax - EditorGUIUtility.singleLineHeight - 2f;
                        removeRect.y += 4f;
                        removeRect.width = EditorGUIUtility.singleLineHeight;

                        if (GUI.Button(removeRect, EditorUtils.Styles.MinusIcon, EditorStyles.iconButton))
                        {
                            Properties["FlowDirections"].DeleteArrayElementAtIndex(i);
                            break;
                        }

                        Rect buttonRect = headerRect;
                        buttonRect.height = EditorGUIUtility.singleLineHeight + 1f;
                        buttonRect.xMin = buttonRect.xMax - 65f;
                        buttonRect.y += 3f;
                        buttonRect.x -= 4f + EditorGUIUtility.singleLineHeight;

                        if (GUI.Toggle(buttonRect, selectedFlowIndex == i, "Select", GUI.skin.button))
                        {
                            selectedFlowIndex = i;
                        }

                        if(i != Target.FlowDirections.Count - 1)
                            EditorGUILayout.Space(1f);
                    }

                    EditorGUILayout.Space(2f);
                    if (GUILayout.Button("Add Circuit Flow"))
                    {
                        Target.FlowDirections.Add(new());
                    }
                }
            }

            EditorGUILayout.Space();
            using (new EditorDrawing.BorderBoxScope(new GUIContent("Debug")))
            {
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    Properties.Draw("Coords");
                    Properties.Draw("Angle");
                }
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Rotate", GUILayout.Height(25f)))
            {
                Target.InteractStart();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawComponentPreview(Rect rect)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            float spacing = 5f;
            float iconSize = rect.width / 2;

            Rect centerIconRect = rect;
            centerIconRect.width = iconSize;
            centerIconRect.height = iconSize;
            centerIconRect.x += rect.width / 2 - centerIconRect.width / 2;
            centerIconRect.y += rect.height / 2 - centerIconRect.height / 2;

            EditorGUI.DrawRect(centerIconRect, Color.black.Alpha(0.5f));
            Texture icon = Properties["ComponentIcon"].objectReferenceValue as Texture;
            if(icon != null) EditorDrawing.DrawTransparentTexture(centerIconRect, icon);

            float directionBoxSize = centerIconRect.width * 0.3f;

            Rect leftDirection = centerIconRect;
            leftDirection.width = directionBoxSize;
            leftDirection.x -= spacing + leftDirection.width;
            DrawDirectionBox(leftDirection, PartDirection.Left);

            Rect upDirection = centerIconRect;
            upDirection.height = directionBoxSize;
            upDirection.y -= spacing + upDirection.height;
            DrawDirectionBox(upDirection, PartDirection.Up);

            Rect rightDirection = centerIconRect;
            rightDirection.width = directionBoxSize;
            rightDirection.x += spacing + rightDirection.height;
            DrawDirectionBox(rightDirection, PartDirection.Right);

            Rect downDirection = centerIconRect;
            downDirection.height = directionBoxSize;
            downDirection.y += spacing + downDirection.width;
            DrawDirectionBox(downDirection, PartDirection.Down);

            Repaint();
        }

        private void DrawDirectionBox(Rect rect, PartDirection direction)
        {
            Color rectColor = Color.black.Alpha(0.5f);
            int alphaIndex = -1;

            if (Target.FlowDirections.Any(x => x.FlowDirections.Any(y => y == direction)))
            {
                bool flag = false;

                for (int i = 0; i < Target.FlowDirections.Count; i++)
                {
                    foreach (var flowDirection in Target.FlowDirections[i].FlowDirections)
                    {
                        if (flowDirection == direction) 
                        { 
                            alphaIndex = i;
                            flag = true;
                            break;
                        }
                    }

                    if (flag) break;
                }

                rectColor = AlphaToColor(alphaIndex).Alpha(0.3f);
            }

            Event e = Event.current;
            if (rect.Contains(e.mousePosition))
            {
                rectColor = Color.white.Alpha(0.35f);
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    if (selectedFlowIndex < Target.FlowDirections.Count && !Target.FlowDirections.Any(x => x != Target.FlowDirections[selectedFlowIndex] && x.FlowDirections.Any(x => x == direction)))
                    {
                        if (!Target.FlowDirections[selectedFlowIndex].FlowDirections.Contains(direction))
                        {
                            Target.FlowDirections[selectedFlowIndex].FlowDirections.Add(direction);
                        }
                        else
                        {
                            Target.FlowDirections[selectedFlowIndex].FlowDirections.Remove(direction);
                        }
                    }
                }
            }

            EditorGUI.DrawRect(rect, rectColor);
            string label = alphaIndex > -1 ? ElectricalCircuitPuzzleEditor.ALPHA[alphaIndex].ToString() : "-";
            GUI.Label(rect, label, EditorDrawing.CenterStyle(EditorStyles.miniBoldLabel));
        }

        private Color AlphaToColor(int alpha)
        {
            return alpha switch
            {
                0 => Color.green,
                1 => Color.red,
                2 => Color.cyan,
                3 => Color.yellow,
                _ => Color.black
            };
        }
    }
}