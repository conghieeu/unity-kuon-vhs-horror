using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(SoundClip))]
    public class SoundClipDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty clip = property.FindPropertyRelative("audioClip");
            SerializedProperty volume = property.FindPropertyRelative("volume");

            EditorGUI.BeginProperty(position, label, property);

            Rect lineRect = position;
            lineRect.width = 2f;
            lineRect.height -= 4f;
            lineRect.x += 2f;
            lineRect.y += 2f;
            EditorGUI.DrawRect(lineRect, new Color(0.5f, 0.5f, 0.5f, 1));

            position.height = EditorGUIUtility.singleLineHeight;
            position.xMin += 8f;

            Rect clipRect = EditorGUI.PrefixLabel(position, label);
            clipRect.xMin -= 8f;
            EditorGUI.PropertyField(clipRect, clip, GUIContent.none);

            position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            Rect volumeRect = EditorGUI.PrefixLabel(position, new GUIContent("Clip Volume"), EditorStyles.miniBoldLabel);
            volumeRect.xMin -= 8f;
            volume.floatValue = EditorGUI.Slider(volumeRect, volume.floatValue, 0f, 1f);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}