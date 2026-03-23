using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEditor;
using UnityEngine;
using UHFPS.Rendering;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(UHFPSScreenEffects))]
    public class UHFPSScreenEffectsEditor : InspectorEditor<UHFPSScreenEffects>
    {
        public struct EffectPair
        {
            public Type effectType;
            public string effectName;
        }

        private List<EffectPair> effects;

        public override void OnEnable()
        {
            base.OnEnable();
            RefreshEffects();
        }

        private void RefreshEffects()
        {
            effects = new();
            foreach (var type in TypeCache.GetTypesDerivedFrom<EffectFeature>().Where(x => !x.IsAbstract))
            {
                if (Target.Features.Any(x => x.GetType() == type))
                    continue;

                EffectFeature instance = Activator.CreateInstance(type) as EffectFeature;
                effects.Add(new EffectPair()
                {
                    effectType = type,
                    effectName = instance.Name
                });
                instance = null;
            }
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorDrawing.DrawInspectorHeader(new GUIContent("UHFPS Screen Effects"));
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                int arraySize = Properties["Features"].arraySize;
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Rect labelRect = EditorGUILayout.GetControlRect();
                    EditorGUI.LabelField(labelRect, new GUIContent("Screen Effects"), EditorStyles.boldLabel);

                    Rect countLabelRect = labelRect;
                    countLabelRect.xMin = countLabelRect.xMax - 25f;

                    GUI.enabled = false;
                    EditorGUI.IntField(countLabelRect, arraySize);
                    GUI.enabled = true;

                    EditorGUILayout.Space(2f);

                    for (int i = 0; i < arraySize; i++)
                    {
                        SerializedProperty feature = Properties["Features"].GetArrayElementAtIndex(i);
                        SerializedProperty enabled = feature.FindPropertyRelative("Enabled");

                        string name = Target.Features[i].Name;
                        GUIContent title = EditorDrawing.IconTextContent($"{name} (Screen Effect)", "PreTextureArrayFirstSlice", 14f);

                        if (EditorDrawing.BeginFoldoutToggleBorderLayout(title, enabled, out Rect headerRect))
                        {
                            PropertyCollection effectProperties = EditorDrawing.GetAllProperties(feature);
                            effectProperties.DrawAll(true, 1);

                            EditorDrawing.EndBorderHeaderLayout();
                        }

                        EditorDrawing.ResetIconSize();

                        Rect moduleRemoveButton = headerRect;
                        moduleRemoveButton.xMin = moduleRemoveButton.xMax - EditorGUIUtility.singleLineHeight - 2f;
                        moduleRemoveButton.y += 3f;

                        using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                        {
                            int index = i;
                            GUIContent menuIcon = EditorDrawing.IconContent("_Menu");

                            if (GUI.Button(moduleRemoveButton, menuIcon, EditorStyles.iconButton))
                            {
                                GenericMenu popup = new GenericMenu();
                                {
                                    if (index > 0)
                                    {
                                        popup.AddItem(new GUIContent("Move Up"), false, () =>
                                        {
                                            Properties["Features"].MoveArrayElement(index, index - 1);
                                            serializedObject.ApplyModifiedProperties();
                                        });
                                    }
                                    else popup.AddDisabledItem(new GUIContent("Move Up"));

                                    if (index < Properties["Features"].arraySize - 1)
                                    {
                                        popup.AddItem(new GUIContent("Move Down"), false, () =>
                                        {
                                            Properties["Features"].MoveArrayElement(index, index + 1);
                                            serializedObject.ApplyModifiedProperties();
                                        });
                                    }
                                    else popup.AddDisabledItem(new GUIContent("Move Down"));

                                    popup.AddItem(new GUIContent("Delete"), false, () =>
                                    {
                                        Properties["Features"].DeleteArrayElementAtIndex(index);
                                        serializedObject.ApplyModifiedProperties();
                                        serializedObject.Update();
                                        RefreshEffects();
                                    });
                                }
                                popup.ShowAsContext();
                            }
                            EditorDrawing.ResetIconSize();
                        }

                        if (i + 1 < arraySize) EditorGUILayout.Space(1f);
                    }

                    EditorGUILayout.Space(2f);
                    EditorDrawing.Separator();
                    EditorGUILayout.Space(2f);

                    EditorGUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        Rect moduleButtonRect = EditorGUILayout.GetControlRect(GUILayout.Width(100f));

                        using (new EditorGUI.DisabledGroupScope(Application.isPlaying || effects.Count <= 0))
                        {
                            if (GUI.Button(moduleButtonRect, "Add Effect"))
                            {
                                Rect dropdownRect = moduleButtonRect;
                                dropdownRect.width = 250f;
                                dropdownRect.height = 0f;
                                dropdownRect.y += 21f;
                                dropdownRect.x += (moduleButtonRect.width - dropdownRect.width) / 2;

                                EffectsList modulesDropdown = new(new(), effects);
                                modulesDropdown.OnItemPressed = (type) => AddFeature(type);
                                modulesDropdown.Show(dropdownRect);
                            }
                        }
                        GUILayout.FlexibleSpace();
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void AddFeature(Type feature)
        {
            EffectFeature effect = (EffectFeature)Activator.CreateInstance(feature);
            Target.Features.Add(effect);
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            RefreshEffects();
        }

        public class EffectsList : AdvancedDropdown
        {
            private readonly IEnumerable<EffectPair> effects;
            public Action<Type> OnItemPressed;

            private class EffectElement : AdvancedDropdownItem
            {
                public Type moduleType;

                public EffectElement(string displayName, Type moduleType) : base(displayName)
                {
                    this.moduleType = moduleType;
                }
            }

            public EffectsList(AdvancedDropdownState state, IEnumerable<EffectPair> effects) : base(state)
            {
                this.effects = effects;
                minimumSize = new Vector2(minimumSize.x, 270f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Screen Effects");
                var groupMap = new Dictionary<string, AdvancedDropdownItem>();

                foreach (var effect in effects)
                {
                    Type type = effect.effectType;
                    string name = effect.effectName;

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
                    EffectElement item = new EffectElement(groups.Last(), type);

                    item.icon = (Texture2D)EditorGUIUtility.TrIconContent("PreTextureArrayFirstSlice").image;
                    parent.AddChild(item);
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                EffectElement element = (EffectElement)item;
                OnItemPressed?.Invoke(element.moduleType);
            }
        }
    }
}