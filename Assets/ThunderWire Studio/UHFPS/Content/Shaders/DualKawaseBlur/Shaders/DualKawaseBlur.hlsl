#ifndef DUALKAWASEBLUR_HLSL
#define DUALKAWASEBLUR_HLSL

uniform half _Offset;

struct Varyings_DownSample
{
    float4 positionCS : SV_Position;
    float2 uv : TEXCOORD0;

    // Extra UV variations
    float4 uv01 : TEXCOORD1;
    float4 uv23 : TEXCOORD2;

    UNITY_VERTEX_OUTPUT_STEREO
};

struct Varyings_UpSample
{
    float4 positionCS : SV_Position;
    float2 uv : TEXCOORD0;

    // Extra UV variations
    float4 uv01 : TEXCOORD1;
    float4 uv23 : TEXCOORD2;
    float4 uv45 : TEXCOORD3;
    float4 uv67 : TEXCOORD4;

    UNITY_VERTEX_OUTPUT_STEREO
};

// ---------------------------------------------------
// DOWN SAMPLE VERTEX
// ---------------------------------------------------
Varyings_DownSample Vert_DownSample(Attributes input)
{
    Varyings_DownSample o;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    // Get a full-screen triangle (or quad) position & UV
    float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
    float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);

    // We will shift the default UV to apply the blur
    float4 texelSize = _BlitTexture_TexelSize * 0.5;
    float2 offset = float2(1 + _Offset, 1 + _Offset) * _Offset;

    o.positionCS = pos;
    o.uv = uv;

    o.uv01.xy = uv - texelSize.xy * offset;
    o.uv01.zw = uv + texelSize.xy * offset;

    // Offsets in diagonals
    o.uv23.xy = uv - float2(texelSize.x, -texelSize.y) * offset;
    o.uv23.zw = uv + float2(texelSize.x, -texelSize.y) * offset;

    return o;
}

// ---------------------------------------------------
// DOWN SAMPLE FRAGMENT
// ---------------------------------------------------
half4 Frag_DownSample(Varyings_DownSample i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    // Weighted 5-tap blur (Dual Kawase step)
    half4 sum = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv) * 4.0h;
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv01.xy);
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv01.zw);
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv23.xy);
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv23.zw);

    // Average (4 + 1 taps = 5 → 1/8 = 0.125)
    return sum * 0.125h;
}

// ---------------------------------------------------
// UP SAMPLE VERTEX
// ---------------------------------------------------
Varyings_UpSample Vert_UpSample(Attributes input)
{
    Varyings_UpSample o;
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    float4 pos = GetFullScreenTriangleVertexPosition(input.vertexID);
    float2 uv = GetFullScreenTriangleTexCoord(input.vertexID);

    float4 texelSize = _BlitTexture_TexelSize * 0.5;
    float2 offset = float2(1 + _Offset, 1 + _Offset) * _Offset;

    o.positionCS = pos;
    o.uv = uv;

    // Sample offsets in cardinal + diagonal directions
    o.uv01.xy = uv + float2(-texelSize.x * 2, 0) * offset;
    o.uv01.zw = uv + float2(-texelSize.x, texelSize.y) * offset;

    o.uv23.xy = uv + float2(0, texelSize.y * 2) * offset;
    o.uv23.zw = uv + texelSize.xy * offset;

    o.uv45.xy = uv + float2(texelSize.x * 2, 0) * offset;
    o.uv45.zw = uv + float2(texelSize.x, -texelSize.y) * offset;

    o.uv67.xy = uv + float2(0, -texelSize.y * 2) * offset;
    o.uv67.zw = uv - texelSize.xy * offset;

    return o;
}

// ---------------------------------------------------
// UP SAMPLE FRAGMENT
// ---------------------------------------------------
half4 Frag_UpSample(Varyings_UpSample i) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

    // Weighted 8-tap blur
    half4 sum = 0;
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv01.xy);
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv01.zw) * 2.0h;
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv23.xy);
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv23.zw) * 2.0h;
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv45.xy);
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv45.zw) * 2.0h;
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv67.xy);
    sum += SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.uv67.zw) * 2.0h;

    return sum * 0.0833h;
}

#endif // DUALKAWASEBLUR_HLSL