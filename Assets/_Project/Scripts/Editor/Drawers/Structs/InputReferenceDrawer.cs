using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UHFPS.Runtime;
using UHFPS.Input;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(InputReference))]
    public class InputReferenceDrawer : PropertyDrawer
    {
        public static Texture2D InputActionIcon => Resources.Load<Texture2D>("EditorIcons/InputAction");

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty actionNameProp = property.FindPropertyRelative("ActionName");
            SerializedProperty bindingIndexProp = property.FindPropertyRelative("BindingIndex");

            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, label);

                Rect dropdownRect = position;
                dropdownRect.width = 250f;
                dropdownRect.height = 0f;
                dropdownRect.y += 21f;
                dropdownRect.x += position.xMax - dropdownRect.width - EditorGUIUtility.singleLineHeight;

                InputPicker inputPicker = null;
                if (InputManager.HasReference)
                {
                    inputPicker = new(new AdvancedDropdownState(), InputManager.Instance.inputActions);
                    inputPicker.OnItemPressed = (name, index) =>
                    {
                        if (string.IsNullOrEmpty(name))
                        {
                            actionNameProp.stringValue = string.Empty;
                            bindingIndexProp.intValue = -1;
                        }
                        else
                        {
                            actionNameProp.stringValue = name;
                            bindingIndexProp.intValue = index;
                        }

                        property.serializedObject.ApplyModifiedProperties();
                    };
                }

                GUIContent fieldText = new GUIContent("None (InputReference)");
                if (!string.IsNullOrEmpty(actionNameProp.stringValue))
                {
                    fieldText.text = actionNameProp.stringValue + $" [{bindingIndexProp.intValue}]";
                    fieldText.image = InputActionIcon;
                }

                using (new EditorGUI.DisabledGroupScope(!InputManager.HasReference || inputPicker == null))
                {
                    if (EditorDrawing.ObjectField(position, fieldText))
                    {
                        inputPicker.Show(dropdownRect, 370);
                    }
                }
            }
            EditorGUI.EndProperty();
        }

        private class InputPicker : AdvancedDropdown
        {
            private class InputElement : AdvancedDropdownItem
            {
                public string actionName;
                public int bindingIndex;
                public bool isNone;

                public InputElement(string displayName, string actionName, int bindingIndex) : base(displayName)
                {
                    this.actionName = actionName;
                    this.bindingIndex = bindingIndex;
                }

                public InputElement(string displayName) : base(displayName) { }

                public InputElement() : base("Empty")
                {
                    isNone = true;
                }
            }

            public string SelectedKey;
            public Action<string, int> OnItemPressed;

            private readonly InputActionAsset inputAsset;

            public InputPicker(AdvancedDropdownState state, InputActionAsset inputAsset) : base(state)
            {
                this.inputAsset = inputAsset;
                minimumSize = new Vector2(200f, 250f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Input Action Selector");
                root.AddChild(new InputElement()); // none selector

                foreach (var map in inputAsset.actionMaps)
                {
                    InputElement section = new(map.name);

                    foreach (var action in map.actions)
                    {
                        int bindingsCount = action.bindings.Count;
                        for (int bindingIndex = 0; bindingIndex < bindingsCount; bindingIndex++)
                        {
                            if (action.bindings[bindingIndex].isComposite)
                            {
                                int firstPartIndex = bindingIndex + 1;
                                int lastPartIndex = firstPartIndex;
                                while (lastPartIndex < bindingsCount && action.bindings[lastPartIndex].isPartOfComposite)
                                    ++lastPartIndex;

                                int partCount = lastPartIndex - firstPartIndex;
                                for (int i = 0; i < partCount; i++)
                                {
                                    int bindingPartIndex = firstPartIndex + i;
                                    InputBinding binding = action.bindings[bindingPartIndex];
                                    AddBinding(section, binding, bindingPartIndex);
                                }

                                bindingIndex += partCount;
                            }
                            else
                            {
                                InputBinding binding = action.bindings[bindingIndex];
                                AddBinding(section, binding, bindingIndex);
                            }
                        }
                    }

                    root.AddChild(section);
                }

                return root;
            }

            void AddBinding(InputElement section, InputBinding binding, int bindingIndex)
            {
                string partString = string.Empty;
                if (!string.IsNullOrEmpty(binding.name))
                {
                    NameAndParameters nameParameters = NameAndParameters.Parse(binding.name);
                    partString = nameParameters.name;
                }

                string name = binding.action;
                if (!string.IsNullOrEmpty(partString))
                    name += $" ({partString})";

                name += $" [{bindingIndex}]";
                InputElement inputAction = new(name, binding.action, bindingIndex);
                inputAction.icon = InputActionIcon;

                section.AddChild(inputAction);
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                var element = item as InputElement;
                if (!element.isNone) OnItemPressed?.Invoke(element.actionName, element.bindingIndex);
                else OnItemPressed?.Invoke(null, -1);
            }
        }
    }
}