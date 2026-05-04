using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;
using static UHFPS.Runtime.SaveableObject;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(SaveableObject))]
    public class SaveableObjectEditor : InspectorEditor<SaveableObject>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Saveable Object"), Target);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("This script is used to save basic object properties such as position, rotation, scale, etc.. To save custom object properties, use the ISaveable interface in your own script.", MessageType.Info);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                Properties.Draw("SaveableFlags");
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();

                bool rendererFlag = Target.SaveableFlags.HasFlag(SaveableFlagsEnum.RendererActive);
                bool referencesFlag = Target.SaveableFlags.HasFlag(SaveableFlagsEnum.ReferencesActive);

                if(rendererFlag || referencesFlag)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Object Properties", EditorStyles.boldLabel);
                }

                if (rendererFlag)
                {
                    Properties.Draw("MeshRenderer");
                }

                if (referencesFlag)
                {
                    Properties.Draw("References");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}