// Forward screen space decal shader (URP). Converted to HLSL.
Shader "Easy Decal/SSD/Unlit Multiply SSD" 
{
	Properties 
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Tint("Tint (RGBA)", Color) = (1,1,1,1)
		_Threshold ("Clipping Threshold", Range(0,1)) = 0
	}
	SubShader 
	{
		Tags 
		{ 
			"RenderType"= "Transparent" 
			"RenderPipeline" = "UniversalPipeline"
			"Queue" = "Transparent+1" 
			"DisableBatching" = "True" 
		}

		Stencil
		{
			Ref 1
			Comp notequal
			Pass keep
		}

		ZWrite Off 
		ZTest Always
		Lighting Off
		Cull Front
		Blend Zero SrcColor
		Offset -1,-1

		Pass
		{		
			Name "Forward"
            Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

			TEXTURE2D(_MainTex);
			SAMPLER(sampler_MainTex);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				half4 _Tint;
				half _Threshold;
			CBUFFER_END

			struct Attributes
			{
				float4 positionOS : POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				float4 color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS : SV_POSITION;
				float4 screenPos : TEXCOORD0;
				float4 uv : TEXCOORD1; // xy = scale, zw = offset (from vertex)
				float4 color : COLOR;
				float fogFactor : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			Varyings vert(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

				output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
				output.screenPos = ComputeScreenPos(output.positionCS);
				output.uv = float4(input.uv, input.uv2);
				output.color = input.color;
				output.fogFactor = ComputeFogFactor(output.positionCS.z);

				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

				float2 screenUV = input.screenPos.xy / input.screenPos.w;
				
				#if UNITY_REVERSED_Z
					real depth = SampleSceneDepth(screenUV);
				#else
					real depth = lerp(UNITY_NEAR_CLIP_VALUE, 1, SampleSceneDepth(screenUV));
				#endif

				float3 worldPos = ComputeWorldSpacePosition(screenUV, depth, UNITY_MATRIX_I_VP);
				float3 localPos = TransformWorldToObject(worldPos);

				// Discriminate pixels outside the decal volume
				clip(0.5 - abs(localPos));

				// Map xz in [-0.5, 0.5] to UV [0, 1]
				float2 projectedUV = localPos.xz + 0.5;
				
				// Apply mesh-specific atlas transform
				float2 finalUV = (input.uv.xy * _MainTex_ST.xy) * projectedUV + (input.uv.zw + _MainTex_ST.zw);

				half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, finalUV);
				
				half4 color = texColor * _Tint * input.color;
				
				clip((color.a * input.color.a) - _Threshold);

				color.rgb = MixFog(color.rgb, input.fogFactor);

				return color;
			}
			ENDHLSL
		}
	} 
	FallBack "Hidden/InternalErrorShader"
}
