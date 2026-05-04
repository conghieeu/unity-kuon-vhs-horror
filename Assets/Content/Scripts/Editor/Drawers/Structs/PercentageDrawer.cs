using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(Percentage))]
    public class PercentageDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty value = property.FindPropertyRelative("Value");
            GUIContent percentLabel = new GUIContent("%");

            EditorGUI.BeginProperty(position, label, property);
            {
                float labelWidth = EditorStyles.boldLabel.CalcSize(percentLabel).x;
                position.xMax -= labelWidth + 2f;

                EditorGUI.PropertyField(position, value, label);

                position.xMin = position.xMax + 2f;
                position.xMax += labelWidth + 2f;
                EditorGUI.LabelField(position, percentLabel, EditorStyles.boldLabel);
            }

            value.intValue = (ushort)Mathf.Clamp(value.intValue, 0, 100);
            EditorGUI.EndProperty();
        }
    }
}