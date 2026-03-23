using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(MinMax))]
    public class MinMaxDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty min = prop.FindPropertyRelative("min");
            SerializedProperty max = prop.FindPropertyRelative("max");

            EditorGUI.BeginProperty(pos, label, prop);
            pos = EditorGUI.PrefixLabel(pos, label);
            pos.xMax -= EditorGUIUtility.singleLineHeight + 2f;

            float[] values = new float[2];
            values[0] = min.floatValue;
            values[1] = max.floatValue;

            EditorGUI.MultiFloatField(pos, new GUIContent[] 
            {
                new GUIContent("Min"),
                new GUIContent("Max"),
            }, values);

            min.floatValue = values[0];
            max.floatValue = values[1];

            Rect flipRect = pos;
            flipRect.width = EditorGUIUtility.singleLineHeight;
            flipRect.x = pos.xMax + 2f;

            Vector2 iconSize = EditorGUIUtility.GetIconSize(); 
            EditorGUIUtility.SetIconSize(new Vector2(15, 15));

            GUIContent flipIcon = EditorGUIUtility.TrIconContent("preAudioLoopOff", "Flip min. max. values.");
            if (GUI.Button(flipRect, flipIcon, EditorStyles.iconButton))
            {
                float _min = min.floatValue;
                min.floatValue = max.floatValue;
                max.floatValue = _min;

                if(prop.serializedObject != null)
                    prop.serializedObject.ApplyModifiedProperties();
            }

            EditorGUIUtility.SetIconSize(iconSize);
            EditorGUI.EndProperty();
        }
    }
}