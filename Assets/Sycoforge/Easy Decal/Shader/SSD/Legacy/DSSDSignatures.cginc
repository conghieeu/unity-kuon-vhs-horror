//-----------------------------------------------------
// Deferred screen space decals function signatures. Version 0.1 [Beta]
// Copyright (c) 2017 by Sycoforge
//-----------------------------------------------------

#ifndef SSD_SYCO_DSSDSIG_CG_INCLUDED
#define SSD_SYCO_DSSDSIG_CG_INCLUDED


#define SIGNATURE_DSN \
	out float4 outDiffuse		: SV_Target0, \
	out float4 outSpecSmooth	: SV_Target1, \
	out float4 outNormal		: SV_Target2, \
	out float4 outEmission		: SV_Target3) \

#define SIGNATURE_DS \
	out float4 outDiffuse		: SV_Target0, \
	out float4 outSpecSmooth	: SV_Target1, \
	out float4 outEmission		: SV_Target2) \

#define SIGNATURE_DN \
	out float4 outDiffuse		: SV_Target0, \
	out float4 outNormal		: SV_Target1, \
	out float4 outEmission		: SV_Target2) \
	
#define SIGNATURE_SN \
	out float4 outSpecSmooth	: SV_Target0, \
	out float4 outNormal		: SV_Target1, \
	out float4 outEmission		: SV_Target2) \

#define SIGNATURE_D \
	out float4 outDiffuse		: SV_Target0, \
	out float4 outEmission		: SV_Target1) \

#define SIGNATURE_S \
	out float4 outSpecSmooth	: SV_Target0, \
	out float4 outEmission		: SV_Target1) \

#define SIGNATURE_N \
	out float4 outNormal		: SV_Target0, \
	out float4 outEmission		: SV_Target1) \


#endif // SSD_SYCO_DSSDSIG_CG_INCLUDED