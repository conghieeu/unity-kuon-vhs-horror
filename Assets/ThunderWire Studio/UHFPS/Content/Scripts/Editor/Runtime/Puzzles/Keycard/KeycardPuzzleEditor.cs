using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(KeycardPuzzle))]
    public class KeycardPuzzleEditor : PuzzleSimpleEditor<KeycardPuzzle>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Keycard Puzzle"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                if (Target.SingleKeycard) Properties.Draw("KeycardItem");
                else Properties.Draw("UsableKeycards");
                EditorGUILayout.Space();

                using(new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    EditorGUILayout.BeginVertical(GUI.skin.box);
                    Properties.Draw("UseInteract");
                    EditorGUILayout.HelpBox("Automatically detects if there is a card in the inventory and uses it instead of opening the inventory selection window.", MessageType.Info);
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.Space(2f);
                    Properties.Draw("SingleKeycard");
                    Properties.Draw("RemoveKeycardAfterUse");
                    Properties.Draw("AccessUpdateTime");
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Keycard Level"), Properties["CheckKeycardLevel"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("CheckKeycardLevel")))
                    {
                        Properties.Draw("RequiredLevel");
                        EditorGUILayout.HelpBox("Granted or denied state will be determined by the level of the card. Add custom item data with the name \"level: [your level]\" to keycard item!", MessageType.Info);
                    }
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Keycard Light"), Properties["UseLight"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("UseLight")))
                    {
                        Properties.Draw("KeycardLight");
                        Properties.Draw("GrantedColor");
                        Properties.Draw("DeniedColor");
                    }
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Keycard Emission"), Properties["UseEmission"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("UseEmission")))
                    {
                        Properties.Draw("KeycardRenderer");
                        Properties.Draw("EmissionShaderKey");
                        Properties.Draw("GrantedShaderKey");
                    }
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sounds")))
                {
                    Properties.Draw("AccessGrantedSound");
                    Properties.Draw("AccessDeniedSound");
                }

                EditorGUILayout.Space(2f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnAccessGranted"], new GUIContent("Events")))
                {
                    Properties.Draw("OnAccessGranted");
                    Properties.Draw("OnAccessDenied");
                    Properties.Draw("OnWrongItem");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}