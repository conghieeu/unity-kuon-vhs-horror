using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using VladStorm;

public class VHSProPass : ScriptableRenderPass { 
    
   protected string RenderTag => "VHSPro";
   protected VHSPro cmpt;     //component from Post Processing Stack Inspector 

   //Materials
   Material mat1;         //1st pass (signal distortion)
   Material matBleed;     //2nd pass vhs bleeding + mix with feedback
   Material matTape;      //tape noise
   Material matFeedback;  //feedback

   //textures (URP way)
   int texIdPass1 =        Shader.PropertyToID("_TexPass1");
   int texIdTape =         Shader.PropertyToID("_TexTape");
   int texIdFeedback =     Shader.PropertyToID("_TexFeedback");
   RenderTargetIdentifier texPass1;
   RenderTargetIdentifier texTape;
   RenderTargetIdentifier texFeedback;

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
   }


   //configure render targets, their clear state, and temporary render target textures.
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

      //init palettes and resolution presets
      VHSHelper.Init();

      //Resolution Presents
      ResPreset resPreset = VHSHelper.GetResPresets()[cmpt.screenResPresetId.value];
      if(resPreset.isCustom!=true){
         cmpt.screenWidth.value  = resPreset.screenWidth;
         cmpt.screenHeight.value = resPreset.screenHeight;
      }
      if(resPreset.isFirst==true || cmpt.pixelOn.value==false){
         cmpt.screenWidth.value  = desc.width;
         cmpt.screenHeight.value = desc.height;
      }

      //original screen resolution (.xy resolution .zw one pixel)
      _ResOg = new Vector4(desc.width, desc.height, 0f, 0f);
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


      //load materials 
      // if(mat1==null)          InitMat(ref mat1,          "Hidden/VHSPro_pass1");
      // if(matTape==null)       InitMat(ref matTape,       "Hidden/VHSPro_tape");
      // if(matBleed==null)      InitMat(ref matBleed,      "Hidden/VHSPro_bleed");
      // if(matFeedback==null)   InitMat(ref matFeedback,   "Hidden/VHSPro_feedback");
      
      if(mat1==null)          LoadMat(ref mat1,          "Materials/VHSPro_pass1");
      if(matTape==null)       LoadMat(ref matTape,       "Materials/VHSPro_tape");
      if(matBleed==null)      LoadMat(ref matBleed,      "Materials/VHSPro_bleed");
      if(matFeedback==null)   LoadMat(ref matFeedback,   "Materials/VHSPro_feedback");


      //init textures
      cmd.GetTemporaryRT(texIdPass1,         desc.width, desc.height); //default FilterMode is Point
      texPass1 = new RenderTargetIdentifier(texIdPass1);  

      if(cmpt.tapeNoiseOn.value || cmpt.filmgrainOn.value || cmpt.lineNoiseOn.value){
         cmd.GetTemporaryRT(texIdTape,          desc.width, desc.height);
         texTape = new RenderTargetIdentifier(texIdTape);  
      }

      if(cmpt.feedbackOn.value){
         cmd.GetTemporaryRT(texIdFeedback,      desc.width, desc.height);          
         texFeedback =     new RenderTargetIdentifier(texIdFeedback);  

         //if cam res change or 1st pass -> create textures -> keep them for the next frame
         //Note: unity has a bug with renderingData.cameraData.cameraType, 
         //always shows CameraType.Game
         //TODO dont re-create RTs when CameraType.Preview
         if(texFeedbackLast==null || texFeedbackLast.width!=desc.width || texFeedbackLast.height!=desc.height){
            texFeedbackLast = new RenderTexture(desc);
            // Debug.Log("xx" + texFeedbackLast.width + " " + desc.width + " " + renderingData.cameraData.cameraType);
         } 
         if(texLast==null || texLast.width!=desc.width || texLast.height!=desc.height){
            texLast = new RenderTexture(desc);
         }
      }

   }


   //Cleans the temporary RTs when we don't need them anymore
   public override void OnCameraCleanup(CommandBuffer cmd) {
      
      //textures   
      cmd.ReleaseTemporaryRT(texIdPass1);
      cmd.ReleaseTemporaryRT(texIdTape);
      cmd.ReleaseTemporaryRT(texIdFeedback);

   }

    
   // The actual execution of the pass 
   public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {

      //from PostProcessPass
      RTHandle texSource = renderingData.cameraData.renderer.cameraColorTargetHandle;

      // Skipping post processing rendering inside the scene view
      if(renderingData.cameraData.isSceneViewCamera) return;
       
      if(!cmpt.active || !IsActive()) {
         return;
      }

      
      CommandBuffer cmd = CommandBufferPool.Get(RenderTag);


      if(cmpt.independentTimeOn.value) _time = Time.unscaledTime; 
      else                             _time = Time.time; 

      mat1.SetFloat("_time",      _time);  
      mat1.SetVector("_ResOg",    _ResOg);
      mat1.SetVector("_Res",      _Res);
      mat1.SetVector("_ResN",     _ResN);

      //Pixelation
      //...

      //Color Decimat1ion
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
         cmd.SetGlobalTexture(Shader.PropertyToID("_PaletteTex"), texPaletteSorted);
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
      // cmd.SetGlobalFloat(Shader.PropertyToID("twitchHFreq"), cmpt.twitchHFreq.value);

      FeatureToggle(mat1, cmpt.twitchVOn.value, "VHS_TWITCH_V_ON");
      mat1.SetFloat("twitchVFreq", cmpt.twitchVFreq.value);

      FeatureToggle(mat1, cmpt.scanLinesOn.value, "VHS_SCANLINES_ON");
      mat1.SetFloat("scanLineWidth", cmpt.scanLineWidth.value);

      FeatureToggle(mat1, cmpt.signalNoiseOn.value, "VHS_YIQNOISE_ON");
      mat1.SetFloat("signalNoisePower", cmpt.signalNoisePower.value);
      mat1.SetFloat("signalNoiseAmount", cmpt.signalNoiseAmount.value);

      FeatureToggle(mat1, cmpt.stretchOn.value, "VHS_STRETCH_ON");

      
      //Noises Pass
      if(cmpt.tapeNoiseOn.value || cmpt.filmgrainOn.value || cmpt.lineNoiseOn.value){

         matTape.SetFloat("_time",  _time);  
         matTape.SetVector("_ResN", _ResN); //URP

         FeatureToggle(matTape, cmpt.filmgrainOn.value, "VHS_FILMGRAIN_ON");
         matTape.SetFloat("filmGrainAmount", cmpt.filmGrainAmount.value);
         
         FeatureToggle(matTape, cmpt.tapeNoiseOn.value, "VHS_TAPENOISE_ON");
         matTape.SetFloat("tapeNoiseTH", cmpt.tapeNoiseTH.value);
         matTape.SetFloat("tapeNoiseAmount", cmpt.tapeNoiseAmount.value);
         matTape.SetFloat("tapeNoiseSpeed", cmpt.tapeNoiseSpeed.value);
         
         FeatureToggle(matTape, cmpt.lineNoiseOn.value, "VHS_LINENOISE_ON");
         matTape.SetFloat("lineNoiseAmount", cmpt.lineNoiseAmount.value);
         matTape.SetFloat("lineNoiseSpeed", cmpt.lineNoiseSpeed.value);


         cmd.Blit(null, texTape, matTape);  
         
         cmd.SetGlobalTexture(Shader.PropertyToID("_TapeTex"), texTape);
         mat1.SetFloat("tapeNoiseAmount", cmpt.tapeNoiseAmount.value);          

      }


      //VHS 2nd Pass (Bleed)
      matBleed.SetFloat("_time",  _time);  
      matBleed.SetVector("_ResOg", _ResOg);//  - resolution before pixelation
      matBleed.SetVector("_Res",   _Res);//  - resolution after pixelation

      //CRT       
      FeatureToggle(matBleed, cmpt.bleedOn.value, "VHS_BLEED_ON");

      matBleed.DisableKeyword("VHS_OLD_THREE_PHASE");
      matBleed.DisableKeyword("VHS_THREE_PHASE");
      matBleed.DisableKeyword("VHS_TWO_PHASE");           
           if(cmpt.crtMode.value==0){ matBleed.EnableKeyword("VHS_OLD_THREE_PHASE"); }
      else if(cmpt.crtMode.value==1){ matBleed.EnableKeyword("VHS_THREE_PHASE"); }
      else if(cmpt.crtMode.value==2){ matBleed.EnableKeyword("VHS_TWO_PHASE"); }

      matBleed.SetFloat("bleedAmount", cmpt.bleedAmount.value);


      //1st pass
      //Bypass Texture
      if(cmpt.bypassOn.value==true){
         cmd.SetGlobalTexture(Shader.PropertyToID("_InputTex"), cmpt.bypassTex.value);
      }else{
         cmd.SetGlobalTexture(Shader.PropertyToID("_InputTex"), texSource); //texSource.rt
      }

      //Note: we are using null and _InputTexture, 
      //instead of passing texture directly as _MainTex
      cmd.Blit(null, texPass1, mat1);


      
      if(cmpt.feedbackOn.value){

         //recalc feedback buffer
         matFeedback.SetFloat("feedbackThresh",   cmpt.feedbackThresh.value);
         matFeedback.SetFloat("feedbackAmount",   cmpt.feedbackAmount.value);
         matFeedback.SetFloat("feedbackFade",     cmpt.feedbackFade.value);
         matFeedback.SetColor("feedbackColor",    cmpt.feedbackColor.value);

         cmd.SetGlobalTexture(Shader.PropertyToID("_InputTex"),      texPass1);
         cmd.SetGlobalTexture(Shader.PropertyToID("_LastTex"),       texLast);
         cmd.SetGlobalTexture(Shader.PropertyToID("_FeedbackTex"),   texFeedbackLast);

         cmd.Blit(null, texFeedback, matFeedback); 

         cmd.Blit(texFeedback,   texFeedbackLast);  //save prev frame feedback
         cmd.Blit(texPass1,      texLast);          //save prev frame color

      }

      matBleed.SetInt("feedbackOn",            cmpt.feedbackOn.value?1:0);
      matBleed.SetInt("feedbackDebugOn",       cmpt.feedbackDebugOn.value?1:0);
      if(cmpt.feedbackOn.value || cmpt.feedbackDebugOn.value){
         cmd.SetGlobalTexture(Shader.PropertyToID("_FeedbackTex"),   texFeedback);
      }
      

      //2nd pass
      if(cmpt.bleedOn.value==true){         
         cmd.SetGlobalTexture(Shader.PropertyToID("_InputTex"), texPass1);
         cmd.Blit(null, texSource, matBleed); 
      }else{
         //TODO add feedback pass?
         cmd.Blit(texPass1, texSource); //no bleed pass
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

