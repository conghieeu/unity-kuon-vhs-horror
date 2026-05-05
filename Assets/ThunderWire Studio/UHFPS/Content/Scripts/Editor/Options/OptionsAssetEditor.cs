using UnityEngine;
using UnityEditor;
using UHFPS.Scriptable;
using ThunderWire.Editors;
using UnityEditor.Callbacks;
using UnityEditorInternal;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(OptionsAsset))]
    public class OptionsAssetEditor : InspectorEditor<OptionsAsset>
    {
        private ReorderableList optionPrefabs;
        private bool expanded;

        public override void OnEnable()
        {
            base.OnEnable();
            optionPrefabs = new ReorderableList(serializedObject, Properties["OptionPrefabs"], true, false, true, true);
            EditorDrawing.SetupReorderableList(optionPrefabs);
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Options Asset"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Option Prefabs"), ref expanded))
                {
                    optionPrefabs.DoLayoutList();
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    {
                        if (GUILayout.Button("Open Options Builder", GUILayout.Width(180f), GUILayout.Height(25)))
                        {
                            OpenOptionsBuilder(Target);
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
            }
            serializedObject.ApplyModifiedProperties();
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            var asset = obj as OptionsAsset;
            if (asset == null) return false;

            OpenOptionsBuilder(asset);
            return true;
        }

        private static void OpenOptionsBuilder(OptionsAsset asset)
        {
            EditorWindow window = EditorWindow.GetWindow<OptionsBuilder>(false, "Options Builder", true);

            Vector2 windowSize = new(1000, 500);
            window.minSize = windowSize;
            (window as OptionsBuilder).Show(asset);
        }
    }
}