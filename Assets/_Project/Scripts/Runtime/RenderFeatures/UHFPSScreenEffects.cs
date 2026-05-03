using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using ThunderWire.Attributes;

namespace UHFPS.Rendering
{
    [Docs("https://docs.twgamesdev.com/uhfps/guides/urp-specific#uhfps-screen-effects")]
    public class UHFPSScreenEffects : ScriptableRendererFeature
    {
        [SerializeReference]
        public List<EffectFeature> Features = new()
        {
            new ScanlinesFeature(),
            new BloodDisortionFeature(),
            new EyeBlinkFeature(),
            new FearTentanclesFeature(),
            new RaindropFeature()
        };

        public override void Create()
        {
            Features.ForEach(feature => feature.OnCreate());
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            foreach (var feature in Features)
            {
                ScriptableRenderPass pass = feature.OnGetRenderPass();
                if (pass != null && feature.Enabled) renderer.EnqueuePass(pass);
            }
        }
    }
}