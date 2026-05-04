using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    public class PuzzleSimpleEditor<T> : Editor where T : MonoBehaviour
    {
        public T Target { get; private set; }
        public PropertyCollection Properties { get; private set; }

        private bool settingsFoldout;

        public virtual void OnEnable()
        {
            Target = target as T;
            Properties = EditorDrawing.GetAllProperties(serializedObject);
        }

        public override void OnInspectorGUI()
        {
            GUIContent puzzleBaseContent = EditorGUIUtility.TrTextContentWithIcon(" Puzzle Base Settings", "Settings");
            if (EditorDrawing.BeginFoldoutBorderLayout(puzzleBaseContent, ref settingsFoldout))
            {
                Properties.Draw("DisabledLayer");
                EditorDrawing.EndBorderHeaderLayout();
            }
        }
    }
}
