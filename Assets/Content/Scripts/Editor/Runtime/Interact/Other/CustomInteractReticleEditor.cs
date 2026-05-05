using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(CustomInteractReticle))]
    public class CustomInteractReticleEditor : InspectorEditor<CustomInteractReticle>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Custom Interact Reticle"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("DynamicHoldReticle");
                Properties.Draw("OverrideReticle");

                if (Properties.BoolValue("DynamicHoldReticle"))
                {
                    Properties.Draw("HoldReticle");
                    EditorGUILayout.Space();
                    Properties.Draw("DynamicHold");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}