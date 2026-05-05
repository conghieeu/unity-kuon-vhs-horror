using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(PlayerStateMachine))]
    public class PlayerStateMachineEditor : InspectorEditor<PlayerStateMachine>
    {
        public static Texture2D FSMIcon => Resources.Load<Texture2D>("EditorIcons/fsm");

        private ReorderableList reorderableList;

        public override void OnEnable()
        {
            base.OnEnable();
            SerializedProperty customStates = Properties["CustomStates"];
            reorderableList = new(serializedObject, customStates, true, false, true, true);

            reorderableList.elementHeightCallback = (int index) =>
            {
                SerializedProperty element = customStates.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(element, true) + EditorGUIUtility.standardVerticalSpacing;
            };

            reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                SerializedProperty element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                rect.xMin += 12f;
                rect.y += 1f;

                // custom label
                string name = $"Custom State {index}";
                EditorGUI.PropertyField(rect, element, new GUIContent(name), true);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                EditorDrawing.DrawInspectorHeader(new GUIContent("Player State Machine"), Target);
                EditorGUILayout.Space();

                Properties.Draw("StatesAsset");
                Properties.Draw("SurfaceMask");
                Properties.Draw("ControllerOffset");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope())
                {
                    GUIContent title = EditorDrawing.IconTextContent("Player Settings", "CharacterController Icon", 14f);
                    EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
                    EditorDrawing.ResetIconSize();
                    EditorGUILayout.Space(2f);

                    EditorDrawing.DrawClassBorderFoldout(Properties["PlayerBasicSettings"], new GUIContent("Basic Settings"));
                    EditorDrawing.DrawClassBorderFoldout(Properties["PlayerFeatures"], new GUIContent("Player Features"));
                    EditorDrawing.DrawClassBorderFoldout(Properties["PlayerSliding"], new GUIContent("Player Sliding"));
                    EditorDrawing.DrawClassBorderFoldout(Properties["PlayerStamina"], new GUIContent("Player Stamina"));
                    EditorDrawing.DrawClassBorderFoldout(Properties["PlayerControllerSettings"], new GUIContent("Controller Settings"));
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope())
                {
                    GUIContent title = EditorDrawing.IconTextContent("Controller States", "CapsuleCollider Icon", 14f);
                    EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
                    EditorDrawing.ResetIconSize();
                    EditorGUILayout.Space(2f);

                    EditorDrawing.DrawClassBorderFoldout(Properties["StandingState"], new GUIContent("Standing State"));
                    EditorDrawing.DrawClassBorderFoldout(Properties["CrouchingState"], new GUIContent("Crouching State"));
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["CustomStates"], new GUIContent("Custom States")))
                    {
                        reorderableList.DoLayoutList();
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Player States", EditorStyles.boldLabel);
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                if (Target.StatesAsset != null)
                {
                    SerializedObject statesSerializedObject = Application.isPlaying && Target.StatesAssetRuntime != null
                        ? new SerializedObject(Target.StatesAssetRuntime)
                        : new SerializedObject(Target.StatesAsset);

                    PropertyCollection stateProperties = EditorDrawing.GetAllProperties(statesSerializedObject);
                    SerializedProperty statesProperty = stateProperties["PlayerStates"];

                    statesSerializedObject.Update();
                    {
                        if (stateProperties.Count > 2)
                        {

                            GUIContent stateHeader = EditorDrawing.IconTextContent("State Properties" + (Application.isPlaying ? "*" : ""), "Settings");

                            EditorDrawing.SetLabelColor("#E0FBFC");
                            if (EditorDrawing.BeginFoldoutBorderLayout(stateProperties["PlayerStates"], stateHeader))
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

                        if (Target.StatesAsset.PlayerStates.Count > 0)
                        {
                            Type currentState = null;
                            Type previousState = null;
                            if (Application.isPlaying)
                            {
                                if (Target.CurrentState != null)
                                    currentState = Target.CurrentState?.stateData.stateAsset.GetType();

                                if (Target.PreviousState != null)
                                    previousState = Target.PreviousState?.stateData.stateAsset.GetType();
                            }

                            for (int i = 0; i < statesProperty.arraySize; i++)
                            {
                                SerializedProperty state = statesProperty.GetArrayElementAtIndex(i);
                                SerializedProperty stateAsset = state.FindPropertyRelative("stateAsset");
                                SerializedProperty isEnabled = state.FindPropertyRelative("isEnabled");

                                bool expanded = state.isExpanded;
                                bool toggle = isEnabled.boolValue;

                                string name = ((PlayerStateAsset)stateAsset.objectReferenceValue).Name.Split('/').Last();
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
                    EditorGUILayout.HelpBox("To add new states open player state asset.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Assign a player state asset to display all states.", MessageType.Info);
                }

                EditorGUILayout.Space();
                EditorDrawing.Separator();
                EditorGUILayout.Space();

                GUIContent gizomsHeader = EditorDrawing.IconTextContent("Player Gizmos", "BodySilhouette", 12f);
                if(EditorDrawing.BeginFoldoutToggleBorderLayout(gizomsHeader, Properties["DrawPlayerGizmos"]))
                {
                    EditorDrawing.ResetIconSize();
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("DrawPlayerGizmos")))
                    {
                        Properties.Draw("GizmosColor");
                        Properties.Draw("ScaleOffset");
                        Properties.Draw("DrawPlayerWireframe");
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }
                EditorDrawing.ResetIconSize();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}