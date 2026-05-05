using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(FloatingIconObject))]
    public class FloatingIconObjectEditor : InspectorEditor<FloatingIconObject>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Floating Icon Object"), target);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("An object with this script attached will be marked as a floating icon object, so a floating icon will appear following the object.", MessageType.Info);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using(new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Override Icon"), Properties["Override"]))
                {
                    Properties.Draw("CustomIcon");
                    Properties.Draw("IconSize");

                    EditorGUILayout.Space();
                    using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Override Culling"), Properties["OverrideCulling"]))
                    {
                        Properties.Draw("CullLayers");
                        Properties.Draw("DistanceShow");
                        Properties.Draw("DistanceHide");
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}