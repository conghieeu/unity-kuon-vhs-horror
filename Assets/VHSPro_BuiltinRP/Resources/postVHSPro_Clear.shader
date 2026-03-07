// Converted from BRP to URP

Shader "Hidden/postVHSPro_Clear" {

	Properties {
		_MainTex ("Render Input", 2D) = "white" {} 		//current frame without feedback
	}

	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		ZTest Always Cull Off ZWrite Off

		Pass {
			HLSLPROGRAM

				//#pragma exclude_renderers d3d11 xbox360			
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 3.0
				#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

				//properties
				sampler2D _MainTex;
			
				struct appdata{
				   float4 vertex : POSITION;
				   float4 texcoord : TEXCOORD0;   	
				};

				struct v2f {
			   	float2 uv : TEXCOORD2; // non normilized coordinates -1..1
			   	float4 uvn : TEXCOORD1; //normilized coordicated 0..1				   	
			   	float4 pos : SV_POSITION; //The position of the vertex after being transformed into projection space
				};

				v2f vert (appdata i){
			   	v2f o;
			   	o.pos = TransformObjectToHClip( i.vertex.xyz );
			   	o.uv = o.pos.xy/o.pos.w;
			   	o.uvn = float4( i.texcoord.xy, 0, 0);			   	
			   	return o;
				}
				///// end of initial structures

				//main
				half4 frag( v2f i ) : SV_Target {
				   return half4(0.0, 0.0, 0.0, 1.0);  //black
				}

			ENDHLSL
		}
	}
}
