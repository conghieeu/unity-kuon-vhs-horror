using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(KeypadButton))]
    public class KeypadButtonEditor : InspectorEditor<KeypadButton>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Keypad Button"), Target);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Specifies which button should be pressed and called in the main KeypadPuzzle script. The KeypadPuzzle script should be added in the parent object of this object.", MessageType.Info);
            EditorGUILayout.Space();

            serializedObject.Update();
            Properties.Draw("Button");
            serializedObject.ApplyModifiedProperties();
        }
    }
}