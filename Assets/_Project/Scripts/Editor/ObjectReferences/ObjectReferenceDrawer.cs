using System;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;
using UnityEditor.IMGUI.Controls;
using UHFPS.Scriptable;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(ObjectReference))]
    public class ObjectReferenceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ObjectReferences objectReferences = null;
            SerializedProperty GUID = property.FindPropertyRelative("GUID");
            SerializedProperty Object = property.FindPropertyRelative("Object");

            string guid = GUID.stringValue;
            GameObject obj = Object.objectReferenceValue as GameObject;

            bool hasSaveManagerReference = SaveGameManager.HasReference;
            GUIContent fieldText = new GUIContent("None (ObjectReference)");

            if (hasSaveManagerReference)
            {
                objectReferences = SaveGameManager.Instance.ObjectReferences;
                if (!string.IsNullOrEmpty(guid) && !objectReferences.HasReference(guid))
                {
                    fieldText.text = " Invalid GUID Reference";
                    fieldText.image = EditorGUIUtility.TrIconContent("console.erroricon.sml").image;
                }
                else if(obj != null)
                {
                    string title = $" {obj.name} ({guid})";
                    fieldText = EditorGUIUtility.TrTextContentWithIcon(title, "Prefab Icon");
                }
            }

            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, label);
                position.xMax -= EditorGUIUtility.singleLineHeight + 2f;

                ObjectReferencePicker objectReferencePicker = new(new AdvancedDropdownState(), objectReferences);
                objectReferencePicker.OnItemPressed += (reference) =>
                {
                    if (reference.HasValue)
                    {
                        Object.objectReferenceValue = reference.Value.Object;
                        GUID.stringValue = reference.Value.GUID;
                    } 
                    else
                    {
                        Object.objectReferenceValue = null;
                        GUID.stringValue = string.Empty;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                };

                if (EditorDrawing.ObjectField(position, fieldText))
                {
                    Rect pickerRect = position;
                    objectReferencePicker.Show(pickerRect, 370);
                }

                if (obj != null)
                {
                    Event e = Event.current;
                    Rect pingRect = position;
                    pingRect.xMax -= 19;

                    if (pingRect.Contains(e.mousePosition) && e.type == EventType.MouseDown)
                    {
                        EditorGUIUtility.PingObject(obj);
                    }
                }

                Rect scriptablePingRect = position;
                scriptablePingRect.xMin = position.xMax + 2f;
                scriptablePingRect.width = EditorGUIUtility.singleLineHeight;
                scriptablePingRect.y += 1f;

                GUIContent scriptableIcon = EditorGUIUtility.TrIconContent("ScriptableObject Icon");
                if(objectReferences == null)
                {
                    scriptableIcon.tooltip = "The ObjectReferences asset is not assigned in SaveGameManager!";
                }

                using (new EditorGUI.DisabledGroupScope(objectReferences == null))
                {
                    if (GUI.Button(scriptablePingRect, scriptableIcon, EditorStyles.iconButton))
                    {
                        EditorGUIUtility.PingObject(objectReferences);
                    }
                }
            }
            EditorGUI.EndProperty();
        }

        private class ObjectReferencePicker : AdvancedDropdown
        {
            private class ObjectReferenceElement : AdvancedDropdownItem
            {
                public ObjectReferences.ObjectGuidPair reference;
                public bool isNone;

                public ObjectReferenceElement(ObjectReferences.ObjectGuidPair reference, string displayName) : base(displayName)
                {
                    this.reference = reference;
                }

                public ObjectReferenceElement() : base("None")
                {
                    isNone = true;
                }
            }

            public string SelectedObjective;
            public string[] SelectedSubObjectives;
            public event Action<ObjectReferences.ObjectGuidPair?> OnItemPressed;

            private readonly ObjectReferences objectReferencesAsset;

            public ObjectReferencePicker(AdvancedDropdownState state, ObjectReferences objectReferencesAsset) : base(state)
            {
                this.objectReferencesAsset = objectReferencesAsset;
                minimumSize = new Vector2(200f, 250f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Object References");
                root.AddChild(new ObjectReferenceElement()); // none selector

                if (objectReferencesAsset != null)
                {
                    foreach (var reference in objectReferencesAsset.References)
                    {
                        var referenceElement = new ObjectReferenceElement(reference, " " + reference.Object.name);
                        referenceElement.icon = (Texture2D)EditorGUIUtility.IconContent("Prefab Icon").image;
                        root.AddChild(referenceElement);
                    }
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                var element = item as ObjectReferenceElement;
                if (element.isNone) OnItemPressed?.Invoke(null);
                else OnItemPressed?.Invoke(element.reference);
            }
        }
    }
}