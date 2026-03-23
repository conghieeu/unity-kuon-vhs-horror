using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using ThunderWire.Editors;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    public class ExternalMotionsDrawer
    {
        public struct ModulePair
        {
            public Type moduleType;
            public string moduleName;
        }

        public static Texture2D MotionIcon => Resources.Load<Texture2D>("EditorIcons/motion3");

        private readonly List<ModulePair> modules;
        private readonly SerializedObject serializedObject;
        private readonly SerializedProperty stateMotions;
        private readonly ExternalMotions externalMotions;

        public ExternalMotionsDrawer(SerializedProperty property, ExternalMotions target)
        {
            modules = new();
            foreach (var type in TypeCache.GetTypesDerivedFrom<ExternalMotionData>().Where(x => !x.IsAbstract))
            {
                ExternalMotionData instance = Activator.CreateInstance(type) as ExternalMotionData;
                modules.Add(new ModulePair()
                {
                    moduleType = type,
                    moduleName = instance.Name
                });
                instance = null;
            }

            serializedObject = property.serializedObject;
            stateMotions = property.FindPropertyRelative("MotionStates");
            externalMotions = target;
        }

        public void DrawExternalMotions(GUIContent title)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                Rect labelRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

                Rect countLabelRect = labelRect;
                countLabelRect.xMin = countLabelRect.xMax - 25f;

                GUI.enabled = false;
                EditorGUI.IntField(countLabelRect, stateMotions.arraySize);
                GUI.enabled = true;

                EditorGUILayout.Space(2f);

                if (stateMotions.arraySize <= 0)
                {
                    EditorGUILayout.HelpBox("There are currently no states. To create a new state, click the 'Add State' button.", MessageType.Info);
                }

                for (int i = 0; i < stateMotions.arraySize; i++)
                {
                    SerializedProperty stateProperty = stateMotions.GetArrayElementAtIndex(i);
                    SerializedProperty eventID = stateProperty.FindPropertyRelative("EventID");
                    SerializedProperty motions = stateProperty.FindPropertyRelative("ExternalMotions");

                    string stateName = string.IsNullOrEmpty(eventID.stringValue) ? $"Element {i}" : eventID.stringValue;
                    GUIContent header = EditorGUIUtility.TrTextContentWithIcon($" {stateName} (State)", "AnimatorController On Icon");

                    EditorDrawing.SetIconSize(14f);
                    if (EditorDrawing.BeginFoldoutBorderLayout(stateProperty, header, out Rect foldoutRect, roundedBox: false))
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        EditorGUILayout.PropertyField(eventID);
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Space(3f);
                        Rect motionDataLabel = EditorGUILayout.GetControlRect();
                        EditorGUI.LabelField(motionDataLabel, new GUIContent("Motion Data"));

                        Rect addMotionRect = motionDataLabel;
                        addMotionRect.xMin = addMotionRect.xMax - EditorGUIUtility.singleLineHeight;

                        using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                        {
                            if (GUI.Button(addMotionRect, EditorUtils.Styles.PlusIcon))
                            {
                                int currentIndex = i;

                                Rect dropdownRect = addMotionRect;
                                dropdownRect.width = 250f;
                                dropdownRect.height = 0f;
                                dropdownRect.y += 21f;
                                dropdownRect.x = addMotionRect.x - (250f - EditorGUIUtility.singleLineHeight);

                                ModulesDropdown modulesDropdown = new(new(), modules);
                                modulesDropdown.OnItemPressed = (type) => OnAddModule(type, currentIndex);
                                modulesDropdown.Show(dropdownRect);
                            }
                        }

                        if (motions != null && motions.arraySize > 0)
                        {
                            EditorGUILayout.Space(1f);
                            for (int j = 0; j < motions.arraySize; j++)
                            {
                                SerializedProperty moduleProperty = motions.GetArrayElementAtIndex(j);
                                PropertyCollection moduleProperties = EditorDrawing.GetAllProperties(moduleProperty);

                                string motionName = externalMotions.MotionStates[i].ExternalMotions[j].Name.Split('/').Last();
                                GUIContent motionHeader = EditorGUIUtility.TrTextContentWithIcon($" {motionName} (Module)", MotionIcon);

                                if (EditorDrawing.BeginFoldoutBorderLayout(moduleProperty, motionHeader, out Rect moduleFoldoutRect))
                                {
                                    EditorGUILayout.BeginVertical(GUI.skin.box);
                                    bool positionEnable = moduleProperties.DrawGetBool("PositionEnable");
                                    bool rotationEnable = moduleProperties.DrawGetBool("RotationEnable");
                                    EditorGUILayout.EndVertical();

                                    string positionExcept = positionEnable ? "" : "position";
                                    string rotationExcept = rotationEnable ? "" : "rotation";

                                    // draw all properties except those that have PositionEnable, RotationEnable or position, rotation based on enabled state in their name
                                    moduleProperties.DrawAllPredicate(true, 0, (key) =>
                                    {
                                        return key != "PositionEnable" && key != "RotationEnable"
                                            && (!key.ToLower().Contains(positionExcept) || positionExcept == "")
                                            && (!key.ToLower().Contains(rotationExcept) || rotationExcept == "");
                                    });

                                    EditorDrawing.EndBorderHeaderLayout();
                                }

                                Rect moduleRemoveButton = moduleFoldoutRect;
                                moduleRemoveButton.xMin = moduleRemoveButton.xMax - EditorGUIUtility.singleLineHeight;
                                moduleRemoveButton.y += 3f;

                                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                                {
                                    if (GUI.Button(moduleRemoveButton, EditorUtils.Styles.MinusIcon, EditorStyles.iconButton))
                                    {
                                        motions.DeleteArrayElementAtIndex(j);
                                    }
                                }
                            }
                        }
                        else
                        {
                            EditorGUILayout.HelpBox("There are currently no state motions. To create a new state motion, click the 'plus (+)' button.", MessageType.Info);
                        }

                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    Rect removeButton = foldoutRect;
                    removeButton.xMin = removeButton.xMax - EditorGUIUtility.singleLineHeight;
                    removeButton.y += 3f;

                    using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                    {
                        if (GUI.Button(removeButton, EditorUtils.Styles.MinusIcon, EditorStyles.iconButton))
                        {
                            stateMotions.DeleteArrayElementAtIndex(i);
                        }
                    }

                    if (i + 1 < stateMotions.arraySize) EditorGUILayout.Space(1f);
                }

                EditorDrawing.ResetIconSize();

                EditorGUILayout.Space(2f);
                EditorDrawing.Separator();
                EditorGUILayout.Space(2f);

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    Rect moduleButtonRect = EditorGUILayout.GetControlRect(GUILayout.Width(100f));

                    using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                    {
                        if (GUI.Button(moduleButtonRect, "Add State"))
                        {
                            externalMotions.MotionStates.Add(new());
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnAddModule(Type moduleType, int state)
        {
            ExternalMotionData motionData = (ExternalMotionData)Activator.CreateInstance(moduleType);
            var stateRef = externalMotions.MotionStates[state];

            stateRef.ExternalMotions.Add(motionData);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        public class ModulesDropdown : AdvancedDropdown
        {
            private readonly IEnumerable<ModulePair> modules;
            public Action<Type> OnItemPressed;

            private class ModuleElement : AdvancedDropdownItem
            {
                public Type moduleType;

                public ModuleElement(string displayName, Type moduleType) : base(displayName)
                {
                    this.moduleType = moduleType;
                }
            }

            public ModulesDropdown(AdvancedDropdownState state, IEnumerable<ModulePair> modules) : base(state)
            {
                this.modules = modules;
                minimumSize = new Vector2(minimumSize.x, 270f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Motion Modules");
                var groupMap = new Dictionary<string, AdvancedDropdownItem>();

                foreach (var module in modules)
                {
                    Type type = module.moduleType;
                    string name = module.moduleName;

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
                    ModuleElement item = new ModuleElement(groups.Last(), type);

                    item.icon = MotionIcon;
                    parent.AddChild(item);
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                ModuleElement element = (ModuleElement)item;
                OnItemPressed?.Invoke(element.moduleType);
            }
        }
    }
}