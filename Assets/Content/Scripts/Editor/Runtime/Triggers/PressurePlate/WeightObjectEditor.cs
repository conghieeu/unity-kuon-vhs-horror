using UHFPS.Editors;
using UnityEngine;
using UnityEditor;
using ThunderWire.Editors;

namespace UHFPS.Runtime
{
    [CustomEditor(typeof(WeightObject))]
    public class WeightObjectEditor : InspectorEditor<WeightObject>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Weight Object"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    if (Properties.DrawToggleLeft("UseRigidbodyMass"))
                    {
                        string mass = $"{Target.Rigidbody.mass} (Rigidbody)";
                        EditorDrawing.DrawPrefixLabel("Object Weight", mass, EditorStyles.label);
                    }
                    else Properties.Draw("ObjectWeight");
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space(2f);

                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    if (Properties.DrawToggleLeft("AllowStacking"))
                    {
                        string mass = $"{Target.StackedWeight}";
                        EditorDrawing.DrawPrefixLabel("Stacked Weight", mass, EditorStyles.label);
                    }
                    else EditorDrawing.DrawPrefixLabel("Stacked Weight", "none", EditorStyles.label);
                }
                EditorGUILayout.EndVertical();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}