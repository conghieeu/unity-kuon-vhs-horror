// Easy Decal shader using URP PBR workflow (converted from Legacy Reflect Bumped Specular Cut)
Shader "Easy Decal/Legacy/Reflect Bumped Specular Cut" 
{
	Properties 
	{
		_Color ("Main Color", Color) = (1,1,1,1)
		_SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
		_Shininess ("Shininess", Range (0.01, 1)) = 0.078125
		_Light ("Light Intesity", Range (0, 1)) = 0.5
		_ReflectColor ("Reflec. Color (RGB) Reflec. Intensity (A)", Color) = (1,1,1,0.5)
		_MainTex ("Base (RGB) Transparent (A)", 2D) = "white" {}
		_Cube ("Reflection Cubemap", Cube) = "" {}
		_BumpMap ("Normalmap", 2D) = "bump" {}
		_SpecMap ("Reflection (RGB) Spec (A) ", 2D) = "white" {}
		_Cutoff ("Base Alpha cutoff", Range (0,.9)) = .5
	}

	SubShader 
	{
		Tags 
		{ 
			"Queue"="AlphaTest" 
			"RenderType"="TransparentCutout" 
			"RenderPipeline"="UniversalPipeline"
			"ForceNoShadowCasting" = "True"
		}
		LOD 400
		Offset -1,-1

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
			TEXTURE2D(_SpecMap);    SAMPLER(sampler_SpecMap);
			TEXTURECUBE(_Cube);     SAMPLER(sampler_Cube);

			CBUFFER_START(UnityPerMaterial)
				float4 _MainTex_ST;
				float4 _BumpMap_ST;
				half4 _Color;
				half4 _SpecColor; // URP defines _SpecColor internally but for custom properties we map it
				half4 _ReflectColor;
				half _Shininess;
				half _Light;
				half _Cutoff;
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

			half4 frag(Varyings input) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(input);

				half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
				half4 spec = SAMPLE_TEXTURE2D(_SpecMap, sampler_SpecMap, input.uv);
				half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv));

				half4 c = tex * _Color;
				clip(c.a - _Cutoff);

				float3x3 tangentToWorld = float3x3(input.tangentWS, input.bitangentWS, input.normalWS);
				float3 normalWS = normalize(mul(normalTS, tangentToWorld));

				// Reflection mapping approach matching legacy logic mostly
				// o.Emission = ((c.rgb * cc) + (reflcol.rgb * _ReflectColor.rgb * (1 - spec.r)));
				float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
				float3 worldRefl = reflect(-viewDirWS, normalWS);
				half4 reflcol = SAMPLE_TEXTURECUBE(_Cube, sampler_Cube, worldRefl);
				reflcol *= _ReflectColor.a;

				half lightFactor = (_Light <= 0.001) ? 1.0 : (1.0 - _Light);
				half3 emission = (c.rgb * lightFactor) + (reflcol.rgb * _ReflectColor.rgb * (1.0 - spec.r));

				InputData inputData = (InputData)0;
				inputData.positionWS = input.positionWS;
				inputData.normalWS = normalWS;
				inputData.viewDirectionWS = viewDirWS;
				inputData.fogCoord = input.fogFactor;
				#if defined(_MAIN_LIGHT_SHADOWS) || defined(_MAIN_LIGHT_SHADOWS_CASCADE)
					inputData.shadowCoord = TransformWorldToShadowCoord(input.positionWS);
				#else
					inputData.shadowCoord = float4(0, 0, 0, 0);
				#endif
				inputData.bakedGI = SampleSH(normalWS);
				inputData.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(input.positionCS);

				SurfaceData surfaceData = (SurfaceData)0;
				surfaceData.albedo = c.rgb;
				surfaceData.metallic = 0;
				surfaceData.specular = _SpecColor.rgb * spec.a; // URP Specular
				surfaceData.smoothness = _Shininess;
				surfaceData.normalTS = normalTS;
				surfaceData.emission = emission;
				surfaceData.occlusion = 1.0;
				surfaceData.alpha = c.a;

				half4 finalColor = UniversalFragmentPBR(inputData, surfaceData);
				finalColor.rgb = MixFog(finalColor.rgb, input.fogFactor);
				finalColor.a = c.a;

				return finalColor;
			}
			ENDHLSL
		}
	}
	FallBack "Universal Render Pipeline/Lit"
}
