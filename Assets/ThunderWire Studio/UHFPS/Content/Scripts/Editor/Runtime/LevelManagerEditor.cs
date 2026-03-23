using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : InspectorEditor<LevelManager>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Level Manager"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorDrawing.DrawList(Properties["LevelInfos"], new GUIContent("Level Infos"));
                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("UI")))
                {
                    Properties.Draw("Title");
                    Properties.Draw("Description");
                    Properties.Draw("Background");
                    Properties.Draw("FadingBackground");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("LoadPriority");
                    if (GUILayout.Button("Loading Priority Documentation"))
                    {
                        Application.OpenURL("https://docs.unity3d.com/ScriptReference/Application-backgroundLoadingPriority.html");
                    }

                    EditorGUILayout.Space();
                    Properties.Draw("FadeSpeed");
                    Properties.Draw("SwitchManually");
                    Properties.Draw("FadeBackground");
                    Properties.Draw("Debugging");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Switch Panels"), Properties["SwitchPanels"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("SwitchPanels")))
                    {
                        Properties.Draw("SwitchFadeSpeed");
                        Properties.Draw("CurrentPanel");
                        Properties.Draw("NewPanel");
                    }
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Events")))
                {
                    Properties.Draw("OnProgressUpdate");
                    Properties.Draw("OnLoadingDone");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}