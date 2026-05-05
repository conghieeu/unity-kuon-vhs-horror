using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Tools;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ElectricalCircuitPuzzle))]
    public class ElectricalCircuitPuzzleEditor : PuzzleEditor<ElectricalCircuitPuzzle>
    {
        public const string ALPHA = "ABCDEFGHIKLMNOPQRSTVXYZ";
        private readonly bool[] foldout = new bool[1];
        private int circuitEditType = 0;

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Electrical Circuit Puzzle"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Circuit Builder")))
                {
                    float circuitPreviewSize = 270f;
                    Rect circuitPreviewRect = GUILayoutUtility.GetRect(circuitPreviewSize, circuitPreviewSize);
                    Rect maskRect = circuitPreviewRect;
                    circuitPreviewRect.y = 0f;
                    circuitPreviewRect.x = (circuitPreviewRect.width / 2) - (circuitPreviewSize / 2);
                    circuitPreviewRect.width = circuitPreviewSize;

                    GUI.BeginGroup(maskRect);
                    DrawCircuitPreview(circuitPreviewRect, Target.Rows, Target.Columns);
                    GUI.EndGroup();

                    EditorGUILayout.Space(2f);

                    Rect circuitEditButtonsRect = GUILayoutUtility.GetRect(1f, 20f);
                    circuitEditButtonsRect.x = (circuitEditButtonsRect.width / 2) - (170f / 2) + 23f;
                    circuitEditButtonsRect.width = 170f;
                    circuitEditType = GUI.Toolbar(circuitEditButtonsRect, circuitEditType, new string[] { "Change", "Rotate", "Clear" });

                    EditorGUILayout.Space(2f);

                    Rect circuitRandomRect = GUILayoutUtility.GetRect(1f, 20f);
                    circuitRandomRect.x = (circuitRandomRect.width / 2) - (100f / 2) + 23f;
                    circuitRandomRect.width = 100f;
                    if (GUI.Button(circuitRandomRect, "Randomize"))
                    {
                        Undo.RegisterFullObjectHierarchyUndo(Target, "Randomize Circuit Puzzle");
                        foreach (var component in Target.ComponentsFlow)
                        {
                            System.Random random = new System.Random();
                            component.Component = Target.CircuitComponents[random.Next(0, Target.CircuitComponents.Length)];
                            component.Rotation = random.Next(1, 4) * 90;
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUI.BeginChangeCheck();
                    {
                        Target.Rows = (ushort)EditorGUILayout.Slider(new GUIContent("Rows"), Target.Rows, 1, 5);
                        Target.Columns = (ushort)EditorGUILayout.Slider(new GUIContent("Columns"), Target.Columns, 1, 5);
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RegisterFullObjectHierarchyUndo(Target, "Rows or Columns Change");
                        Target.PowerFlow = new ElectricalCircuitPuzzle.PowerComponent[Target.Rows * Target.Columns];
                        Target.ComponentsFlow = new ElectricalCircuitPuzzle.ComponentFlow[Target.Rows * Target.Columns];
                        Target.InputEvents.Clear();
                        serializedObject.ApplyModifiedProperties();
                        return;
                    }

                    EditorGUILayout.HelpBox("Changing the number of rows or columns resets the entire circuit!", MessageType.Warning);

                    var powerFlowArray = Target.PowerFlow
                        .Where(x => x.PowerType != PowerType.None && x.PowerID > 0)
                        .OrderBy(x => x.PowerType);

                    if (powerFlowArray.Count() > 0)
                    {
                        EditorGUILayout.Space();
                        using (new EditorDrawing.BorderBoxScope(new GUIContent("Power Flow")))
                        {
                            foreach (var component in powerFlowArray)
                            {
                                int alphaPowerID = component.PowerID - 1;

                                if (component.PowerType == PowerType.Output)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    {
                                        EditorGUILayout.PrefixLabel($"[{ALPHA[alphaPowerID]}, {component.PowerDirection}, {component.PowerID}] <b>{component.PowerType}</b>", "Button", EditorDrawing.Styles.RichLabel);

                                        IDictionary<string, ElectricalCircuitPuzzle.PowerComponent> contents = Target.PowerFlow
                                            .Where(x => x.PowerType == PowerType.Input && x.PowerID > 0)
                                            .ToDictionary(x => $"[{ALPHA[x.PowerID - 1]}, {x.PowerDirection}] {x.PowerType}", y => y);

                                        string selected = contents.Where(x => x.Value.PowerID == component.ConnectPowerID)
                                            .Select(x => x.Key)
                                            .FirstOrDefault();

                                        Rect popupRect = EditorGUILayout.GetControlRect();
                                        EditorDrawing.DrawStringSelectPopup(popupRect, new GUIContent("Outputs To"), contents.Keys.ToArray(), selected, selection =>
                                        {
                                            component.ConnectPowerID = contents[selection].PowerID;
                                            serializedObject.ApplyModifiedProperties();
                                        });
                                    }
                                    EditorGUILayout.EndHorizontal();
                                }
                                else if (component.PowerType == PowerType.Input)
                                {
                                    EditorGUILayout.LabelField($"[{ALPHA[alphaPowerID]}, {component.PowerDirection}, {component.PowerID}] <b>{component.PowerType}</b>", EditorDrawing.Styles.RichLabel);
                                }
                            }
                        }
                    }

                    EditorGUILayout.Space();
                    using (new EditorDrawing.BorderBoxScope(new GUIContent("Circuit Settings")))
                    {
                        EditorGUI.indentLevel++;
                        Properties.Draw("CircuitComponents");
                        EditorGUI.indentLevel--;

                        EditorGUILayout.Space();
                        Properties.Draw("ComponentsParent");
                        Properties.Draw("ComponentsSpacing");
                        Properties.Draw("ComponentsSize");

                        if (Properties.DrawGetBool("DisableWhenConnected"))
                            Properties.Draw("PowerConnectedWaitTime");
                    }

                    EditorGUILayout.Space();
                    bool flag1 = Properties["CircuitComponents"].arraySize == 0;
                    bool flag2 = !Target.ComponentsFlow.Any(x => x.Component != null);
                    using (new EditorGUI.DisabledGroupScope(flag1 || flag2))
                    {
                        if (GUILayout.Button("Build Circuit", GUILayout.Height(25)))
                        {
                            BuildCircuit(false);
                        }

                        if (GUILayout.Button("Build Circuit Random", GUILayout.Height(25)))
                        {
                            BuildCircuit(true);
                        }
                    }
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sounds")))
                {
                    Properties.Draw("RotateComponent");
                    Properties.Draw("PowerConnected");
                    Properties.Draw("PowerDisconnected");
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Events")))
                {
                    for (int i = 0; i < Properties["InputEvents"].arraySize; i++)
                    {
                        SerializedProperty input = Properties["InputEvents"].GetArrayElementAtIndex(i);
                        var inputEvent = Target.InputEvents[i];
                        var powerComp = inputEvent.PowerComponent;

                        if (inputEvent.PowerComponent.PowerType == PowerType.Input)
                        {
                            SerializedProperty inputLight = input.FindPropertyRelative("InputLight");
                            SerializedProperty onConnected = input.FindPropertyRelative("OnConnected");
                            SerializedProperty onDisconnected = input.FindPropertyRelative("OnDisconnected");

                            if (EditorDrawing.BeginFoldoutBorderLayout(input, new GUIContent($"[{ALPHA[powerComp.PowerID - 1]}, {powerComp.PowerDirection}, {powerComp.PowerID}] Input Events")))
                            {
                                EditorGUILayout.PropertyField(inputLight);
                                EditorGUILayout.Space(1f);

                                if (EditorDrawing.BeginFoldoutBorderLayout(onConnected, new GUIContent("Events")))
                                {
                                    EditorGUILayout.PropertyField(onConnected);
                                    EditorGUILayout.Space(1f);
                                    EditorGUILayout.PropertyField(onDisconnected);
                                    EditorDrawing.EndBorderHeaderLayout();
                                }

                                EditorDrawing.EndBorderHeaderLayout();
                            }
                        }

                        EditorGUILayout.Space(1f);
                    }

                    if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Global Events"), ref foldout[0]))
                    {
                        Properties.Draw("OnConnected");
                        EditorGUILayout.Space(1f);
                        Properties.Draw("OnDisconnected");
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCircuitPreview(Rect rect, int rows, int columns)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            float spacing = 5f;
            float slotSize = ((rect.width - spacing) / 5f) - spacing;
            float powerSlotSize = slotSize * 0.3f;
            slotSize = ((rect.width - powerSlotSize - spacing * 2) / 5f) - spacing * 2;

            float Y = (rect.height / 2) - (rows * slotSize + spacing * (rows + 1)) / 2;
            float X = (rect.width / 2) - (columns * slotSize + spacing * (columns + 1)) / 2;

            GUI.BeginGroup(rect);
            for (int y = 0; y < rows; y++)
            {
                Vector2 slotPosition = new Vector2(X + spacing, Y + y * slotSize + spacing * (y + 1));

                for (int x = 0; x < columns; x++)
                {
                    Vector2 localSlotPosition = slotPosition + (x * new Vector2(slotSize + spacing, 0));

                    if(y == 0)
                    {
                        Vector2 powerYPos = new Vector2(localSlotPosition.x, localSlotPosition.y - spacing - powerSlotSize);
                        DrawCircuitPower(new Rect(powerYPos, new Vector2(slotSize, powerSlotSize)), x, y, PartDirection.Up);
                    }
                    if(y == rows - 1)
                    {
                        Vector2 powerYPos = new Vector2(localSlotPosition.x, localSlotPosition.y + spacing + slotSize);
                        DrawCircuitPower(new Rect(powerYPos, new Vector2(slotSize, powerSlotSize)), x, y, PartDirection.Down);
                    }

                    if(x == 0)
                    {
                        Vector2 powerXPos = new Vector2(localSlotPosition.x - spacing - powerSlotSize, localSlotPosition.y);
                        DrawCircuitPower(new Rect(powerXPos, new Vector2(powerSlotSize, slotSize)), x, y, PartDirection.Left);
                    }
                    if(x == columns - 1)
                    {
                        Vector2 powerXPos = new Vector2(localSlotPosition.x + spacing + slotSize, localSlotPosition.y);
                        DrawCircuitPower(new Rect(powerXPos, new Vector2(powerSlotSize, slotSize)), x, y, PartDirection.Right);
                    }

                    DrawCircuitSlot(new Rect(localSlotPosition, new Vector2(slotSize, slotSize)), x, y);
                }
            }
            GUI.EndGroup();
            Repaint();
        }

        private void DrawCircuitPower(Rect rect, int x, int y, PartDirection direction)
        {
            // set normal rect color
            Color rectColor = Color.black.Alpha(0.5f);

            int index = y * Target.Columns + x;
            ElectricalCircuitPuzzle.PowerComponent powerComponent = Target.PowerFlow[index];
            PowerType powerType = powerComponent.PowerType;
            PartDirection powerDirection = powerComponent.PowerDirection;

            // set rect color depending of the power type
            if (powerDirection == direction)
            {
                if (powerType == PowerType.Output) rectColor = Color.green.Alpha(0.35f);
                else if (powerType == PowerType.Input) rectColor = Color.red.Alpha(0.35f);
            }

            Event e = Event.current;
            if (rect.Contains(e.mousePosition))
            {
                rectColor = Color.white.Alpha(0.35f);
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    // register undo
                    Undo.RegisterFullObjectHierarchyUndo(Target, "Circuit Power Change");

                    // reset output component connected power id
                    if (powerType == PowerType.Output) powerComponent.ConnectPowerID = 0;
                    else if (powerType == PowerType.Input)
                    {
                        foreach (var flow in Target.PowerFlow)
                        {
                            if (flow.PowerType == PowerType.Output && flow.ConnectPowerID == powerComponent.PowerID)
                                flow.ConnectPowerID = 0;
                        }
                    }

                    // reset component variables when changing power direction
                    if (powerDirection != direction)
                    {
                        powerType = PowerType.None;
                        powerComponent.PowerID = 0;
                        powerComponent.ConnectPowerID = 0;
                    }

                    // change power type and direction
                    int powerTypeEnumCount = Enum.GetValues(typeof(PowerType)).Length;
                    powerComponent.PowerType = (PowerType)((int)(powerType + 1) % powerTypeEnumCount);
                    powerComponent.PowerDirection = direction;

                    // assign power id
                    if (powerComponent.PowerType != PowerType.None)
                    {
                        for (int i = 0; i < ALPHA.Length; i++)
                        {
                            if (!Target.PowerFlow.Any(x => x.PowerType == powerComponent.PowerType && x.PowerID == i + 1))
                            {
                                powerComponent.PowerID = i + 1;
                                break;
                            }
                        }
                    }
                    else
                    {
                        powerComponent.PowerID = 0;
                    }

                    // update the input events list
                    if (powerComponent.PowerType == PowerType.Input && !Target.InputEvents.Any(x => x.PowerComponent == powerComponent))
                    {
                        Target.InputEvents.Add(new ElectricalCircuitPuzzle.PowerInputEvents() { PowerComponent = powerComponent });
                    }
                    else if (powerComponent.PowerType == PowerType.None)
                    {
                        Target.InputEvents.RemoveAll(x => x.PowerComponent.PowerID == 0);
                    }

                    // apply changes
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.UpdateIfRequiredOrScript();
                    e.Use();
                }
            }

            // draw rect and alphabet label
            EditorGUI.DrawRect(rect, rectColor);
            if (powerType != PowerType.None && powerDirection == direction)
            {
                string label = powerComponent.PowerID > 0 ? ALPHA[powerComponent.PowerID - 1].ToString() : "-";
                GUI.Label(rect, label, EditorDrawing.CenterStyle(EditorStyles.miniBoldLabel));
            }
        }

        private void DrawCircuitSlot(Rect rect, int x, int y)
        {
            Color rectColor = Color.black.Alpha(0.5f);

            int index = y * Target.Columns + x;
            ElectricalCircuitPuzzle.ComponentFlow componentFlow = Target.ComponentsFlow.Length > 0 ? Target.ComponentsFlow[index] : new();

            Event e = Event.current;
            if (rect.Contains(e.mousePosition))
            {
                rectColor = Color.white.Alpha(0.35f);
                if (e.type == EventType.MouseDown && e.button == 0)
                {
                    // register undo
                    Undo.RegisterFullObjectHierarchyUndo(Target, "Circuit Slot Change");

                    // chnage component flow component
                    if (circuitEditType == 0)
                    {
                        int componentIndex = Array.IndexOf(Target.CircuitComponents, componentFlow.Component);
                        componentFlow.Component = componentIndex + 1 > Target.CircuitComponents.Count() - 1 ? null
                            : Target.CircuitComponents[componentIndex + 1];
                    }
                    // rotate component flow
                    else if(circuitEditType == 1)
                    {
                        componentFlow.Rotation = (componentFlow.Rotation + 90) % 360;
                    }
                    // clear component
                    else if(circuitEditType == 2)
                    {
                        componentFlow.Component = null;
                    }

                    // apply changes
                    serializedObject.ApplyModifiedProperties();
                    e.Use();
                }
            }

            // draw rect
            EditorGUI.DrawRect(rect, rectColor);

            // draw component icon with rotation
            Matrix4x4 matrix = GUI.matrix;
            GUIUtility.RotateAroundPivot(componentFlow.Rotation, new Vector2(rect.xMin + rect.width * 0.5f, rect.yMin + rect.height * 0.5f));
            if(componentFlow.Component != null && componentFlow.Component.ComponentIcon != null)
                EditorDrawing.DrawTransparentTexture(rect, componentFlow.Component.ComponentIcon);
            GUI.matrix = matrix;
        }

        private void BuildCircuit(bool random)
        {
            foreach (var component in Target.Components)
            {
                if (component.TryGetComponent(out Collider collider))
                    Target.CollidersEnable.Remove(collider);
            }

            Target.Components.ForEach(x => DestroyImmediate(x.gameObject));
            Target.Components.Clear();

            float componentSize = Target.CircuitComponents[0].ComponentMesh.sharedMesh.bounds.size.x;
            componentSize *= Target.ComponentsSize;
            float panelSize = componentSize * Target.Columns + Target.ComponentsSpacing * (Target.Columns - 1);

            Vector2 localStart = new Vector2(panelSize, panelSize) / 2;
            Vector2 position = localStart;

            for (int i = 0; i < Target.ComponentsFlow.Length; i++)
            {
                var component = Target.ComponentsFlow[i];
                int x = i % Target.Columns;
                int y = i / Target.Rows;

                GameObject componentGO = Instantiate(component.Component.gameObject, Target.ComponentsParent);
                ElectricalCircuitComponent instance = componentGO.GetComponent<ElectricalCircuitComponent>();

                float angle = component.Rotation;
                if (random)
                {
                    System.Random rand = new System.Random();
                    angle = rand.Next(1, 4) * 90;
                }

                Vector2 localPos = componentGO.transform.localPosition;
                localPos.x = localStart.x - (x * (componentSize + Target.ComponentsSpacing)) - componentSize / 2;
                localPos.y = localStart.y - (y * (componentSize + Target.ComponentsSpacing)) - componentSize / 2;

                Vector3 localRot = componentGO.transform.localEulerAngles;
                localRot = localRot.SetComponent(instance.ComponentUp, angle);

                componentGO.transform.localPosition = localPos;
                componentGO.transform.localEulerAngles = localRot;
                componentGO.transform.localScale = Vector3.one * Target.ComponentsSize;

                instance.ElectricalCircuit = Target;
                instance.Coords = new Vector2Int(x, y);
                instance.Angle = angle;

                Target.Components.Add(instance);
                if (componentGO.TryGetComponent(out Collider collider))
                    Target.CollidersEnable.Add(collider);
            }
        }
    }
}