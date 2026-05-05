using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    public class PuzzleEditor<T> : Editor where T : MonoBehaviour
    {
        public T Target { get; private set; }
        public PropertyCollection Properties { get; private set; }

        private SerializedProperty foldoutProperty;

        public virtual void OnEnable()
        {
            Target = target as T;
            Properties = EditorDrawing.GetAllProperties(serializedObject);
            foldoutProperty = Properties["PuzzleCamera"];
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
                    Properties.Draw("PuzzleCamera");
                    Properties.Draw("SwitchCameraFadeSpeed");
                    EditorGUI.indentLevel++;
                    Properties.Draw("ControlsContexts");
                    EditorGUI.indentLevel--;
                }

                EditorGUILayout.Space(1f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Puzzle Layers")))
                {
                    Properties.Draw("CullLayers");
                    Properties.Draw("InteractLayer");
                    Properties.Draw("DisabledLayer");
                    Properties.Draw("EnablePointer");
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

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnScreenFade"], new GUIContent("Puzzle Events")))
                {
                    Properties.Draw("OnScreenFade");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorDrawing.EndBorderHeaderLayout();
            }
            EditorDrawing.ResetLabelColor();
        }
    }
}