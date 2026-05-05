using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UHFPS.Input;
using UHFPS.Tools;
using ThunderWire.Editors;
using Binding = UHFPS.Input.InputManager.ActionMap.Action.Binding;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(InputManager))]
    public class InputManagerEditor : InspectorEditor<InputManager>
    {
        private readonly List<bool> mapFoldouts = new();
        private readonly List<bool> actionFoldouts = new();
        private readonly List<bool> rebindFoldouts = new();
        private readonly Dictionary<string, bool> actionBindings = new();

        private readonly bool[] foldouts = new bool[3];
        private string actionName;
        private int bindingIndex;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorDrawing.DrawInspectorHeader(new GUIContent("Input Manager"), Target);
            EditorGUILayout.Space();

            Properties.Draw("inputActions");
            Properties.Draw("inputSpritesAsset");
            Properties.Draw("debugMode");

            EditorGUILayout.Space();
            if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Functions Debug (Runtime Only)"), ref foldouts[0]))
            {
                using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
                {
                    actionName = EditorGUILayout.TextField("Action Name", actionName);
                    bindingIndex = EditorGUILayout.IntField("Binding Index", bindingIndex);
                    if (GUILayout.Button("Start Rebind Operation"))
                    {
                        InputManager.StartRebindOperation(actionName, bindingIndex);
                    }
                    if(GUILayout.Button("Apply Prepared Rebinds"))
                    {
                        InputManager.SetInputRebindOverrides();
                    }
                }

                EditorDrawing.EndBorderHeaderLayout();
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(1);
                if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Input Action Map"), ref foldouts[1]))
                {
                    if (mapFoldouts.Count < Target.actionMap.Count)
                        mapFoldouts.AddRange(new bool[Target.actionMap.Count]);

                    int mapIndex = 0;
                    foreach (var map in Target.actionMap)
                    {
                        if (mapFoldouts[mapIndex] = EditorDrawing.BeginFoldoutBorderLayout(new GUIContent(map.Key), mapFoldouts[mapIndex++]))
                        {
                            if (actionFoldouts.Count < map.Value.actions.Count)
                                actionFoldouts.AddRange(new bool[map.Value.actions.Count]);

                            int actionIndex = 0;
                            foreach (var action in map.Value.actions)
                            {
                                if (actionFoldouts[actionIndex] = EditorDrawing.BeginFoldoutBorderLayout(new GUIContent(action.Key), actionFoldouts[actionIndex++]))
                                {
                                    foreach (var binding in action.Value.bindings)
                                    {
                                        string bindingKey = action.Key + "_" + binding.Value.bindingIndex;
                                        if (!actionBindings.ContainsKey(bindingKey))
                                            actionBindings.Add(bindingKey, false);

                                        string name = !string.IsNullOrEmpty(binding.Value.name)
                                            ? $"Binding Index [{binding.Value.bindingIndex}] [{binding.Value.name.ToTitleCase()}]"
                                            : $"Binding Index [{binding.Value.bindingIndex}]";

                                        if (actionBindings[bindingKey] = EditorDrawing.BeginFoldoutBorderLayout(new GUIContent(name), actionBindings[bindingKey]))
                                        {
                                            DrawBinding(binding.Value);
                                            EditorDrawing.EndBorderHeaderLayout();
                                        }
                                    }
                                    EditorDrawing.EndBorderHeaderLayout();
                                }
                            }
                            EditorDrawing.EndBorderHeaderLayout();
                        }
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1);
                if (Target.preparedRebinds.Count > 0)
                {
                    if (rebindFoldouts.Count != Target.preparedRebinds.Count)
                    {
                        rebindFoldouts.Clear();
                        rebindFoldouts.AddRange(new bool[Target.preparedRebinds.Count]);
                    }

                    if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Prepared Rebinds"), ref foldouts[2]))
                    {
                        int rebindIndex = 0;
                        foreach (var rebind in Target.preparedRebinds)
                        {
                            InputBinding inputBinding = rebind.action.bindings[rebind.bindingIndex];
                            string actionName = rebind.action.name;
                            string bindingName = inputBinding.name;

                            if (!string.IsNullOrEmpty(bindingName))
                                actionName += "." + bindingName;

                            if (rebindFoldouts[rebindIndex] = EditorDrawing.BeginFoldoutBorderLayout(new GUIContent(actionName), rebindFoldouts[rebindIndex++]))
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.TextField("Binding Index", rebind.bindingIndex.ToString());
                                    EditorGUILayout.TextField("Override Path", rebind.overridePath);
                                }
                                EditorDrawing.EndBorderHeaderLayout();
                            }
                        }
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }
                else
                {
                    rebindFoldouts.Clear();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBinding(Binding binding)
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.TextField("Binding Path", binding.bindingPath.bindingPath);
                EditorGUILayout.TextField("Override Path", binding.bindingPath.overridePath);
                EditorGUILayout.TextField("Effective Path", binding.bindingPath.EffectivePath);
                EditorGUILayout.TextField("Glyph Path", binding.bindingPath.inputGlyph.GlyphPath);
            }
        }
    }
}