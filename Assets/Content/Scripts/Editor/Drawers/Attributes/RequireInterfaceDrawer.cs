using UnityEditor;
using UnityEngine;
using ThunderWire.Attributes;

namespace ThunderWire.Editors
{
    [CustomPropertyDrawer(typeof(RequireInterfaceAttribute))]
    public class RequireInterfaceDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            RequireInterfaceAttribute att = attribute as RequireInterfaceAttribute;

            if (property.propertyType != SerializedPropertyType.ObjectReference)
            {
                EditorGUI.LabelField(position, label.text, "InterfaceType Attribute can only be used with MonoBehaviour Components!");
                return;
            }

            label.text += $" ({att.InterfaceType.Name})";
            position.height = EditorGUIUtility.singleLineHeight;

            MonoBehaviour component = EditorGUI.ObjectField(position, label, property.objectReferenceValue, typeof(MonoBehaviour), true) as MonoBehaviour;

            if (component != null && !att.InterfaceType.IsAssignableFrom(component.GetType()))
            {
                component = component.gameObject.GetComponent(att.InterfaceType) as MonoBehaviour;
                if (component == null)
                {
                    Debug.LogError($"Cannot assign MonoBehaviour because it does not contain the '{att.InterfaceType.Name}' interface!");
                    return;
                }
            }
                
            property.objectReferenceValue = component;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}