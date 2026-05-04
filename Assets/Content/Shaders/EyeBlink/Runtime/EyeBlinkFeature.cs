using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace UHFPS.Rendering
{
    public class EyeBlinkFeature : EffectFeature
    {
        public override string Name => "Eye Blink";

        public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material EffectMaterial;

        public override void OnCreate()
        {
            RenderPass = new EyeBlinkRGPass(RenderPassEvent, EffectMaterial);
        }
    }
}