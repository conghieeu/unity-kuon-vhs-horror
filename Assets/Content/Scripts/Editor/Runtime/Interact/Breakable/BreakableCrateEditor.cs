using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;
using UHFPS.Runtime;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(BreakableCrate))]
    public class BreakableCrateEditor : InspectorEditor<BreakableCrate>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Breakable Crate"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                if (Application.isPlaying)
                {
                    float health = Target.EntityHealth / 100f;
                    Rect healthProgressBarRect = EditorGUILayout.GetControlRect();
                    EditorGUI.ProgressBar(healthProgressBarRect, health, $"Crate Health ({Target.EntityHealth}%)");
                    EditorGUILayout.Space();
                }

                if (Properties.BoolValue("SpawnRandomItem"))
                {
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["CrateItems"], new GUIContent("Crate Items")))
                    {
                        int arraySize = EditorDrawing.BeginDrawCustomList(Properties["CrateItems"], new GUIContent("Items"));
                        {
                            for (int i = 0; i < arraySize; i++)
                            {
                                SerializedProperty element = Properties["CrateItems"].GetArrayElementAtIndex(i);
                                SerializedProperty item = element.FindPropertyRelative("Item");
                                SerializedProperty probability = element.FindPropertyRelative("Probability");

                                if (EditorDrawing.BeginFoldoutBorderLayout(element, new GUIContent($"Item {i}"), out Rect itemRect, roundedBox: false))
                                {
                                    EditorGUILayout.PropertyField(item);
                                    EditorGUILayout.PropertyField(probability);
                                    EditorDrawing.EndBorderHeaderLayout();
                                }

                                Rect minusIcon = itemRect;
                                minusIcon.xMin = minusIcon.xMax - EditorGUIUtility.singleLineHeight - 2f;
                                minusIcon.y += 4f;

                                if (GUI.Button(minusIcon, EditorUtils.Styles.MinusIcon, EditorStyles.iconButton))
                                {
                                    Properties["CrateItems"].DeleteArrayElementAtIndex(i);
                                    break;
                                }
                            }
                        }
                        EditorDrawing.EndDrawCustomList(new GUIContent("Add Item"), true, () =>
                        {
                            Target.CrateItems.Add(new());
                            serializedObject.ApplyModifiedProperties();
                        });

                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(2f);
                }
                else
                {
                    Properties.Draw("ItemInside");
                }

                Properties.Draw("BrokenCratePrefab");
                Properties.Draw("CrateCenter");

                EditorGUILayout.Space();
                using(new EditorDrawing.BorderBoxScope(new GUIContent("Breakable Settings")))
                {
                    Properties.Draw("SpawnRandomItem");
                    Properties.Draw("ShowFloatingIcon");
                    Properties.Draw("EnableItemsGravity");
                    Properties.Draw("PiecesKeepTime");
                    Properties.Draw("BrokenRotation");
                    Properties.Draw("SpawnedRotation");
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Explosion Settings")))
                {
                    Properties.Draw("ExplosionEffect");
                    Properties.Draw("UpwardsModifer");
                    Properties.Draw("ExplosionPower");
                    Properties.Draw("ExplosionRadius");
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sounds")))
                {
                    Properties.Draw("BreakSound");
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Events")))
                {
                    Properties.Draw("OnCrateBreak");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}