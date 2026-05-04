using UnityEngine;
using UnityEditor;
using Unity.Cinemachine;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    public class PuzzleBlendEditor<T> : Editor where T : MonoBehaviour
    {
        public T Target { get; private set; }
        public PropertyCollection Properties { get; private set; }

        private SerializedProperty foldoutProperty;

        public virtual void OnEnable()
        {
            Target = target as T;
            Properties = EditorDrawing.GetAllProperties(serializedObject);
            foldoutProperty = Properties["VirtualCamera"];
        }

        public override void OnInspectorGUI()
        {
            GUIContent headerContent = EditorDrawing.IconTextContent("Puzzle Settings", "Settings");
            EditorDrawing.SetLabelColor("#E0FBFC");

            if (EditorDrawing.BeginFoldoutBorderLayout(foldoutProperty, headerContent))
            {
                EditorDrawing.ResetLabelColor();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Puzzle Camera")))
                {
                    Properties.Draw("VirtualCamera");
                    EditorGUI.indentLevel++;
                    Properties.Draw("ControlsContexts");
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(1f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Blend Settings")))
                {
                    Properties.Draw("BlendDefinition");
                }

                EditorGUILayout.Space(1f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Ignore Colliders")))
                {
                    EditorGUI.indentLevel++;
                    {
                        Properties.Draw("CollidersEnable");
                        Properties.Draw("CollidersDisable");
                    }
                    EditorGUI.indentLevel--;
                }

                EditorDrawing.EndBorderHeaderLayout();
            }
            EditorDrawing.ResetLabelColor();
        }
    }
}