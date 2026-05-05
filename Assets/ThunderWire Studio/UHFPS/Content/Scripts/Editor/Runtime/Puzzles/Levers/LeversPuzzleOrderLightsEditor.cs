using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LeversPuzzleOrderLights))]
    public class LeversPuzzleOrderLightsEditor : InspectorEditor<LeversPuzzleOrderLights>
    {
        public override void OnEnable()
        {
            base.OnEnable();

            if(Properties["LeversPuzzle"].objectReferenceValue != null)
                Properties["OrderLights"].arraySize = Target.LeversPuzzle.Levers.Count;
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Levers Puzzle Order Lights"), Target);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("A helper script that is used to display the number of lever interactions you need to validate the levers order.", MessageType.Info);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUI.BeginChangeCheck();
                Properties.Draw("LeversPuzzle");
                if (EditorGUI.EndChangeCheck())
                {
                    LeversPuzzle leversPuzzle = (LeversPuzzle)Properties["LeversPuzzle"].objectReferenceValue;
                    if (leversPuzzle != null) Properties["OrderLights"].arraySize = leversPuzzle.Levers.Count;
                    else Properties["OrderLights"].arraySize = 0;
                }

                if (Properties["LeversPuzzle"].objectReferenceValue != null)
                {
                    EditorGUILayout.Space(1f);
                    DrawOrderLeverList();
                }

                EditorGUILayout.Space();
                Properties.Draw("EmissionKeyword");
                using(new EditorGUI.DisabledGroupScope(true))
                {
                    Properties.Draw("OrderIndex");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawOrderLeverList()
        {
            if(EditorDrawing.BeginFoldoutBorderLayout(Properties["OrderLights"], new GUIContent("Order Lights")))
            {
                for (int i = 0; i < Properties["OrderLights"].arraySize; i++)
                {
                    SerializedProperty property = Properties["OrderLights"].GetArrayElementAtIndex(i);
                    if (EditorDrawing.BeginFoldoutBorderLayout(property, new GUIContent("Light " + i)))
                    {
                        EditorDrawing.GetAllProperties(property).DrawAll();
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorGUILayout.Space(1f);
                }

                EditorDrawing.EndBorderHeaderLayout();
            }
        }
    }
}