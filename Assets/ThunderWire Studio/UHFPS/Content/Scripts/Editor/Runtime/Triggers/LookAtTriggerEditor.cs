using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using UHFPS.Tools;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LookAtTrigger))]
    public class LookAtTriggerEditor : InspectorEditor<LookAtTrigger>
    {
        SerializedProperty viewportX;
        SerializedProperty viewportY;

        public override void OnEnable()
        {
            base.OnEnable();
            viewportX = Properties["ViewportOffset"].FindPropertyRelative("x");
            viewportY = Properties["ViewportOffset"].FindPropertyRelative("y");
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Look At Trigger"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("TriggerType");
                Properties.Draw("CullMask");
                Properties.Draw("ViewportOffset");

                viewportX.floatValue = Mathf.Clamp01(viewportX.floatValue);
                viewportY.floatValue = Mathf.Clamp01(viewportY.floatValue);

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("LookAwayViewport");
                    if (Properties.DrawGetBool("UseDistance"))
                    {
                        Properties.Draw("CallEventOutsideDistance");
                        Properties.Draw("VisualizeDistance");
                        Properties.Draw("TriggerDistance");
                    }
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Viewport Preview")))
                {
                    Rect previewRect = GUILayoutUtility.GetRect(125f, 150f);
                    Rect maskRect = previewRect;

                    previewRect.y = 0f;
                    previewRect.x = (previewRect.width / 2) - (125f / 2);
                    previewRect.width = 125f;

                    GUI.BeginGroup(maskRect);
                    DrawGLViewportPreview(previewRect);
                    GUI.EndGroup();
                }

                EditorGUILayout.Space(2f);
                EditorGUILayout.HelpBox("Define the viewport on which look detection will be performed. The white rectangle represents the screen size and the red rectangle represents the screen size on which the object must be for the event to trigger.", MessageType.Info);

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnLookAt"], new GUIContent("Look Events")))
                {
                    Properties.Draw("OnLookAt");
                    Properties.Draw("OnLookAway");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGLViewportPreview(Rect rect)
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
                        float width = 125f;
                        float height = rect.height - 20;

                        GL.Begin(GL.LINES);
                        {
                            GL.Color(Color.white);

                            GL.Vertex(center - new Vector2(width, 0));
                            GL.Vertex(center + new Vector2(width, 0));

                            GL.Vertex(center - new Vector2(width, 0));
                            GL.Vertex(center - new Vector2(width, height));

                            GL.Vertex(center - new Vector2(width, height));
                            GL.Vertex(center - new Vector2(width, height) + new Vector2(width * 2, 0));

                            GL.Vertex(center - new Vector2(width, height) + new Vector2(width * 2, 0));
                            GL.Vertex(center + new Vector2(width, 0));
                        }
                        GL.End();

                        float x = viewportX.floatValue;
                        float y = viewportY.floatValue;

                        float xOffset = GameTools.Remap(0, 1, width, 0, x);
                        float yOffset = GameTools.Remap(0, 1, height / 2, 0, y);

                        GL.Begin(GL.LINES);
                        {
                            GL.Color(Color.red);

                            GL.Vertex(center - new Vector2(width - xOffset, yOffset));
                            GL.Vertex(center + new Vector2(width - xOffset, -yOffset));

                            GL.Vertex(center - new Vector2(width - xOffset, yOffset));
                            GL.Vertex(center - new Vector2(width - xOffset, height - yOffset));

                            GL.Vertex(center - new Vector2(width - xOffset, height - yOffset));
                            GL.Vertex(center - new Vector2(width - xOffset, height - yOffset) + new Vector2((width - xOffset) * 2, 0));

                            GL.Vertex(center - new Vector2(width - xOffset, height - yOffset) + new Vector2((width - xOffset) * 2, 0));
                            GL.Vertex(center + new Vector2(width - xOffset, -yOffset));
                        }
                        GL.End();
                    }
                    GL.PopMatrix();
                }
                GUI.EndClip();
            }
        }
    }
}