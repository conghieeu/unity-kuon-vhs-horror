using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System;

namespace ThunderWire.Editors
{
    public class PropertyCollection : Dictionary<string, SerializedProperty>
    {
        public SerializedProperty GetRelative(string propertyPath)
        {
            string[] paths = propertyPath.Split(new char[] { '.' });
            if (TryGetValue(paths[0], out SerializedProperty pathProperty))
            {
                for (int i = 1; i < paths.Length; i++)
                {
                    pathProperty = pathProperty.FindPropertyRelative(paths[i]);
                }

                return pathProperty;
            }

            return null;
        }

        public void DrawRelative(string propertyPath)
        {
            string[] paths = propertyPath.Split(new char[] { '.' });
            if (TryGetValue(paths[0], out SerializedProperty pathProperty))
            {
                for (int i = 1; i < paths.Length; i++)
                {
                    pathProperty = pathProperty.FindPropertyRelative(paths[i]);
                }

                EditorGUILayout.PropertyField(pathProperty);
            }
        }

        public void DrawRelative(string propertyPath, GUIContent label)
        {
            string[] paths = propertyPath.Split(new char[] { '.' });
            if (TryGetValue(paths[0], out SerializedProperty pathProperty))
            {
                for (int i = 1; i < paths.Length; i++)
                {
                    pathProperty = pathProperty.FindPropertyRelative(paths[i]);
                }

                EditorGUILayout.PropertyField(pathProperty, label);
            }
        }

        public void Draw(string propertyName, int indent = 0)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
            {
                if (indent > 0) EditorGUI.indentLevel += indent;
                EditorGUILayout.PropertyField(property);
                if (indent > 0) EditorGUI.indentLevel -= indent;
            }
        }

        public void DrawBacking(string propertyName)
        {
            propertyName = $"<{propertyName}>k__BackingField";
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUILayout.PropertyField(property);
        }

        public bool DrawGetBool(string propertyName)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
            {
                EditorGUILayout.PropertyField(property);
                return property.boolValue;
            }

            return false;
        }

        public bool DrawToggleLeft(string propertyName)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
            {
                GUIContent label = new(property.displayName, property.tooltip);
                return property.boolValue = EditorGUILayout.ToggleLeft(label, property.boolValue);
            }

            return false;
        }

        public void DrawAll(bool indentDropdowns = false, int skip = 0)
        {
            foreach (var property in this.Skip(skip))
            {
                bool shouldIndent = property.Value.propertyType == SerializedPropertyType.Generic;

                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.Value);
                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel--;
            }
        }

        public void DrawAllExcept(bool indentDropdowns = false, int skip = 0, params string[] except)
        {
            foreach (var property in this.Skip(skip))
            {
                if (except.Contains(property.Key))
                    continue;

                bool shouldIndent = property.Value.propertyType == SerializedPropertyType.Generic;

                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.Value);
                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel--;
            }
        }

        public void DrawAllPredicate(bool indentDropdowns, int skip, Predicate<string> predicate)
        {
            foreach (var property in this.Skip(skip))
            {
                if (!predicate(property.Key))
                    continue;

                bool shouldIndent = property.Value.propertyType == SerializedPropertyType.Generic;

                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.Value);
                if (indentDropdowns && shouldIndent) EditorGUI.indentLevel--;
            }
        }

        public bool BoolValue(string propertyName)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                return property.boolValue;

            return false;
        }

        public void DrawArray(string propertyName)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
            {
                if (property.isArray) EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property);
                if (property.isArray) EditorGUI.indentLevel--;
            }
        }

        public void Draw(string propertyName, bool includeChildren)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUILayout.PropertyField(property, includeChildren);
        }

        public void Draw(string propertyName, GUIContent label)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUILayout.PropertyField(property, label);
        }

        public bool DrawGetBool(string propertyName, GUIContent label)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
            {
                EditorGUILayout.PropertyField(property, label);
                return property.boolValue;
            }

            return false;
        }

        public void Draw(string propertyName, GUIContent label, bool includeChildren)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUILayout.PropertyField(property, label, includeChildren);
        }

        public void Draw(Rect rect, string propertyName)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUI.PropertyField(rect, property);
        }

        public void Draw(Rect rect, string propertyName, bool includeChildren)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUI.PropertyField(rect, property, includeChildren);
        }

        public void Draw(Rect rect, string propertyName, GUIContent label)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUI.PropertyField(rect, property, label);
        }

        public void Draw(Rect rect, string propertyName, GUIContent label, bool includeChildren)
        {
            if (TryGetValue(propertyName, out SerializedProperty property))
                EditorGUI.PropertyField(rect, property, label, includeChildren);
        }
    }
}