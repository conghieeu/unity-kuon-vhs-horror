using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

namespace UHFPS.Rendering
{
    public class ScanlinesRGPass : ScriptableRenderPass
    {
        private static readonly int ScanlinesStrength = Shader.PropertyToID("_ScanlinesStrength");
        private static readonly int ScanlinesSharpness = Shader.PropertyToID("_ScanlinesSharpness");
        private static readonly int ScanlinesScroll = Shader.PropertyToID("_ScanlinesScroll");
        private static readonly int ScanlinesFrequency = Shader.PropertyToID("_ScanlinesFrequency");
        private static readonly int GlitchIntensity = Shader.PropertyToID("_GlitchIntensity");
        private static readonly int GlitchFrequency = Shader.PropertyToID("_GlitchFrequency");

        private const string PROFILER_TAG = "Scanlines";
        private readonly Material material;

        public ScanlinesRGPass(RenderPassEvent renderPassEvent, Material material)
        {
            this.renderPassEvent = renderPassEvent;
            this.material = material;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // The following line ensures that the render pass doesn't blit
            // from the back buffer.
            if (resourceData.isActiveTargetBackBuffer || cameraData.isSceneViewCamera)
                return;

            VolumeStack stack = VolumeManager.instance.stack;
            Scanlines scanlinesVolume = stack.GetComponent<Scanlines>();
            if (!scanlinesVolume.IsActive()) return;

            material.SetFloat(ScanlinesStrength, scanlinesVolume.ScanlinesStrength.value);
            material.SetFloat(ScanlinesSharpness, scanlinesVolume.ScanlinesSharpness.value);
            material.SetFloat(ScanlinesScroll, scanlinesVolume.ScanlinesScroll.value);
            material.SetFloat(ScanlinesFrequency, scanlinesVolume.ScanlinesFrequency.value);
            material.SetFloat(GlitchIntensity, scanlinesVolume.GlitchIntensity.value);
            material.SetFloat(GlitchFrequency, scanlinesVolume.GlitchFrequency.value);

            // Blit Pass
            var source = resourceData.activeColorTexture;

            var destinationDesc = renderGraph.GetTextureDesc(source);
            destinationDesc.name = $"CameraColor-{PROFILER_TAG}";
            destinationDesc.clearBuffer = false;

            TextureHandle destination = renderGraph.CreateTexture(destinationDesc);

            RenderGraphUtils.BlitMaterialParameters para = new(source, destination, material, 0);
            renderGraph.AddBlitPass(para, passName: PROFILER_TAG);

            resourceData.cameraColor = destination;
        }
    }
}