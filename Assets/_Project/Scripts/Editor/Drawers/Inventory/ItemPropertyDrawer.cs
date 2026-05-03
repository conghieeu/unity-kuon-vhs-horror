using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using UHFPS.Tools;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Editors;
using System;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(ItemProperty))]
    public class ItemPropertyDrawer : PropertyDrawer
    {
        private readonly InventoryDatabase inventoryDatabase;
        private readonly List<CustomDropdownItem> items;
        private readonly bool hasInvReference;

        private bool isItemChecked;
        private bool isDirty;

        public ItemPropertyDrawer()
        {
            if (Inventory.HasReference)
            {
                inventoryDatabase = Inventory.Instance.inventoryDatabase;
                hasInvReference = true;

                if (inventoryDatabase != null && inventoryDatabase.Sections.Count > 0)
                {
                    var _sections = inventoryDatabase.Sections;

                    items = new();
                    foreach (var section in _sections)
                    {
                        foreach (var item in section.Items)
                        {
                            items.Add(new CustomDropdownItem()
                            {
                                Item = item.GUID,
                                Path = section.Section.Name + "/" + item.Title
                            });
                        }
                    }
                }
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var guidProp = property.FindPropertyRelative("m_GUID");
            string guid = guidProp.stringValue;

            var itemProp = property.FindPropertyRelative("m_Item");
            var sectionProp = property.FindPropertyRelative("m_Section");

            var itemNameProp = itemProp.FindPropertyRelative("Name");
            var itemIconProp = itemProp.FindPropertyRelative("Icon");
            var sectionNameProp = sectionProp.FindPropertyRelative("Name");

            EditorGUI.BeginProperty(position, label, property);
            {
                GUIContent itemTitleGUI = GUIContent.none;
                Texture2D itemIcon = null;

                string itemTitle = "Select Item";
                string itemGUID = "None";

                if (!hasInvReference)
                {
                    itemTitleGUI = EditorGUIUtility.TrTextContentWithIcon(" No Inventory Reference!", "console.warnicon");
                }
                else if (inventoryDatabase == null)
                {
                    itemTitleGUI = EditorGUIUtility.TrTextContentWithIcon(" Assign InventoryDatabase!", "console.warnicon");
                }
                else
                {
                    string section = sectionNameProp.stringValue;
                    string item = itemNameProp.stringValue;

                    bool isEmpty = section.IsEmpty() || item.IsEmpty();
                    if (!isEmpty) itemTitle = section + "/" + item;

                    if (isEmpty || CheckItemValidity(property, guid))
                    {
                        itemTitleGUI = new GUIContent(itemTitle);
                    }
                    else
                    {
                        itemTitleGUI = EditorGUIUtility.TrTextContentWithIcon(" " + itemTitle, "console.warnicon");
                        itemTitleGUI.tooltip = "The item is not valid. Please refresh!";
                    }

                    itemGUID = guid.Or("None");
                    Texture2D icon = itemIconProp.objectReferenceValue as Texture2D;

                    if (icon != null)
                        itemIcon = icon;
                }

                Rect dropdownRect = position;
                dropdownRect.width = 250f;
                dropdownRect.height = 0f;
                dropdownRect.y += 21f;
                dropdownRect.x += position.xMax - dropdownRect.width - EditorGUIUtility.singleLineHeight;

                Rect headerRect = EditorDrawing.DrawHeaderWithBorder(ref position, label);

                Rect refreshButton = headerRect;
                refreshButton.width = EditorGUIUtility.singleLineHeight;
                refreshButton.height = EditorGUIUtility.singleLineHeight;
                refreshButton.x = position.width;
                refreshButton.y += 2f;

                using (new EditorGUI.DisabledGroupScope(!hasInvReference || guid.IsEmpty()))
                {
                    if (GUI.Button(refreshButton, EditorUtils.Styles.RefreshIcon, EditorStyles.iconButton))
                    {
                        if (inventoryDatabase == null)
                            throw new NullReferenceException("Inventory database asset is not assigned!");

                        Refresh(property, guid);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }

                Rect linkButton = refreshButton;
                linkButton.x = refreshButton.x - 19f;

                using (new EditorDrawing.IconSizeScope(16))
                {
                    using (new EditorGUI.DisabledGroupScope(inventoryDatabase == null))
                    {
                        if (GUI.Button(linkButton, EditorUtils.Styles.Linked, EditorStyles.iconButton))
                        {
                            CustomDropdown customDropdown = new(new AdvancedDropdownState(), "Item", items);
                            customDropdown.OnItemSelected += (item) =>
                            {
                                string newGUID = (string)item.Item;
                                guidProp.stringValue = newGUID;
                                Refresh(property, newGUID);
                                property.serializedObject.ApplyModifiedProperties();
                            };
                            customDropdown.Show(dropdownRect);
                        }
                    }
                }

                Rect iconPreviewRect = position;
                float iconPreviewSize = 3f + EditorGUIUtility.singleLineHeight * 2;
                iconPreviewRect.width = iconPreviewSize;
                iconPreviewRect.height = iconPreviewSize;
                iconPreviewRect.y += 2f;
                iconPreviewRect.x += 2f;

                Rect iconPreviewTextureRect = iconPreviewRect;
                if (!itemIcon)
                {
                    iconPreviewTextureRect.width -= 18f;
                    iconPreviewTextureRect.height -= 18f;
                    iconPreviewTextureRect.y += 9f;
                    iconPreviewTextureRect.x += 9f;
                }

                GUI.Box(iconPreviewRect, GUIContent.none, EditorStyles.helpBox);
                Texture missingImage = Resources.Load<Texture2D>("EditorIcons/no_icon");
                EditorDrawing.DrawTransparentTexture(iconPreviewTextureRect, itemIcon != null ? itemIcon : missingImage);

                Rect itemTitleRect = iconPreviewRect;
                itemTitleRect.width = position.width;
                itemTitleRect.height = EditorGUIUtility.singleLineHeight;
                itemTitleRect.xMin += iconPreviewSize + EditorGUIUtility.standardVerticalSpacing;
                itemTitleRect.xMax -= 4f;
                itemTitleRect.y += 2f;

                Rect itemGUIDRect = itemTitleRect;
                itemGUIDRect.y += EditorGUIUtility.singleLineHeight - 1f;

                EditorGUI.LabelField(itemTitleRect, itemTitleGUI, EditorStyles.miniBoldLabel);
                EditorGUI.LabelField(itemGUIDRect, "GUID: " + itemGUID, EditorStyles.miniBoldLabel);
            }
            EditorGUI.EndProperty();
        }

        private void Refresh(SerializedProperty property, string GUID)
        {
            if (inventoryDatabase.TryGetItemWithSection(GUID, out var section, out var item))
            {
                var itemProp = property.FindPropertyRelative("m_Item");
                var sectionProp = property.FindPropertyRelative("m_Section");

                var itemNameProp = itemProp.FindPropertyRelative("Name");
                var itemGUIDProp = itemProp.FindPropertyRelative("GUID");
                var itemIconProp = itemProp.FindPropertyRelative("Icon");
                var sectionNameProp = sectionProp.FindPropertyRelative("Name");
                var sectionGUIDProp = sectionProp.FindPropertyRelative("GUID");

                itemNameProp.stringValue = item.Title;
                itemGUIDProp.stringValue = item.GUID;

                if(item.Icon != null)
                    itemIconProp.objectReferenceValue = item.Icon.texture;

                sectionNameProp.stringValue = section.Name;
                sectionGUIDProp.stringValue = section.GUID;
            }
            else throw new MissingReferenceException("Could not find item with GUID: " + GUID + ". Please check Inventory Database!");

            isDirty = false;
            isItemChecked = false;
        }

        private bool CheckItemValidity(SerializedProperty property, string GUID)
        {
            if ((isDirty || !isItemChecked) && hasInvReference && inventoryDatabase != null)
            {
                if (inventoryDatabase.TryGetItemWithSection(GUID, out var section, out var item))
                {
                    var itemProp = property.FindPropertyRelative("m_Item");
                    var sectionProp = property.FindPropertyRelative("m_Section");

                    var itemNameProp = itemProp.FindPropertyRelative("Name");
                    var itemGUIDProp = itemProp.FindPropertyRelative("GUID");
                    var itemIconProp = itemProp.FindPropertyRelative("Icon");
                    var sectionNameProp = sectionProp.FindPropertyRelative("Name");
                    var sectionGUIDProp = sectionProp.FindPropertyRelative("GUID");

                    if(itemNameProp.stringValue != item.Title)
                    {
                        isDirty = true;
                        return false;
                    }

                    if (itemGUIDProp.stringValue != item.GUID)
                    {
                        isDirty = true;
                        return false;
                    }

                    if (item.Icon != null && itemIconProp.objectReferenceValue != item.Icon.texture)
                    {
                        isDirty = true;
                        return false;
                    }

                    if (sectionNameProp.stringValue != section.Name)
                    {
                        isDirty = true;
                        return false;
                    }

                    if (sectionGUIDProp.stringValue != section.GUID)
                    {
                        isDirty = true;
                        return false;
                    }
                }
                else
                {
                    isDirty = true;
                    return false;
                }

                isItemChecked = true;
            }

            return !isDirty;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 11f;
        }
    }
}