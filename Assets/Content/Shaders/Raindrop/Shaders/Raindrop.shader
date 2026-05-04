Shader "UHFPS/Raindrop"
{
    Properties
    {
        _DropletsMask("Droplets Mask", 2D) = "white" {}
        _Tiling("Tiling", Vector) = (1, 1, 0, 0)
        _Raining("Raining", Range(0, 1)) = 1
        _Distortion("Distortion", Float) = 1.95
        _GlobalRotation("Global Rotation", Range(-180, 180)) = 0
        _DropletsGravity("Droplets Gravity", Range(0, 1)) = 0
        _DropletsSpeed("Droplets Speed", Range(0, 2)) = 1
        _DropletsStrength("Droplets Strength", Range(0, 1)) = 1
    }

    SubShader
    {
        ZWrite Off
        ZTest Always
        Blend Off
        Cull Off

        Pass
        {
            Name "Raindrop"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            TEXTURE2D(_DropletsMask);
            SAMPLER(sampler_DropletsMask);

            float2 _Tiling;
            float _Raining;
            float _Distortion;
            float _GlobalRotation;
            float _DropletsGravity;
            float _DropletsSpeed;
            float _DropletsStrength;

            float Remap(float In, float2 InMinMax, float2 OutMinMax)
            {
                return OutMinMax.x + (In - InMinMax.x) * (OutMinMax.y - OutMinMax.x) / (InMinMax.y - InMinMax.x);
            }

            float2 Rotate(float2 UV, float2 Center, float Rotation)
            {
                UV -= Center;
                float s = sin(Rotation);
                float c = cos(Rotation);
                float2x2 rMatrix = float2x2(c, -s, s, c);
                rMatrix *= 0.5;
                rMatrix += 0.5;
                rMatrix = rMatrix * 2 - 1;
                UV.xy = mul(UV.xy, rMatrix);
                UV += Center;
                return UV;
            }

            float4 ComputeRaindrops(float4 raindropMask)
            {
                float droplets = Remap(raindropMask.b, float2(0, 1), float2(-1, 1));
                droplets += _Time.y * _DropletsSpeed;
                droplets = frac(droplets);
                droplets = raindropMask.a - droplets;

                float2 dropletStrength = float2(1.0 - _DropletsStrength, 1.0);
                droplets = Remap(droplets, dropletStrength, float2(0, 1));
                droplets = ceil(droplets);
                droplets = saturate(droplets);

                float4 splitMask = raindropMask * 2.0 - 1.0;
                return splitMask * droplets;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.texcoord * _Tiling;
                float2 raindropGravity = float2(0.0, _DropletsGravity);
                float2 raindropPan = _Time.y * raindropGravity + uv;
                float2 rotation = Rotate(raindropPan, float2(0.5, 0.5), radians(_GlobalRotation));

                float4 raindropMask = _DropletsMask.Sample(sampler_DropletsMask, rotation);
                float4 raindrops = ComputeRaindrops(raindropMask);

                float4 rain = raindrops * _Raining;
                rain *= _Distortion;

                float2 distortedCoords = input.texcoord + rain.xy;
                float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedCoords);
                return color;
            }

            ENDHLSL
        }
    }
}
