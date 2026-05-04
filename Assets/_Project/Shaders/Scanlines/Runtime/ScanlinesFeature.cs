using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace UHFPS.Rendering
{
    public class ScanlinesFeature : EffectFeature
    {
        public override string Name => "Scanlines";

        public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material EffectMaterial;

        public override void OnCreate()
        {
            RenderPass = new ScanlinesRGPass(RenderPassEvent, EffectMaterial);
        }
    }
}