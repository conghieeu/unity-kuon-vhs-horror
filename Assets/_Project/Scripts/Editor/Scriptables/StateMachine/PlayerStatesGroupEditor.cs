using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UHFPS.Scriptable;
using ThunderWire.Editors;
using UnityEditor.IMGUI.Controls;
using static UnityEditor.VersionControl.Asset;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(PlayerStatesGroup), true)]
    public class PlayerStatesGroupEditor : Editor
    {
        public struct StatePair
        {
            public Type stateType;
            public string stateName;
        }

        PropertyCollection Properties;
        PlayerStatesGroup Target;

        private List<StatePair> states = new();

        public static Texture2D FSMIcon => Resources.Load<Texture2D>("EditorIcons/fsm");

        private void OnEnable()
        {
            Properties = EditorDrawing.GetAllProperties(serializedObject);
            Target = target as PlayerStatesGroup;

            foreach (var type in TypeCache.GetTypesDerivedFrom<PlayerStateAsset>().Where(x => !x.IsAbstract))
            {
                if (Target.PlayerStates.Any(x => x.stateAsset.GetType() == type))
                    continue;

                PlayerStateAsset instance = (PlayerStateAsset)CreateInstance(type);
                states.Add(new StatePair()
                {
                    stateType = type,
                    stateName = instance.Name
                });
                instance = null;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Player States Group"));
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                if (Properties.Count > 2)
                {
                    GUIContent stateHeader = EditorDrawing.IconTextContent("State Properties", "Settings");
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["PlayerStates"], stateHeader))
                    {
                        foreach (var item in Properties.Skip(2))
                        {
                            EditorGUILayout.PropertyField(item.Value);
                        }
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorDrawing.ResetIconSize();
                    EditorGUILayout.Space();
                }

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Player States", EditorStyles.miniBoldLabel);
                EditorGUILayout.EndVertical();

                if (Properties["PlayerStates"].arraySize > 0)
                {
                    for (int i = 0; i < Properties["PlayerStates"].arraySize; i++)
                    {
                        SerializedProperty state = Properties["PlayerStates"].GetArrayElementAtIndex(i);
                        SerializedProperty stateAsset = state.FindPropertyRelative("stateAsset");
                        SerializedProperty isEnabled = state.FindPropertyRelative("isEnabled");

                        bool expanded = state.isExpanded;
                        bool toggle = isEnabled.boolValue;

                        string name = ((PlayerStateAsset)stateAsset.objectReferenceValue).Name.Split('/').Last();
                        EditorDrawing.SetIconSize(12f);

                        GUIContent title = EditorGUIUtility.TrTextContentWithIcon(" " + name, FSMIcon);
                        Rect headerRect = EditorDrawing.DrawScriptableBorderFoldoutToggle(stateAsset, title, ref expanded, ref toggle);
                        state.isExpanded = expanded;
                        isEnabled.boolValue = toggle;

                        Rect dropdownRect = headerRect;
                        dropdownRect.xMin = headerRect.xMax - EditorGUIUtility.singleLineHeight;
                        dropdownRect.x -= EditorGUIUtility.standardVerticalSpacing;
                        dropdownRect.y += headerRect.height / 2 - 8f;

                        EditorDrawing.ResetIconSize();
                        GUIContent dropdownIcon = EditorGUIUtility.TrIconContent("_Menu", "State Menu");
                        int index = i;

                        if (GUI.Button(dropdownRect, dropdownIcon, EditorStyles.iconButton))
                        {
                            GenericMenu popup = new GenericMenu();

                            if (index > 0)
                            {
                                popup.AddItem(new GUIContent("Move Up"), false, () =>
                                {
                                    Properties["PlayerStates"].MoveArrayElement(index, index - 1);
                                    serializedObject.ApplyModifiedProperties();
                                });
                            }
                            else popup.AddDisabledItem(new GUIContent("Move Up"));

                            if (index < Properties["PlayerStates"].arraySize - 1)
                            {
                                popup.AddItem(new GUIContent("Move Down"), false, () =>
                                {
                                    Properties["PlayerStates"].MoveArrayElement(index, index + 1);
                                    serializedObject.ApplyModifiedProperties();
                                });
                            }
                            else popup.AddDisabledItem(new GUIContent("Move Down"));

                            popup.AddItem(new GUIContent("Delete"), false, () =>
                            {
                                UnityEngine.Object stateAssetObj = stateAsset.objectReferenceValue;
                                Properties["PlayerStates"].DeleteArrayElementAtIndex(index);
                                serializedObject.ApplyModifiedProperties();
                                AssetDatabase.RemoveObjectFromAsset(stateAssetObj);
                                EditorUtility.SetDirty(target);
                                AssetDatabase.SaveAssets();
                            });

                            popup.ShowAsContext();
                        }
                    }

                    EditorGUILayout.Space();
                }

                EditorGUILayout.Space(2f);
                EditorDrawing.Separator();
                EditorGUILayout.Space(2f);

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    Rect stateButtonRect = EditorGUILayout.GetControlRect(GUILayout.Width(100f), GUILayout.Height(20f));

                    using (new EditorGUI.DisabledGroupScope(Application.isPlaying || states.Count <= 0))
                    {
                        if (GUI.Button(stateButtonRect, "Add State"))
                        {
                            Rect dropdownRect = stateButtonRect;
                            dropdownRect.width = 250f;
                            dropdownRect.height = 0f;
                            dropdownRect.y += 21f;
                            dropdownRect.x += (stateButtonRect.width - dropdownRect.width) / 2;

                            StatesDropdown statesDropdown = new(new(), states);
                            statesDropdown.OnItemPressed = (type) => AddPlayerState(type);
                            statesDropdown.Show(dropdownRect);
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void AddPlayerState(Type type)
        {
            ScriptableObject component = CreateInstance(type);
            PlayerStateAsset state = (PlayerStateAsset)component;
            component.name = state.Name.Split("/").Last();

            Undo.RegisterCreatedObjectUndo(component, "Add Player State");

            if (EditorUtility.IsPersistent(target))
                AssetDatabase.AddObjectToAsset(component, target);

            Target.PlayerStates.Add(new PlayerStateData()
            {
                stateAsset = state,
                isEnabled = true
            });

            states.RemoveAll(x => x.stateType == type);
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }

        public class StatesDropdown : AdvancedDropdown
        {
            private readonly IEnumerable<StatePair> modules;
            public Action<Type> OnItemPressed;

            private class StateElement : AdvancedDropdownItem
            {
                public Type moduleType;

                public StateElement(string displayName, Type moduleType) : base(displayName)
                {
                    this.moduleType = moduleType;
                }
            }

            public StatesDropdown(AdvancedDropdownState state, IEnumerable<StatePair> states) : base(state)
            {
                this.modules = states;
                minimumSize = new Vector2(minimumSize.x, 270f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Player States");
                var groupMap = new Dictionary<string, AdvancedDropdownItem>();

                foreach (var module in modules)
                {
                    Type type = module.stateType;
                    string name = module.stateName;

                    // Split the name into groups
                    string[] groups = name.Split('/');

                    // Create or find the groups
                    AdvancedDropdownItem parent = root;
                    for (int i = 0; i < groups.Length - 1; i++)
                    {
                        string groupPath = string.Join("/", groups.Take(i + 1));
                        if (!groupMap.ContainsKey(groupPath))
                        {
                            var newGroup = new AdvancedDropdownItem(groups[i]);
                            parent.AddChild(newGroup);
                            groupMap[groupPath] = newGroup;
                        }
                        parent = groupMap[groupPath];
                    }

                    // Create the item and add it to the last group
                    StateElement item = new StateElement(groups.Last(), type);

                    //item.icon = MotionIcon;
                    parent.AddChild(item);
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                StateElement element = (StateElement)item;
                OnItemPressed?.Invoke(element.moduleType);
            }
        }
    }
}