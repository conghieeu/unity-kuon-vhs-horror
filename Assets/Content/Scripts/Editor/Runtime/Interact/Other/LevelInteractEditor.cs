using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LevelInteract))]
    public class LevelInteractEditor : InspectorEditor<LevelInteract>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            LevelInteract.LevelTypeEnum levelType = (LevelInteract.LevelTypeEnum)Properties["LevelType"].enumValueIndex;

            EditorDrawing.DrawInspectorHeader(new GUIContent("Level Interact"), target);
            EditorGUILayout.Space();

            Properties.Draw("TriggerType");
            EditorGUILayout.Space();

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Next Level"), 18f, true))
            {
                Properties.Draw("LevelType");
                Properties.Draw("NextLevelName");
                EditorGUILayout.Space();

                if(levelType == LevelInteract.LevelTypeEnum.NextLevel)
                    EditorGUILayout.HelpBox("The current world state will be saved and the player data will be saved and transferred to the next level.", MessageType.Info);
                else if (levelType == LevelInteract.LevelTypeEnum.WorldState)
                    EditorGUILayout.HelpBox("The current world state will be saved, the world state of the next level will be loaded and the player data will be transferred. (Previous Scene Persistency must be enabled!)", MessageType.Info);
                else if (levelType == LevelInteract.LevelTypeEnum.PlayerData)
                    EditorGUILayout.HelpBox("Only the player data will be saved and transferred to the next level.", MessageType.Info);

            }

            EditorGUILayout.Space();

            Properties["CustomTransform"].boolValue = EditorDrawing.BeginToggleBorderLayout(new GUIContent("Custom Transform"), Properties["CustomTransform"].boolValue);
            using (new EditorGUI.DisabledGroupScope(!Properties["CustomTransform"].boolValue))
            {
                Properties.Draw("TargetTransform");
                Properties.Draw("LookUpDown");
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The player position and rotation will be replaced by custom position and rotation specified by the target transform.", MessageType.Info);
            }
            EditorDrawing.EndBorderHeaderLayout();

            serializedObject.ApplyModifiedProperties();
        }
    }
}