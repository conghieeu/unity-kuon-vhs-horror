using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UnityEditor.IMGUI.Controls;

namespace UHFPS.Editors
{
    [CustomPropertyDrawer(typeof(GString))]
    public class GStringDrawer : PropertyDrawer
    {
        private GameLocalization gameLocalization;
        private GameLocalization GameLocalization
        {
            get
            {
                if (gameLocalization == null && GameLocalization.HasReference)
                    gameLocalization = GameLocalization.Instance;

                return gameLocalization;
            }
        }

        private readonly List<CustomDropdownItem> localizationStrings;

        public GStringDrawer()
        {
            if (GameLocalization != null && GameLocalization.LocalizationTable != null)
            {
                localizationStrings = new();
                foreach (var tableData in GameLocalization.LocalizationTable.TableSheet)
                {
                    string sectionName = tableData.SectionName;

                    foreach (var item in tableData.SectionSheet)
                    {
                        string keyName = item.Key;
                        string path = $"{sectionName}/{keyName}";

                        string _sectionName = sectionName.Replace(" ", "");
                        string _keyName = keyName.Replace(" ", "");
                        string key = _sectionName + "." + _keyName;

                        localizationStrings.Add(new CustomDropdownItem()
                        {
                            Path = path,
                            Item = key
                        });
                    }
                }
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty glocProp = property.FindPropertyRelative("GlocText");
            SerializedProperty textProp = property.FindPropertyRelative("NormalText");

            EditorGUI.BeginProperty(position, label, property);
            {
#if UHFPS_LOCALIZATION
                Rect finderRect = position;
                finderRect.xMin = finderRect.xMax - EditorGUIUtility.singleLineHeight;

                Rect dropdownRect = position;
                dropdownRect.width = 250f;
                dropdownRect.height = 0f;
                dropdownRect.y += 21f;
                dropdownRect.x += position.xMax - dropdownRect.width - EditorGUIUtility.singleLineHeight;

                position.xMax -= EditorGUIUtility.singleLineHeight + 2f;
                glocProp.stringValue = EditorGUI.TextField(position, label, glocProp.stringValue);

                GUIContent finderIcon = EditorGUIUtility.TrIconContent("Search Icon", "Localization Key Selector");
                using (new EditorGUI.DisabledGroupScope(GameLocalization == null))
                {
                    if (GUI.Button(finderRect, finderIcon, EditorStyles.iconButton))
                    {
                        CustomDropdown customDropdown = new(new AdvancedDropdownState(), "Key Selector", localizationStrings);
                        customDropdown.OnItemSelected += (item) =>
                        {
                            if (item.Item != null) glocProp.stringValue = item.Item.ToString();
                            else glocProp.stringValue = string.Empty;

                            property.serializedObject.ApplyModifiedProperties();
                        };
                        customDropdown.Show(dropdownRect);
                    }
                }
#else
                textProp.stringValue = EditorGUI.TextField(position, label, textProp.stringValue);
#endif
            }
            EditorGUI.EndProperty();
        }
    }
}