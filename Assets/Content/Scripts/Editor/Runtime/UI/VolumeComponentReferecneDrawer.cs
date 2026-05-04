using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(VolumeComponentReferecne))]
    public class VolumeComponentReferecneDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty volume = property.FindPropertyRelative("Volume");
            SerializedProperty index = property.FindPropertyRelative("ComponentIndex");

            EditorGUI.BeginProperty(position, label, property);
            {
                position.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(position, volume);

                position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                using (new EditorGUI.DisabledGroupScope(volume.objectReferenceValue == null))
                {
                    Volume volumeRef = (Volume)volume.objectReferenceValue;
                    List<VolumeComponent> componentsList = new();
                    int selectedIndex = index.intValue;

                    if (volumeRef != null) componentsList = volumeRef.sharedProfile.components;

                    string[] components = componentsList.Select(x => x.name).ToArray();
                    string selected = volumeRef == null || components.Length <= 0
                        ? null : selectedIndex < componentsList.Count 
                        ? components[selectedIndex] : components[0];

                    position = EditorGUI.PrefixLabel(position, new GUIContent("Volume Component"));
                    EditorDrawing.DrawStringSelectPopup(position, new GUIContent("Select Component"), components, selected, (item) =>
                    {
                        index.intValue = Array.IndexOf(components, item);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                }
            }
            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}