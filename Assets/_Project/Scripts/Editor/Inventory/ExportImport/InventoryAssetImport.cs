using UnityEditor;
using UnityEngine;
using UHFPS.Tools;
using UHFPS.Scriptable;
using static UHFPS.Scriptable.InventoryDatabase;

namespace UHFPS.Editors
{
    public class InventoryAssetImport : EditorWindow
    {
        private InventoryBuilder builder;
        private InventoryDatabase database;

        private InventoryAsset inventoryAsset;
        private string sectionName = "Imported Items";

        public void Show(InventoryBuilder builder, InventoryDatabase database)
        {
            this.builder = builder;
            this.database = database;
        }

        private void OnGUI()
        {
            Rect rect = position;
            rect.xMin += 5f;
            rect.xMax -= 5f;
            rect.yMin += 5f;
            rect.yMax -= 5f;
            rect.x = 5;
            rect.y = 5;

            GUILayout.BeginArea(rect);
            {
                EditorGUILayout.HelpBox("This tool imports all items from the old Inventory Asset SO to the new Inventory Database SO into one section. You can then move the items to the appropriate sections.", MessageType.Info);

                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    inventoryAsset = (InventoryAsset)EditorGUILayout.ObjectField(new GUIContent("Inventory Asset"), inventoryAsset, typeof(InventoryAsset), false);
                    sectionName = EditorGUILayout.TextField(new GUIContent("Section Name"), sectionName);

                    EditorGUILayout.Space();
                    using (new EditorGUI.DisabledGroupScope(inventoryAsset == null))
                    {
                        if (GUILayout.Button(new GUIContent("Import Items"), GUILayout.Height(25f)))
                        {
                            ItemsSection itemsSection = new()
                            {
                                Section = new()
                                {
                                    Name = sectionName,
                                    GUID = GameTools.GetGuid(),
                                },
                                Items = new()
                            };

                            foreach (var item in inventoryAsset.Items)
                            {
                                var newItem = item.item.DeepCopy();
                                newItem.SectionGUID = itemsSection.Section.GUID;
                                newItem.GUID = item.guid;

                                itemsSection.Items.Add(newItem);
                            }

                            database.Sections.Add(itemsSection);
                            builder.ReloadBuilder();

                            Debug.Log("Items from Inventory Asset have been successfully imported!");
                        }
                    }
                }
            }
            GUILayout.EndArea();
        }
    }
}