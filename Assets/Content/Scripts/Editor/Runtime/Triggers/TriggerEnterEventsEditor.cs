using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(TriggerEnterEvents))]
    public class TriggerEnterEventsEditor : InspectorEditor<TriggerEnterEvents>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Trigger Enter Events"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUI.indentLevel++;
                Properties.Draw("TriggerTags");
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Trigger Settings")))
                {
                    Properties.Draw("TriggerOnce");
                    Properties.Draw("TriggerStayRate");
                }
                EditorGUILayout.Space();

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["TriggerEnter"], new GUIContent("Events")))
                {
                    Properties.Draw("TriggerEnter");
                    Properties.Draw("TriggerExit");
                    Properties.Draw("TriggerStay");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}