using UnityEngine.UI;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LayoutElementResizer))]
    public class LayoutElementResizerEditor : InspectorEditor<LayoutElementResizer>
    {
        private readonly bool[] foldout = new bool[2];

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Layout Element Resizer"), target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                GUIContent layoutElementGUI = EditorGUIUtility.TrTextContentWithIcon(" Layout Element", "Settings");
                if (EditorDrawing.BeginFoldoutBorderLayout(layoutElementGUI, ref foldout[0]))
                {
                    if (!Properties.DrawGetBool("m_IgnoreLayout"))
                    {
                        EditorGUILayout.Space();

                        LayoutElementField(Properties["m_MinWidth"], 0);
                        LayoutElementField(Properties["m_MinHeight"], 0);

                        using (new EditorGUI.DisabledGroupScope(Properties.BoolValue("AutoResizeWidth")))
                        {
                            LayoutElementField(Properties["m_PreferredWidth"], t => t.rect.width);
                        }

                        using (new EditorGUI.DisabledGroupScope(Properties.BoolValue("AutoResizeHeight")))
                        {
                            LayoutElementField(Properties["m_PreferredHeight"], t => t.rect.height);
                        }

                        LayoutElementField(Properties["m_FlexibleWidth"], 1);
                        LayoutElementField(Properties["m_FlexibleHeight"], 1);
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space();

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Auto Resize Width"), Properties["AutoResizeWidth"]))
                {
                    if (Properties.DrawGetBool("CustomWidthResize"))
                        Properties.Draw("WidthTarget");

                    Properties.Draw("WidthPadding");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Auto Resize Height"), Properties["AutoResizeHeight"]))
                {
                    if (Properties.DrawGetBool("CustomHeightResize"))
                        Properties.Draw("HeightTarget");

                    Properties.Draw("HeightPadding");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        void LayoutElementField(SerializedProperty property, float defaultValue)
        {
            LayoutElementField(property, _ => defaultValue);
        }

        void LayoutElementField(SerializedProperty property, System.Func<RectTransform, float> defaultValue)
        {
            Rect position = EditorGUILayout.GetControlRect();

            // Label
            GUIContent label = EditorGUI.BeginProperty(position, null, property);

            // Rects
            Rect fieldPosition = EditorGUI.PrefixLabel(position, label);

            Rect toggleRect = fieldPosition;
            toggleRect.width = 16;

            Rect floatFieldRect = fieldPosition;
            floatFieldRect.xMin += 16;

            // Checkbox
            EditorGUI.BeginChangeCheck();
            bool enabled = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, property.floatValue >= 0);
            if (EditorGUI.EndChangeCheck())
            {
                // This could be made better to set all of the targets to their initial width, but mimizing code change for now
                property.floatValue = (enabled ? defaultValue((target as LayoutElement).transform as RectTransform) : -1);
            }

            if (!property.hasMultipleDifferentValues && property.floatValue >= 0)
            {
                // Float field
                EditorGUIUtility.labelWidth = 4; // Small invisible label area for drag zone functionality
                EditorGUI.BeginChangeCheck();
                float newValue = EditorGUI.FloatField(floatFieldRect, new GUIContent(" "), property.floatValue);
                if (EditorGUI.EndChangeCheck())
                {
                    property.floatValue = Mathf.Max(0, newValue);
                }
                EditorGUIUtility.labelWidth = 0;
            }

            EditorGUI.EndProperty();
        }
    }
}