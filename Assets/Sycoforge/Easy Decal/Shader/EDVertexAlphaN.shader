// Easy Decal shader using URP PBR workflow with Normal Strength. v2.0 (converted from Built-in RP)
Shader "Easy Decal/ED Standard Normal (Vertex Alpha)" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_BumpMap ("Normal Map", 2D) = "bump" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		_NormalStrength ("Normal Strength", Range(0.1,10)) = 1.0
	}
	SubShader 
	{
		Tags 
		{ 
			"Queue"="Transparent" 
			"RenderType"="Transparent" 
			"RenderPipeline"="UniversalPipeline"
			"ForceNoShadowCasting" = "True"
		}
		LOD 200
		Offset -1,-1

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Cull Back
		
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

			TEXTURE2D(_MainTex);    SAMPLER(sampler_MainTex);
			TEXTURE2D(_BumpMap);    SAMPLER(sampler_BumpMap);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				float4 _BumpMap_ST;
				half4 _Color;
				half _Glossiness;
				half _Metallic;
				half _NormalStrength;
			CBUFFER_END

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float3 normalOS     : NORMAL;
				float4 tangentOS    : TANGENT;
				float2 uv           : TEXCOORD0;
				float4 color        : COLOR;
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
				float4 color        : COLOR;
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
				output.color = input.color;
				output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);

				half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
				half4 normalSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv);
				half3 normalTS = UnpackNormalScale(normalSample, _NormalStrength);

				float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
				float3 normalWS = normalize(mul(normalTS, tangentToWorld));

				half finalAlpha = albedo.a * input.color.a * _Color.a;

				InputData inputData = (InputData)0;
				inputData.positionWS = input.positionWS;
				inputData.normalWS = normalWS;
				inputData.viewDirectionWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
				inputData.fogCoord = input.fogFactor;
				#if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
					inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif
				inputData.bakedGI = SampleSH(normalWS);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

				SurfaceData surfaceData = (SurfaceData)0;
				surfaceData.albedo = albedo.rgb;
				surfaceData.metallic = _Metallic;
				surfaceData.specular = half3(0, 0, 0);
				surfaceData.smoothness = _Glossiness;
				surfaceData.normalTS = normalTS;
				surfaceData.emission = half3(0, 0, 0);
				surfaceData.occlusion = 1.0;
				surfaceData.alpha = finalAlpha;

				half4 color = UniversalFragmentPBR(inputData, surfaceData);
				color.rgb = MixFog(color.rgb, input.fogFactor);
				color.a = finalAlpha;

				return color;
			}
			ENDHLSL
		}
	}
	FallBack "Universal Render Pipeline/Lit"
}
