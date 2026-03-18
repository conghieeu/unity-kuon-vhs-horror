using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VHSProRendererFeature : ScriptableRendererFeature {

    private VHSProPass pass;

    public override void Create() {
        this.name = "VHSPro";
        pass = new VHSProPass(settings.renderPassEvent); //settings.renderPassEvent, settings.shader
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        
        // pass.Setup(renderer.cameraColorTarget); //we dont need this. we are grabbing it in the pass 
        renderer.EnqueuePass(pass);
    }

    [System.Serializable]
    public class Settings {
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }
    public Settings settings = new Settings();
    
}