using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(InputSpritesAsset))]
    public class InputSpritesAssetEditor : Editor
    {
        InputSpritesAsset asset;
        SerializedProperty spriteAsset;
        SerializedProperty glyphMap;

        int currPage = 0;
        bool unassignedFoldout = false;

        private void OnEnable()
        {
            asset = target as InputSpritesAsset;
            spriteAsset = serializedObject.FindProperty("SpriteAsset");
            glyphMap = serializedObject.FindProperty("GlyphMap");
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Input Sprites"));
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.PropertyField(spriteAsset);
                EditorGUILayout.Space();

                using (new EditorGUI.DisabledGroupScope(spriteAsset.objectReferenceValue == null))
                {
                    if (GUILayout.Button("Refresh Glyph Map", GUILayout.Height(25)))
                    {
                        if (glyphMap.arraySize > 0)
                        {
                            if (!EditorUtility.DisplayDialog("Refresh Glyph Map", $"Are you sure you want to refresh the glyph map?", "Yes", "No"))
                            {
                                return;
                            }
                        }

                        glyphMap.arraySize = asset.SpriteAsset.spriteGlyphTable.Count;
                        serializedObject.ApplyModifiedProperties();

                        for (int i = 0; i < glyphMap.arraySize; i++)
                        {
                            asset.GlyphMap[i].Glyph = asset.SpriteAsset.spriteGlyphTable[i];
                            asset.GlyphMap[i].Scale = Vector2.one;
                        }
                    }
                }

                if (GUILayout.Button("Clear Glyph Map", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("Clear Glyph Maps", $"Are you sure you want to clear the glyph map?", "Yes", "No"))
                    {
                        glyphMap.ClearArray();
                    }
                }

                if (GUILayout.Button("Save Asset", GUILayout.Height(25)))
                {
                    EditorUtility.SetDirty(asset);
                    AssetDatabase.SaveAssetIfDirty(asset);
                }

                EditorGUILayout.Space();

                int itemsPerPage = 10;
                int arraySize = glyphMap.arraySize;
                int totalPages = (int)(arraySize / (float)itemsPerPage + 0.999f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Glyph Map")))
                {
                    if(totalPages > 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            int index = (currPage * itemsPerPage) + i;
                            if (index >= arraySize)
                                break;

                            SerializedProperty glyph = glyphMap.GetArrayElementAtIndex(index);
                            DrawGlyphElement(glyph, index);
                        }

                        Rect pagePos = EditorGUILayout.GetControlRect(false, 20);
                        pagePos.width /= 3;

                        if (GUI.Button(pagePos, "Previous Page"))
                        {
                            currPage = currPage > 0 ? currPage - 1 : 0;
                        }

                        GUIStyle centeredLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
                        pagePos.x += pagePos.width;
                        GUI.Label(pagePos, "Page " + (currPage + 1) + " / " + totalPages, centeredLabel);

                        pagePos.x += pagePos.width;
                        if (GUI.Button(pagePos, "Next Page"))
                        {
                            currPage = currPage < (totalPages - 1) ? currPage + 1 : (totalPages - 1);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Glyph Map is empty!", EditorDrawing.CenterStyle(EditorStyles.label));
                    }
                }

                EditorGUILayout.Space();
                if(totalPages > 0 && EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Unassigned Keys"), ref unassignedFoldout))
                {
                    string[] controlKeys = InputSpritesAsset.AllKeys.Except(from map in asset.GlyphMap
                                                                               from key in map.MappedKeys
                                                                               select key).ToArray();

                    EditorGUILayout.HelpBox(string.Join(", ", controlKeys
                        .Select(x => InputControlPath.ToHumanReadableString(x, InputControlPath.HumanReadableStringOptions.OmitDevice))
                        .Select(x => char.ToUpper(x[0]) + x[1..])), MessageType.None);
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGlyphElement(SerializedProperty glyphProperty, int index)
        {
            InputSpritesAsset.GlyphKeysPair glyphKeyPair = asset.GlyphMap[index];

            SerializedProperty glyph = glyphProperty.FindPropertyRelative("Glyph");
            SerializedProperty keys = glyphProperty.FindPropertyRelative("MappedKeys");
            SerializedProperty scale = glyphProperty.FindPropertyRelative("Scale");
            SerializedProperty sprite = glyph.FindPropertyRelative("sprite");
            Texture2D texture = AssetPreview.GetAssetPreview(sprite.objectReferenceValue);
            int glyphIndex = glyph.FindPropertyRelative("m_Index").intValue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                Rect glyphRect = GUILayoutUtility.GetRect(50, 50);
                GUI.Label(glyphRect, texture);

                Rect popupRect = glyphRect;
                popupRect.height = EditorGUIUtility.singleLineHeight;
                popupRect.xMin += 54f;
                popupRect.y += 8f;

                GUIContent popupTitle = new GUIContent("None");
                if(glyphKeyPair.MappedKeys.Length > 0)
                {
                    string[] selectedTitles = glyphKeyPair.MappedKeys.Select(x =>
                    {
                        string displayName = InputControlPath.ToHumanReadableString(x, InputControlPath.HumanReadableStringOptions.OmitDevice);
                        return char.ToUpper(displayName[0]) + displayName[1..];
                    }).ToArray();

                    popupTitle.text = string.Join(", ", selectedTitles.Take(5));
                    if (selectedTitles.Length > 5)
                        popupTitle.text += "...";
                }

                GlyphKeyPicker glyphKeyPicker = new(new AdvancedDropdownState());
                glyphKeyPicker.Selected = glyphKeyPair.MappedKeys;
                glyphKeyPicker.OnItemPressed += path =>
                {
                    if (path == "none") keys.ClearArray();
                    else
                    {
                        string[] selected = glyphKeyPair.MappedKeys;
                        asset.GlyphMap[index].MappedKeys = selected.Contains(path)
                            ? selected.Except(new string[] { path }).ToArray()
                            : selected.Concat(new string[] { path }).ToArray();
                    }

                    serializedObject.ApplyModifiedProperties();
                };

                if (GUI.Button(popupRect, popupTitle, EditorStyles.popup))
                {
                    Rect glyphPickerRect = popupRect;
                    glyphPickerRect.width = 250;
                    glyphKeyPicker.Show(glyphPickerRect, 370);
                }

                Rect glyphScaleRect = popupRect;
                glyphScaleRect.y += EditorGUIUtility.singleLineHeight + 2f;
                glyphScaleRect.xMax = glyphScaleRect.xMax / 2f + 100f;
                EditorGUI.PropertyField(glyphScaleRect, scale, GUIContent.none);

                GUIContent indexLabel = new("ID: " + glyphIndex);
                float labelWidth = EditorStyles.boldLabel.CalcSize(indexLabel).x;

                Rect glyphIndexRect = popupRect;
                glyphIndexRect.y += EditorGUIUtility.singleLineHeight + 2f;
                glyphIndexRect.xMin = glyphIndexRect.xMax - labelWidth - 2f;
                EditorGUI.LabelField(glyphIndexRect, indexLabel, EditorStyles.boldLabel);
            }
            EditorGUILayout.EndVertical();
        }

        private class GlyphKeyPicker : AdvancedDropdown
        {
            private class GlyphKeyElement : AdvancedDropdownItem
            {
                public string controlPath;
                public string displayName;

                public GlyphKeyElement(string controlPath, string displayName) : base(displayName)
                {
                    this.controlPath = controlPath;
                    this.displayName = displayName;
                }
            }

            public string[] Selected;
            public event Action<string> OnItemPressed;

            public GlyphKeyPicker(AdvancedDropdownState state) : base(state)
            {

                minimumSize = new Vector2(200f, 250f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Input Controls");
                root.AddChild(new GlyphKeyElement("none", "None [Deselect All]"));

                var invalidElement = new GlyphKeyElement("invalid", "Invalid [Null Key]");
                if(Selected.Contains("invalid")) 
                    invalidElement.icon = (Texture2D)EditorGUIUtility.TrIconContent("FilterSelectedOnly").image;
                root.AddChild(invalidElement);

                foreach (var path in InputSpritesAsset.AllKeys)
                {
                    string displayName = InputControlPath.ToHumanReadableString(path);
                    displayName = char.ToUpper(displayName[0]) + displayName[1..];
                    var dropdownItem = new GlyphKeyElement(path, displayName);

                    if(Selected.Contains(path))
                        dropdownItem.icon = (Texture2D)EditorGUIUtility.TrIconContent("FilterSelectedOnly").image;

                    root.AddChild(dropdownItem);
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                OnItemPressed?.Invoke((item as GlyphKeyElement).controlPath);
            }
        }
    }
}