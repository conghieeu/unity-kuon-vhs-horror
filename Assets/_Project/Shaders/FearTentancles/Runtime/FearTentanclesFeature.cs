using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace UHFPS.Rendering
{
    public class FearTentanclesFeature : EffectFeature
    {
        public override string Name => "Fear Tentancles";

        public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material EffectMaterial;

        public override void OnCreate()
        {
            RenderPass = new FearTentanclesRGPass(RenderPassEvent, EffectMaterial);
        }
    }
}