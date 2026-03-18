Shader "Hidden/VHSPro_pass1"{
   Properties {
      // _MainTex ("Main Texture", 2D) = "white" {}
   }
   SubShader {

      Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline" }    
      Pass{
   
         /*
         ZWrite Off
         ZTest Always
         Blend Off
         Cull Off
         */

         HLSLPROGRAM
         // #pragma target 4.5
         // #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch
         #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"      
         #pragma vertex vert
         #pragma fragment frag

         struct Attributes {
            float4 positionOS   : POSITION;
            float2 uv           : TEXCOORD0;
         };

         struct Varyings { //same
            float4 vertex : SV_POSITION;
            float2 uv     : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
         };


         // List of properties to control your post process effect
         //TEXTURE2D_X is use to be compatible with single instancing VR path
         //https://teodutra.com/unity/shaders/urp/graphics/2020/05/18/From-Built-in-to-URP/
         TEXTURE2D_X(_InputTex); 
         SAMPLER(sampler_InputTex);

         float    _time;
         float4   _ResOg; // before pixelation (.xy resolution, .zw one pixel )
         float4   _Res;   // after pixelation  (.xy resolution, .zw one pixel )
         float4   _ResN;   // noise resolution  (.xy resolution, .zw one pixel )

         //Color Reduction Part (former graphics adapter pro)
         #pragma shader_feature VHS_COLOR
         #pragma shader_feature VHS_PALETTE   
         #pragma shader_feature VHS_DITHER   
         #pragma shader_feature VHS_SIGNAL_TWEAK_ON   
         TEXTURE2D_X(_PaletteTex); 
         SAMPLER(sampler_PaletteTex);
         #include "vhs_gap.hlsl"


         //Signal Distortion Part
         #pragma shader_feature VHS_FILMGRAIN_ON
         #pragma shader_feature VHS_LINENOISE_ON
         #pragma shader_feature VHS_TAPENOISE_ON
         #pragma shader_feature VHS_YIQNOISE_ON
         #pragma shader_feature VHS_TWITCH_H_ON
         #pragma shader_feature VHS_TWITCH_V_ON  
         #pragma shader_feature VHS_JITTER_H_ON
         #pragma shader_feature VHS_JITTER_V_ON 
         #pragma shader_feature VHS_LINESFLOAT_ON
         #pragma shader_feature VHS_SCANLINES_ON
         #pragma shader_feature VHS_STRETCH_ON  
         
         TEXTURE2D_X(_TapeTex);  
         SAMPLER(sampler_TapeTex);  
         #include "vhs_pass1.hlsl" 


         Varyings vert(Attributes input) {

            Varyings output = (Varyings)0;
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
            output.vertex = vertexInput.positionCS;
            output.uv = input.uv;
             
            return output;
         }

         float4 frag(Varyings input) : SV_Target {

            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
         
            float3 outColor = vhs(input);    
            return float4(outColor, 1);

         }

         ENDHLSL
      }
   } 
   FallBack Off
}
