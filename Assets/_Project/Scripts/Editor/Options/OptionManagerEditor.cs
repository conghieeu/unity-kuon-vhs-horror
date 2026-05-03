using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(OptionsManager))]
    public class OptionManagerEditor : InspectorEditor<OptionsManager>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Options Manager"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("OptionsAsset");
                    Properties.Draw("GlobalVolume");

                    Properties.Draw("ApplyAndSaveInputs");
                    Properties.Draw("ShowDebug");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Option Links")))
                {
                    var optionLinks = Properties["OptionLinks"];

                    if (optionLinks.arraySize > 0)
                    {
                        EditorGUILayout.HelpBox("Set the parent transform for each option section below. These transforms will act as the containers for options matching their section names.", MessageType.Warning);
                        EditorGUILayout.Space();

                        DrawOptionLinks(optionLinks);
                    }
                    else
                    {
                        EditorDrawing.RichHelpBox("No option sections found. Click the <b>Refresh Options</b> button to populate the sections.", MessageType.Warning);
                    }
                }

                EditorGUILayout.Space();

                using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorDrawing.RichHelpBox("After adding or removing options from the asset, click <b>Refresh Options</b> and then <b>Build Options</b> button to update the runtime options list.", MessageType.Warning);
                    EditorGUILayout.Space();

                    using (new EditorDrawing.BackgroundColorScope("#E0FBFC"))
                    {
                        if (GUILayout.Button("1. Refresh Options", GUILayout.Height(25f)))
                        {
                            Target.RefreshOptions();
                        }
                        if (GUILayout.Button("2. Build Options", GUILayout.Height(25f)))
                        {
                            Target.BuildOptions();
                        }
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawOptionLinks(SerializedProperty optionLinks)
        {
            for (int i = 0; i < optionLinks.arraySize; i++)
            {
                var link = optionLinks.GetArrayElementAtIndex(i);
                DrawOptionSection(link);
            }
        }

        private void DrawOptionSection(SerializedProperty section)
        {
            using(new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                Rect headerRect = EditorGUILayout.GetControlRect(true, 22f);
                string name = section.Find("SectionReference.Name").stringValue;
                var parent = section.FindPropertyRelative("SectionParent");
                var items = section.FindPropertyRelative("OptionItems");

                float foldoutOffset = 12f;
                int indent = EditorGUI.indentLevel * 15;
                Rect foldoutPos = new(headerRect.x + indent + foldoutOffset, headerRect.y + 2f, EditorGUIUtility.labelWidth - indent - foldoutOffset, EditorGUIUtility.singleLineHeight);
                Rect parentPos = new(headerRect.x + EditorGUIUtility.labelWidth + 2f, headerRect.y + 2f, headerRect.width - EditorGUIUtility.labelWidth - 2f, EditorGUIUtility.singleLineHeight);

                EditorGUI.PropertyField(parentPos, parent, GUIContent.none);
                if (section.isExpanded = EditorGUI.Foldout(foldoutPos, section.isExpanded, new GUIContent(name), true))
                {
                    EditorDrawing.SeparatorSpaced(1f);
                    for (int i = 0; i < items.arraySize; i++)
                    {
                        var item = items.GetArrayElementAtIndex(i);
                        string optionName = item.Find("OptionReference.Name").stringValue;
                        var optionItem = item.FindPropertyRelative("OptionBehaviour");

                        Rect optionRect = EditorGUILayout.GetControlRect();
                        optionRect = EditorGUI.PrefixLabel(optionRect, new GUIContent(optionName));
                        EditorGUI.PropertyField(optionRect, optionItem, GUIContent.none);
                    }
                }
            }
        }
    }
}