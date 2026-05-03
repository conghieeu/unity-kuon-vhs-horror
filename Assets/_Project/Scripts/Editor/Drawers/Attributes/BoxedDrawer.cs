using System.Linq;
using UnityEngine;
using UnityEditor;
using ThunderWire.Attributes;

namespace ThunderWire.Editors
{
    [CustomPropertyDrawer(typeof(BoxedAttribute))]
    public class BoxedDrawer : PropertyDrawer
    {
        public BoxedAttribute Attribute => attribute as BoxedAttribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.yMax -= EditorGUIUtility.standardVerticalSpacing;
            position.y += EditorGUIUtility.standardVerticalSpacing;

            if (property.propertyType == SerializedPropertyType.Generic)
            {
                Rect headerRect = position;
                headerRect.height = Attribute.headerHeight;

                var propertyChildrens = property.GetVisibleChildrens();
                bool hasChilds = propertyChildrens.Count() > 0;

                GUIContent headerLabel = label;
                if (!string.IsNullOrEmpty(Attribute.icon))
                {
                    EditorGUIUtility.SetIconSize(new Vector2(15, 15));

                    if (Attribute.resourcesIcon) headerLabel = new GUIContent(Resources.Load<Texture>(Attribute.icon));
                    else headerLabel = EditorGUIUtility.TrIconContent(Attribute.icon);

                    headerLabel.text = " " + label.text;
                }

                if (!string.IsNullOrEmpty(Attribute.title))
                {
                    headerLabel.text = !string.IsNullOrEmpty(Attribute.icon) ? " " + Attribute.title : Attribute.title;
                }

                GUI.Box(position, GUIContent.none, new GUIStyle(Attribute.rounded ? "HelpBox" : "Tooltip"));
                property.isExpanded = EditorDrawing.DrawFoldoutHeader(headerRect, headerLabel, property.isExpanded);

                if (hasChilds && property.isExpanded)
                {
                    Rect childRect = position;
                    childRect.yMin += headerRect.height;

                    childRect.width -= 10f;
                    childRect.x += 5f;
                    childRect.y += 2f;

                    foreach (var child in propertyChildrens)
                    {
                        EditorGUI.BeginChangeCheck();
                        {
                            childRect.height = EditorGUI.GetPropertyHeight(child, true);
                            bool isArray = EditorDrawing.IsArray(child);

                            if (isArray) EditorGUI.indentLevel++;
                            {
                                EditorGUI.PropertyField(childRect, child, true);
                            }
                            if (isArray) EditorGUI.indentLevel--;

                            childRect.y += childRect.height + EditorGUIUtility.standardVerticalSpacing;
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            property.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
            }
            else
            {
                base.OnGUI(position, property, label);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.Generic)
            {
                if (property.isExpanded)
                {
                    return EditorGUI.GetPropertyHeight(property) + EditorGUIUtility.standardVerticalSpacing * 5;
                }

                return Attribute.headerHeight + EditorGUIUtility.standardVerticalSpacing;
            }
            
            return base.GetPropertyHeight(property, label) + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}