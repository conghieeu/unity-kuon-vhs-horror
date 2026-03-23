using UnityEngine;
using UHFPS.Tools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UHFPS.Runtime
{
    public class ModifyTreeHeight : MonoBehaviour
    {
        public GameObject SpecificTreeFilter;

        [Header("Settings")]
        public bool ChangeHeight = false;
        public float NewHeightScale = 1f;
        public bool ChangeWidth = false;
        public float NewWidthScale = 1f;

        [Header("Random")]
        public bool UseRandomSize = false;
        public MinMax RandomSize = new(1f, 2f);

        public void ModifyTerrain()
        {
            if (!gameObject.TryGetComponent(out Terrain terrain))
                throw new MissingReferenceException("Missing a reference to the terrain! Place this component to the Terrain object.");

            TerrainData terrainData = terrain.terrainData;
            TreeInstance[] trees = terrainData.treeInstances;
            TreePrototype[] prototypes = terrainData.treePrototypes;

            for (int i = 0; i < trees.Length; i++)
            {
                TreeInstance tree = trees[i];

                if (SpecificTreeFilter != null)
                {
                    int prototypeIndex = tree.prototypeIndex;
                    if (prototypeIndex < 0 || prototypeIndex >= prototypes.Length)
                        continue;

                    if (prototypes[prototypeIndex].prefab != SpecificTreeFilter)
                        continue;
                }

                if (!UseRandomSize)
                {
                    if (ChangeHeight) tree.heightScale = NewHeightScale;
                    if (ChangeWidth) tree.widthScale = NewWidthScale;
                }
                else
                {
                    float size = RandomSize.Random();
                    if (ChangeHeight) tree.heightScale = size;
                    if (ChangeWidth) tree.widthScale = size;
                }

                trees[i] = tree;
            }

            terrainData.treeInstances = trees;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ModifyTreeHeight))]
    public class ModifyTreeHeightEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            if (GUILayout.Button("Modify Terrain", GUILayout.Height(25f)))
            {
                (target as ModifyTreeHeight).ModifyTerrain();
            }
        }
    }
#endif
}
