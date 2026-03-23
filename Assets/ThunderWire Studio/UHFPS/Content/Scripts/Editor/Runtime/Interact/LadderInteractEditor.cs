using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(LadderInteract))]
    public class LadderInteractEditor : InspectorEditor<LadderInteract>
    {
        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Ladder Interact"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Ladder Offsets")))
                {
                    Properties.Draw("LadderUpOffset");
                    Properties.Draw("LadderExitOffset");
                    Properties.Draw("LadderArcOffset");
                    Properties.Draw("CenterOffset");
                }

                EditorGUILayout.Space();

                if(EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Use Mouse Limits"), Properties["UseMouseLimits"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties["UseMouseLimits"].boolValue))
                    {
                        Properties.Draw("MouseVerticalLimits");
                        Properties.Draw("MouseHorizontalLimits");
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["LadderPart"], new GUIContent("Ladder Builder")))
                {
                    Properties.Draw("LadderPart");
                    Properties.Draw("VerticalIncrement");

                    EditorGUILayout.Space();
                    if (GUILayout.Button("Build Ladder", GUILayout.Height(25f)))
                    {
                        GenerateLadder();
                    }

                    if (GUILayout.Button("Calculate Bounds", GUILayout.Height(25f)))
                    {
                        GenerateCollider();
                    }

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Gizmos", EditorStyles.boldLabel);
                Properties.Draw("DrawGizmos");
                Properties.Draw("DrawGizmosSteps");
                Properties.Draw("DrawGizmosLabels");
                if (Properties.DrawGetBool("DrawPlayerPreview"))
                {
                    Properties.Draw("DrawPlayerAtEnd");
                    Properties.Draw("PlayerRadius");
                    Properties.Draw("PlayerHeight");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void GenerateLadder()
        {
            float increment = Target.VerticalIncrement;
            int steps = Mathf.RoundToInt(Target.LadderUpOffset.y / increment);

            Transform oldMesh = Target.transform.Find("LadderMesh");
            if (oldMesh != null) DestroyImmediate(oldMesh.gameObject);

            GameObject ladder = new GameObject("LadderMesh");
            ladder.transform.SetParent(Target.transform);
            ladder.transform.localPosition = Vector3.zero;

            float y = 0;
            for (int i = 0; i < steps; i++)
            {
                GameObject part = Instantiate(Target.LadderPart, ladder.transform);
                part.name = "LadderPart_" + i;
                Vector3 pos = part.transform.localPosition;
                pos.y += y; 
                y += increment;
                part.transform.localPosition = pos;
            }
        }

        private void GenerateCollider()
        {
            if (Target.GetComponentsInChildren<Renderer>().Length > 0)
            {
                BoxCollider collider = Target.GetComponent<BoxCollider>();
                if (collider == null) collider = Target.gameObject.AddComponent<BoxCollider>();

                Bounds bounds = CalculateBounds();
                collider.size = bounds.size;
                collider.center = bounds.center - Target.transform.position;
                collider.isTrigger = true;
            }
        }

        private Bounds CalculateBounds()
        {
            Quaternion oldRotation = Target.transform.rotation;
            Target.transform.rotation = Quaternion.Euler(0f, 0f, 0f);

            Bounds bounds = new Bounds(Target.transform.position, Vector3.zero);
            Renderer[] renderers = Target.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            Target.transform.rotation = oldRotation;
            return bounds;
        }
    }
}