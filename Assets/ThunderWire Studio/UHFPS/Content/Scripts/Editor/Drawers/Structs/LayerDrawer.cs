using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(Layer))]
    public class LayerAttributeEditor : PropertyDrawer
    {
        public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
        {
            SerializedProperty indexProp = prop.FindPropertyRelative("index");
            EditorGUI.BeginProperty(pos, label, prop);
            {
                int index = indexProp.intValue;

                if (index > 31)
                {
                    Debug.Log("CustomPropertyDrawer, layer index is to high '" + index + "', is set to 31.");
                    index = 31;
                }
                else if (index < 0)
                {
                    Debug.Log("CustomPropertyDrawer, layer index is to low '" + index + "', is set to 0");
                    index = 0;
                }

                indexProp.intValue = EditorGUI.LayerField(pos, label, index);
            }
            EditorGUI.EndProperty();
        }
    }
}