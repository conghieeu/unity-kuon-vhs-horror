using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(PlayerPresenceManager))]
    public class PlayerPresenceManagerEditor : InspectorEditor<PlayerPresenceManager>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorDrawing.DrawInspectorHeader(new GUIContent("Player Presence Manager"), target);
            EditorGUILayout.Space();

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Player Presence"), 18f, true))
            {
                Properties.Draw("PlayerUnlockType");
                Properties.Draw("Player");
                EditorGUILayout.Space();

                if (Target.PlayerUnlockType == PlayerPresenceManager.UnlockType.Automatically)
                    EditorGUILayout.HelpBox("Player will be unlocked at the start or after the game state is loaded.", MessageType.Info);
                else if(Target.PlayerUnlockType == PlayerPresenceManager.UnlockType.Manually)
                    EditorGUILayout.HelpBox("Player will be unlocked after calling the UnlockPlayer() function.", MessageType.Info);
            }

            EditorGUILayout.Space();

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Start Fade Settings"), 18f, true))
            {
                Properties.Draw("WaitFadeOutTime");
                Properties.Draw("FadeOutSpeed");
            }

            if (Target.PlayerUnlockType == PlayerPresenceManager.UnlockType.Manually)
            {
                EditorGUILayout.Space();

                if (GUILayout.Button("Unlock Player", GUILayout.Height(25f)))
                {
                    Target.UnlockPlayer();
                }

                if (GUILayout.Button("Fade Out Background", GUILayout.Height(25f)))
                {
                    Target.FadeBackground(true, null);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}