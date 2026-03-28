Shader "Hidden/VHSPro_bleed"{
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
         TEXTURE2D_X(_InputTex); 
         SAMPLER(sampler_InputTex);
         TEXTURE2D_X(_FeedbackTex); 
         SAMPLER(sampler_FeedbackTex);

         #pragma shader_feature VHS_BLEED_ON
         #pragma shader_feature VHS_OLD_THREE_PHASE
         #pragma shader_feature VHS_THREE_PHASE
         #pragma shader_feature VHS_TWO_PHASE

         #pragma shader_feature VHS_SIGNAL_TWEAK_ON
         #include "vhs_bleed.hlsl" 


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

            float3 outColor = vhs2(input);
            return float4(outColor, 1);

         }

         ENDHLSL
      }
   } 
   FallBack Off
}
