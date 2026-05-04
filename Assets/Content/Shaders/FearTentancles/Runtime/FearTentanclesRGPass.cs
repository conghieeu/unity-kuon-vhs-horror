using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

namespace UHFPS.Rendering
{
    public class FearTentanclesRGPass : ScriptableRenderPass
    {
        private static readonly int EffectTime = Shader.PropertyToID("_EffectTime");
        private static readonly int EffectFade = Shader.PropertyToID("_EffectFade");
        private static readonly int TentaclesPosition = Shader.PropertyToID("_TentaclesPosition");
        private static readonly int LayerPosition = Shader.PropertyToID("_LayerPosition");
        private static readonly int VignetteStrength = Shader.PropertyToID("_VignetteStrength");
        private static readonly int TentaclesNum = Shader.PropertyToID("_NumOfTentacles");
        private static readonly int TopLayer = Shader.PropertyToID("_ShowLayer");

        private const string PROFILER_TAG = "FearTentancles";
        private readonly Material material;
        private float effectTime;

        public FearTentanclesRGPass(RenderPassEvent renderPassEvent, Material material)
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
            FearTentancles fearTent = stack.GetComponent<FearTentancles>();

            if (!fearTent.IsActive())
            {
                effectTime = 0f;
                return;
            }

            material.SetFloat(EffectTime, effectTime);
            material.SetFloat(EffectFade, fearTent.EffectFade.value);
            material.SetFloat(TentaclesPosition, fearTent.TentaclesPosition.value);
            material.SetFloat(LayerPosition, fearTent.LayerPosition.value);
            material.SetFloat(VignetteStrength, fearTent.VignetteStrength.value);
            material.SetFloat(TentaclesNum, fearTent.Tentacles.value);
            material.SetInteger(TopLayer, fearTent.TopLayer.value ? 1 : 0);
            effectTime += Time.deltaTime * fearTent.TentaclesSpeed.value;

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