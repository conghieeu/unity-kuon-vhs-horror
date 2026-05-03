using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ManagerModulesAsset))]
    public class ManagerModulesAssetEditor : Editor
    {
        SerializedProperty ManagerModules;
        ManagerModulesAsset Target;
        IEnumerable<Type> Modules;

        private void OnEnable()
        {
            Target = (ManagerModulesAsset)target;
            ManagerModules = serializedObject.FindProperty("ManagerModules");
            Modules = from type in TypeCache.GetTypesDerivedFrom<ManagerModule>()
                      where !type.IsAbstract && !Target.ManagerModules.Where(x => x != null).Any(x => x.GetType() == type)
                      select type;
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Manager Modules"));
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.LabelField("Manager Modules", EditorStyles.boldLabel);
                EditorGUILayout.Space(2f);

                if (ManagerModules.arraySize <= 0)
                {
                    EditorGUILayout.HelpBox("To add new modules to the manager, click the Add Module button and select the module you want to add to the manager.", MessageType.Info);
                }
                else if (Target.ManagerModules.Any(x => x == null))
                {
                    EditorGUILayout.HelpBox("There are elements that have an empty module reference, switch the inspector to debug mode and remove the element that has the missing reference.", MessageType.Warning);
                }

                for (int i = 0; i < ManagerModules.arraySize; i++)
                {
                    SerializedProperty moduleProperty = ManagerModules.GetArrayElementAtIndex(i);
                    PropertyCollection moduleProperties = EditorDrawing.GetAllProperties(moduleProperty);
                    string moduleName = ((ManagerModule)moduleProperty.boxedValue).Name;

                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        Rect headerRect = EditorGUILayout.GetControlRect(true, 22f);
                        Texture2D icon = Resources.Load<Texture2D>("EditorIcons/module");
                        GUIContent header = new($" {moduleName} (Module)", icon);

                        using (new EditorDrawing.IconSizeScope(14f))
                        {
                            if (moduleProperty.isExpanded = DrawFoldoutHeader(headerRect, header, moduleProperty.isExpanded))
                            {
                                if(moduleProperties.Count > 0)
                                    EditorGUILayout.Space(1f);

                                moduleProperties.DrawAll();
                            }
                        }

                        Rect menuRect = headerRect;
                        menuRect.xMin = menuRect.xMax - EditorGUIUtility.singleLineHeight;
                        menuRect.x -= EditorGUIUtility.standardVerticalSpacing;
                        menuRect.y += headerRect.height / 2 - 8f;

                        GUIContent menuIcon = EditorGUIUtility.TrIconContent("_Menu", "Module Menu");
                        int index = i;

                        if (GUI.Button(menuRect, menuIcon, EditorStyles.iconButton))
                        {
                            GenericMenu popup = new();

                            if (index > 0)
                            {
                                popup.AddItem(new GUIContent("Move Up"), false, () =>
                                {
                                    ManagerModules.MoveArrayElement(index, index - 1);
                                    serializedObject.ApplyModifiedProperties();
                                });
                            }
                            else popup.AddDisabledItem(new GUIContent("Move Up"));

                            if (index < ManagerModules.arraySize - 1)
                            {
                                popup.AddItem(new GUIContent("Move Down"), false, () =>
                                {
                                    ManagerModules.MoveArrayElement(index, index + 1);
                                    serializedObject.ApplyModifiedProperties();
                                });
                            }
                            else popup.AddDisabledItem(new GUIContent("Move Down"));

                            popup.AddItem(new GUIContent("Delete"), false, () =>
                            {
                                ManagerModules.DeleteArrayElementAtIndex(index);
                                serializedObject.ApplyModifiedProperties();
                                serializedObject.Update();
                            });

                            popup.ShowAsContext();
                        }
                    }
                }

                EditorGUILayout.Space();
                using (new EditorGUI.DisabledGroupScope(Modules.Count() == 0))
                {
                    if (GUILayout.Button("Add Module", GUILayout.Height(25f)))
                    {
                        GenericMenu popup = new();
                        foreach (var module in Modules)
                        {
                            popup.AddItem(new GUIContent(module.Name), false, AddModule, module);
                        }
                        popup.ShowAsContext();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void AddModule(object type)
        {
            ManagerModule module = (ManagerModule)Activator.CreateInstance((Type)type);
            Target.ManagerModules.Add(module);

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
        }

        private bool DrawFoldoutHeader(Rect headerRect, GUIContent title, bool expanded)
        {
            // Constants
            float singleLineHeight = EditorGUIUtility.singleLineHeight;

            // Define and draw foldout toggle
            Rect foldoutRect = new(headerRect.x + 4f, headerRect.y, singleLineHeight, headerRect.height);
            GUI.Toggle(foldoutRect, expanded, GUIContent.none, EditorStyles.foldout);

            // Define and draw title label
            Rect labelRect = new(foldoutRect.xMax, headerRect.y - 1f, headerRect.width - foldoutRect.xMax + 4f, headerRect.height);
            EditorGUI.LabelField(labelRect, title, EditorDrawing.Styles.miniBoldLabelCenter);

            // Handle mouse events for foldout interaction
            headerRect.xMax -= singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            Event e = Event.current;

            if (headerRect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
            {
                expanded = !expanded;
                e.Use();
            }

            return expanded;
        }
    }
}