using UnityEngine;
using UnityEngine.Rendering.Universal;
using System;

using ThunderWire.Attributes;

namespace UHFPS.Rendering
{
    [Serializable]
    [Summary("Lớp trừu tượng cho các tính năng hiệu ứng hình ảnh (Render Feature).")]
    public abstract class EffectFeature
    {
        [Tooltip("Render Pass thực hiện hiệu ứng.")]
        public ScriptableRenderPass RenderPass;
        [Tooltip("Bật/tắt hiệu ứng.")]
        public bool Enabled = true;

        public abstract string Name { get; }
        public abstract void OnCreate();

        public virtual ScriptableRenderPass OnGetRenderPass()
        {
            if (RenderPass == null)
                return null;

            RenderPass.ConfigureInput(ScriptableRenderPassInput.Color);
            return RenderPass;
        }
    }
}