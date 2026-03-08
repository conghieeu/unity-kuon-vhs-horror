// Easy Decal shader using URP Specular PBR workflow. v2.0 (converted from Built-in RP)
Shader "Easy Decal/ED Standard (Specular, Vertex Alpha)" 
{
	Properties 
	{
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_SpecMap ("Specular (RGB) Smoothness (A)", 2D) = "white" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_Occlusion ("Ambient Occlusion (R)", 2D) = "white" {}
		_Specularity ("Specular Multiplier", Range(0,1)) = 1.0
		_Smoothness ("Smoothness Multiplier", Range(0,1)) = 1.0
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
			TEXTURE2D(_SpecMap);    SAMPLER(sampler_SpecMap);
			TEXTURE2D(_BumpMap);    SAMPLER(sampler_BumpMap);
			TEXTURE2D(_Occlusion);  SAMPLER(sampler_Occlusion);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				float4 _BumpMap_ST;
				half4 _Color;
				half _Specularity;
				half _Smoothness;
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
				float2 uvBump       : TEXCOORD1;
				float3 positionWS   : TEXCOORD2;
				float3 normalWS     : TEXCOORD3;
				float3 tangentWS    : TEXCOORD4;
				float3 bitangentWS  : TEXCOORD5;
				float4 color        : COLOR;
				float fogFactor     : TEXCOORD6;
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
				output.uvBump = TRANSFORM_TEX(input.uv, _BumpMap);
				output.color = input.color;
				output.fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

				return output;
			}

			half4 frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);

				half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
				half4 specMap = SAMPLE_TEXTURE2D(_SpecMap, sampler_SpecMap, input.uv);
				half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uvBump));
				half ao = SAMPLE_TEXTURE2D(_Occlusion, sampler_Occlusion, input.uvBump).r;

				float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
				float3 normalWS = normalize(mul(normalTS, tangentToWorld));

				half3 finalAlbedo = albedo.rgb * input.color.rgb * _Color.rgb;
				half3 finalSpecular = specMap.rgb * _Specularity;
				half finalSmoothness = specMap.a * _Smoothness;
				half finalAlpha = albedo.a * input.color.a;

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
				surfaceData.albedo = finalAlbedo;
				surfaceData.metallic = 0;
				surfaceData.specular = finalSpecular;
				surfaceData.smoothness = finalSmoothness;
				surfaceData.normalTS = normalTS;
				surfaceData.emission = half3(0, 0, 0);
				surfaceData.occlusion = ao;
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
