using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

namespace UHFPS.Rendering
{
    public class BloodDisortionRGPass : ScriptableRenderPass
    {
        private static readonly int BlendColor = Shader.PropertyToID("_BlendColor");
        private static readonly int OverlayColor = Shader.PropertyToID("_OverlayColor");
        private static readonly int BlendTexture = Shader.PropertyToID("_BlendTex");
        private static readonly int BumpTexture = Shader.PropertyToID("_BumpMap");
        private static readonly int BloodAmount = Shader.PropertyToID("_BloodAmount");
        private static readonly int BlendAmount = Shader.PropertyToID("_BlendAmount");
        private static readonly int EdgeSharpness = Shader.PropertyToID("_EdgeSharpness");
        private static readonly int Distortion = Shader.PropertyToID("_Distortion");

        private const string PROFILER_TAG = "BloodDisortion";
        private readonly Material material;

        public BloodDisortionRGPass(RenderPassEvent renderPassEvent, Material material)
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
            BloodDisortion bloodDisortion = stack.GetComponent<BloodDisortion>();
            if (bloodDisortion == null || !bloodDisortion.IsActive()) return;

            material.SetColor(BlendColor, bloodDisortion.BlendColor.value);
            material.SetColor(OverlayColor, bloodDisortion.OverlayColor.value);

            material.SetTexture(BlendTexture, bloodDisortion.BlendTexture.value);
            material.SetTexture(BlendTexture, bloodDisortion.BlendTexture.value);
            material.SetTexture(BumpTexture, bloodDisortion.BumpTexture.value);
            material.SetFloat(EdgeSharpness, bloodDisortion.EdgeSharpness.value);
            material.SetFloat(Distortion, bloodDisortion.Distortion.value);

            float minBlood = bloodDisortion.MinBloodAmount.value;
            float maxBlood = bloodDisortion.MaxBloodAmount.value;

            float bloodAmount = bloodDisortion.BloodAmount.value;
            material.SetFloat(BloodAmount, bloodAmount);

            float blendAmount = Mathf.Clamp01(bloodAmount * (maxBlood - minBlood) + minBlood);
            material.SetFloat(BlendAmount, blendAmount);

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