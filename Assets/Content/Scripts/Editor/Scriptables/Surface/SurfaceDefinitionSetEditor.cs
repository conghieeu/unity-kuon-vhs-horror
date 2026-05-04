using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(SurfaceDefinitionSet)), CanEditMultipleObjects]
    public class SurfaceDefinitionSetEditor : InspectorEditor<SurfaceDefinitionSet>
    {
        private ReorderableList surfaces;

        public override void OnEnable()
        {
            base.OnEnable();

            surfaces = new ReorderableList(serializedObject, Properties["Surfaces"], true, false, true, true);
            surfaces.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = Properties["Surfaces"].GetArrayElementAtIndex(index);
                Rect elementRect = new(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.ObjectField(elementRect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Surface Definition Set"), Target);
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                surfaces.DoLayoutList();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}