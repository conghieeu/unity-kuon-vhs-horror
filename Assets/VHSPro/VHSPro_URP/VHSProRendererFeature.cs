using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class VHSProRendererFeature : ScriptableRendererFeature
{
    private VHSProRenderPass renderPass;

    public override void Create()
    {
        renderPass = new VHSProRenderPass();
        renderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Find postVHSPro component on the current camera
        var camera = renderingData.cameraData.camera;
        var vhsPro = camera.GetComponent<postVHSPro>();

        if (vhsPro != null && vhsPro.enabled && vhsPro.gameObject.activeInHierarchy)
        {
            renderPass.Setup(vhsPro);
            renderer.EnqueuePass(renderPass);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (renderPass != null)
        {
            renderPass.Cleanup();
        }
    }
}
