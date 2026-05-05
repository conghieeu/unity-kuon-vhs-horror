using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(MazePuzzle))]
    public class MazePuzzleEditor : PuzzleBlendEditor<MazePuzzle>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Maze Puzzle"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.Draw("MazeTransform");
                Properties.Draw("CullLayers");
                Properties.Draw("InteractLayer");

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["MazeAnimator"], new GUIContent("Animation Settings")))
                {
                    Properties.Draw("MazeAnimator");
                    Properties.Draw("OpenDrawerState");
                    Properties.Draw("GrabBallState");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["PutBallTrigger"], new GUIContent("Reference Settings")))
                {
                    Properties.Draw("PutBallTrigger");
                    Properties.Draw("GrabBallTrigger");
                    Properties.Draw("GrabBallAnim");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["BallItem"], new GUIContent("Ball Settings")))
                {
                    Properties.Draw("BallItem");
                    Properties.Draw("BallObject");
                    Properties.Draw("BallStart");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["MazeLiftOffset"], new GUIContent("Offset Settings")))
                {
                    Properties.Draw("MazeLiftOffset");
                    Properties.Draw("RotationOffset");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["LiftDuration"], new GUIContent("Maze Settings")))
                {
                    Properties.Draw("LiftDuration");
                    Properties.Draw("ReturnDuration");
                    Properties.Draw("RotateSpeed");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["GrabBallPosition"], new GUIContent("Grab Ball Settings")))
                {
                    Properties.Draw("GrabBallPosition");
                    Properties.Draw("GrabBallRotation");
                    Properties.Draw("GrabBallDuration");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["VerticalAxis"], new GUIContent("Limit Settings")))
                {
                    Properties.Draw("VerticalAxis");
                    Properties.Draw("HorizontalAxis");

                    Properties.Draw("VerticalLimits");
                    Properties.Draw("HorizontalLimits");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnBallEnterWrongHole"], new GUIContent("Events")))
                {
                    Properties.Draw("OnBallEnterWrongHole");
                    Properties.Draw("OnBallEnterFinishHole");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}