using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;
using UHFPS.Runtime;
using ThunderWire.Editors;
using UHFPS.Scriptable;
using Object = UnityEngine.Object;

namespace UHFPS.Editors
{
    public class MotionListDrawer
    {
        public struct ModulePair
        {
            public Type moduleType;
            public string moduleName;
        }

        private const string Default = MotionBlender.Default;
        public static Texture2D MotionIcon => Resources.Load<Texture2D>("EditorIcons/motion2");

        private readonly List<ModulePair> modules;
        private Vector2 defaultIconSize;

        public Action<Type, int> OnAddModule;
        public Action OnAddState;

        public MotionListDrawer()
        {
            defaultIconSize = EditorGUIUtility.GetIconSize();
            modules = new();

            foreach (var type in TypeCache.GetTypesDerivedFrom<MotionModule>().Where(x => !x.IsAbstract))
            {
                MotionModule instance = Activator.CreateInstance(type) as MotionModule;
                modules.Add(new ModulePair()
                {
                     moduleType = type,
                     moduleName = instance.Name
                });
                instance = null;
            }
        }

        public void DrawMotionsList(SerializedProperty stateMotions, GUIContent title)
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
                    SerializedProperty stateID = stateProperty.FindPropertyRelative("StateID");
                    SerializedProperty motions = stateProperty.FindPropertyRelative("Motions");

                    string name = stateID.stringValue;
                    string iconName = name == Default ? "sv_icon_dot0_pix16_gizmo" : "AnimatorController On Icon";
                    GUIContent header = EditorGUIUtility.TrTextContentWithIcon($" {name} (State)", iconName);

                    EditorGUIUtility.SetIconSize(new Vector2(14, 14));
                    if (EditorDrawing.BeginFoldoutBorderLayout(stateProperty, header, out Rect foldoutRect, roundedBox: false))
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        EditorGUILayout.PropertyField(stateID, new GUIContent("State"));
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

                                string motionName = moduleProperty.managedReferenceFullTypename.Split('.').Last();
                                GUIContent motionHeader = EditorGUIUtility.TrTextContentWithIcon($" {motionName} (Module)", MotionIcon);

                                if (EditorDrawing.BeginFoldoutBorderLayout(moduleProperty, motionHeader, out Rect moduleFoldoutRect))
                                {
                                    int skip = 1;
                                    EditorGUILayout.BeginVertical(GUI.skin.box);
                                    {
                                        moduleProperties.Draw("Weight");
                                        using (new EditorGUI.IndentLevelScope())
                                        {
                                            var positionSpring = moduleProperties.GetRelative("PositionSpringSettings");
                                            if (positionSpring != null)
                                            {
                                                EditorGUILayout.PropertyField(positionSpring, new GUIContent("Position Spring"));
                                                skip++;
                                            }

                                            var rotationSpring = moduleProperties.GetRelative("RotationSpringSettings");
                                            if (rotationSpring != null)
                                            {
                                                EditorGUILayout.PropertyField(rotationSpring, new GUIContent("Rotation Spring"));
                                                skip++;
                                            }
                                        }
                                    }
                                    EditorGUILayout.EndVertical();

                                    moduleProperties.DrawAll(true, skip);
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
                            EditorGUIUtility.SetIconSize(defaultIconSize);
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

                EditorGUIUtility.SetIconSize(defaultIconSize);

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
                            OnAddState?.Invoke();
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();
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

    public class MotionListHelper
    {
        private readonly MotionListDrawer motionListDrawer;
        private readonly MotionPreset motionPreset;

        private SerializedObject motionPresetObject;
        private SerializedProperty stateMotions;
        private bool isInstance;

        public MotionListHelper(MotionPreset preset)
        {
            motionPreset = preset;
            motionListDrawer = new();
            motionListDrawer.OnAddState = AddState;
            motionListDrawer.OnAddModule = AddModule;

            if(preset != null)
            {
                motionPresetObject = new SerializedObject(preset);
                stateMotions = motionPresetObject.FindProperty("StateMotions");
                isInstance = false;
            }
        }

        public void DrawMotionPresetField(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                Object obj = property.objectReferenceValue;
                UpdatePreset((MotionPreset)obj);
            }
        }

        public void UpdatePreset(MotionPreset preset)
        {
            motionPresetObject = preset != null ? new SerializedObject(preset) : null;
            stateMotions = motionPresetObject.FindProperty("StateMotions");
        }

        public void DrawMotionsList(MotionPreset presetInstance, bool showHelp = true, bool showSave = true)
        {
            if (motionPreset != null)
            {
                GUIContent stateMotionsTitle = new GUIContent("State Motions");
                if (Application.isPlaying && presetInstance != null)
                {
                    stateMotionsTitle = new GUIContent("State Motions (Instance)");
                    if (!isInstance)
                    {
                        motionPresetObject = new SerializedObject(presetInstance);
                        stateMotions = motionPresetObject.FindProperty("StateMotions");
                        isInstance = true;
                    }
                }
                else
                {
                    if (isInstance)
                    {
                        motionPresetObject = new SerializedObject(motionPreset);
                        stateMotions = motionPresetObject.FindProperty("StateMotions");
                        isInstance = false;
                    }
                }

                if (motionPresetObject != null)
                {
                    motionPresetObject.Update();
                    motionListDrawer.DrawMotionsList(stateMotions, stateMotionsTitle);
                    motionPresetObject.ApplyModifiedProperties();
                }
            }
            else
            {
                if (showHelp) EditorGUILayout.HelpBox("The motion preset is not selected. To manage the motion states, please pick a motion preset.", MessageType.Info);
                motionPresetObject = null;
            }

            if (showSave && Application.isPlaying && presetInstance != null)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Save Preset Settings", GUILayout.Height(25f)))
                {
                    motionPreset.StateMotions = presetInstance.StateMotions;
                    new SerializedObject(motionPreset).ApplyModifiedProperties();
                }

                EditorGUILayout.Space(1f);
                EditorGUILayout.HelpBox("During runtime, it's not possible to add or remove motion states. Any modifications made to values will not be retained once you stop playing the game. To ensure that these changes are preserved, click Save Preset Settings button.", MessageType.Info);
            }
        }

        private void AddState()
        {
            if (motionPresetObject == null)
                return;

            motionPreset.StateMotions.Add(new());
            motionPresetObject.ApplyModifiedProperties();
            motionPresetObject.Update();
        }

        private void AddModule(Type moduleType, int state)
        {
            if (motionPresetObject == null)
                return;

            MotionModule motionModule = (MotionModule)Activator.CreateInstance(moduleType);
            var stateRef = motionPreset.StateMotions[state];

            stateRef.Motions.Add(motionModule);
            motionPresetObject.ApplyModifiedProperties();
            motionPresetObject.Update();
        }
    }
}