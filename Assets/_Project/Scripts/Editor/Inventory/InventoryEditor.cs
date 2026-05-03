using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(Inventory))]
    public class InventoryEditor : InspectorEditor<Inventory>
    {
        private List<CustomDropdownItem> items;

        public override void OnEnable()
        {
            base.OnEnable();
            if (Target.inventoryDatabase != null)
            {
                var _sections = Target.inventoryDatabase.Sections;
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

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorDrawing.DrawInspectorHeader(new GUIContent("Inventory"), Target);
            EditorGUILayout.Space();

            Properties.Draw("inventoryDatabase");
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
            Properties.Draw("inventoryContainers");
            Properties.Draw("slotsLayoutGrid");
            Properties.Draw("itemsTransform");
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            if (EditorDrawing.BeginFoldoutBorderLayout(Properties["itemsTransform"], new GUIContent("Controls Settings")))
            {
                EditorGUI.indentLevel++;
                Properties.Draw("ControlsContexts");
                EditorGUI.indentLevel--;
                EditorDrawing.EndBorderHeaderLayout();
            }
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["settings"], new GUIContent("Items Settings"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["slotSettings"], new GUIContent("Slot Settings"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["containerSettings"], new GUIContent("Container Settings"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["itemInfo"], new GUIContent("Item Info"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["shortcutSettings"], new GUIContent("Shortcut Settings"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["promptSettings"], new GUIContent("Prompt Settings"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["contextMenu"], new GUIContent("Context Menu"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["sounds"], new GUIContent("Sounds"));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Features", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledGroupScope(Target.inventoryDatabase == null))
            {
                bool siExpanded = Properties["startingItems"].isExpanded;
                if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Starting Items"), ref siExpanded))
                {
                    for (int i = 0; i < Properties["startingItems"].arraySize; i++)
                    {
                        SerializedProperty property = Properties["startingItems"].GetArrayElementAtIndex(i);
                        DrawStartingItem(property, i);
                        EditorGUILayout.Space(1f);
                    }

                    EditorGUILayout.Space(1f);
                    if (GUILayout.Button("Add Starting Item"))
                    {
                        Target.startingItems.Add(new Inventory.StartingItem());
                    }

                    EditorDrawing.EndBorderHeaderLayout();
                }
                Properties["startingItems"].isExpanded = siExpanded;
            }

            EditorGUILayout.Space(1f);

            bool esEnabled = Properties.GetRelative("expandableSlots.enabled").boolValue;
            if (EditorDrawing.BeginFoldoutToggleBorderLayout(Properties["expandableSlots"], new GUIContent("Expandable Slots"), ref esEnabled))
            {
                using (new EditorGUI.DisabledScope(!esEnabled))
                {
                    EditorGUILayout.PropertyField(Properties.GetRelative("expandableSlots.showExpandableSlots"));
                    EditorGUILayout.PropertyField(Properties.GetRelative("expandableSlots.expandableRows"));
                }
                EditorDrawing.EndBorderHeaderLayout();
            }
            Properties.GetRelative("expandableSlots.enabled").boolValue = esEnabled;

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStartingItem(SerializedProperty property, int index)
        {
            SerializedProperty guid = property.FindPropertyRelative("GUID");
            SerializedProperty title = property.FindPropertyRelative("title");
            SerializedProperty quantity = property.FindPropertyRelative("quantity");
            SerializedProperty setShortcut = property.FindPropertyRelative("setShortcut");
            SerializedProperty shortcut = property.FindPropertyRelative("shortcutKey");
            SerializedProperty data = property.FindPropertyRelative("data");
            SerializedProperty jsonData = data.FindPropertyRelative("JsonData");

            GUIContent headerTitle = new GUIContent($"[{index}] None");
            string itemTitle = string.Empty;

            if (Target.inventoryDatabase != null && !string.IsNullOrEmpty(guid.stringValue))
            {
                if(Target.inventoryDatabase.TryGetItemByGUID(guid.stringValue, out Item item))
                {
                    headerTitle.text = $"[{index}] {item.Title}";
                    itemTitle = item.Title;
                }
            }

            bool isExpanded = property.isExpanded;
            if (EditorDrawing.BeginFoldoutBorderLayout(headerTitle, ref isExpanded, out Rect headerRect, 18f, false))
            {
                DrawItemGUID(guid, itemTitle);
                EditorGUILayout.PropertyField(quantity);
                if (quantity.intValue < 1) quantity.intValue = 1;

                EditorGUILayout.PropertyField(setShortcut);
                if (setShortcut.boolValue)
                {
                    EditorGUILayout.PropertyField(shortcut);
                    if (shortcut.intValue > 4) shortcut.intValue = 4;
                }

                EditorGUI.indentLevel++;
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
                EditorGUI.indentLevel--;

                EditorDrawing.EndBorderHeaderLayout();
            }
            property.isExpanded = isExpanded;

            Rect removeRect = headerRect;
            removeRect.xMin = removeRect.xMax - EditorGUIUtility.singleLineHeight;
            removeRect.y += 3f;
            removeRect.x -= 2f;

            if (GUI.Button(removeRect, EditorUtils.Styles.TrashIcon, EditorStyles.iconButton))
            {
                Properties["startingItems"].DeleteArrayElementAtIndex(index);
            }
        }

        private void DrawItemGUID(SerializedProperty guid, string itemTitle)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, new GUIContent("GUID"));

            GUIContent buttonContent = new("Select Item");

            if (Target.inventoryDatabase == null)
            {
                buttonContent.text = "Inventory database not assigned!";
            }
            else if(!string.IsNullOrEmpty(guid.stringValue))
            {
                buttonContent = EditorGUIUtility.TrTextContentWithIcon(itemTitle, "Prefab On Icon");
            }

            Rect dropdownRect = rect;
            dropdownRect.width = 250f;
            dropdownRect.height = 0f;
            dropdownRect.y += 21f;
            dropdownRect.x += rect.xMax - dropdownRect.width - EditorGUIUtility.singleLineHeight;

            if (EditorDrawing.ObjectField(rect, buttonContent))
            {
                CustomDropdown customDropdown = new(new AdvancedDropdownState(), "Item", items);
                customDropdown.OnItemSelected += (item) =>
                {
                    guid.stringValue = (string)item.Item;
                    serializedObject.ApplyModifiedProperties();
                };
                customDropdown.Show(dropdownRect);
            }
        }
    }
}