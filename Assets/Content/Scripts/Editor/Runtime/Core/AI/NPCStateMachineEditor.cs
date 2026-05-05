using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;
using UHFPS.Scriptable;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(NPCStateMachine))]
    public class NPCStateMachineEditor : InspectorEditor<NPCStateMachine>
    {
        public static Texture2D FSMIcon => Resources.Load<Texture2D>("EditorIcons/fsm");

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                EditorDrawing.DrawInspectorHeader(new GUIContent("NPC State Machine"), Target);
                EditorGUILayout.Space();

                Properties.Draw("StatesAsset");
                Properties.Draw("Animator");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("AI Settings")))
                {
                    Properties.Draw("HeadBone");
                    Properties.Draw("SightsMask");
                    Properties.Draw("NPCType");
                    Properties.Draw("SteeringSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sensor Settings")))
                {
                    Properties.Draw("SightsFOV");
                    Properties.Draw("SightsDistance");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Visualization")))
                {
                    Properties.Draw("ShowDestination");
                    Properties.Draw("ShowSights");
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("AI States", EditorStyles.boldLabel);
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                if (Target.StatesAsset != null)
                {
                    SerializedObject statesSerializedObject = Application.isPlaying && Target.StatesAssetRuntime != null
                        ? new SerializedObject(Target.StatesAssetRuntime)
                        : new SerializedObject(Target.StatesAsset);

                    PropertyCollection stateProperties = EditorDrawing.GetAllProperties(statesSerializedObject);
                    SerializedProperty statesProperty = stateProperties["AIStates"];

                    statesSerializedObject.Update();
                    {
                        if (stateProperties.Count > 2)
                        {
                            GUIContent stateHeader = EditorDrawing.IconTextContent("State Properties" + (Application.isPlaying ? "*" : ""), "Settings");

                            EditorDrawing.SetLabelColor("#E0FBFC");
                            if (EditorDrawing.BeginFoldoutBorderLayout(stateProperties["AIStates"], stateHeader))
                            {
                                EditorDrawing.ResetLabelColor();
                                foreach (var item in stateProperties.Skip(2))
                                {
                                    EditorGUILayout.PropertyField(item.Value);
                                }
                                EditorDrawing.EndBorderHeaderLayout();
                            }
                            EditorDrawing.ResetLabelColor();
                            EditorDrawing.ResetIconSize();
                            EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                        }

                        if (Target.StatesAsset.AIStates.Count > 0)
                        {
                            Type currentState = null;
                            Type previousState = null;
                            if (Application.isPlaying)
                            {
                                if (Target.CurrentState.HasValue)
                                    currentState = Target.CurrentState?.StateData.StateAsset.GetType();

                                if (Target.PreviousState.HasValue)
                                    previousState = Target.PreviousState?.StateData.StateAsset.GetType();
                            }

                            for (int i = 0; i < statesProperty.arraySize; i++)
                            {
                                SerializedProperty state = statesProperty.GetArrayElementAtIndex(i);
                                SerializedProperty stateAsset = state.FindPropertyRelative("StateAsset");
                                SerializedProperty isEnabled = state.FindPropertyRelative("IsEnabled");

                                bool expanded = state.isExpanded;
                                bool toggle = isEnabled.boolValue;

                                string name = ((AIStateAsset)stateAsset.objectReferenceValue).Name.Split('/').Last();
                                EditorDrawing.SetIconSize(12f);

                                GUIContent title = EditorGUIUtility.TrTextContentWithIcon(" " + name, FSMIcon);
                                Rect header = EditorDrawing.DrawScriptableBorderFoldoutToggle(stateAsset, title, ref expanded, ref toggle);

                                EditorDrawing.ResetIconSize();
                                state.isExpanded = expanded;
                                isEnabled.boolValue = toggle;

                                if (Application.isPlaying)
                                {
                                    if (currentState != null && stateAsset.objectReferenceValue.GetType() == currentState)
                                    {
                                        Rect currStateRect = header;
                                        currStateRect.xMin = header.xMax - EditorGUIUtility.singleLineHeight;

                                        GUIContent currStateIndicator = EditorGUIUtility.TrIconContent("greenLight", "Current State");
                                        EditorGUI.LabelField(currStateRect, currStateIndicator);
                                    }

                                    if (previousState != null && stateAsset.objectReferenceValue.GetType() == previousState)
                                    {
                                        Rect prevStateRect = header;
                                        prevStateRect.xMin = header.xMax - EditorGUIUtility.singleLineHeight;

                                        GUIContent prevStateIndicator = EditorGUIUtility.TrIconContent("orangeLight", "Previous State");
                                        EditorGUI.LabelField(prevStateRect, prevStateIndicator);
                                    }
                                }
                            }
                        }
                    }
                    statesSerializedObject.ApplyModifiedProperties();

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                    EditorGUILayout.HelpBox("To add new states open AI state asset.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Assign a player AI asset to display all states.", MessageType.Info);
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}