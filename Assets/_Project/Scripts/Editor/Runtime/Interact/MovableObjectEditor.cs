using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(MovableObject))]
    public class MovableObjectEditor : InspectorEditor<MovableObject>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Movable Object"), Target);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("If you cannot move with a movable object, adjust the position and scale of the green capsule, change the ground layer, and exclude it from the collision mask field.", MessageType.Warning);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                Properties.Draw("AudioSource");
                Properties.Draw("Rigidbody");
                Properties.Draw("ForwardAxis");
                Properties.Draw("DrawGizmos");
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Object Properties")))
                {
                    Properties.Draw("MoveDirection");
                    Properties.Draw("CollisionMask");
                    Properties.Draw("HoldOffset");
                    Properties.Draw("AllowRotation");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("HoldDistance");
                    Properties.Draw("ObjectWeight");
                    Properties.Draw("PlayerRadius");
                    Properties.Draw("PlayerHeight");
                    Properties.Draw("PlayerFeetOffset");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Multipliers")))
                {
                    Properties.Draw("WalkMultiplier");
                    Properties.Draw("LookMultiplier");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sounds")))
                {
                    Properties.Draw("SlideVolume");
                    Properties.Draw("VolumeFadeSpeed");
                }

                EditorGUILayout.Space();

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Use Mouse Limits"), Properties["UseMouseLimits"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties["UseMouseLimits"].boolValue))
                    {
                        Properties.Draw("MouseVerticalLimits");
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}