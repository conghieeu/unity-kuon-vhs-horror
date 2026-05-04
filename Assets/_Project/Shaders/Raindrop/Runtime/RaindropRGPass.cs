using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

namespace UHFPS.Rendering
{
    public class RaindropRGPass : ScriptableRenderPass
    {
        private static readonly int Raining = Shader.PropertyToID("_Raining");
        private static readonly int DropletsMask = Shader.PropertyToID("_DropletsMask");
        private static readonly int Tiling = Shader.PropertyToID("_Tiling");
        private static readonly int Distortion = Shader.PropertyToID("_Distortion");
        private static readonly int GlobalRotation = Shader.PropertyToID("_GlobalRotation");
        private static readonly int DropletsGravity = Shader.PropertyToID("_DropletsGravity");
        private static readonly int DropletsSpeed = Shader.PropertyToID("_DropletsSpeed");
        private static readonly int DropletsStrength = Shader.PropertyToID("_DropletsStrength");

        private const string PROFILER_TAG = "RaindropFX";
        private readonly Material material;

        public RaindropRGPass(RenderPassEvent renderPassEvent, Material material)
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
            Raindrop raindropVolume = stack.GetComponent<Raindrop>();
            if (!raindropVolume.IsActive()) return;

            Vector2 tiling = raindropVolume.Tiling.value;
            float tilingScale = raindropVolume.TilingScale.value;
            tiling *= tilingScale;

            material.SetFloat(Raining, raindropVolume.Raining.value);
            material.SetTexture(DropletsMask, raindropVolume.DropletsMask.value);
            material.SetVector(Tiling, tiling);
            material.SetFloat(Distortion, raindropVolume.Distortion.value);
            material.SetFloat(GlobalRotation, raindropVolume.GlobalRotation.value);
            material.SetFloat(DropletsGravity, raindropVolume.DropletsGravity.value);
            material.SetFloat(DropletsSpeed, raindropVolume.DropletsSpeed.value);
            material.SetFloat(DropletsStrength, raindropVolume.DropletsStrength.value);

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