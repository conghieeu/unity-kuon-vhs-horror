using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

namespace UHFPS.Rendering
{
    public class DualKawaseBlurRGPass : ScriptableRenderPass
    {
        internal struct Level
        {
            internal TextureHandle down;
            internal TextureHandle up;
        }

        private class PassData
        {
            public int iteration;
            public Material material;
            public TextureHandle source;
            public TextureHandle destination;
            public Level[] pyramid = new Level[k_MaxPyramidSize];
        }

        private const string PROFILER_TAG = "DualKawaseBlur";
        private const int k_MaxPyramidSize = 16;

        private readonly int BlurOffset = Shader.PropertyToID("_Offset");
        private readonly Material material;

        public DualKawaseBlurRGPass(RenderPassEvent renderPassEvent, Material material)
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
            DualKawaseBlur blurVolume = stack.GetComponent<DualKawaseBlur>();
            if (blurVolume == null || !blurVolume.IsActive()) return;

            Camera camera = cameraData.camera;
            int tw = (int)(camera.pixelWidth / blurVolume.RTDownScaling.value);
            int th = (int)(camera.pixelHeight / blurVolume.RTDownScaling.value);

            material.SetFloat(BlurOffset, Mathf.Sqrt(blurVolume.BlurRadius.value));

            using (var builder = renderGraph.AddUnsafePass<PassData>(PROFILER_TAG, out var passData))
            {
                passData.source = resourceData.activeColorTexture;
                passData.iteration = blurVolume.Iteration.value;
                passData.material = material;

                TextureDesc descriptor = renderGraph.GetTextureDesc(passData.source);
                descriptor.msaaSamples = MSAASamples.None;
                descriptor.depthBufferBits = DepthBits.None;
                descriptor.clearBuffer = false;

                descriptor.name = "CameraColor-FinalKawaseBlur";
                TextureHandle finalColor = renderGraph.CreateTexture(descriptor);
                passData.destination = finalColor;

                // Prepare the pyramid
                for (int i = 0; i < blurVolume.Iteration.value; i++)
                {
                    descriptor.width = tw;
                    descriptor.height = th;

                    descriptor.name = "_BlurMipDown" + i;
                    TextureHandle mipDown = renderGraph.CreateTexture(descriptor);
                    builder.UseTexture(mipDown, AccessFlags.ReadWrite);
                    passData.pyramid[i].down = mipDown;

                    descriptor.name = "_BlurMipUp" + i;
                    TextureHandle mipUp = renderGraph.CreateTexture(descriptor);
                    builder.UseTexture(mipUp, AccessFlags.ReadWrite);
                    passData.pyramid[i].up = mipUp;

                    tw = Mathf.Max(tw / 2, 1);
                    th = Mathf.Max(th / 2, 1);
                }

                // We declare the src texture as an input and dest as output
                builder.UseTexture(passData.source, AccessFlags.Read);
                builder.UseTexture(passData.destination, AccessFlags.Write);

                // Sets the render function.
                builder.AllowPassCulling(false);
                builder.SetRenderFunc((PassData passData, UnsafeGraphContext context) => ExecutePass(passData, context));
            }
        }

        static void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

            // Downsample
            TextureHandle lastDown = data.source;
            for (int i = 0; i < data.iteration; i++)
            {
                TextureHandle mipDown = data.pyramid[i].down;

                // Blit: source = lastDown, destination = mipDown
                Blitter.BlitCameraTexture(cmd, lastDown, mipDown, data.material, 0);
                lastDown = mipDown;
            }

            // Upsample
            TextureHandle lastUp = data.pyramid[data.iteration - 1].down;
            for (int i = data.iteration - 2; i >= 0; i--)
            {
                TextureHandle mipUp = data.pyramid[i].up;

                // Blit: source = lastUp, destination = mipUp
                Blitter.BlitCameraTexture(cmd, lastUp, mipUp, data.material, 1);
                lastUp = mipUp;
            }

            // Render blurred texture in blend pass
            // Blit: source = lastUp, destination = camera color
            Blitter.BlitCameraTexture(cmd, lastUp, data.source, data.material, 1);
        }
    }
}