using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(FuseboxPuzzle))]
    public class FuseboxPuzzleEditor : PuzzleSimpleEditor<FuseboxPuzzle>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Fusebox Puzzle"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.Draw("FuseItem");
                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Fusebox Settings")))
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    Properties.Draw("UseInteract");
                    EditorGUILayout.HelpBox("Automatically detects if there are any fuses in the inventory and inserts them into the fusebox instead of opening the inventory selection window.", MessageType.Info);
                    EditorGUILayout.EndVertical();

                    Properties.DrawArray("Fuses");
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Color Settings")))
                {
                    Properties.Draw("UseFuseColors");
                    Properties.Draw("EmissionKeyword");
                    if (Properties.BoolValue("UseFuseColors"))
                    {
                        Properties.Draw("EmissionColorName");
                        Properties.Draw("BaseColorName");
                        Properties.Draw("InsertedFuseColor");
                        Properties.Draw("NoFuseColor");
                    }
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sounds")))
                {
                    Properties.Draw("FuseInsertSound");
                    Properties.Draw("FusesConnectedSound");
                }

                EditorGUILayout.Space(2f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnAllFusesConnected"], new GUIContent("Events")))
                {
                    Properties.Draw("OnAllFusesConnected");
                    Properties.Draw("OnFuseConnected");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}