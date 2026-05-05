using UnityEngine;
using UnityEditor;
using UHFPS.Runtime;
using ThunderWire.Editors;

namespace UHFPS.Editors
{
    [CustomEditor(typeof(ProceduralCable))]
    public class ProceduralCableEditor : Editor
    {
        private ProceduralCable proceduralCable;
        private bool infoFoldout = false;

        private void OnEnable()
        {
            proceduralCable = (ProceduralCable)target;
            Undo.undoRedoPerformed += () => { proceduralCable.RegenerateCable(); };
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorDrawing.DrawInspectorHeader(new GUIContent("Procedural Cable"), target);
            EditorGUILayout.Space();

            using (new EditorDrawing.BorderBoxScope(new GUIContent("Cable Settings")))
            {
                Material cableMaterial = (Material)EditorGUILayout.ObjectField("Cable Material", proceduralCable.settings.CableMaterial, typeof(Material), false);
                float newCurvature = EditorGUILayout.FloatField("Curvature", proceduralCable.settings.Curvature);
                int newStep = EditorGUILayout.IntField("Steps", proceduralCable.settings.Steps);
                int newRadiusStep = EditorGUILayout.IntField("Radius Step", proceduralCable.settings.RadiusStep);
                float newRadius = EditorGUILayout.FloatField("Radius", proceduralCable.settings.Radius);
                float colliderRadius = EditorGUILayout.FloatField("Collider Radius", proceduralCable.settings.ColliderRadius);
                Vector2 newUvMultiply = EditorGUILayout.Vector2Field("UV Multiply", proceduralCable.settings.uvMultiply);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);
                bool generateCollider = EditorGUILayout.Toggle("Generate Collider", proceduralCable.settings.GenerateCollider);
                bool manual = EditorGUILayout.Toggle("Manual Generation", proceduralCable.manualGeneration);
                bool gizmos = EditorGUILayout.Toggle("Draw Gizmos", proceduralCable.drawGizmos);
                bool cableGizmos = EditorGUILayout.Toggle("Draw Cable Gizmos", proceduralCable.drawCableGizmos);

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(proceduralCable, "Parameters Change");

                    proceduralCable.settings.CableMaterial = cableMaterial;
                    proceduralCable.settings.Curvature = newCurvature;

                    newStep = newStep < 1 ? 1 : newStep;
                    proceduralCable.settings.Steps = newStep;

                    newRadiusStep = newRadiusStep < 3 ? 3 : newRadiusStep;
                    proceduralCable.settings.RadiusStep = newRadiusStep;

                    newRadius = newRadius < 0 ? 0 : newRadius;
                    proceduralCable.settings.Radius = newRadius;

                    colliderRadius = colliderRadius < 1 ? 1 : colliderRadius;
                    proceduralCable.settings.ColliderRadius = colliderRadius;

                    proceduralCable.settings.GenerateCollider = generateCollider;
                    proceduralCable.settings.uvMultiply = newUvMultiply;
                    proceduralCable.manualGeneration = manual;
                    proceduralCable.drawGizmos = gizmos;
                    proceduralCable.drawCableGizmos = cableGizmos;

                    if (!manual) proceduralCable.RegenerateCable();

                    EditorUtility.SetDirty(proceduralCable);
                }
            }

            EditorGUILayout.Space();

            if (!proceduralCable.cableGenerated)
            {
                if (GUILayout.Button("Generate Cable", GUILayout.Height(25)))
                {
                    foreach (Transform child in proceduralCable.transform)
                    {
                        DestroyImmediate(child.gameObject);
                    }

                    proceduralCable.GenerateCable(new Vector3(-5, 0, 0), new Vector3(5, 0, 0));
                }
            }
            else
            {
                if (GUILayout.Button("Refresh Cable", GUILayout.Height(25)))
                {
                    proceduralCable.RegenerateCable();
                }

                EditorGUILayout.Space();
                if (infoFoldout = EditorGUILayout.Foldout(infoFoldout, "Info"))
                {
                    using (new EditorGUI.DisabledGroupScope(true))
                    {
                        EditorGUILayout.Vector3Field("Start Position", proceduralCable.CableStart);
                        EditorGUILayout.Vector3Field("End Position", proceduralCable.CableEnd);
                    }
                }

                if (!proceduralCable._startTransform || !proceduralCable._endTransform)
                {
                    proceduralCable.cableGenerated = false;
                    EditorUtility.SetDirty(proceduralCable);
                }
            }
        }
    }
}