using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UHFPS.Scriptable;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ObjectReferences))]
    public class ObjectReferencesEditor : Editor
    {
        private static ObjectReferences Target;

        private void OnEnable()
        {
            Target = target as ObjectReferences;
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);

            if (obj != null && obj is ObjectReferences)
            {
                OpenWindow();
                return true;
            }

            return false;
        }

        static void OpenWindow()
        {
            if (Target != null)
            {
                ObjectReferencesWindow objRefWindow = EditorWindow.GetWindow<ObjectReferencesWindow>(false, Target.name, true);

                Rect position = objRefWindow.position;
                position.width = 800;
                position.height = 450;

                objRefWindow.minSize = new Vector2(800, 450);
                objRefWindow.position = position;
                objRefWindow.Init(Target);
            }
            else
            {
                Debug.LogError("[OpenDatabaseEditor] Scriptable object is not initialized!");
            }
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Object References"));
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.HelpBox("Contains references to objects that can be instantiated and saved at runtime.", MessageType.Info, true);
                EditorGUILayout.HelpBox("Assign this asset to SaveGameManager script to enable reference picker with this asset.", MessageType.Warning, true);

                EditorGUILayout.Space();
                if (GUILayout.Button("Open Object References Window", GUILayout.Height(30)))
                {
                    OpenWindow();
                }

                EditorGUILayout.Space(2);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("References Count: " + Target.References.Count, EditorStyles.miniBoldLabel);
                EditorGUILayout.EndVertical();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}