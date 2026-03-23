using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace UHFPS.Rendering
{
    public class RaindropFeature : EffectFeature
    {
        public override string Name => "Raindrop";

        public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material EffectMaterial;

        public override void OnCreate()
        {
            RenderPass = new RaindropRGPass(RenderPassEvent, EffectMaterial);
        }
    }
}