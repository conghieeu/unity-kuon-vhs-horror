using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(InteractIconObject)), CanEditMultipleObjects]
    public class InteractIconObjectEditor : InspectorEditor<InteractIconObject>
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.HelpBox("An object with this script attached will be marked as a floating icon object, so that when you hover mouse over the object, the floating icon will appear.", MessageType.Info);
            EditorGUILayout.HelpBox("If you hold down the use button, the icon will change to a hold icon and back when you release the use button.", MessageType.Info);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Hover Icon")))
                {
                    Properties.Draw("HoverIcon");
                    Properties.Draw("HoverSize");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Hold Icon")))
                {
                    Properties.Draw("HoldIcon");
                    Properties.Draw("HoldSize");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("IconOffset");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}