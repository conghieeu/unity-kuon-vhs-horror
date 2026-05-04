using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(PuzzleExamine))]
    public class PuzzleExamineEditor : PuzzleBlendEditor<PuzzleExamine>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Puzzle Examine"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}