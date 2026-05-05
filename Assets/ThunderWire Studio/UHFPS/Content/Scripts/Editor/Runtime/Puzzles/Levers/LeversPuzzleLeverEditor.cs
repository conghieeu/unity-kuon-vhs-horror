using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LeversPuzzleLever))]
    public class LeversPuzzleLeverEditor : InspectorEditor<LeversPuzzleLever>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Levers Puzzle Lever"), Target);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Determines whether the object is a lever object that you can interact with, and determines what lever is pressed. The LeversPuzzle script should be added in the any parent object of this object.", MessageType.Info);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using(new EditorGUI.DisabledGroupScope(true))
                {
                    Properties.Draw("LeverState");
                }
                EditorGUILayout.Space();

                using(new EditorDrawing.BorderBoxScope(new GUIContent("Limits")))
                {
                    Properties.Draw("Target");
                    Properties.Draw("LimitsObject");
                    Properties.Draw("SwitchLimits");
                    Properties.Draw("LimitsForward");
                    Properties.Draw("LimitsNormal");
                    Properties.Draw("SwitchSpeed");
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sounds")))
                {
                    Properties.Draw("LeverOnSound");
                    Properties.Draw("LeverOffSound");
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Light"), Properties["UseLight"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("UseLight")))
                    {
                        Properties.Draw("LeverLight");
                        Properties.Draw("EmissionKeyword");
                        Properties.Draw("LightRenderer");
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}