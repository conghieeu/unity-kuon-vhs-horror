using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(MinMaxInt))]
    public class MinMaxIntDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty min = prop.FindPropertyRelative("min");
            SerializedProperty max = prop.FindPropertyRelative("max");

            EditorGUI.BeginProperty(pos, label, prop);
            pos = EditorGUI.PrefixLabel(pos, label);
            pos.xMax -= EditorGUIUtility.singleLineHeight + 2f;

            int[] values = new int[2];
            values[0] = min.intValue;
            values[1] = max.intValue;

            EditorGUI.MultiIntField(pos, new GUIContent[]
            {
                new GUIContent("Min"),
                new GUIContent("Max"),
            }, values);

            min.intValue = values[0];
            max.intValue = values[1];

            Rect flipRect = pos;
            flipRect.width = EditorGUIUtility.singleLineHeight;
            flipRect.x = pos.xMax + 2f;

            Vector2 iconSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(15, 15));

            GUIContent flipIcon = EditorGUIUtility.TrIconContent("preAudioLoopOff", "Flip min. max. values.");
            if (GUI.Button(flipRect, flipIcon, EditorStyles.iconButton))
            {
                int _min = min.intValue;
                min.intValue = max.intValue;
                max.intValue = _min;

                if (prop.serializedObject != null)
                    prop.serializedObject.ApplyModifiedProperties();
            }

            EditorGUIUtility.SetIconSize(iconSize);
            EditorGUI.EndProperty();
        }
    }
}