using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Tools;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LockpickInteract))]
    public class LockpickInteractEditor : InspectorEditor<LockpickInteract>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Lockpick Interact"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("LockpickModel");
                EditorGUILayout.Space(2f);

                Properties.Draw("IsDynamicUnlockComponent");
                Properties.Draw("RandomUnlockAngle");
                using (new EditorGUI.DisabledGroupScope(Target.RandomUnlockAngle))
                {
                    Properties.Draw("UnlockAngle");
                }
                EditorGUILayout.Space();

                GUIContent puzzleBaseContent = EditorGUIUtility.TrTextContentWithIcon(" Puzzle Base Settings", "Settings");
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["LockpickRotation"], puzzleBaseContent))
                {
                    Properties.Draw("LockpickRotation");
                    Properties.Draw("LockpickDistance");
                    Properties.Draw("LockpicksText");
                    EditorGUI.indentLevel++;
                    Properties.Draw("ControlsContexts");
                    EditorGUI.indentLevel--;
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Lockpick Settings")))
                {
                    Properties.Draw("BobbyPinItem");
                    Properties.Draw("BobbyPinLimits");
                    Properties.Draw("BobbyPinLifetime");
                    Properties.Draw("BobbyPinUnlockDistance");
                    Properties.Draw("UnbreakableBobbyPin");

                    SerializedProperty limitsMin = Properties["BobbyPinLimits"].FindPropertyRelative("min");
                    SerializedProperty limitsMax = Properties["BobbyPinLimits"].FindPropertyRelative("max");
                    limitsMin.floatValue = Mathf.Clamp(limitsMin.floatValue, -90, 90);
                    limitsMax.floatValue = Mathf.Clamp(limitsMax.floatValue, -90, 90);
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Keyhole Settings")))
                {
                    Properties.Draw("KeyholeMaxTestRange");
                    Properties.Draw("KeyholeUnlockTarget");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Lockpick Preview")))
                {
                    Rect previewRect = EditorGUILayout.GetControlRect(false, 100);
                    DrawGLLockpickPreview(previewRect);
                }

                if (!Properties.BoolValue("IsDynamicUnlockComponent"))
                {
                    EditorGUILayout.Space();
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnUnlock"], new GUIContent("Lockpick Events")))
                    {
                        Properties.Draw("OnUnlock");
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private Vector2[] GetArcPoints(Vector2 center, float min, float max)
        {
            float radius = 80f;
            float increment = Mathf.PI;

            List<Vector2> points = new();
            for (float theta = min; theta < max; theta += increment)
            {
                Vector2 arcPoint = center;
                arcPoint.x -= radius * Mathf.Cos((theta + 90) * Mathf.Deg2Rad);
                arcPoint.y -= radius * Mathf.Sin((theta + 90) * Mathf.Deg2Rad);
                points.Add(arcPoint);
            }

            Vector2 endPoint = center;
            endPoint.x -= radius * Mathf.Cos((max + 90) * Mathf.Deg2Rad);
            endPoint.y -= radius * Mathf.Sin((max + 90) * Mathf.Deg2Rad);
            points.Add(endPoint);

            return points.ToArray();
        }

        private void DrawGLLockpickPreview(Rect rect)
        {
            Material _uiMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
            if (Event.current.type == EventType.Repaint)
            {
                GUI.BeginClip(rect);
                {
                    GL.PushMatrix();
                    GL.LoadPixelMatrix();
                    _uiMaterial.SetPass(0);
                    {
                        Vector2 center = new Vector2(rect.width / 2, rect.height - 10);
                        Vector2[] arcPoints = GetArcPoints(center, -90, 90);

                        GL.Begin(GL.TRIANGLES);
                        {
                            // draw base arc
                            GL.Color(Color.white.Alpha(0.25f));
                            for (int i = 1; i < arcPoints.Length; i++)
                            {
                                GL.Vertex(center);
                                GL.Vertex(arcPoints[i - 1]);
                                GL.Vertex(arcPoints[i]);
                            }

                            // draw lockpick limits
                            GL.Color(Color.yellow.Alpha(0.1f));
                            Vector2[] limitPoints = GetArcPoints(center, Target.BobbyPinLimits.RealMin, Target.BobbyPinLimits.RealMax);
                            for (int i = 1; i < limitPoints.Length; i++)
                            {
                                GL.Vertex(center);
                                GL.Vertex(limitPoints[i - 1]);
                                GL.Vertex(limitPoints[i]);
                            }

                            if (!Application.isPlaying)
                            {
                                // draw test range
                                GL.Color(Color.green.Alpha(0.3f));
                                Vector2[] testRangePoints = GetArcPoints(center, -Target.KeyholeMaxTestRange, Target.KeyholeMaxTestRange);
                                for (int i = 1; i < testRangePoints.Length; i++)
                                {
                                    GL.Vertex(center);
                                    GL.Vertex(testRangePoints[i - 1]);
                                    GL.Vertex(testRangePoints[i]);
                                }
                            }
                        }
                        GL.End();

                        // draw lockpick unlock angle
                        if (Application.isPlaying && Target.UnlockAngle != 0 || !Target.RandomUnlockAngle)
                        {
                            GL.Begin(GL.LINES);
                            {
                                Vector2 unlockAngle = center;
                                unlockAngle.x -= 80f * Mathf.Cos((Target.UnlockAngle + 90) * Mathf.Deg2Rad);
                                unlockAngle.y -= 80f * Mathf.Sin((Target.UnlockAngle + 90) * Mathf.Deg2Rad);

                                GL.Color(Color.red);
                                GL.Vertex(center);
                                GL.Vertex(unlockAngle);
                            }
                            GL.End();
                        }
                    }
                    GL.PopMatrix();
                }
                GUI.EndClip();
            }
        }
    }
}