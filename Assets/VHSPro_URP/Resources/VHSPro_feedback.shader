Shader "Hidden/VHSPro_feedback"{
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


         Varyings vert(Attributes input) {

            Varyings output = (Varyings)0;
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

            VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
            output.vertex = vertexInput.positionCS;
            output.uv = input.uv;
             
            return output;
         }


         // List of properties to control your post process effect
         TEXTURE2D_X(_InputTex); 
         SAMPLER(sampler_InputTex);
         TEXTURE2D_X(_FeedbackTex); 
         SAMPLER(sampler_FeedbackTex);
         TEXTURE2D_X(_LastTex); 
         SAMPLER(sampler_LastTex);   


         float feedbackAmount;
         float feedbackFade;
         float feedbackThresh;
         half3 feedbackColor;


         half3 bms(half3 a, half3 b){  return 1.-(1.-a)*(1.-b); }
         half grey(half3 a){  return (a.x+a.y+a.z)/3.; }

         half len(half3 a, half3 b){
            return (abs(a.x-b.x)+abs(a.y-b.y)+abs(a.z-b.z))/3.;
         }


         float4 frag(Varyings input) : SV_Target {

            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 p = input.uv; // og normalized tex coordnates 0..1  
            float one_x = 1./_ScreenParams.x;

            //new feedback value
            half3 fc = SAMPLE_TEXTURE2D_X(_InputTex, sampler_InputTex, p ).xyz; 
            half3 fl = SAMPLE_TEXTURE2D_X(_LastTex,  sampler_FeedbackTex, p ).xyz; 

            // return half4(fl, 0.);
            // return half4(grey(saturate(fc-fl)).xxx, 0.);
            // half3 fc =  tex2D( _MainTex, i.uvn).rgb;     //current frame without feedback
            // half3 fl =  tex2D( _LastTex, i.uvn).rgb;     //last frame without feedback
            float diff = grey(saturate(fc-fl)); //dfference between frames
            // float diff = len(fc,fl); //dfference between frames
            // float diff = len(fl,fc); //dfference between frames
            // float diff = abs(fl.x-fc.x + fl.y-fc.y + fl.z-fc.z)/3.; //dfference between frames
            if(diff<feedbackThresh) diff = 0.;

            half3 fbn = fc*diff*feedbackAmount; //feedback new
            // half3 fbn = fc*diff*feedbackAmount; //feedback new
            // fbn = half3(0.0, 0.0, 0.0);
            

            //old feedback buffer
            half3 fbb = ( //blur old buffer a bit
               SAMPLE_TEXTURE2D_X(_FeedbackTex, sampler_FeedbackTex, p ).xyz *.5 +
               SAMPLE_TEXTURE2D_X(_FeedbackTex, sampler_FeedbackTex, (p+ float2(one_x,0)) ).xyz *.25 +
               SAMPLE_TEXTURE2D_X(_FeedbackTex, sampler_FeedbackTex, (p- float2(one_x,0)) ).xyz *.25 
            );// / 3.;
            fbb *= feedbackFade;
            // if( (fbb.x+fbb.y+fbb.z)/3.<.01 ) fbb = half3(0,0,0);

            fbn = bms(fbn, fbb); 

            return float4(fbn * feedbackColor, 1.); //*feedbackColor 

         }


         ENDHLSL
      }
   } 
   FallBack Off
}
