using System.Reflection;
using UnityEngine;
using UnityEditor;
using ThunderWire.Attributes;

namespace ThunderWire.Editors
{
    [CustomEditor(typeof(Object), true), CanEditMultipleObjects]
    public class CustomInspectorDrawer : Editor
    {
        bool showInspectorHeader;
        InspectorHeaderAttribute headerAttribute;

        bool showInspectorHelp;
        HelpBoxAttribute helpAttribute;

        private void OnEnable()
        {
            headerAttribute = (InspectorHeaderAttribute)target.GetType().GetCustomAttribute(typeof(InspectorHeaderAttribute), false);
            helpAttribute = (HelpBoxAttribute)target.GetType().GetCustomAttribute(typeof(HelpBoxAttribute), false);

            showInspectorHeader = headerAttribute != null;
            showInspectorHelp = helpAttribute != null;
        }

        public override void OnInspectorGUI()
        {
            if (showInspectorHeader)
            {
                string title = headerAttribute.title;
                string icon = headerAttribute.icon;

                GUIContent titleGUI = new GUIContent(title);
                if (!string.IsNullOrEmpty(icon))
                    titleGUI = EditorGUIUtility.TrTextContentWithIcon(" " + title, icon);

                EditorDrawing.DrawInspectorHeader(titleGUI, target);
                if(headerAttribute.space) EditorGUILayout.Space();
            }

            if (showInspectorHelp)
            {
                EditorGUILayout.HelpBox(helpAttribute.message, (MessageType)helpAttribute.messageType);
            }

            if (showInspectorHeader)
            {
                serializedObject.Update();

                EditorGUI.BeginChangeCheck();
                DrawPropertiesExcluding(serializedObject, "m_Script");

                if (EditorGUI.EndChangeCheck())
                    serializedObject.ApplyModifiedProperties();
            }

            if (showInspectorHeader || showInspectorHelp)
                return;

            base.OnInspectorGUI();
        }
    }
}