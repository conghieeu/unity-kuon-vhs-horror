// Easy Decal SSD masking shader using URP PBR workflow. v2.0 (converted from Built-in RP)
Shader "Easy Decal/SSD/Standard DSSD Mask Double Sided (Metallic)" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MetallicMap ("Metallic (R) Smoothness (A)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_Occlusion ("Ambient Occlusion (R)", 2D) = "white" {}
		_Metallic ("Metallic Multiplier", Range(0,1)) = 1.0
		_Smoothness ("Smoothness Multiplier", Range(0,1)) = 1.0
		_SSDMasking("Decal Masking", Range(0.0, 1.0)) = 1.0
	}

	SubShader 
	{
		Tags 
		{ 
			"RenderType"="Opaque"
			"RenderPipeline"="UniversalPipeline"
		}
		LOD 200

		Cull Off

		Pass
		{
			Name "ForwardLit"
			Tags { "LightMode"="UniversalForward" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
			#pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
			#pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS
			#pragma multi_compile _ _SHADOWS_SOFT
			#pragma multi_compile_fog
			#pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			TEXTURE2D(_MainTex);        SAMPLER(sampler_MainTex);
			TEXTURE2D(_MetallicMap);    SAMPLER(sampler_MetallicMap);
			TEXTURE2D(_BumpMap);        SAMPLER(sampler_BumpMap);
			TEXTURE2D(_Occlusion);      SAMPLER(sampler_Occlusion);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				float4 _BumpMap_ST;
				half4 _Color;
				half _Metallic;
				half _Smoothness;
				half _SSDMasking;
			CBUFFER_END

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float3 normalOS     : NORMAL;
				float4 tangentOS    : TANGENT;
				float2 uv           : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float4 positionCS   : SV_POSITION;
				float2 uv           : TEXCOORD0;
				float3 positionWS   : TEXCOORD1;
				float3 normalWS     : TEXCOORD2;
				float3 tangentWS    : TEXCOORD3;
				float3 bitangentWS  : TEXCOORD4;
				float fogFactor     : TEXCOORD5;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			Varyings vert(Attributes input)
			{
				Varyings output;
				UNITY_SETUP_INSTANCE_ID(input);
				UNITY_TRANSFER_INSTANCE_ID(input, output);

				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
				VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);

				output.positionCS = vertexInput.positionCS;
				output.positionWS = vertexInput.positionWS;
				output.normalWS = normalInput.normalWS;
				output.tangentWS = normalInput.tangentWS;
				output.bitangentWS = normalInput.bitangentWS;

				output.uv = TRANSFORM_TEX(input.uv, _MainTex);
				output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

				return output;
			}

			half4 frag(Varyings input, float facing : VFACE) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);

				half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
				half4 metallicMap = SAMPLE_TEXTURE2D(_MetallicMap, sampler_MetallicMap, input.uv);
				half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv));
				half ao = SAMPLE_TEXTURE2D(_Occlusion, sampler_Occlusion, input.uv).r;

				float3 normalWS = input.normalWS;
				float3 tangentWS = input.tangentWS;
				float3 bitangentWS = input.bitangentWS;

				// Handle double-sided normals
				if(facing < 0) {
					normalWS = -normalWS;
					tangentWS = -tangentWS;
					bitangentWS = -bitangentWS;
				}

				float3x3 tangentToWorld = float3x3(tangentWS, bitangentWS, normalWS);
				float3 finalNormalWS = normalize(mul(normalTS, tangentToWorld));

				half3 finalAlbedo = albedo.rgb * _Color.rgb;
				half finalMetallic = metallicMap.r * _Metallic;
				half finalSmoothness = metallicMap.a * _Smoothness;

				InputData inputData = (InputData)0;
				inputData.positionWS = input.positionWS;
				inputData.normalWS = finalNormalWS;
				inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
				inputData.fogCoord = input.fogFactor;
				#if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
					inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif
				inputData.bakedGI = SampleSH(finalNormalWS);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

				SurfaceData surfaceData = (SurfaceData)0;
				surfaceData.albedo = finalAlbedo;
				surfaceData.metallic = finalMetallic;
				surfaceData.specular = half3(0, 0, 0);
				surfaceData.smoothness = finalSmoothness;
				surfaceData.normalTS = normalTS;
				surfaceData.emission = half3(0, 0, 0);
				surfaceData.occlusion = ao;
				surfaceData.alpha = albedo.a;

				half4 color = UniversalFragmentPBR(inputData, surfaceData);
				color.rgb = MixFog(color.rgb, input.fogFactor);
				color.a = 1.0;

				return color;
			}
			ENDHLSL
		}

		// Depth Only Pass for shadows etc
		UsePass "Universal Render Pipeline/Lit/DepthOnly"
		// Shadow Caster Pass
		UsePass "Universal Render Pipeline/Lit/ShadowCaster"
		// Meta pass
		UsePass "Universal Render Pipeline/Lit/Meta"
	}
	FallBack "Universal Render Pipeline/Lit"
}
