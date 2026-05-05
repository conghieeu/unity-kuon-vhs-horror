using System;
using UnityEngine;
using ThunderWire.Attributes;

namespace UHFPS.Runtime
{
    /// <summary>
    /// Cấu trúc tham chiếu đến một vật liệu cụ thể của Renderer thông qua chỉ số.
    /// </summary>
    [Serializable]
    public struct RendererMaterial
    {
        [Tooltip("Renderer mục tiêu.")]
        public Renderer meshRenderer;
        [Tooltip("Vật liệu mục tiêu.")]
        public Material material;
        [Tooltip("Chỉ số vật liệu trong mảng materials của Renderer.")]
        public int materialIndex;

        public bool IsAssigned => meshRenderer != null && material != null;

        public Material ClonedMaterial
        {
            get => material = meshRenderer.materials[materialIndex];
            set
            {
                Material[] materials = meshRenderer.materials;
                materials[materialIndex] = value;
                meshRenderer.materials = materials;
            }
        }
    }
}