using System.Linq;
using UnityEditor;
using UnityEngine;
using UHFPS.Scriptable;

namespace UHFPS.Editors
{
    public class InventoryItemsExport : EditorWindow
    {
        private InventoryDatabase asset;
        private GameLocalizationAsset localizationAsset;
        private string keysPrefix = "item";

        public int ItemsCount => asset.Sections.Sum(x => x.Items.Count());

        public void Show(InventoryDatabase asset)
        {
            this.asset = asset;
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
                EditorGUILayout.HelpBox("This tool automatically generates keys for the item title and description. These keys will be exported to the GameLocalization asset and assigned to the items. The title and description will be populated with the item's Title and Description text.", MessageType.Info);
                EditorGUILayout.HelpBox((ItemsCount * 2) + " keys will be exported.", MessageType.Info);

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    localizationAsset = (GameLocalizationAsset)EditorGUILayout.ObjectField(new GUIContent("GameLocalization Asset"), localizationAsset, typeof(GameLocalizationAsset), false);
                    keysPrefix = EditorGUILayout.TextField(new GUIContent("Keys Prefix"), keysPrefix);

                    EditorGUILayout.Space();
                    using (new EditorGUI.DisabledGroupScope(localizationAsset == null))
                    {
                        if (GUILayout.Button(new GUIContent("Export Keys"), GUILayout.Height(25f)))
                        {
                            SerializedObject serializedObject = new SerializedObject(asset);
                            SerializedProperty sectionList = serializedObject.FindProperty("Sections");

                            localizationAsset.RemoveSection(keysPrefix);

                            for (int i = 0; i < sectionList.arraySize; i++)
                            {
                                SerializedProperty section = sectionList.GetArrayElementAtIndex(i);
                                SerializedProperty itemsList = section.FindPropertyRelative("Items");

                                for(int j = 0; j < itemsList.arraySize; j++)
                                {
                                    SerializedProperty item = itemsList.GetArrayElementAtIndex(j);
                                    SerializedProperty title = item.FindPropertyRelative("Title");
                                    SerializedProperty description = item.FindPropertyRelative("Description");

                                    SerializedProperty localization = item.FindPropertyRelative("LocalizationSettings");
                                    SerializedProperty titleKeyProp = localization.FindPropertyRelative("titleKey");
                                    SerializedProperty descKeyProp = localization.FindPropertyRelative("descriptionKey");

                                    SerializedProperty gTitle = titleKeyProp.FindPropertyRelative("GlocText");
                                    SerializedProperty gDesc = descKeyProp.FindPropertyRelative("GlocText");

                                    string itemTitle = title.stringValue.Replace(" ", "").ToLower();
                                    string titleKey = keysPrefix + ".title." + itemTitle;
                                    string descriptionKey = keysPrefix + ".description." + itemTitle;

                                    gTitle.stringValue = titleKey;
                                    gDesc.stringValue = descriptionKey;

                                    localizationAsset.AddSectionKey(titleKey, title.stringValue);
                                    localizationAsset.AddSectionKey(descriptionKey, description.stringValue);
                                }
                            }

                            serializedObject.ApplyModifiedProperties();
                            Debug.Log("Localization keys have been successfully exported!");
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndArea();
        }
    }
}