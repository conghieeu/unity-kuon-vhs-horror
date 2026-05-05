using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Experimental.Rendering;

namespace UHFPS.Runtime.Rendering
{
    public class ScreenshotFeature : ScriptableRendererFeature
    {
        private class AsyncReadbackPassData
        {
            public TextureHandle source;
            public TextureHandle destination;
            public string filePath;
            public int width;
            public int height;
        }

        public RenderPassEvent RenderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        public Vector2Int OutputImageSize = new(640, 360);

        public static ScreenshotFeature Instance { get; private set; }
        public ScreenshotPass Pass => scriptablePass;

        private ScreenshotPass scriptablePass;

        public override void Create()
        {
            // Only create once
            if (Instance != null)
                return;

            scriptablePass = new ScreenshotPass(RenderPassEvent, OutputImageSize);
            Instance = this;
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (scriptablePass == null)
                return;

            renderer.EnqueuePass(scriptablePass);
        }

        /// <summary>
        /// Call this from your game code when you want to trigger a screenshot save.
        /// </summary>
        public void SaveScreenshotToDisk(string path)
        {
            if (scriptablePass == null)
            {
                Debug.LogWarning("[ScreenshotFeature] No scriptablePass available!");
                return;
            }

            // Let the pass know we should schedule an async readback pass
            scriptablePass.TriggerAsyncScreenshot(path);
        }

        // The main pass
        public class ScreenshotPass : ScriptableRenderPass
        {
            private const string PROFILER_TAG = "ScreenshotPass Blit";

            private readonly Vector2Int renderTextureSize;

            private bool scheduleReadback = false;
            private string screenshotPath = null;

            public ScreenshotPass(RenderPassEvent renderPassEvent, Vector2Int imageSize)
            {
                this.renderPassEvent = renderPassEvent;
                renderTextureSize = imageSize;
            }

            public void TriggerAsyncScreenshot(string outputPath)
            {
                screenshotPath = outputPath;
                scheduleReadback = true;
            }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                if (resourceData.isActiveTargetBackBuffer)
                    return;

                if (scheduleReadback && !string.IsNullOrEmpty(screenshotPath))
                {
                    using (var builder = renderGraph.AddUnsafePass<AsyncReadbackPassData>(PROFILER_TAG, out var passData))
                    {
                        passData.source = resourceData.activeColorTexture;

                        passData.filePath = screenshotPath;
                        passData.width = renderTextureSize.x;
                        passData.height = renderTextureSize.y;

                        // Create destination TextureDescriptor
                        TextureDesc screenshotDesc = renderGraph.GetTextureDesc(passData.source);
                        screenshotDesc.format = GraphicsFormat.R8G8B8A8_SRGB;
                        screenshotDesc.width = renderTextureSize.x;
                        screenshotDesc.height = renderTextureSize.y;
                        screenshotDesc.clearBuffer = false;
                        screenshotDesc.enableRandomWrite = false;
                        screenshotDesc.name = "ScreenshotTarget";

                        // Create destination texture
                        TextureHandle destination = renderGraph.CreateTexture(screenshotDesc);
                        passData.destination = destination;

                        // We declare the src texture as an input and dest as output
                        builder.UseTexture(passData.source, AccessFlags.Read);
                        builder.UseTexture(passData.destination, AccessFlags.Write);

                        // Sets the render function.
                        builder.AllowPassCulling(false);
                        builder.SetRenderFunc((AsyncReadbackPassData passData, UnsafeGraphContext context) => ExecutePass(passData, context));
                    }
                }

                // Reset the flags so we only do the readback for this one frame.
                scheduleReadback = false;
                screenshotPath = null;
            }

            static void ExecutePass(AsyncReadbackPassData data, UnsafeGraphContext context)
            {
                CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

                // Blit source texture to destination
                Material material = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
                Blitter.BlitTexture(cmd, data.source, data.destination, material, 0);

                // Use destination texture and write it to disk asynchronously
                cmd.RequestAsyncReadback(data.destination, (request) =>
                {
                    if (request.hasError)
                    {
                        Debug.LogError("[ScreenshotPass] Async GPU readback error!");
                        return;
                    }

                    try
                    {
                        // Create an LDR Texture2D on CPU
                        Texture2D tex = new(data.width, data.height, TextureFormat.RGBA32, false);

                        // Copy data to the CPU texture
                        var pixelData = request.GetData<Color32>();
                        tex.SetPixelData(pixelData, 0);
                        tex.Apply();

                        // Encode to disk
                        byte[] pngData = tex.EncodeToPNG();
                        File.WriteAllBytes(data.filePath, pngData);

                        // Clean up
                        Destroy(tex);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[ScreenshotPass] Error writing screenshot: {e}");
                    }
                });
            }
        }
    }
}
