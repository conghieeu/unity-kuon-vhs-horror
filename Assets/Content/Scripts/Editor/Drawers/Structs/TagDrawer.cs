using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(Tag))]
    public class TagDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty tagProp = property.FindPropertyRelative("tag");
            EditorGUI.BeginProperty(position, label, property);
            {
                if (string.IsNullOrEmpty(tagProp.stringValue)) tagProp.stringValue = "Untagged";
                tagProp.stringValue = EditorGUI.TagField(position, label, tagProp.stringValue);
            }
            EditorGUI.EndProperty();
        }
    }
}