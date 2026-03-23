using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    public class InspectorEditor<T> : Editor where T : Object
    {
        public T Target { get; private set; }
        public PropertyCollection Properties { get; private set; }

        public virtual void OnEnable()
        {
            Target = target as T;
            Properties = EditorDrawing.GetAllProperties(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            string name = Target.GetType().Name;
            string spacedName = Regex.Replace(name, "(\\B[A-Z])", " $1");
            EditorDrawing.DrawInspectorHeader(new GUIContent(spacedName), Target);
            EditorGUILayout.Space();
        }
    }
}