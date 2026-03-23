using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;
using System.Linq;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ItemsStorage))]
    public class ItemsStorageEditor : InspectorEditor<ItemsStorage>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Items Storage"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Storage Settings")))
                {
                    Properties.Draw("ContainerTitle");
                    Properties.Draw("Rows");
                    Properties.Draw("Columns");
                }

                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                Properties.Draw("AutoCoords");
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                if (Properties.DrawGetBool("TimedOpen"))
                {
                    Properties.Draw("KeepSearched");
                    Properties.DrawBacking("InteractTime");
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                int arraySize = EditorDrawing.BeginDrawCustomList(Properties["StoredItems"], new GUIContent("Stored Items"));
                {
                    for (int i = 0; i < arraySize; i++)
                    {
                        SerializedProperty item = Properties["StoredItems"].GetArrayElementAtIndex(i);
                        SerializedProperty itemGuid = item.FindPropertyRelative("Item");

                        float minusWidth = EditorGUIUtility.labelWidth;
                        if (EditorDrawing.BeginFoldoutBorderLayout(item, GUIContent.none, out Rect itemRect, minusWidth, headerHeight: 20f, roundedBox: false))
                        {
                            SerializedProperty quantity = item.FindPropertyRelative("Quantity");
                            SerializedProperty coords = item.FindPropertyRelative("Coords");
                            SerializedProperty itemData = item.FindPropertyRelative("ItemData");

                            EditorGUILayout.PropertyField(quantity);
                            if(!Properties.BoolValue("AutoCoords"))
                                EditorGUILayout.PropertyField(coords);
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(itemData);
                            EditorGUI.indentLevel--;

                            EditorDrawing.EndBorderHeaderLayout();
                        }

                        Rect itemGuiRect = itemRect;
                        itemGuiRect.height = EditorGUIUtility.singleLineHeight;
                        itemGuiRect.xMin += EditorGUIUtility.singleLineHeight;
                        itemGuiRect.xMax -= EditorGUIUtility.singleLineHeight + 4f;
                        itemGuiRect.y += 3f;

                        EditorGUI.PropertyField(itemGuiRect, itemGuid, new GUIContent($"[{i}] Item"));

                        Rect minusIcon = itemRect;
                        minusIcon.xMin = minusIcon.xMax - EditorGUIUtility.singleLineHeight - 2f;
                        minusIcon.y += 4f;

                        if(GUI.Button(minusIcon, EditorUtils.Styles.MinusIcon, EditorStyles.iconButton))
                        {
                            Properties["StoredItems"].DeleteArrayElementAtIndex(i);
                            break;
                        }
                    }
                }
                EditorDrawing.EndDrawCustomList(new GUIContent("Add Item"), true, () =>
                {
                    Target.StoredItems.Add(new() 
                    { 
                        Item = new(),
                        Quantity = 1
                    });

                    serializedObject.ApplyModifiedProperties();
                });

                EditorGUILayout.Space();

                if(!Target.AutoCoords && Target.StoredItems.GroupBy(item => item.Coords)
                    .Where(group => group.Count() > 1)
                    .Select(group => group.Key).Any())
                {
                    EditorGUILayout.HelpBox("There are items in the storage that have the same coordinates, which may cause overlap!", MessageType.Error);
                    EditorGUILayout.Space();
                }

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sound Settings")))
                {
                    Properties.Draw("AudioSource");
                    EditorGUILayout.Space(1f);
                    Properties.Draw("SearchingSound");
                    Properties.Draw("OpenStorageSound");
                    Properties.Draw("CloseStorageSound");
                }

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnStartSearch"], new GUIContent("Events")))
                {
                    Properties.Draw("OnStartSearch");
                    Properties.Draw("OnOpenStorage");
                    Properties.Draw("OnCloseStorage");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}