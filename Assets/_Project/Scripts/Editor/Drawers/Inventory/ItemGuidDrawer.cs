using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEditor;
using UHFPS.Tools;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Editors;
using NUnit.Framework.Internal;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(ItemGuid))]
    public class ItemGuidDrawer : PropertyDrawer
    {
        private readonly InventoryDatabase inventoryDatabase;
        private readonly List<CustomDropdownItem> items;
        private readonly bool hasInvReference;

        private bool isItemChecked;
        private bool isDirty;

        public ItemGuidDrawer()
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
            var sectionNameProp = sectionProp.FindPropertyRelative("Name");

            EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, label);
            {
                GUIContent buttonContent = GUIContent.none;
                string itemTitle = "Select Item";

                if (!hasInvReference)
                {
                    buttonContent = EditorGUIUtility.TrTextContentWithIcon(" No Inventory Reference", "console.warnicon");
                }
                else if (inventoryDatabase == null)
                {
                    buttonContent = EditorGUIUtility.TrTextContentWithIcon(" Assign InventoryDatabase", "console.warnicon");
                }
                else
                {
                    string section = sectionNameProp.stringValue;
                    string item = itemNameProp.stringValue;

                    bool isEmpty = section.IsEmpty() || item.IsEmpty();
                    if (!isEmpty) itemTitle = section + "/" + item;

                    if (isEmpty || CheckItemValidity(property, guid))
                    {
                        buttonContent = EditorGUIUtility.TrTextContentWithIcon(itemTitle, "Prefab On Icon");
                    }
                    else
                    {
                        buttonContent = EditorGUIUtility.TrTextContentWithIcon(itemTitle, "console.warnicon");
                        buttonContent.tooltip = "The item is not valid. Please refresh!";
                    }
                }

                Rect dropdownRect = position;
                dropdownRect.width = 250f;
                dropdownRect.height = 0f;
                dropdownRect.y += EditorGUIUtility.singleLineHeight;
                dropdownRect.x += position.xMax - dropdownRect.width - EditorGUIUtility.singleLineHeight;

                Rect objectRect = position;
                objectRect.xMax -= EditorGUIUtility.singleLineHeight + 2f;

                using (new EditorGUI.DisabledGroupScope(!hasInvReference))
                {
                    if (EditorDrawing.ObjectField(objectRect, buttonContent))
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

                Rect refreshRect = position;
                refreshRect.xMin = position.xMax - EditorGUIUtility.singleLineHeight;
                refreshRect.y += 1f;

                using (new EditorGUI.DisabledGroupScope(!hasInvReference || guid.IsEmpty()))
                {
                    if (GUI.Button(refreshRect, EditorUtils.Styles.RefreshIcon, EditorStyles.iconButton))
                    {
                        if (inventoryDatabase == null)
                            throw new NullReferenceException("Inventory database asset is not assigned!");

                        Refresh(property, guid);
                        property.serializedObject.ApplyModifiedProperties();
                    }
                }
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

                if (item.Icon != null)
                    itemIconProp.objectReferenceValue = item.Icon.texture;

                sectionNameProp.stringValue = section.Name;
                sectionGUIDProp.stringValue = section.GUID;
            }
            else throw new MissingReferenceException("Could not find item with GUID: " + GUID);

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

                    if (itemNameProp.stringValue != item.Title)
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

                isItemChecked = true;
            }

            return !isDirty;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}