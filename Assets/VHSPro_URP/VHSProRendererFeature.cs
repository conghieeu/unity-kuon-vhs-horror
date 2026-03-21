using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VHSProRendererFeature : ScriptableRendererFeature {

    private VHSProPass pass;

    public override void Create() {
        this.name = "VHSPro";
        pass = new VHSProPass(settings.renderPassEvent);
    }

    // This is used by Compatibility Mode (legacy path).
    // In Render Graph mode, RecordRenderGraph on the pass is called directly.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        
        renderer.EnqueuePass(pass);
    }

    [System.Serializable]
    public class Settings {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    public Settings settings = new Settings();
    
}