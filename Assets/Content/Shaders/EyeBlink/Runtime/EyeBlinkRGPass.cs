using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

namespace UHFPS.Rendering
{
    public class EyeBlinkRGPass : ScriptableRenderPass
    {
        private static readonly int Blink = Shader.PropertyToID("_Blink");
        private static readonly int VignetteOuterRing = Shader.PropertyToID("_VignetteOuterRing");
        private static readonly int VignetteInnerRing = Shader.PropertyToID("_VignetteInnerRing");
        private static readonly int VignetteAspectRatio = Shader.PropertyToID("_VignetteAspectRatio");

        private const string PROFILER_TAG = "EyeBlink";
        private readonly Material material;

        public EyeBlinkRGPass(RenderPassEvent renderPassEvent, Material material)
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
            EyeBlink eyeBlinkVolume = stack.GetComponent<EyeBlink>();
            if (!eyeBlinkVolume.IsActive()) return;

            material.SetFloat(Blink, eyeBlinkVolume.Blink.value);
            material.SetFloat(VignetteOuterRing, eyeBlinkVolume.VignetteOuterRing.value);
            material.SetFloat(VignetteInnerRing, eyeBlinkVolume.VignetteInnerRing.value);
            material.SetFloat(VignetteAspectRatio, eyeBlinkVolume.VignetteAspectRatio.value);

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