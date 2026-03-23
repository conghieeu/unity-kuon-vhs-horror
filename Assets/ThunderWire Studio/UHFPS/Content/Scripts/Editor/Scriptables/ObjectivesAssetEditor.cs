using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ObjectivesAsset))]
    public class ObjectivesAssetEditor : InspectorEditor<ObjectivesAsset>
    {
        private ReorderableList[] reorderableLists;

        public override void OnEnable()
        {
            base.OnEnable();
            CreateReorderableLists();
        }

        private void CreateReorderableLists()
        {
            int arraySize = Properties["Objectives"].arraySize;
            reorderableLists = new ReorderableList[arraySize];

            for (int i = 0; i < arraySize; i++)
            {
                SerializedProperty objective = Properties["Objectives"].GetArrayElementAtIndex(i);
                SerializedProperty subObjectives = objective.FindPropertyRelative("SubObjectives");
                reorderableLists[i] = SetupReorderableListFor(subObjectives);
            }
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Objectives Asset"));
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                int arraySize = EditorDrawing.BeginDrawCustomList(Properties["Objectives"], new GUIContent("Objectives"));
                {
                    for (int i = 0; i < arraySize; i++)
                    {
                        SerializedProperty objective = Properties["Objectives"].GetArrayElementAtIndex(i);
                        SerializedProperty objKey = objective.FindPropertyRelative("ObjectiveKey");
                        SerializedProperty objTitle = objective.FindPropertyRelative("ObjectiveTitle");
                        SerializedProperty subObjectives = objective.FindPropertyRelative("SubObjectives");

                        // get reorderable list
                        ReorderableList subList = reorderableLists[i];

                        string title = string.IsNullOrEmpty(objKey.stringValue) ? $"Objective {i}" : "(Objective) " + objKey.stringValue;
                        GUIContent titleContent = EditorDrawing.IconTextContent(title, "sv_icon_dot5_pix16_gizmo", 14f);

                        if (EditorDrawing.BeginFoldoutBorderLayout(objective, titleContent, out Rect headerRect, roundedBox: false))
                        {
                            EditorDrawing.ResetIconSize();

                            EditorGUILayout.PropertyField(objKey);
                            EditorGUILayout.PropertyField(objTitle);
                            EditorGUILayout.Space();

                            GUIContent subContent = EditorDrawing.IconTextContent("Sub Objectives", "sv_icon_dot13_pix16_gizmo", 14f);
                            if (EditorDrawing.BeginFoldoutBorderLayout(subObjectives, subContent, out Rect subHeaderRect))
                            {
                                EditorDrawing.ResetIconSize();

                                subList.DoLayoutList();
                                EditorDrawing.EndBorderHeaderLayout();
                            }

                            Rect countLabelRect = subHeaderRect;
                            countLabelRect.height = EditorGUIUtility.singleLineHeight;
                            countLabelRect.xMin = countLabelRect.xMax - 25f;
                            countLabelRect.x -= 2f;
                            countLabelRect.y += 2f;

                            GUI.enabled = false;
                            EditorGUI.IntField(countLabelRect, subObjectives.arraySize);
                            GUI.enabled = true;

                            EditorDrawing.EndBorderHeaderLayout();
                        }

                        Rect removeButton = headerRect;
                        removeButton.xMin = removeButton.xMax - EditorGUIUtility.singleLineHeight;
                        removeButton.y += 3f;

                        if (GUI.Button(removeButton, EditorUtils.Styles.MinusIcon, EditorStyles.iconButton))
                        {
                            Properties["Objectives"].DeleteArrayElementAtIndex(i);
                            break;
                        }

                        if (i + 1 < arraySize) EditorGUILayout.Space(1f);
                    }
                }
                EditorDrawing.EndDrawCustomList(new GUIContent("Add Objective"), true, () =>
                {
                    Target.Objectives.Add(new());
                    serializedObject.Update();
                    CreateReorderableLists();
                });
            }
            serializedObject.ApplyModifiedProperties();
        }

        private ReorderableList SetupReorderableListFor(SerializedProperty property)
        {
            ReorderableList reorderableList = new(serializedObject, property, true, false, true, true);

            reorderableList.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = property.GetArrayElementAtIndex(index);
                SerializedProperty subKey = element.FindPropertyRelative("SubObjectiveKey");
                rect.xMin += 12f;

                // Use a foldout to toggle display of the element's properties
                string foldoutLabel = string.IsNullOrEmpty(subKey.stringValue) ? $"SubObjective {index}" : subKey.stringValue;
                element.isExpanded = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element.isExpanded, foldoutLabel);

                if (element.isExpanded)
                {
                    SerializedProperty subCount = element.FindPropertyRelative("CompleteCount");
                    SerializedProperty subTitle = element.FindPropertyRelative("ObjectiveText");

                    float lineHeight = EditorGUIUtility.singleLineHeight;
                    float padding = 2f;

                    // Draw SubObjectiveKey field
                    Rect subKeyRect = new Rect(rect.x, rect.y + lineHeight + padding, rect.width, lineHeight);
                    EditorGUI.PropertyField(subKeyRect, subKey);

                    // Draw CompleteCount field
                    Rect subCountRect = new Rect(rect.x, rect.y + (lineHeight + padding) * 2, rect.width, lineHeight);
                    EditorGUI.PropertyField(subCountRect, subCount);

                    // Draw ObjectiveText field
                    Rect subTitleRect = new Rect(rect.x, rect.y + (lineHeight + padding) * 3, rect.width, lineHeight);
                    EditorGUI.PropertyField(subTitleRect, subTitle);
                }
            };

            // Setting the element height
            reorderableList.elementHeightCallback += (int index) =>
            {
                SerializedProperty element = property.GetArrayElementAtIndex(index);
                float lineHeight = EditorGUIUtility.singleLineHeight;
                float padding = 2f;

                return element.isExpanded ? (lineHeight * 4 + padding * 3) : (lineHeight + padding);
            };

            return reorderableList;
        }
    }
}