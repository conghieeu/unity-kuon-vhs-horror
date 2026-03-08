//-----------------------------------------------------
// Forward screen space decal includes. Version 0.8 [Beta]
// Copyright (c) 2016 by Sycoforge
//-----------------------------------------------------

#ifndef FSSD_SYCO_CG_INCLUDED
#define FSSD_SYCO_CG_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardInput.cginc"

#include "Util.cginc"
#include "SycoSSD.cginc"	

//uniform sampler2D _CameraDepthTexture;


//CBUFFER_START(UnityPerCamera2)
//float4x4 _CameraToWorld;
//CBUFFER_END

fixed4 _Tint;
fixed _Threshold;

//-------------------------
// FSSD
//-------------------------
#define FOG_MUL_SSD(color, text, st, input, localPos) \
	color =  ATLAS_TEX2D(_MainTex, st, input, localPos); \
	color = lerp(fixed4(1.0f, 1.0f, 1.0f, 1.0f), color * _Tint, color.a * input.color.a * _Tint.a); \
	UNITY_APPLY_FOG_COLOR(input.fogCoord, color, fixed4(1.0f, 1.0f, 1.0f, 1.0f))

#define FOG_MUL_CLIP_SSD(color, text, st, input, localPos) \
	color =  ATLAS_TEX2D(_MainTex, st, input, localPos); \
	color = lerp(fixed4(1.0f, 1.0f, 1.0f, 1.0f), color * _Tint, color.a *  _Tint.a); \
	UNITY_APPLY_FOG_COLOR(input.fogCoord, color, fixed4(1.0f, 1.0f, 1.0f, 1.0f))

#define FOG_ALPHA_SSD(color, text, st, input, localPos) \
	color =  ATLAS_TEX2D(_MainTex, st, input, localPos) * _Tint; \
	color.a = color.a * input.color.a * _Tint.a; \
	UNITY_APPLY_FOG_COLOR(input.fogCoord, color, fixed4(1.0f, 1.0f, 1.0f, 1.0f))

FragmentInputFSSD vert (VertexInputExt input)
{
	FragmentInputFSSD o;

	SETUP_BASIC_SSD(input, o)
	UNITY_TRANSFER_FOG(o, o.position);

	return o;
}

fixed4 fragMultiply(FragmentInputFSSD input) : SV_Target  
{
	fixed4 color;

	PROCESS_BASIC_SSD(input);
	//FOG_MUL_SSD(color, text, _MainTex_ST, input, localPos); 
	FOG_MUL_CLIP_SSD(color, text, _MainTex_ST, input, localPos);
	float a = input.color.a * color.a;

	clip(a - _Threshold);

	return color;
}

fixed4 fragAlpha(FragmentInputFSSD input) : SV_Target
{
	fixed4 color;

	PROCESS_BASIC_SSD(input);
	//FOG_MUL_SSD(color, text, _MainTex_ST, input, localPos); 
	FOG_ALPHA_SSD(color, text, _MainTex_ST, input, localPos);
	float a = input.color.a * color.a;

	clip(a - _Threshold);

	return color;
}

#endif // FSSD_SYCO_CG_INCLUDED
