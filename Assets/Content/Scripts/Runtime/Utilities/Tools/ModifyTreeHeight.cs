using UnityEngine;
using UHFPS.Tools;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UHFPS.Runtime
{
    [ThunderWire.Attributes.Summary("Công cụ chỉnh sửa chiều cao và chiều rộng của cây trên Terrain hàng loạt.")]
    public class ModifyTreeHeight : MonoBehaviour
    {
        [Tooltip("Chỉ chỉnh sửa loại cây cụ thể này (Prefab). Nếu để trống sẽ áp dụng cho tất cả.")]
        public GameObject SpecificTreeFilter;

        [Header("Settings")]
        [Tooltip("Bật/tắt việc thay đổi chiều cao.")]
        public bool ChangeHeight = false;
        [Tooltip("Tỉ lệ chiều cao mới.")]
        public float NewHeightScale = 1f;
        [Tooltip("Bật/tắt việc thay đổi chiều rộng.")]
        public bool ChangeWidth = false;
        [Tooltip("Tỉ lệ chiều rộng mới.")]
        public float NewWidthScale = 1f;

        [Header("Random")]
        [Tooltip("Sử dụng kích thước ngẫu nhiên trong khoảng Min/Max.")]
        public bool UseRandomSize = false;
        [Tooltip("Khoảng kích thước ngẫu nhiên.")]
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
