using System;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Scriptable;
using ThunderWire.Editors;
using UnityEditor.IMGUI.Controls;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(SingleObjectiveSelect))]
    public class SingleObjectiveSelectDrawer : PropertyDrawer
    {
        SingleObjectiveSelect GetTarget(SerializedProperty property)
        {
            UnityEngine.Object target = property.serializedObject.targetObject;
            return fieldInfo.GetValue(target) as SingleObjectiveSelect;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            {
                SingleObjectiveSelect objectiveSelect = GetTarget(property);
                ObjectivesAsset objectivesAsset = null;

                bool hasObjReference = ObjectiveManager.HasReference;
                if (hasObjReference) objectivesAsset = ObjectiveManager.Instance.ObjectivesAsset;

                Rect dropdownRect = position;
                dropdownRect.width = 250f;
                dropdownRect.height = 0f;
                dropdownRect.y += 21f;
                dropdownRect.x += position.xMax - dropdownRect.width - EditorGUIUtility.singleLineHeight;

                Rect headerRect = EditorDrawing.DrawHeaderWithBorder(ref position, label);
                headerRect.width = EditorGUIUtility.singleLineHeight;
                headerRect.height = EditorGUIUtility.singleLineHeight;
                headerRect.x = position.width;
                headerRect.y += 2f;

                ObjectivePicker objectivePicker = new(new AdvancedDropdownState(), objectivesAsset);
                objectivePicker.SelectedObjective = objectiveSelect.ObjectiveKey;
                objectivePicker.SelectedSubObjective = objectiveSelect.SubObjectiveKey;
                objectivePicker.OnItemPressed += (obj, sub) =>
                {
                    if (obj != null)
                    {
                        objectiveSelect.ObjectiveKey = obj;
                        objectiveSelect.SubObjectiveKey = sub;
                    }
                    else
                    {
                        objectiveSelect.ObjectiveKey = string.Empty;
                        objectiveSelect.SubObjectiveKey = string.Empty;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                };

                using (new EditorDrawing.IconSizeScope(16))
                {
                    using (new EditorGUI.DisabledGroupScope(!hasObjReference))
                    {
                        if (GUI.Button(headerRect, EditorUtils.Styles.Linked, EditorStyles.iconButton))
                        {
                            Rect pickerRect = dropdownRect;
                            objectivePicker.Show(pickerRect, 370);
                        }
                    }
                }

                using (new EditorGUI.DisabledGroupScope(true))
                {
                    Rect objectiveRect = position;
                    objectiveRect.y += 2f;
                    objectiveRect.xMin += 2f;
                    objectiveRect.xMax -= 2f;
                    objectiveRect.height = EditorGUIUtility.singleLineHeight;
                    EditorGUI.TextField(objectiveRect, "Objective", objectiveSelect.ObjectiveKey);

                    Rect subObjectiveRect = objectiveRect;
                    subObjectiveRect.y += EditorGUIUtility.singleLineHeight + 2f;
                    EditorGUI.TextField(subObjectiveRect, "SubObjective", objectiveSelect.SubObjectiveKey);
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 11f;
        }

        private class ObjectivePicker : AdvancedDropdown
        {
            private class ObjectiveElement : AdvancedDropdownItem
            {
                public string objectiveKey;
                public string subObjectiveKey;
                public bool isNone;

                public ObjectiveElement(string displayName) : base(displayName)
                {
                    objectiveKey = displayName;
                }

                public ObjectiveElement(string objective, string displayName) : base(displayName)
                {
                    objectiveKey = objective;
                    subObjectiveKey = displayName;
                }

                public ObjectiveElement() : base("None [Deselect All]")
                {
                    isNone = true;
                }
            }

            public string SelectedObjective;
            public string SelectedSubObjective;
            public event Action<string, string> OnItemPressed;

            private readonly ObjectivesAsset objectivesAsset;

            public ObjectivePicker(AdvancedDropdownState state, ObjectivesAsset objectivesAsset) : base(state)
            {
                this.objectivesAsset = objectivesAsset;
                minimumSize = new Vector2(200f, 250f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Objective Selector");
                root.AddChild(new ObjectiveElement()); // none selector

                foreach (var objective in objectivesAsset.Objectives)
                {
                    var objectiveElement = new ObjectiveElement(objective.ObjectiveKey);
                    if (SelectedObjective == objective.ObjectiveKey && !string.IsNullOrEmpty(SelectedSubObjective))
                    {
                        objectiveElement.icon = (Texture2D)EditorGUIUtility.TrIconContent("FilterSelectedOnly").image;
                    }

                    foreach (var subObjective in objective.SubObjectives)
                    {
                        var subObjectiveElement = new ObjectiveElement(objective.ObjectiveKey, subObjective.SubObjectiveKey);
                        if (SelectedSubObjective == subObjective.SubObjectiveKey)
                        {
                            subObjectiveElement.icon = (Texture2D)EditorGUIUtility.TrIconContent("FilterSelectedOnly").image;
                        }
                        objectiveElement.AddChild(subObjectiveElement);
                    }

                    root.AddChild(objectiveElement);
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                var element = item as ObjectiveElement;
                if (!element.isNone) OnItemPressed?.Invoke(element.objectiveKey, element.subObjectiveKey);
                else OnItemPressed?.Invoke(null, null);
            }
        }
    }
}