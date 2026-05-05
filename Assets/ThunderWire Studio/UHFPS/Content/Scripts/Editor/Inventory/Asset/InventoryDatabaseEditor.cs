using UnityEngine;
using UnityEditor;
using UHFPS.Scriptable;
using ThunderWire.Editors;
using UnityEditor.Callbacks;
using System.Linq;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(InventoryDatabase))]
    public class InventoryDatabaseEditor : InspectorEditor<InventoryDatabase>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Inventory Database"), Target);
            EditorGUILayout.Space();

            string sections = "There are no sections yet.";
            if (Target.Sections.Count > 0)
            {
                string[] allSections = Target.Sections
                    .Select(x => $"{x.Section.Name} ({x.Items.Count})")
                    .ToArray();
                sections = string.Join(", ", allSections);
            }

            using (new EditorDrawing.BorderBoxScope())
            {
                GUIContent title = EditorDrawing.IconTextContent("Sections", "Prefab On Icon", 14f);
                EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
                EditorGUILayout.Space(2f);

                EditorGUILayout.LabelField(sections, EditorStyles.wordWrappedMiniLabel);
            }

            EditorGUILayout.Space();
            EditorDrawing.Separator();
            EditorGUILayout.Space(2f);

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                {
                    if (GUILayout.Button("Open Inventory Builder", GUILayout.Width(180f), GUILayout.Height(25)))
                    {
                        OpenDatabaseEditor(Target);
                    }
                }
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            var asset = obj as InventoryDatabase;
            if (asset == null) return false;

            OpenDatabaseEditor(asset);
            return true;
        }

        private static void OpenDatabaseEditor(InventoryDatabase asset)
        {
            EditorWindow window = EditorWindow.GetWindow<InventoryBuilder>(false, "Inventory Builder", true);

            Vector2 windowSize = new(1000, 500);
            window.minSize = windowSize;
            (window as InventoryBuilder).Show(asset);
        }
    }
}