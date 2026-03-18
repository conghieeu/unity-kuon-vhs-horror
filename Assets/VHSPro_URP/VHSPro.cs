//Note: VHSPro.cs just contains list of params 
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

using VladStorm;


[Serializable, VolumeComponentMenuForRenderPipeline("Post-processing/VHS Pro", typeof(UniversalRenderPipeline))]
public class VHSPro : VolumeComponent, IPostProcessComponent { 

   //Toggles
   public bool g_pixel = true;
   public bool g_color = true;
   public bool g_palette = true;
   public bool g_crt = true;
   public bool g_noise = true;
   public bool g_jitter = true;
   public bool g_signal = true;
   public bool g_feedback = true;
   public bool g_extra = false;
   public bool g_bypass = false;   


   //Screen
   public BoolParameter            pixelOn = new BoolParameter(false);
   public IntParameter             screenResPresetId = new IntParameter(2);
   public NoInterpIntParameter     screenWidth = new NoInterpIntParameter(640);
   public NoInterpIntParameter     screenHeight = new NoInterpIntParameter(480);

   //Color encoding
   public BoolParameter            colorOn = new BoolParameter(false);
   public IntParameter             colorMode = new IntParameter(2);
   public BoolParameter            colorSyncedOn = new BoolParameter(true);
   public ClampedIntParameter      bitsR = new ClampedIntParameter(2,0,255);
   public ClampedIntParameter      bitsG = new ClampedIntParameter(2,0,255);
   public ClampedIntParameter      bitsB = new ClampedIntParameter(2,0,255);
   public ClampedIntParameter      bitsSynced = new ClampedIntParameter(2,0,255); 
   public ClampedIntParameter      bitsGray = new ClampedIntParameter(1,0,255);
   public ColorParameter           grayscaleColor = new ColorParameter(Color.white);

   //Dither
   public BoolParameter            ditherOn = new BoolParameter(false);
   public NoInterpIntParameter     ditherMode = new NoInterpIntParameter(3);
   public ClampedFloatParameter    ditherAmount = new ClampedFloatParameter(1f, -1f, 3f);

   //Palette
   public BoolParameter            paletteOn = new BoolParameter(false);
   public NoInterpIntParameter     paletteId = new NoInterpIntParameter(0);
   public ClampedIntParameter      paletteDelta = new ClampedIntParameter(5,0,30);
   public TextureParameter         paletteTex = new TextureParameter(null);
   
   public PalettePreset paletteCustom; 
   public string paletteCustomName = ""; //for automatic update when drag and drop texture
   public bool paletteCustomInit = false; 

   //crt
   public BoolParameter bleedOn  = new BoolParameter(false); 
   public NoInterpIntParameter crtMode = new NoInterpIntParameter(0); 
   public ClampedFloatParameter bleedAmount  = new ClampedFloatParameter(1f, 0f, 5f);


   //Noise
   public BoolParameter noiseResGlobal  = new BoolParameter(true); 
   public NoInterpIntParameter noiseResWidth = new NoInterpIntParameter(640);
   public NoInterpIntParameter noiseResHeight = new NoInterpIntParameter(480);

   public BoolParameter filmgrainOn  = new BoolParameter(false);
   public ClampedFloatParameter filmGrainAmount = new ClampedFloatParameter(0.016f, 0f, 1f); 

   public BoolParameter signalNoiseOn  = new BoolParameter(false);
   public ClampedFloatParameter signalNoiseAmount = new ClampedFloatParameter(0.3f, 0f, 1f);
   public ClampedFloatParameter signalNoisePower  = new ClampedFloatParameter(0.83f, 0f, 1f);

   public BoolParameter lineNoiseOn  = new BoolParameter(false);
   public ClampedFloatParameter lineNoiseAmount = new ClampedFloatParameter(1f, 0f, 10f);
   public ClampedFloatParameter lineNoiseSpeed = new ClampedFloatParameter(5f, 0f, 10f);

   public BoolParameter tapeNoiseOn  = new BoolParameter(false);
   public ClampedFloatParameter tapeNoiseTH = new ClampedFloatParameter(0.63f, 0f, 1.5f);
   public ClampedFloatParameter tapeNoiseAmount = new ClampedFloatParameter(1f, 0f, 1.5f); 
   public ClampedFloatParameter tapeNoiseSpeed = new ClampedFloatParameter(1f, 0f, 1.5f);

   //Jitter
   public BoolParameter scanLinesOn  = new BoolParameter(false);
   public ClampedFloatParameter scanLineWidth = new ClampedFloatParameter(10f,0f,20f);
   
   public BoolParameter linesFloatOn  = new BoolParameter(false); 
   public ClampedFloatParameter linesFloatSpeed = new ClampedFloatParameter(1f,-3f,3f);
   public BoolParameter stretchOn  = new BoolParameter(false);

   public BoolParameter jitterHOn  = new BoolParameter(false);
   public ClampedFloatParameter jitterHAmount = new ClampedFloatParameter(.5f,0f,5f);
   public BoolParameter jitterVOn  = new BoolParameter(false); 
   public ClampedFloatParameter jitterVAmount = new ClampedFloatParameter(1f,0f,15f);
   public ClampedFloatParameter jitterVSpeed = new ClampedFloatParameter(1f,0f,5f);

   public BoolParameter twitchHOn  = new BoolParameter(false);
   public ClampedFloatParameter twitchHFreq = new ClampedFloatParameter(1f,0f,5f);
   public BoolParameter twitchVOn  = new BoolParameter(false);
   public ClampedFloatParameter twitchVFreq = new ClampedFloatParameter(1f,0f,5f);
    
   //Signal Tweak
   public BoolParameter signalTweakOn  = new BoolParameter(false); 
   public ClampedFloatParameter signalAdjustY = new ClampedFloatParameter(0f,-0.25f, 0.25f);
   public ClampedFloatParameter signalAdjustI = new ClampedFloatParameter(0f,-0.25f, 0.25f);
   public ClampedFloatParameter signalAdjustQ = new ClampedFloatParameter(0f,-0.25f, 0.25f);
   public ClampedFloatParameter signalShiftY = new ClampedFloatParameter(1f,-2f, 2f);
   public ClampedFloatParameter signalShiftI = new ClampedFloatParameter(1f,-2f, 2f);
   public ClampedFloatParameter signalShiftQ = new ClampedFloatParameter(1f,-2f, 2f);

   //Feedback
   public BoolParameter feedbackOn  = new BoolParameter(false); 
   public ClampedFloatParameter feedbackThresh = new ClampedFloatParameter(.1f, 0f, 1f);
   public ClampedFloatParameter feedbackAmount = new ClampedFloatParameter(2.0f, 0f, 3f);  
   public ClampedFloatParameter feedbackFade = new ClampedFloatParameter(.82f, 0f, 1f);
   public ColorParameter feedbackColor = new ColorParameter(new Color(1f,.5f,0f)); 
   public BoolParameter feedbackDebugOn  = new BoolParameter(false); 
   public int feedbackMode = 0; 

   //Tools 
   public BoolParameter independentTimeOn  = new BoolParameter(false); 
   // float _time = 0f;

   //Bypass     
   public BoolParameter            bypassOn = new BoolParameter(false);  
   public TextureParameter         bypassTex = new TextureParameter(null);



   // public bool IsActive() => intensity.value > 0f;
   public bool IsActive(){

      //everything is off by default
      if(pixelOn.value==false &&
         colorOn.value==false &&
         ditherOn.value==false &&
         paletteOn.value==false &&
         bleedOn.value==false &&
         filmgrainOn.value==false &&
         signalNoiseOn.value==false &&
         lineNoiseOn.value==false &&
         tapeNoiseOn.value==false &&
         scanLinesOn.value==false &&
         linesFloatOn.value==false &&
         jitterHOn.value==false &&
         jitterVOn.value==false &&
         twitchHOn.value==false &&
         twitchVOn.value==false &&
         signalTweakOn.value==false &&
         feedbackOn.value==false &&
         bypassOn.value==false) {
         return false;
      }

      return true;
   } 

   public bool IsTileCompatible() => true; //?


}
