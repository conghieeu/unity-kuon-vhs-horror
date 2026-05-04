using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;
using UHFPS.Tools;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    public class InventoryBuilder : EditorWindow
    {
        const float ITEMS_VIEW_WIDTH = 300f;
        const string NEW_SECTION_PREFIX = "NewSection";
        const string NEW_ITEM_PREFIX = "NewItem";

        private float Spacing => EditorGUIUtility.standardVerticalSpacing * 2;

        public abstract class BuilderSelection { }
        public struct SelectionArgs
        {
            public BuilderSelection Selection;
            public TreeViewItem TreeViewItem;
        }

        public struct ItemCache
        {
            public SerializedProperty width;
            public SerializedProperty height;
            public SerializedProperty orientation;
            public SerializedProperty flipDirection;

            public SerializedProperty usableSettings;
            public SerializedProperty combineSettings;
            public SerializedProperty localizationSettings;

            public ItemCache(PropertyCollection properties)
            {
                width = properties["Width"];
                height = properties["Height"];
                orientation = properties["Orientation"];
                flipDirection = properties["FlipDirection"];
                usableSettings = properties["UsableSettings"];
                combineSettings = properties["CombineSettings"];
                localizationSettings = properties["LocalizationSettings"];
            }
        }

        public class BuilderItem : BuilderSelection
        {
            public PropertyCollection Properties;
            public ItemCache ItemCache;

            public string GUID => Properties["GUID"].stringValue;
            public string Title => Properties["Title"].stringValue;

            public BuilderItem(SerializedProperty item)
            {
                Properties = EditorDrawing.GetAllProperties(item);
                ItemCache = new(Properties);
            }
        }

        public class BuilderSection : BuilderSelection
        {
            public SerializedProperty SectionName;
            public SerializedProperty SectionGUID;
            public List<BuilderItem> Items;

            public string GUID => SectionGUID.stringValue;
            public string Name => SectionName.stringValue;

            public BuilderSection(SerializedProperty section)
            {
                Items = new();
                SerializedProperty sectionCls = section.FindPropertyRelative("Section");
                SerializedProperty items = section.FindPropertyRelative("Items");
                SectionName = sectionCls.FindPropertyRelative("Name");
                SectionGUID = sectionCls.FindPropertyRelative("GUID");

                for (int i = 0; i < items.arraySize; i++)
                {
                    SerializedProperty item = items.GetArrayElementAtIndex(i);
                    Items.Add(new BuilderItem(item));
                }
            }
        }

        public class BuilderData
        {
            public SerializedObject SerializedObject;
            public SerializedProperty SectionsArray;
            public List<BuilderSection> Sections;

            public BuilderData(InventoryDatabase inventory)
            {
                SerializedObject = new SerializedObject(inventory);
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

            public bool TryFindItem(string guid, out BuilderItem item)
            {
                foreach (var section in Sections)
                {
                    foreach (var itm in section.Items)
                    {
                        if(itm.GUID == guid)
                        {
                            item = itm;
                            return true;
                        }
                    }
                }

                item = null;
                return false;
            }
        }

        private InventoryDatabase inventory;
        private PlayerItemsManager playerItemsManager;

        private BuilderData builderData;
        private SelectionArgs? selection;
        private Vector2 scrollPosition;

        [SerializeField]
        private TreeViewState itemsViewState;
        private ItemsTreeView itemsTreeView;

        private List<ItemField> itemFields;

        public void Show(InventoryDatabase inventory)
        {
            this.inventory = inventory;
            InitializeTreeView();
            GetAllItemFields();

            if (PlayerPresenceManager.HasReference && PlayerPresenceManager.Instance.Player != null)
                playerItemsManager = PlayerPresenceManager.Instance.Player.GetComponentInChildren<PlayerItemsManager>();
        }

        private void InitializeTreeView()
        {
            builderData = new BuilderData(inventory);
            itemsViewState = new TreeViewState();
            itemsTreeView = new ItemsTreeView(itemsViewState, builderData)
            {
                OnItemSelect = OnItemSelect,
                OnAddNewSection = OnAddNewSection,
                OnAddNewItem = OnAddNewItem,
                OnDeleteSection = OnDeleteSection,
                OnDeleteItem = OnDeleteItem,
                OnMoveItem = OnMoveItem,
                OnMoveItemToSection = OnMoveItemToSection,
                OnMoveItemToSectionAt = OnMoveItemToSectionAt,
                OnMoveSection = OnMoveSection,
                OnRebuild = ReloadBuilder
            };
        }

        private void GetAllItemFields()
        {
            MonoBehaviour[] allMonoBehaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            itemFields = new();

            foreach (MonoBehaviour monoBehaviour in allMonoBehaviours)
            {
                FieldInfo[] fields = monoBehaviour.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (FieldInfo field in fields)
                {
                    if (typeof(ItemField).IsAssignableFrom(field.FieldType))
                    {
                        if (field.GetValue(monoBehaviour) is ItemField itemField)
                        {
                            itemFields.Add(itemField);
                        }
                    }
                }
            }
        }

        private void RefreshAllItemFields()
        {
            foreach (var itemField in itemFields)
            {
                itemField.Refresh();
            }
        }

        public void ReloadBuilder()
        {
            selection = null;
            builderData.SerializedObject.ApplyModifiedProperties();
            builderData.SerializedObject.Update();
            builderData.Reload();
            itemsTreeView.Reload();
        }

        private void OnItemSelect(SelectionArgs? selection)
        {
            this.selection = selection;
        }

        private void OnAddNewSection()
        {
            int sections = inventory.Sections.Count;

            inventory.Sections.Add(new()
            {
                Section = new()
                {
                    Name = NEW_SECTION_PREFIX + "_" + sections,
                    GUID = GameTools.GetGuid(),
                },
                Items = new()
            });
        }

        private void OnAddNewItem(string guid)
        {
            var section = inventory.GetSection(guid);
            int items = section.Items.Count;

            section.Items.Add(new()
            {
                GUID = GameTools.GetGuid(),
                Title = NEW_ITEM_PREFIX + "_" + items,
                SectionGUID = section.Section.GUID
            });
        }

        private void OnDeleteSection(string guid)
        {
            var section = inventory.GetSection(guid);
            if (section.Items.Count > 0 && !EditorUtility.DisplayDialog("Delete Section", $"Are you sure you want to delete section \"{section.Section.Name}\" and it's items?", "Delete", "NO"))
                return;

            for (int i = 0; i < inventory.Sections.Count; i++)
            {
                var _section = inventory.Sections[i];
                if (_section.Section.GUID == guid)
                {
                    inventory.Sections.RemoveAt(i);
                    break;
                }
            }
        }

        private void OnDeleteItem(string sectionGuid, string itemGuid)
        {
            var section = inventory.GetSection(sectionGuid);
            for (int i = 0; i < section.Items.Count; i++)
            {
                var item = section.Items[i];
                if (item.GUID == itemGuid)
                {
                    section.Items.RemoveAt(i);
                    break;
                }
            }
        }

        private void OnMoveItem(string sectionGuid, string itemGuid, int newIndex)
        {
            // find the section
            var sectionIndex = inventory.Sections.FindIndex(s => s.Section.GUID == sectionGuid);
            if (sectionIndex < 0) return;

            // find the item within the section
            var section = inventory.Sections[sectionIndex];
            var itemIndex = section.Items.FindIndex(i => i.GUID == itemGuid);
            if (itemIndex < 0) return;

            // move item within the list
            var item = section.Items[itemIndex];

            int insertTo = newIndex > itemIndex ? newIndex - 1 : newIndex;
            insertTo = Mathf.Clamp(insertTo, 0, section.Items.Count);

            section.Items.RemoveAt(itemIndex);
            section.Items.Insert(insertTo, item);
        }

        private void OnMoveItemToSection(string fromSectionGuid, string itemGuid, string toSectionGuid)
        {
            // find the source section and item
            var fromSectionIndex = inventory.Sections.FindIndex(s => s.Section.GUID == fromSectionGuid);
            if (fromSectionIndex < 0) return;

            var fromSection = inventory.Sections[fromSectionIndex];
            var itemIndex = fromSection.Items.FindIndex(i => i.GUID == itemGuid);
            if (itemIndex < 0) return;

            // remove item from source section
            var item = fromSection.Items[itemIndex];
            fromSection.Items.RemoveAt(itemIndex);

            // find the destination section
            var toSectionIndex = inventory.Sections.FindIndex(s => s.Section.GUID == toSectionGuid);
            if (toSectionIndex < 0) return;

            // add item to destination section
            var toSection = inventory.Sections[toSectionIndex];
            item.SectionGUID = toSection.Section.GUID;
            toSection.Items.Add(item);
        }

        private void OnMoveItemToSectionAt(string fromSectionGuid, string itemGuid, string toSectionGuid, int newIndex)
        {
            // find the source section and item
            var fromSectionIndex = inventory.Sections.FindIndex(s => s.Section.GUID == fromSectionGuid);
            if (fromSectionIndex < 0) return;

            var fromSection = inventory.Sections[fromSectionIndex];
            var itemIndex = fromSection.Items.FindIndex(i => i.GUID == itemGuid);
            if (itemIndex < 0) return;

            // remove item from source section
            var item = fromSection.Items[itemIndex];
            fromSection.Items.RemoveAt(itemIndex);

            // find the destination section
            var toSectionIndex = inventory.Sections.FindIndex(s => s.Section.GUID == toSectionGuid);
            if (toSectionIndex < 0) return;

            // add item to destination section
            var toSection = inventory.Sections[toSectionIndex];
            item.SectionGUID = toSection.Section.GUID;
            toSection.Items.Insert(newIndex, item);
        }

        private void OnMoveSection(string sectionGuid, int newIndex)
        {
            // find the section
            var sectionIndex = inventory.Sections.FindIndex(s => s.Section.GUID == sectionGuid);
            if (sectionIndex < 0) return;

            // move section within the list
            var section = inventory.Sections[sectionIndex];

            int insertTo = newIndex > sectionIndex ? newIndex - 1 : newIndex;
            insertTo = Mathf.Clamp(insertTo, 0, section.Items.Count);

            inventory.Sections.RemoveAt(sectionIndex);
            inventory.Sections.Insert(insertTo, section);
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
                EditorUtility.SetDirty(inventory);
                AssetDatabase.SaveAssetIfDirty(inventory);
                RefreshAllItemFields();
            }

            Rect resetBtn = saveBtn;
            resetBtn.x -= 100f;
            if (GUI.Button(resetBtn, "Restore", EditorStyles.toolbarButton))
            {
                if (EditorUtility.DisplayDialog("Restore Asset", $"Are you sure you want to unload the asset and restore the last saved state?", "Unload", "NO"))
                {
                    Resources.UnloadAsset(inventory);
                    Close();
                }
            }

            Rect localizeBtn = resetBtn;
            localizeBtn.x -= 100f;
            if (GUI.Button(localizeBtn, "Localize Items", EditorStyles.toolbarButton))
            {
                EditorWindow exporter = GetWindow<InventoryItemsExport>(true, "Localize Inventory Items", true);
                exporter.minSize = new Vector2(500, 185);
                exporter.maxSize = new Vector2(500, 185);
                ((InventoryItemsExport)exporter).Show(inventory);
            }

            Rect importBtn = localizeBtn;
            importBtn.x -= 100f;
            if (GUI.Button(importBtn, "Import Items", EditorStyles.toolbarButton))
            {
                EditorWindow importer = GetWindow<InventoryAssetImport>(true, "Import Inventory Asset Items", true);
                importer.minSize = new Vector2(500, 185);
                importer.maxSize = new Vector2(500, 185);
                ((InventoryAssetImport)importer).Show(this, inventory);
            }

            // inspector view
            Rect itemsRect = new(5f, 25f, ITEMS_VIEW_WIDTH, position.height - 30f);
            itemsTreeView.OnGUI(itemsRect);

            if (selection != null)
            {
                string title = "NULL";
                if (selection.Value.Selection is BuilderItem item) title = item.Title;
                else if (selection.Value.Selection is BuilderSection section) title = section.Name;

                Rect inspectorRect = new(ITEMS_VIEW_WIDTH + 10f, 25f, position.width - ITEMS_VIEW_WIDTH - 15f, position.height - 30f);
                GUIContent inspectorTitle = EditorGUIUtility.TrTextContentWithIcon($" INSPECTOR ({title})", "PrefabVariant On Icon");
                EditorDrawing.DrawHeaderWithBorder(ref inspectorRect, inspectorTitle, 20f, false);

                Rect inspectorViewRect = inspectorRect;
                inspectorViewRect.y += Spacing;
                inspectorViewRect.yMax -= Spacing;
                inspectorViewRect.xMin += Spacing;
                inspectorViewRect.xMax -= Spacing;

                GUILayout.BeginArea(inspectorViewRect);
                OnDrawItemInspector(selection.Value);
                GUILayout.EndArea();
            }
        }

        private void OnDrawItemInspector(SelectionArgs selection)
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

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
                var itemCache = item.ItemCache;

                builderData.SerializedObject.Update();
                {
                    // icon
                    Rect baseControlRect = EditorGUILayout.GetControlRect(false, 100);
                    Rect iconRect = baseControlRect;
                    iconRect.width = 100; iconRect.height = 100;
                    properties["Icon"].objectReferenceValue = EditorDrawing.DrawLargeSpriteSelector(iconRect, properties["Icon"].objectReferenceValue);

                    // title
                    Rect titleRect = baseControlRect;
                    titleRect.height = EditorGUIUtility.singleLineHeight;
                    titleRect.xMin = iconRect.xMax + EditorGUIUtility.standardVerticalSpacing * 2;

                    EditorGUI.BeginChangeCheck();
                    properties["Title"].stringValue = EditorGUI.TextField(titleRect, properties["Title"].stringValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        selection.TreeViewItem.displayName = properties["Title"].stringValue;
                    }

                    // description
                    Rect descriptionRect = titleRect;
                    descriptionRect.y = 20f + Spacing;
                    descriptionRect.height = 80f - Spacing;
                    properties["Description"].stringValue = EditorGUI.TextArea(descriptionRect, properties["Description"].stringValue);

#if UHFPS_LOCALIZATION
                    EditorGUILayout.HelpBox("Game localization is enabled, the title and description text will be replaced from the localization asset. To change the text, go to the localization asset and change it from there. If the localization key in the localization section is incorrect, the current title and description will be used.", MessageType.Warning);
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
#endif

                    // item grid view
                    using (new EditorDrawing.IconSizeScope(14))
                    {
                        GUIContent itemViewTitle = EditorGUIUtility.TrTextContentWithIcon("Item View", "GridLayoutGroup Icon");
                        float previewBoxSize = EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing * 2;
                        Rect itemViewRect = EditorGUILayout.GetControlRect(false, 18f + 13f + previewBoxSize);
                        EditorDrawing.DrawHeaderWithBorder(ref itemViewRect, itemViewTitle, 18f, true);
                        {
                            Rect insideItemView = itemViewRect;
                            insideItemView.width -= 10f;
                            insideItemView.height -= 10f;
                            insideItemView.x += 5f;
                            insideItemView.y += 5f;

                            Rect previewControlRect = insideItemView;
                            previewControlRect.height = EditorGUIUtility.singleLineHeight;
                            previewControlRect.xMin += previewBoxSize + EditorGUIUtility.standardVerticalSpacing;

                            // width
                            Rect widthRect = previewControlRect;
                            EditorGUI.LabelField(widthRect, "Width");
                            widthRect.xMin += 50f;
                            itemCache.width.intValue = (ushort)EditorGUI.Slider(widthRect, itemCache.width.intValue, 1, 4);

                            // height
                            Rect heightRect = previewControlRect;
                            heightRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                            EditorGUI.LabelField(heightRect, "Height");
                            heightRect.xMin += 50f;
                            itemCache.height.intValue = (ushort)EditorGUI.Slider(heightRect, itemCache.height.intValue, 1, 4);

                            // orientation
                            Rect orientationRect = previewControlRect;
                            orientationRect.y += EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing * 2;
                            EditorGUI.LabelField(orientationRect, "Image Orientation");
                            orientationRect.xMin += 120f;
                            orientationRect.xMax -= 100f;
                            EditorGUI.PropertyField(orientationRect, itemCache.orientation, GUIContent.none);

                            using (new EditorGUI.DisabledGroupScope(itemCache.orientation.enumValueIndex == 0))
                            {
                                Rect flipDirectionRect = previewControlRect;
                                flipDirectionRect.y = orientationRect.y;
                                flipDirectionRect.xMin = orientationRect.xMax + EditorGUIUtility.standardVerticalSpacing;
                                EditorGUI.PropertyField(flipDirectionRect, itemCache.flipDirection, GUIContent.none);
                            }

                            // inventory preview
                            insideItemView.width = previewBoxSize;
                            DrawGLInventoryPreview(insideItemView, itemCache.width.intValue, itemCache.height.intValue);
                        }

                        EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                        // drop object
                        GUIContent itemObjectTitle = EditorGUIUtility.TrTextContentWithIcon(" Drop Object", "Prefab On Icon");
                        using (new EditorDrawing.BorderBoxScope(itemObjectTitle, roundedBox: true))
                        {
                            properties.Draw("ItemObject", GUIContent.none);
                        }
                    }

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);

                    // settings
                    EditorDrawing.DrawClassBorderFoldout(properties["Settings"], new GUIContent("Settings"));
                    EditorGUILayout.Space(1f);

                    // properties
                    EditorDrawing.DrawClassBorderFoldout(properties["Properties"], new GUIContent("Properties"));
                    EditorGUILayout.Space(1f);

                    // usable settings
                    bool isUsable = properties["Settings"].FindPropertyRelative("isUsable").boolValue;
                    if (isUsable && EditorDrawing.BeginFoldoutBorderLayout(itemCache.usableSettings, new GUIContent("Usable Settings")))
                    {
                        SerializedProperty usableType = itemCache.usableSettings.FindPropertyRelative("usableType");
                        UsableType usableTypeEnum = (UsableType)usableType.enumValueIndex;

                        EditorGUILayout.PropertyField(usableType);
                        if (usableTypeEnum == UsableType.PlayerItem)
                        {
                            SerializedProperty playerItemIndex = itemCache.usableSettings.FindPropertyRelative("playerItemIndex");
                            DrawPlayerItemPicker(playerItemIndex, new GUIContent("Player Item"));
                        }
                        else if (usableTypeEnum == UsableType.HealthItem)
                        {
                            SerializedProperty healthPoints = itemCache.usableSettings.FindPropertyRelative("healthPoints");
                            EditorGUILayout.PropertyField(healthPoints);
                        }
                        else if (usableTypeEnum == UsableType.CustomEvent)
                        {
                            SerializedProperty removeOnUse = itemCache.usableSettings.FindPropertyRelative("removeOnUse");
                            SerializedProperty customData = itemCache.usableSettings.FindPropertyRelative("customData");
                            SerializedProperty jsonData = customData.FindPropertyRelative("JsonData");

                            EditorGUILayout.PropertyField(removeOnUse);

                            Rect foldoutRect = EditorGUILayout.GetControlRect();
                            foldoutRect = EditorGUI.IndentedRect(foldoutRect);

                            if (jsonData.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, jsonData.isExpanded, "Custom JSON Data"))
                            {
                                EditorGUILayout.Space(-EditorGUIUtility.singleLineHeight);
                                EditorGUILayout.PropertyField(jsonData, GUIContent.none);
                            }
                        }

                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    // combine settings
                    EditorGUILayout.Space(1f);
                    if (EditorDrawing.BeginFoldoutBorderLayout(itemCache.combineSettings, new GUIContent("Combine Settings")))
                    {
                        EditorGUILayout.LabelField("Combinations: " + itemCache.combineSettings.arraySize, EditorStyles.miniBoldLabel);

                        for (int i = 0; i < itemCache.combineSettings.arraySize; i++)
                        {
                            DrawCombination(item, i);
                        }

                        EditorGUILayout.Space(2f);
                        EditorDrawing.Separator();
                        EditorGUILayout.Space(2f);

                        EditorGUILayout.BeginHorizontal();
                        {
                            GUILayout.FlexibleSpace();
                            {
                                Rect addSectionRect = EditorGUILayout.GetControlRect(GUILayout.Width(120f));
                                if (GUI.Button(addSectionRect, "Add Combination"))
                                {
                                    int size = itemCache.combineSettings.arraySize++;
                                    SerializedProperty partnerElement = itemCache.combineSettings.GetArrayElementAtIndex(size);
                                    SerializedProperty partnerID = partnerElement.FindPropertyRelative("combineWithID");
                                    partnerID.stringValue = string.Empty;
                                    MirrorCombination(null, partnerElement); // clear new combination values
                                }
                            }
                            GUILayout.FlexibleSpace();
                        }
                        EditorGUILayout.EndHorizontal();

                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    // localization
                    EditorGUILayout.Space(1f);
                    EditorDrawing.DrawClassBorderFoldout(itemCache.localizationSettings, new GUIContent("Localization Settings"));
                }
                builderData.SerializedObject.ApplyModifiedProperties();

                EditorGUILayout.Space(2);
                EditorDrawing.Separator();
                EditorGUILayout.Space(1);

                string sectionGUID = item.Properties["SectionGUID"].stringValue;
                string sectionName = "NULL";

                if (!string.IsNullOrEmpty(sectionGUID))
                {
                    var itemSection = inventory.GetSection(sectionGUID);
                    sectionName = itemSection.Section.Name;
                }

                using (new EditorGUI.DisabledGroupScope(true))
                {
                    sectionGUID = sectionGUID.Or("####");
                    EditorGUILayout.LabelField("Section: " + $"({sectionName}) " + sectionGUID, EditorStyles.miniBoldLabel);
                    EditorGUILayout.LabelField("GUID: " + item.GUID, EditorStyles.miniBoldLabel);
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawPlayerItemPicker(SerializedProperty playerItemProperty, GUIContent title)
        {
            if (playerItemsManager != null)
            {
                Rect playerItemPickerRect = EditorGUILayout.GetControlRect();
                playerItemPickerRect = EditorGUI.PrefixLabel(playerItemPickerRect, title);

                PlayerItemsPicker.PlayerItem[] playerItems = playerItemsManager.PlayerItems
                    .Select((x, i) => new PlayerItemsPicker.PlayerItem() { name = x.Name, index = i }).ToArray();

                GUIContent playerItemFieldContent = new GUIContent("None");
                foreach (var item in playerItems)
                {
                    if (item.index == playerItemProperty.intValue)
                    {
                        playerItemFieldContent = EditorGUIUtility.TrTextContentWithIcon(item.name, "Prefab On Icon");
                        break;
                    }
                }

                if (EditorDrawing.ObjectField(playerItemPickerRect, playerItemFieldContent))
                {
                    PlayerItemsPicker playerItemsPicker = new PlayerItemsPicker(new AdvancedDropdownState(), playerItems);
                    playerItemsPicker.OnItemPressed += index =>
                    {
                        playerItemProperty.intValue = index;
                        builderData.SerializedObject.ApplyModifiedProperties();
                    };

                    Rect playerItemsRect = playerItemPickerRect;
                    playerItemPickerRect.width = 250;
                    playerItemsPicker.Show(playerItemPickerRect);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("To enable player item picker, add a GameManager and a Player to the scene using the Scene Setup option from Tools. Because of this, the property is switched to the classic int property.", MessageType.Warning);
                EditorGUILayout.PropertyField(playerItemProperty, title);
            }
        }

        private void DrawCombination(BuilderItem item, int index)
        {
            SerializedProperty arrayProperty = item.ItemCache.combineSettings;
            SerializedProperty element = arrayProperty.GetArrayElementAtIndex(index);

            SerializedProperty requiredCurrentAmount = element.FindPropertyRelative("requiredCurrentAmount");
            SerializedProperty requiredSecondAmount = element.FindPropertyRelative("requiredSecondAmount");
            SerializedProperty resultItemAmount = element.FindPropertyRelative("resultItemAmount");

            SerializedProperty combineWithID = element.FindPropertyRelative("combineWithID");
            SerializedProperty resultCombineID = element.FindPropertyRelative("resultCombineID");
            SerializedProperty playerItemIndex = element.FindPropertyRelative("playerItemIndex");

            SerializedProperty inheritCustomData = element.FindPropertyRelative("inheritCustomData");
            SerializedProperty inheritFromSecond = element.FindPropertyRelative("inheritFromSecond");
            SerializedProperty inheritKey = element.FindPropertyRelative("inheritKey");

            SerializedProperty customData = element.FindPropertyRelative("customData");
            SerializedProperty jsonData = customData.FindPropertyRelative("JsonData");

            SerializedProperty isCrafting = element.FindPropertyRelative("isCrafting");
            SerializedProperty keepAfterCombine = element.FindPropertyRelative("keepAfterCombine");
            SerializedProperty removeSecondItem = element.FindPropertyRelative("removeSecondItem");
            SerializedProperty eventAfterCombine = element.FindPropertyRelative("eventAfterCombine");
            SerializedProperty selectAfterCombine = element.FindPropertyRelative("selectAfterCombine");
            SerializedProperty haveCustomData = element.FindPropertyRelative("haveCustomData");

            // partner property reference
            BuilderItem partnerItem = null;
            if (!string.IsNullOrEmpty(combineWithID.stringValue))
                builderData.TryFindItem(combineWithID.stringValue, out partnerItem);

            GUIContent headerGUI = new($"Combination {index}");
            if (EditorDrawing.BeginFoldoutBorderLayout(element, headerGUI, out Rect headerRect, 18f, false))
            {
                using (new EditorDrawing.BorderBoxScope())
                {
                    EditorGUILayout.LabelField("Item Settings", EditorStyles.miniBoldLabel);

                    // combine with id field
                    Rect combineWithRect = EditorGUILayout.GetControlRect();
                    combineWithRect = EditorGUI.PrefixLabel(combineWithRect, new GUIContent("Combine With Item"));
                    combineWithRect.xMax -= 80f;

                    GUIContent combineWithGUI = new GUIContent("Set Combination Partner");
                    if (partnerItem != null) combineWithGUI = new GUIContent(partnerItem.Title);

                    // combine with button
                    if (GUI.Button(combineWithRect, combineWithGUI, EditorStyles.miniButton))
                    {
                        CustomDropdownItem[] items = builderData.Sections
                            .SelectMany(x => x.Items.Select(y => new CustomDropdownItem(x.Name + "/" + y.Title, y.GUID)))
                            .ToArray();

                        CustomDropdown itemPicker = new(new(), "Inventory Items", items);
                        itemPicker.OnItemSelected += item =>
                        {
                            combineWithID.stringValue = (string)item.Item;
                            builderData.SerializedObject.ApplyModifiedProperties();
                        };

                        Rect dropdownRect = combineWithRect;
                        dropdownRect.width = 250;
                        itemPicker.Show(dropdownRect);
                    }

                    // combine with mirror button
                    Rect mirrorBtnRect = combineWithRect;
                    mirrorBtnRect.xMin = mirrorBtnRect.xMax;
                    mirrorBtnRect.xMax += 80f;

                    using (new EditorGUI.DisabledScope(partnerItem == null || item.GUID == combineWithID.stringValue))
                    {
                        if (GUI.Button(mirrorBtnRect, new GUIContent("Mirror", "Mirror combination with partner item."), EditorStyles.miniButton))
                        {
                            bool addNew = true;
                            for (int i = 0; i < partnerItem.ItemCache.combineSettings.arraySize; i++)
                            {
                                SerializedProperty partnerElement = partnerItem.ItemCache.combineSettings.GetArrayElementAtIndex(i);
                                SerializedProperty partnerID = partnerElement.FindPropertyRelative("combineWithID");
                                if (partnerID.stringValue.Equals(item.GUID))
                                {
                                    MirrorCombination(element, partnerElement);
                                    addNew = false;
                                    break;
                                }
                            }

                            if (addNew)
                            {
                                int size = partnerItem.ItemCache.combineSettings.arraySize++;
                                SerializedProperty partnerElement = partnerItem.ItemCache.combineSettings.GetArrayElementAtIndex(size);
                                SerializedProperty partnerID = partnerElement.FindPropertyRelative("combineWithID");
                                partnerID.stringValue = item.GUID;
                                MirrorCombination(element, partnerElement);
                            }

                            Debug.Log($"[Inventory Builder] {headerGUI.text} was mirrored with {partnerItem.Title}");
                        }
                    }

                    if (!selectAfterCombine.boolValue)
                    {
                        // combine result field
                        Rect combineResultRect = EditorGUILayout.GetControlRect();
                        combineResultRect = EditorGUI.PrefixLabel(combineResultRect, new GUIContent("Combine Result Item"));

                        GUIContent combineResultGUI = new GUIContent("Set Combination Result");
                        if (!string.IsNullOrEmpty(resultCombineID.stringValue))
                        {
                            if (builderData.TryFindItem(resultCombineID.stringValue, out BuilderItem result))
                            {
                                combineResultGUI = new GUIContent(result.Title);
                            }
                        }

                        // combine result button
                        if (GUI.Button(combineResultRect, combineResultGUI, EditorStyles.miniButton))
                        {
                            CustomDropdownItem[] items = builderData.Sections
                                .SelectMany(x => x.Items.Select(y => new CustomDropdownItem(x.Name + "/" + y.Title, y.GUID)))
                                .ToArray();

                            CustomDropdown itemPicker = new(new(), "Inventory Items", items);
                            itemPicker.OnItemSelected += item =>
                            {
                                resultCombineID.stringValue = (string)item.Item;
                                builderData.SerializedObject.ApplyModifiedProperties();
                            };

                            Rect dropdownRect = combineResultRect;
                            dropdownRect.width = 250;
                            itemPicker.Show(dropdownRect);
                        }
                    }
                    else
                    {
                        DrawPlayerItemPicker(playerItemIndex, new GUIContent("Result Player Item"));
                    }
                }

                // properties
                EditorGUILayout.Space(1f);
                using (new EditorDrawing.BorderBoxScope())
                {
                    EditorGUILayout.LabelField("Properties", EditorStyles.miniBoldLabel);
                    EditorGUILayout.PropertyField(isCrafting);

                    using (new EditorGUI.DisabledGroupScope(isCrafting.boolValue))
                    {
                        EditorGUILayout.PropertyField(keepAfterCombine);
                        EditorGUILayout.PropertyField(removeSecondItem);
                        EditorGUILayout.PropertyField(eventAfterCombine);
                        EditorGUILayout.PropertyField(selectAfterCombine);
                    }

                    EditorGUILayout.PropertyField(haveCustomData);
                }

                // crafting
                if (isCrafting.boolValue)
                {
                    EditorGUILayout.Space(1f);
                    using (new EditorDrawing.BorderBoxScope())
                    {
                        EditorGUILayout.LabelField("Crafting Settings", EditorStyles.miniBoldLabel);
                        EditorGUILayout.PropertyField(requiredCurrentAmount);
                        EditorGUILayout.PropertyField(requiredSecondAmount);
                        EditorGUILayout.PropertyField(resultItemAmount);
                    }
                }

                // custom data
                if (haveCustomData.boolValue)
                {
                    EditorGUILayout.Space(1f);
                    using (new EditorDrawing.BorderBoxScope())
                    {
                        EditorGUILayout.LabelField("Custom Data Settings", EditorStyles.miniBoldLabel);
                        EditorGUILayout.PropertyField(inheritCustomData);

                        if (inheritCustomData.boolValue)
                        {
                            EditorGUILayout.PropertyField(inheritFromSecond);
                            EditorGUILayout.PropertyField(inheritKey);
                        }
                        else
                        {
                            Rect foldoutRect = EditorGUILayout.GetControlRect();
                            foldoutRect = EditorGUI.IndentedRect(foldoutRect);

                            if (jsonData.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, jsonData.isExpanded, "Custom JSON Data"))
                            {
                                EditorGUILayout.Space(-EditorGUIUtility.singleLineHeight);
                                EditorGUILayout.PropertyField(jsonData, GUIContent.none);
                            }
                            EditorGUI.EndFoldoutHeaderGroup();
                        }
                    }
                }

                EditorDrawing.EndBorderHeaderLayout();
            }

            Rect minusRect = headerRect;
            minusRect.xMin = minusRect.xMax - EditorGUIUtility.singleLineHeight;
            minusRect.y += 3f;
            minusRect.x -= 2f;

            if (GUI.Button(minusRect, EditorUtils.Styles.TrashIcon, EditorStyles.iconButton))
            {
                GenericMenu popup = new GenericMenu();

                popup.AddItem(new GUIContent("Delete"), false, () =>
                {
                    arrayProperty.DeleteArrayElementAtIndex(index);
                });

                if (partnerItem != null)
                {
                    popup.AddItem(new GUIContent("Delete With Mirrored"), false, () =>
                    {
                        for (int i = 0; i < partnerItem.ItemCache.combineSettings.arraySize; i++)
                        {
                            SerializedProperty partnerElement = partnerItem.ItemCache.combineSettings.GetArrayElementAtIndex(i);
                            SerializedProperty partnerID = partnerElement.FindPropertyRelative("combineWithID");

                            if (partnerID.stringValue == item.GUID)
                            {
                                partnerItem.ItemCache.combineSettings.DeleteArrayElementAtIndex(i);
                                break;
                            }
                        }

                        arrayProperty.DeleteArrayElementAtIndex(index);
                    });
                }
                else
                {
                    popup.AddDisabledItem(new GUIContent("Delete With Mirrored"));
                }

                popup.ShowAsContext();
            }
        }

        private void MirrorCombination(SerializedProperty current, SerializedProperty partner)
        {
            string resultID = current != null ? current.FindPropertyRelative("resultCombineID").stringValue : "";
            partner.FindPropertyRelative("resultCombineID").stringValue = resultID;

            bool keepAfterCombine = current != null && current.FindPropertyRelative("keepAfterCombine").boolValue;
            partner.FindPropertyRelative("keepAfterCombine").boolValue = keepAfterCombine;

            bool eventAfterCombine = current != null && current.FindPropertyRelative("eventAfterCombine").boolValue;
            partner.FindPropertyRelative("eventAfterCombine").boolValue = eventAfterCombine;
        }

        private void DrawGLInventoryPreview(Rect rect, int w, int h)
        {
            Material _uiMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);

            float spacing = 5f;
            int slots = Math.Clamp(Math.Max(w, h), 2, 4);
            float slotSize = (rect.width - spacing * (slots + 1)) / slots;

            if (Event.current.type == EventType.Repaint)
            {
                GUI.BeginClip(rect);
                {
                    GL.PushMatrix();
                    GL.LoadPixelMatrix();
                    _uiMaterial.SetPass(0);

                    Vector2 slotStart = new Vector2(spacing + 0.5f, spacing);

                    GL.Begin(GL.LINES);
                    {
                        // draw slots
                        Vector2 _tSlotStart = slotStart;
                        for (int y = 0; y < slots; y++)
                        {
                            for (int x = 0; x < slots; x++)
                            {
                                Vector2 slotLU = _tSlotStart + x * new Vector2(slotSize + spacing, 0);
                                Vector2 slotRU = slotLU + new Vector2(slotSize, 0);
                                Vector2 slotLD = slotLU + new Vector2(0, slotSize);
                                Vector2 slotRD = slotLU + new Vector2(slotSize, slotSize);

                                Line(slotLU, slotRU);
                                Line(slotRU, slotRD);
                                Line(slotRD, slotLD);
                                Line(slotLD, slotLU);
                            }

                            _tSlotStart.y += slotSize + spacing;
                        }
                    }
                    GL.End();

                    GL.Begin(GL.QUADS);
                    {
                        // draw item preview
                        Vector2 _tItemStart = slotStart;
                        _tItemStart.y -= 1;
                        GL.Color(Color.red.Alpha(0.35f));

                        Vector2 itemPrewWidth = w * new Vector2(slotSize, 0) + (w - 1) * new Vector2(spacing, 0) + new Vector2(1, 0);
                        Vector2 itemPrewHeight = h * new Vector2(0, slotSize) + (h - 1) * new Vector2(0, spacing) + new Vector2(0, 1);

                        Vector2 itemLU = _tItemStart;
                        Vector2 itemRU = itemLU + itemPrewWidth;
                        Vector2 itemLD = itemLU + itemPrewHeight;
                        Vector2 itemRD = itemLD + itemPrewWidth;

                        Line(itemLU, itemRU);
                        Line(itemRU, itemRD);
                        Line(itemRD, itemLD);
                        Line(itemLD, itemLU);
                    }
                    GL.End();
                    GL.PopMatrix();
                }
                GUI.EndClip();
            }
        }

        private void Line(Vector2 p1, Vector2 p2)
        {
            GL.Vertex(p1);
            GL.Vertex(p2);
        }

        internal class PlayerItemsPicker : AdvancedDropdown
        {
            public class PlayerItem
            {
                public string name;
                public int index;
            }

            private class PlayerItemsDropdownItem : AdvancedDropdownItem
            {
                public PlayerItem playerItem;

                public PlayerItemsDropdownItem(PlayerItem playerItem) : base(playerItem.name)
                {
                    this.playerItem = playerItem;
                }
            }

            private readonly PlayerItem[] playerItems;
            public event Action<int> OnItemPressed;

            public PlayerItemsPicker(AdvancedDropdownState state, PlayerItem[] playerItems) : base(state)
            {
                this.playerItems = playerItems;
                minimumSize = new Vector2(200f, 250f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Player Items");

                if (playerItems.Length > 0)
                {
                    root.AddChild(new PlayerItemsDropdownItem(new PlayerItem() { name = "None", index = -1 }));

                    foreach (var item in playerItems)
                    {
                        var dropdownItem = new PlayerItemsDropdownItem(item);
                        dropdownItem.icon = (Texture2D)EditorGUIUtility.TrIconContent("Prefab On Icon").image;
                        root.AddChild(dropdownItem);
                    }
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                OnItemPressed?.Invoke((item as PlayerItemsDropdownItem).playerItem.index);
            }
        }
    }
}