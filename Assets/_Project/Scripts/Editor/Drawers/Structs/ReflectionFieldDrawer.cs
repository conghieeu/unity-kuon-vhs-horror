using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;
using System.Collections.Generic;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(ReflectionField))]
    public class ReflectionFieldDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty reflectType = property.FindPropertyRelative("ReflectType");
            SerializedProperty instance = property.FindPropertyRelative("Instance");
            SerializedProperty reflectName = property.FindPropertyRelative("ReflectName");
            SerializedProperty reflectDerived = property.FindPropertyRelative("ReflectDerived");

            MonoBehaviour instanceRef = instance.objectReferenceValue as MonoBehaviour;
            ReflectionField.ReflectionType reflectionType = (ReflectionField.ReflectionType)reflectType.enumValueIndex;

            EditorDrawing.DrawHeaderWithBorder(ref position, label);

            Rect instanceRect = position;
            instanceRect.height = EditorGUIUtility.singleLineHeight;
            instanceRect.y += 2f;
            instanceRect.xMin += 2f;
            instanceRect.xMax -= instanceRect.xMax * 0.2f;
            EditorGUI.PropertyField(instanceRect, instance);

            using (new EditorGUI.DisabledGroupScope(instanceRef == null))
            {
                Rect reflectTypeRect = instanceRect;
                reflectTypeRect.xMin = reflectTypeRect.xMax + 2f;
                reflectTypeRect.xMax = position.width + EditorGUIUtility.singleLineHeight - 2f;
                EditorGUI.PropertyField(reflectTypeRect, reflectType, GUIContent.none);

                Rect reflectNameRect = position;
                reflectNameRect.height = EditorGUIUtility.singleLineHeight;
                reflectNameRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing * 2;
                reflectNameRect.xMin += 2f;
                reflectNameRect.xMax -= EditorGUIUtility.singleLineHeight + 2f;

                Rect reflectDerivedRect = new();
                reflectDerivedRect.height = EditorGUIUtility.singleLineHeight;
                reflectDerivedRect.xMin = reflectNameRect.xMax + 2f;
                reflectDerivedRect.xMax = position.xMax - 2f;
                reflectDerivedRect.y = reflectNameRect.y;

                string[] fields = GetReflectionFields(instanceRef, reflectionType, reflectDerived.boolValue);
                int selected = 0;

                if (fields.Length > 0)
                {
                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (fields[i] == reflectName.stringValue)
                        {
                            selected = i;
                            break;
                        }
                    }
                }

                selected = EditorGUI.Popup(reflectNameRect, "Reflect Name", selected, fields);
                reflectName.stringValue = fields[selected];

                reflectDerived.boolValue = EditorGUI.Toggle(reflectDerivedRect, reflectDerived.boolValue);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 3 + 11f;
        }

        private string[] GetReflectionFields(MonoBehaviour instance, ReflectionField.ReflectionType reflectionType, bool reflectDerived)
        {
            List<string> fields = new();

            if (instance != null)
            {
                System.Type instanceType = instance.GetType();

                if (reflectionType == ReflectionField.ReflectionType.Field)
                {
                    fields.AddRange(from field in instanceType.GetFields(BindingFlags.Public | BindingFlags.Instance)
                                    where reflectDerived || field.DeclaringType == instanceType
                                    select field.Name);
                }
                else if (reflectionType == ReflectionField.ReflectionType.Property)
                {
                    fields.AddRange(from prop in instanceType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                    where reflectDerived || prop.DeclaringType == instanceType
                                    select prop.Name);
                }
                else
                {
                    fields.AddRange(from method in instanceType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                    where reflectDerived || method.DeclaringType == instanceType
                                    select $"{method.ReturnType.Name} {method.Name}");
                }
            }

            if (fields.Count == 0)
                fields.Add("None");

            return fields.ToArray();
        }
    }
}