using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(SpritesheetAnimation)), CanEditMultipleObjects]
    public class SpritesheetAnimationEditor : InspectorEditor<SpritesheetAnimation>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Spritesheet Animation"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("Spritesheet");
                Properties.Draw("Image");
                Properties.Draw("FrameRate");
                Properties.Draw("PlayOnStart");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
                using (new EditorGUI.DisabledGroupScope(true))
                {
                    Properties.Draw("sprites");
                }

                EditorGUILayout.Space();
                if(GUILayout.Button("Get Spritesheet Slices", GUILayout.Height(25f)))
                {
                    string spritesheetPath = AssetDatabase.GetAssetPath(Target.Spritesheet);
                    Object[] spritesheetSlices = AssetDatabase.LoadAllAssetsAtPath(spritesheetPath);

                    int spritesCount = spritesheetSlices.Length - 1;
                    Target.sprites = new Sprite[spritesCount];

                    for (int i = 0; i < spritesCount; i++)
                    {
                        Target.sprites[i] = spritesheetSlices[i + 1] as Sprite;
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}