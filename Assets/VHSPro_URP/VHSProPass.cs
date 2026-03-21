using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

using VladStorm;

#pragma warning disable CS0618 // Suppress obsolete warnings for Compatibility Mode methods

public class VHSProPass : ScriptableRenderPass { 
    
   protected string RenderTag => "VHSPro";
   protected VHSPro cmpt;     //component from Post Processing Stack Inspector 

   //Materials
   Material mat1;         //1st pass (signal distortion)
   Material matBleed;     //2nd pass vhs bleeding + mix with feedback
   Material matTape;      //tape noise
   Material matFeedback;  //feedback

   //these 2 we need to pass to the next frame 
   RenderTexture texFeedbackLast;
   RenderTexture texLast;

   float _time = 0f;
   Vector4 _ResOg; 
   Vector4 _Res;
   Vector4 _ResN;


   //when we set up Render Feature 
   public VHSProPass(RenderPassEvent _renderPassEvent) {
      renderPassEvent = _renderPassEvent;
      requiresIntermediateTexture = true;
   }


   //load materials 
   void EnsureMaterials() {
      if(mat1==null)          LoadMat(ref mat1,          "Materials/VHSPro_pass1");
      if(matTape==null)       LoadMat(ref matTape,       "Materials/VHSPro_tape");
      if(matBleed==null)      LoadMat(ref matBleed,      "Materials/VHSPro_bleed");
      if(matFeedback==null)   LoadMat(ref matFeedback,   "Materials/VHSPro_feedback");
   }


   // Shared setup logic used by both Execute (Compatibility) and RecordRenderGraph
   void SetupEffect(int screenWidth, int screenHeight) {

      VHSHelper.Init();

      //Resolution Presets
      ResPreset resPreset = VHSHelper.GetResPresets()[cmpt.screenResPresetId.value];
      if(resPreset.isCustom!=true){
         cmpt.screenWidth.value  = resPreset.screenWidth;
         cmpt.screenHeight.value = resPreset.screenHeight;
      }
      if(resPreset.isFirst==true || cmpt.pixelOn.value==false){
         cmpt.screenWidth.value  = screenWidth;
         cmpt.screenHeight.value = screenHeight;
      }

      //original screen resolution (.xy resolution .zw one pixel)
      _ResOg = new Vector4(screenWidth, screenHeight, 0f, 0f);
      _ResOg[2] = 1f/_ResOg.x; 
      _ResOg[3] = 1f/_ResOg.y;  

      //resolution after pixelation
      _Res = new Vector4(cmpt.screenWidth.value, cmpt.screenHeight.value, 0f,0f);
      _Res[2] = 1f/_Res.x;                                    
      _Res[3] = 1f/_Res.y;                                    

      //resolution of noise 
      _ResN = new Vector4(_Res.x, _Res.y, _Res.z, _Res.w);
      if(!cmpt.noiseResGlobal.value){
         _ResN = new Vector4(cmpt.noiseResWidth.value, cmpt.noiseResHeight.value, 0f, 0f);
         _ResN[2] = 1f/_ResN.x;                                    
         _ResN[3] = 1f/_ResN.y;                                                
      }

      EnsureMaterials();
   }


   // Set all material properties (shared between both paths)
   void SetMaterialProperties() {

      if(cmpt.independentTimeOn.value) _time = Time.unscaledTime; 
      else                             _time = Time.time; 

      mat1.SetFloat("_time",      _time);  
      mat1.SetVector("_ResOg",    _ResOg);
      mat1.SetVector("_Res",      _Res);
      mat1.SetVector("_ResN",     _ResN);

      //Color Decimation
      FeatureToggle(mat1, cmpt.colorOn.value, "VHS_COLOR");       
       
      mat1.SetInt("_colorMode",                cmpt.colorMode.value);
      mat1.SetInt("_colorSyncedOn",            cmpt.colorSyncedOn.value?1:0);

      mat1.SetInt("bitsR",                     cmpt.bitsR.value);
      mat1.SetInt("bitsG",                     cmpt.bitsG.value);
      mat1.SetInt("bitsB",                     cmpt.bitsB.value);
      mat1.SetInt("bitsSynced",                cmpt.bitsSynced.value);

      mat1.SetInt("bitsGray",                  cmpt.bitsGray.value);
      mat1.SetColor("grayscaleColor",          cmpt.grayscaleColor.value);        

      FeatureToggle(mat1, cmpt.ditherOn.value, "VHS_DITHER");        
      mat1.SetInt("_ditherMode",            cmpt.ditherMode.value);
      mat1.SetFloat("ditherAmount",         cmpt.ditherAmount.value);


      //Signal Tweak
      FeatureToggle(mat1, cmpt.signalTweakOn.value, "VHS_SIGNAL_TWEAK_ON");

      mat1.SetFloat("signalAdjustY", cmpt.signalAdjustY.value);
      mat1.SetFloat("signalAdjustI", cmpt.signalAdjustI.value);
      mat1.SetFloat("signalAdjustQ", cmpt.signalAdjustQ.value);

      mat1.SetFloat("signalShiftY", cmpt.signalShiftY.value);
      mat1.SetFloat("signalShiftI", cmpt.signalShiftI.value);
      mat1.SetFloat("signalShiftQ", cmpt.signalShiftQ.value);


      //Palette
      FeatureToggle(mat1, cmpt.paletteOn.value, "VHS_PALETTE");

      if(cmpt.paletteOn.value){

         PalettePreset pal = VHSHelper.GetPalettes()[cmpt.paletteId.value];

         Texture2D texPaletteSorted = pal.texSortedPre; 
         Shader.SetGlobalTexture("_PaletteTex", texPaletteSorted);
         mat1.SetInt("_ResPalette",       pal.texSortedWidth);

         mat1.SetInt("paletteDelta",           cmpt.paletteDelta.value);

      }


      //VHS 1st Pass (Distortions, Decimations) 
      FeatureToggle(mat1, cmpt.filmgrainOn.value, "VHS_FILMGRAIN_ON");
      FeatureToggle(mat1, cmpt.tapeNoiseOn.value, "VHS_TAPENOISE_ON");
      FeatureToggle(mat1, cmpt.lineNoiseOn.value, "VHS_LINENOISE_ON");


      //Jitter & Twitch
      FeatureToggle(mat1, cmpt.jitterHOn.value, "VHS_JITTER_H_ON");
      mat1.SetFloat("jitterHAmount", cmpt.jitterHAmount.value);

      FeatureToggle(mat1, cmpt.jitterVOn.value, "VHS_JITTER_V_ON");
      mat1.SetFloat("jitterVAmount", cmpt.jitterVAmount.value);
      mat1.SetFloat("jitterVSpeed", cmpt.jitterVSpeed.value);

      FeatureToggle(mat1, cmpt.linesFloatOn.value, "VHS_LINESFLOAT_ON");     
      mat1.SetFloat("linesFloatSpeed", cmpt.linesFloatSpeed.value);

      FeatureToggle(mat1, cmpt.twitchHOn.value, "VHS_TWITCH_H_ON");
      mat1.SetFloat("twitchHFreq", cmpt.twitchHFreq.value);

      FeatureToggle(mat1, cmpt.twitchVOn.value, "VHS_TWITCH_V_ON");
      mat1.SetFloat("twitchVFreq", cmpt.twitchVFreq.value);

      FeatureToggle(mat1, cmpt.scanLinesOn.value, "VHS_SCANLINES_ON");
      mat1.SetFloat("scanLineWidth", cmpt.scanLineWidth.value);

      FeatureToggle(mat1, cmpt.signalNoiseOn.value, "VHS_YIQNOISE_ON");
      mat1.SetFloat("signalNoisePower", cmpt.signalNoisePower.value);
      mat1.SetFloat("signalNoiseAmount", cmpt.signalNoiseAmount.value);

      FeatureToggle(mat1, cmpt.stretchOn.value, "VHS_STRETCH_ON");

      //Tape noise materials
      if(cmpt.tapeNoiseOn.value || cmpt.filmgrainOn.value || cmpt.lineNoiseOn.value){
         matTape.SetFloat("_time",  _time);  
         matTape.SetVector("_ResN", _ResN);

         FeatureToggle(matTape, cmpt.filmgrainOn.value, "VHS_FILMGRAIN_ON");
         matTape.SetFloat("filmGrainAmount", cmpt.filmGrainAmount.value);
         
         FeatureToggle(matTape, cmpt.tapeNoiseOn.value, "VHS_TAPENOISE_ON");
         matTape.SetFloat("tapeNoiseTH", cmpt.tapeNoiseTH.value);
         matTape.SetFloat("tapeNoiseAmount", cmpt.tapeNoiseAmount.value);
         matTape.SetFloat("tapeNoiseSpeed", cmpt.tapeNoiseSpeed.value);
         
         FeatureToggle(matTape, cmpt.lineNoiseOn.value, "VHS_LINENOISE_ON");
         matTape.SetFloat("lineNoiseAmount", cmpt.lineNoiseAmount.value);
         matTape.SetFloat("lineNoiseSpeed", cmpt.lineNoiseSpeed.value);

         mat1.SetFloat("tapeNoiseAmount", cmpt.tapeNoiseAmount.value);          
      }

      //VHS 2nd Pass (Bleed)
      matBleed.SetFloat("_time",  _time);  
      matBleed.SetVector("_ResOg", _ResOg);
      matBleed.SetVector("_Res",   _Res);

      //CRT       
      FeatureToggle(matBleed, cmpt.bleedOn.value, "VHS_BLEED_ON");

      matBleed.DisableKeyword("VHS_OLD_THREE_PHASE");
      matBleed.DisableKeyword("VHS_THREE_PHASE");
      matBleed.DisableKeyword("VHS_TWO_PHASE");           
           if(cmpt.crtMode.value==0){ matBleed.EnableKeyword("VHS_OLD_THREE_PHASE"); }
      else if(cmpt.crtMode.value==1){ matBleed.EnableKeyword("VHS_THREE_PHASE"); }
      else if(cmpt.crtMode.value==2){ matBleed.EnableKeyword("VHS_TWO_PHASE"); }

      matBleed.SetFloat("bleedAmount", cmpt.bleedAmount.value);

      //Feedback
      matBleed.SetInt("feedbackOn",            cmpt.feedbackOn.value?1:0);
      matBleed.SetInt("feedbackDebugOn",       cmpt.feedbackDebugOn.value?1:0);

      if(cmpt.feedbackOn.value){
         matFeedback.SetFloat("feedbackThresh",   cmpt.feedbackThresh.value);
         matFeedback.SetFloat("feedbackAmount",   cmpt.feedbackAmount.value);
         matFeedback.SetFloat("feedbackFade",     cmpt.feedbackFade.value);
         matFeedback.SetColor("feedbackColor",    cmpt.feedbackColor.value);
      }
   }


   // =========================================================================
   //  RENDER GRAPH PATH (Unity 6 native)
   // =========================================================================

   // PassData classes for Render Graph
   class VHSPassData {
      public Material mat1;
      public Material matTape;
      public Material matBleed;
      public Material matFeedback;

      public TextureHandle source;
      public TextureHandle texPass1;
      public TextureHandle tapeTexture;
      public TextureHandle feedbackTexture;
      public TextureHandle feedbackLastTexture;
      public TextureHandle lastTexture;
      public TextureHandle destination;

      public bool hasTapeNoise;
      public bool hasFeedback;
      public bool hasFeedbackDebug;
      public bool hasBleed;
      public bool bypassOn;
      public Texture bypassTex;
   }


   public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) {

      UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
      UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

      // Skip scene view camera
      if(cameraData.isSceneViewCamera) return;

      // Get the component
      var volumeStack = VolumeManager.instance.stack;
      cmpt = volumeStack.GetComponent<VHSPro>();
      if(cmpt == null || !cmpt.IsActive()) return;

      // Setup effect parameters
      var desc = cameraData.cameraTargetDescriptor;
      desc.depthBufferBits = 0;
      SetupEffect(desc.width, desc.height);

      if(mat1 == null || matBleed == null) return;

      // Set all material properties
      SetMaterialProperties();

      // Source texture
      TextureHandle source = resourceData.activeColorTexture;

      // Create temp textures via Render Graph
      TextureDesc texDesc = new TextureDesc(desc.width, desc.height);
      texDesc.colorFormat = desc.graphicsFormat;
      texDesc.depthBufferBits = DepthBits.None;
      texDesc.msaaSamples = MSAASamples.None;

      texDesc.name = "VHSPro_Pass1";
      TextureHandle texPass1Handle = renderGraph.CreateTexture(texDesc);

      texDesc.name = "VHSPro_Dest";
      TextureHandle destHandle = renderGraph.CreateTexture(texDesc);

      TextureHandle tapeTexHandle = TextureHandle.nullHandle;
      bool hasTapeNoise = cmpt.tapeNoiseOn.value || cmpt.filmgrainOn.value || cmpt.lineNoiseOn.value;
      if(hasTapeNoise){
         texDesc.name = "VHSPro_Tape";
         tapeTexHandle = renderGraph.CreateTexture(texDesc);
      }

      TextureHandle feedbackHandle = TextureHandle.nullHandle;
      TextureHandle feedbackLastHandle = TextureHandle.nullHandle;
      TextureHandle lastHandle = TextureHandle.nullHandle;
      bool hasFeedback = cmpt.feedbackOn.value;

      if(hasFeedback) {
         texDesc.name = "VHSPro_Feedback";
         feedbackHandle = renderGraph.CreateTexture(texDesc);

         // Ensure persistent textures exist
         if(texFeedbackLast == null || texFeedbackLast.width != desc.width || texFeedbackLast.height != desc.height) {
            if(texFeedbackLast != null) texFeedbackLast.Release();
            texFeedbackLast = new RenderTexture(desc);
            texFeedbackLast.name = "VHSPro_FeedbackLast";
         }
         if(texLast == null || texLast.width != desc.width || texLast.height != desc.height) {
            if(texLast != null) texLast.Release();
            texLast = new RenderTexture(desc);
            texLast.name = "VHSPro_Last";
         }

         feedbackLastHandle = renderGraph.ImportTexture(RTHandles.Alloc(texFeedbackLast));
         lastHandle = renderGraph.ImportTexture(RTHandles.Alloc(texLast));
      }

      // Use an UnsafePass to do all blits in one pass with a standard CommandBuffer
      using (var builder = renderGraph.AddUnsafePass<VHSPassData>("VHSPro_MainPass", out var passData)) {

         passData.mat1 = mat1;
         passData.matTape = matTape;
         passData.matBleed = matBleed;
         passData.matFeedback = matFeedback;

         passData.source = source;
         passData.texPass1 = texPass1Handle;
         passData.tapeTexture = tapeTexHandle;
         passData.feedbackTexture = feedbackHandle;
         passData.feedbackLastTexture = feedbackLastHandle;
         passData.lastTexture = lastHandle;
         passData.destination = destHandle;

         passData.hasTapeNoise = hasTapeNoise;
         passData.hasFeedback = hasFeedback;
         passData.hasFeedbackDebug = cmpt.feedbackDebugOn.value;
         passData.hasBleed = cmpt.bleedOn.value;
         passData.bypassOn = cmpt.bypassOn.value;
         passData.bypassTex = cmpt.bypassTex.value;

         builder.UseTexture(source, AccessFlags.ReadWrite);
         builder.UseTexture(texPass1Handle, AccessFlags.ReadWrite);
         builder.UseTexture(destHandle, AccessFlags.ReadWrite);

         if(tapeTexHandle.IsValid())
            builder.UseTexture(tapeTexHandle, AccessFlags.ReadWrite);
         if(feedbackHandle.IsValid())
            builder.UseTexture(feedbackHandle, AccessFlags.ReadWrite);
         if(feedbackLastHandle.IsValid())
            builder.UseTexture(feedbackLastHandle, AccessFlags.ReadWrite);
         if(lastHandle.IsValid())
            builder.UseTexture(lastHandle, AccessFlags.ReadWrite);

         builder.AllowPassCulling(false);

         builder.SetRenderFunc((VHSPassData data, UnsafeGraphContext ctx) => {
            CommandBuffer cmd = CommandBufferHelpers.GetNativeCommandBuffer(ctx.cmd);

            // === Tape Noise Pass ===
            if(data.hasTapeNoise) {
               cmd.Blit(null, data.tapeTexture, data.matTape);
               cmd.SetGlobalTexture("_TapeTex", data.tapeTexture);
            }

            // === Pass 1 (Signal Distortion) ===
            if(data.bypassOn && data.bypassTex != null) {
               cmd.SetGlobalTexture("_InputTex", data.bypassTex);
            } else {
               cmd.SetGlobalTexture("_InputTex", data.source);
            }
            cmd.Blit(null, data.texPass1, data.mat1);

            // === Feedback Pass ===
            if(data.hasFeedback) {
               cmd.SetGlobalTexture("_InputTex", data.texPass1);
               cmd.SetGlobalTexture("_LastTex", data.lastTexture);
               cmd.SetGlobalTexture("_FeedbackTex", data.feedbackLastTexture);
               cmd.Blit(null, data.feedbackTexture, data.matFeedback);

               cmd.Blit(data.feedbackTexture, data.feedbackLastTexture);  //save prev frame feedback
               cmd.Blit(data.texPass1, data.lastTexture);                //save prev frame color
            }

            if(data.hasFeedback || data.hasFeedbackDebug) {
               cmd.SetGlobalTexture("_FeedbackTex", data.feedbackTexture);
            }

            // === Bleed / Final Pass ===
            if(data.hasBleed) {
               cmd.SetGlobalTexture("_InputTex", data.texPass1);
               cmd.Blit(null, data.destination, data.matBleed);
            } else {
               cmd.Blit(data.texPass1, data.destination);
            }

            // Copy result back to source
            cmd.Blit(data.destination, data.source);
         });
      }
   }


   // =========================================================================
   //  COMPATIBILITY MODE PATH (legacy Execute for Unity 2022 / Compatibility Mode)
   // =========================================================================

   //textures (legacy URP way)
   int texIdPass1 =        Shader.PropertyToID("_TexPass1");
   int texIdTape =         Shader.PropertyToID("_TexTape");
   int texIdFeedback =     Shader.PropertyToID("_TexFeedback");
   RenderTargetIdentifier texPass1_legacy;
   RenderTargetIdentifier texTape_legacy;
   RenderTargetIdentifier texFeedback_legacy;


   //configure render targets (Compatibility Mode only)
   public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {

      // Skipping post processing rendering inside the scene view
      if(renderingData.cameraData.isSceneViewCamera) return;

      //Grab the camera target descriptor. 
      RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
      desc.depthBufferBits = 0;

      // Lets grab the component 
      var volumeStack = VolumeManager.instance.stack;
      cmpt = volumeStack.GetComponent<VHSPro>();
      if( cmpt==null ){
         Debug.LogError($"Unable to find component.");
         return;
      }

      SetupEffect(desc.width, desc.height);

      //init textures
      cmd.GetTemporaryRT(texIdPass1,         desc.width, desc.height);
      texPass1_legacy = new RenderTargetIdentifier(texIdPass1);  

      if(cmpt.tapeNoiseOn.value || cmpt.filmgrainOn.value || cmpt.lineNoiseOn.value){
         cmd.GetTemporaryRT(texIdTape,          desc.width, desc.height);
         texTape_legacy = new RenderTargetIdentifier(texIdTape);  
      }

      if(cmpt.feedbackOn.value){
         cmd.GetTemporaryRT(texIdFeedback,      desc.width, desc.height);          
         texFeedback_legacy =     new RenderTargetIdentifier(texIdFeedback);  

         if(texFeedbackLast==null || texFeedbackLast.width!=desc.width || texFeedbackLast.height!=desc.height){
            texFeedbackLast = new RenderTexture(desc);
         } 
         if(texLast==null || texLast.width!=desc.width || texLast.height!=desc.height){
            texLast = new RenderTexture(desc);
         }
      }

   }


   //Cleans the temporary RTs (Compatibility Mode only)
   public override void OnCameraCleanup(CommandBuffer cmd) {
      
      //textures   
      cmd.ReleaseTemporaryRT(texIdPass1);
      cmd.ReleaseTemporaryRT(texIdTape);
      cmd.ReleaseTemporaryRT(texIdFeedback);

   }

    
   // The actual execution of the pass (Compatibility Mode only)
   public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {

      //from PostProcessPass
      RTHandle texSource = renderingData.cameraData.renderer.cameraColorTargetHandle;

      // Skipping post processing rendering inside the scene view
      if(renderingData.cameraData.isSceneViewCamera) return;
       
      if(!cmpt.active || !IsActive()) {
         return;
      }

      
      CommandBuffer cmd = CommandBufferPool.Get(RenderTag);

      SetMaterialProperties();


      //Noises Pass
      if(cmpt.tapeNoiseOn.value || cmpt.filmgrainOn.value || cmpt.lineNoiseOn.value){
         cmd.Blit(null, texTape_legacy, matTape);  
         cmd.SetGlobalTexture(Shader.PropertyToID("_TapeTex"), texTape_legacy);
      }


      //1st pass
      //Bypass Texture
      if(cmpt.bypassOn.value==true){
         cmd.SetGlobalTexture(Shader.PropertyToID("_InputTex"), cmpt.bypassTex.value);
      }else{
         cmd.SetGlobalTexture(Shader.PropertyToID("_InputTex"), texSource);
      }

      cmd.Blit(null, texPass1_legacy, mat1);


      
      if(cmpt.feedbackOn.value){

         cmd.SetGlobalTexture(Shader.PropertyToID("_InputTex"),      texPass1_legacy);
         cmd.SetGlobalTexture(Shader.PropertyToID("_LastTex"),       texLast);
         cmd.SetGlobalTexture(Shader.PropertyToID("_FeedbackTex"),   texFeedbackLast);

         cmd.Blit(null, texFeedback_legacy, matFeedback); 

         cmd.Blit(texFeedback_legacy,   texFeedbackLast);  //save prev frame feedback
         cmd.Blit(texPass1_legacy,      texLast);          //save prev frame color

      }

      if(cmpt.feedbackOn.value || cmpt.feedbackDebugOn.value){
         cmd.SetGlobalTexture(Shader.PropertyToID("_FeedbackTex"),   texFeedback_legacy);
      }
      

      //2nd pass
      if(cmpt.bleedOn.value==true){         
         cmd.SetGlobalTexture(Shader.PropertyToID("_InputTex"), texPass1_legacy);
         cmd.Blit(null, texSource, matBleed); 
      }else{
         cmd.Blit(texPass1_legacy, texSource); //no bleed pass
      }


      //we render everything back to the camera target
      cmd.Blit(texSource, renderingData.cameraData.renderer.cameraColorTargetHandle);

      context.ExecuteCommandBuffer(cmd);
      CommandBufferPool.Release(cmd);

   }


   //Helper Tools
   void FeatureToggle(Material mat, bool propVal, string featureName){  //turn on/off shader features
      if(propVal)     mat.EnableKeyword(featureName);
      else            mat.DisableKeyword(featureName);
   }

   void LoadMat(ref Material m, string materialPath){      
      m = Resources.Load<Material>(materialPath);
      if(m==null) 
         Debug.LogError($"Unable to find material '{materialPath}'. Post-Process Volume VHSPro is unable to load.");
   }

   protected bool IsActive() {
      return cmpt.IsActive();
   }

}

#pragma warning restore CS0618
