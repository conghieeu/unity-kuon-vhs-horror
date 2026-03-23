using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;
using UHFPS.Tools;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Editors;
using Object = UnityEngine.Object;

namespace UHFPS.Editors
{
    public class OptionsBuilder : EditorWindow
    {
        public const float OPTIONS_VIEW_WIDTH = 300f;
        const string NEW_SECTION_PREFIX = "New Section";

        private float Spacing => EditorGUIUtility.standardVerticalSpacing * 2;

        public abstract class BuilderSelection { }
        public struct SelectionArgs
        {
            public BuilderSelection Selection;
            public TreeViewItem TreeViewItem;
        }

        public class BuilderItem : BuilderSelection
        {
            public PropertyCollection Properties;

            public string GUID => Properties["GUID"].stringValue;
            public string Name => Properties["Name"].stringValue;
            public Object Prefab => Properties["Prefab"].objectReferenceValue;

            public bool IsSeparator { get; private set; }
            public string ModuleType { get; private set; }

            public BuilderItem(SerializedProperty item)
            {
                Properties = EditorDrawing.GetAllProperties(item);

                OptionModule option = (OptionModule)item.managedReferenceValue;
                IsSeparator = option is OptionSeparator;
                ModuleType = option.GetType().Name;
            }
        }

        public class BuilderSection : BuilderSelection
        {
            public SerializedProperty SectionGUID;
            public SerializedProperty SectionName;
            public List<BuilderItem> Items;

            public string GUID => SectionGUID.stringValue;
            public string Name => SectionName.stringValue;

            public BuilderSection(SerializedProperty section)
            {
                Items = new();
                SerializedProperty sectionCls = section.FindPropertyRelative("Section");
                SerializedProperty items = section.FindPropertyRelative("Items");
                SectionGUID = sectionCls.FindPropertyRelative("GUID");
                SectionName = sectionCls.FindPropertyRelative("Name");

                for (int i = 0; i < items.arraySize; i++)
                {
                    SerializedProperty item = items.GetArrayElementAtIndex(i);
                    Items.Add(new BuilderItem(item));
                }
            }
        }

        public class BuilderData
        {
            public OptionsAsset Target;
            public SerializedObject SerializedObject;

            public SerializedProperty SectionsArray;
            public List<BuilderSection> Sections;

            public BuilderData(OptionsAsset optionsAsset)
            {
                Target = optionsAsset;
                SerializedObject = new SerializedObject(optionsAsset);
                SectionsArray = SerializedObject.FindProperty("Sections");
                Reload();
            }

            public void Reload()
            {
                Sections = new();
                for (int i = 0; i < SectionsArray.arraySize; i++)
                {
                    SerializedProperty section = SectionsArray.GetArrayElementAtIndex(i);
                    Sections.Add(new BuilderSection(section));
                }
            }
        }

        private BuilderData builderData;
        private SelectionArgs? selection;
        private Vector2 scrollPosition;
        private List<CustomDropdownItem> prefabs;

        [SerializeField]
        private TreeViewState optionsViewState;
        private OptionsTreeView optionsTreeView;

        public void Show(OptionsAsset optionsAsset)
        {
            builderData = new BuilderData(optionsAsset);
            optionsViewState = new TreeViewState();
            optionsTreeView = new OptionsTreeView(optionsViewState, builderData)
            {
                OnItemSelect = OnItemSelect,
                OnAddNewSection = OnAddNewSection,
                OnAddNewOption = OnAddNewOption,
                OnDeleteItem = OnDeleteItem,
                OnDeleteSection = OnDeleteSection,
                OnRebuild = ReloadBuilder
            };

            prefabs = new() { new() {
                Path = "None",
                Item = null
            }};

            foreach (var prefab in optionsAsset.OptionPrefabs)
            {
                if (prefab == null)
                    continue;

                prefabs.Add(new(prefab.name, prefab, "Prefab Icon"));
            }
        }

        public void ReloadBuilder()
        {
            builderData.SerializedObject.ApplyModifiedProperties();
            builderData.SerializedObject.Update();
            builderData.Reload();
            optionsTreeView.Reload();
            selection = null;
        }

        private void OnItemSelect(SelectionArgs? selection)
        {
            this.selection = selection;
        }

        public void OnAddNewSection()
        {
            builderData.Target.Sections.Add(new()
            {
                Section = new()
                {
                    Name = NEW_SECTION_PREFIX,
                    GUID = GameTools.GetGuid(),
                },
                Items = new()
            });
        }

        public void OnAddNewOption(string sectionGUID, OptionModule option)
        {
            var section = builderData.Target.GetSection(sectionGUID);
            if (section != null)
            {
                bool separator = option is OptionSeparator;
                string optionTitle = option.OptionName;
                string optionName = separator ? "Separator" : "New " + optionTitle;

                option.Name = optionName;
                option.GUID = GameTools.GetGuid();

                section.Items.Add(option);
                ReloadBuilder();
            }
        }


        private void OnDeleteSection(string guid)
        {
            var section = builderData.Target.GetSection(guid);
            if (section.Items.Count > 0 && !EditorUtility.DisplayDialog("Delete Section", $"Are you sure you want to delete section \"{section.Section.Name}\" and it's options?", "Delete", "NO"))
                return;

            for (int i = 0; i < builderData.Target.Sections.Count; i++)
            {
                var _section = builderData.Target.Sections[i];
                if (_section.Section.GUID == guid)
                {
                    builderData.Target.Sections.RemoveAt(i);
                    break;
                }
            }
        }

        private void OnDeleteItem(string sectionGuid, string optionGuid)
        {
            var section = builderData.Target.GetSection(sectionGuid);
            for (int i = 0; i < section.Items.Count; i++)
            {
                var item = section.Items[i];
                if (item.GUID == optionGuid)
                {
                    section.Items.RemoveAt(i);
                    break;
                }
            }
        }

        private void OnGUI()
        {
            Rect toolbarRect = new(0, 0, position.width, 20f);
            GUI.Box(toolbarRect, GUIContent.none, EditorStyles.toolbar);

            // toolbar buttons
            Rect saveBtn = toolbarRect;
            saveBtn.xMin = saveBtn.xMax - 100f;
            if (GUI.Button(saveBtn, "Save Asset", EditorStyles.toolbarButton))
            {
                EditorUtility.SetDirty(builderData.Target);
                AssetDatabase.SaveAssetIfDirty(builderData.Target);
            }

            Rect resetBtn = saveBtn;
            resetBtn.x -= 100f;
            if (GUI.Button(resetBtn, "Restore", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Restore Asset", $"Are you sure you want to unload the asset and restore the last saved state?", "Unload", "NO"))
                {
                    selection = null;
                    Resources.UnloadAsset(builderData.Target);
                    Close();
                }
            }

            // inspector view
            Rect itemsRect = new(5f, 25f, OPTIONS_VIEW_WIDTH, position.height - 30f);
            optionsTreeView.OnGUI(itemsRect);

            if (selection != null)
            {
                string title = "NULL";
                string type = "";

                if (selection.Value.Selection is BuilderItem item)
                {
                    title = item.Name;

                    string withoutOption = item.ModuleType.Replace("Option", "");
                    string result = Regex.Replace(withoutOption, "(?<!^)([A-Z])", " $1");
                    type = $"[{result}] ";
                }
                else if (selection.Value.Selection is BuilderSection section) title = section.Name;

                Rect inspectorRect = new(OPTIONS_VIEW_WIDTH + 10f, 25f, position.width - OPTIONS_VIEW_WIDTH - 15f, position.height - 30f);
                GUIContent inspectorTitle = EditorGUIUtility.TrTextContentWithIcon($" INSPECTOR {type}({title})", "CustomTool");
                EditorDrawing.DrawHeaderWithBorder(ref inspectorRect, inspectorTitle, 20f, false);

                Rect inspectorViewRect = inspectorRect;
                inspectorViewRect.y += Spacing;
                inspectorViewRect.yMax -= Spacing;
                inspectorViewRect.xMin += Spacing;
                inspectorViewRect.xMax -= Spacing;

                GUILayout.BeginArea(inspectorViewRect);
                OnDrawOptionInspector(selection.Value);
                GUILayout.EndArea();
            }
        }

        private void OnDrawOptionInspector(SelectionArgs selection)
        {
            try
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
                {
                    if (selection.Selection is BuilderSection section)
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(section.SectionName, new GUIContent("Name"));
                        if (EditorGUI.EndChangeCheck())
                        {
                            selection.TreeViewItem.displayName = section.SectionName.stringValue;
                            builderData.SerializedObject.ApplyModifiedProperties();
                            builderData.SerializedObject.Update();
                        }

                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            EditorGUILayout.IntField(new GUIContent("Items"), section.Items.Count);
                        }

                        EditorGUILayout.Space(2);
                        EditorDrawing.Separator();
                        EditorGUILayout.Space(1);

                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            EditorGUILayout.LabelField("GUID: " + section.GUID, EditorStyles.miniBoldLabel);
                        }
                    }
                    else if (selection.Selection is BuilderItem item)
                    {
                        var properties = item.Properties;

                        builderData.SerializedObject.Update();
                        {
                            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                            {
                                EditorGUI.BeginChangeCheck();
                                properties.Draw("Name");
                                if (EditorGUI.EndChangeCheck())
                                {
                                    selection.TreeViewItem.displayName = item.Name;
                                    builderData.SerializedObject.ApplyModifiedProperties();
                                    builderData.SerializedObject.Update();
                                }

                                properties.Draw("Title");
                                DrawPrefabSelector(properties["Prefab"]);
                            }

                            if (properties.Count > 4)
                            {
                                using (new EditorDrawing.IconSizeScope(15))
                                {
                                    EditorGUILayout.Space();
                                    EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                                    GUIContent header = EditorGUIUtility.TrTextContentWithIcon(" Option Settings", "Settings");
                                    EditorGUILayout.LabelField(header, EditorStyles.miniBoldLabel);
                                    EditorGUILayout.EndVertical();
                                    EditorGUILayout.Space();
                                }

                                properties.DrawAll(false, 4);
                            }
                        }
                        builderData.SerializedObject.ApplyModifiedProperties();

                        EditorGUILayout.Space(2);
                        EditorDrawing.Separator();
                        EditorGUILayout.Space(1);

                        using (new EditorGUI.DisabledGroupScope(true))
                        {
                            EditorGUILayout.LabelField("GUID: " + item.GUID, EditorStyles.miniBoldLabel);
                            EditorGUILayout.LabelField("Module Type: " + item.ModuleType, EditorStyles.miniBoldLabel);
                        }
                    }
                }
                EditorGUILayout.EndScrollView();
            }
            catch { }
        }

        private void DrawPrefabSelector(SerializedProperty property)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, new GUIContent("Prefab"));

            Rect dropdownRect = rect;
            dropdownRect.width = 250f;
            dropdownRect.height = 0f;
            dropdownRect.y += 21f;
            dropdownRect.x = rect.xMax - dropdownRect.width;

            GUIContent selectorContent = new("Select Prefab");
            if (property.objectReferenceValue != null)
            {
                string name = property.objectReferenceValue.name;
                selectorContent = EditorGUIUtility.TrTextContentWithIcon(name, "Prefab Icon");
            }

            if (EditorDrawing.ObjectField(rect, selectorContent))
            {
                CustomDropdown customDropdown = new(new AdvancedDropdownState(), "Prefabs", prefabs);
                customDropdown.OnItemSelected += (item) =>
                {
                    var obj = item.Item as OptionBehaviour;
                    if (obj == null) property.objectReferenceValue = null;
                    else property.objectReferenceValue = obj.gameObject;
                    builderData.SerializedObject.ApplyModifiedProperties();
                };
                customDropdown.Show(dropdownRect);
            }

            bool flag1 = builderData.Target.OptionPrefabs.Count <= 0;
            bool flag2 = property.objectReferenceValue == null;
            if (flag1 || flag2) EditorGUILayout.Space(1f);

            if (flag1)
            {
                EditorUtils.TrHelpIconText("Please assign at least one option prefab in Options Asset.", MessageType.Warning);
            }

            if (flag2)
            {
                EditorUtils.TrHelpIconText("The prefab reference of the option is null!", MessageType.Warning);
            }
        }
    }
}