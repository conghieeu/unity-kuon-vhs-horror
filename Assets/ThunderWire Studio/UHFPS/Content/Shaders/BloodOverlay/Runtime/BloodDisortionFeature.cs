using UnityEngine.Rendering.Universal;
using UnityEngine;

namespace UHFPS.Rendering
{
    public class BloodDisortionFeature : EffectFeature
    {
        public override string Name => "Blood Disortion";

        public RenderPassEvent RenderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material EffectMaterial;

        public override void OnCreate()
        {
            RenderPass = new BloodDisortionRGPass(RenderPassEvent, EffectMaterial);
        }
    }
}