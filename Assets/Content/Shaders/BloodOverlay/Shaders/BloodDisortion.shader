Shader "UHFPS/BloodDistortion"
{
    Properties
    {
        _BlendColor("Blend Color", Color) = (1,1,1,1)
        _OverlayColor("Overlay Color", Color) = (1,1,1,1)
        _BlendTex("Image", 2D) = "white" {}
        _BumpMap("Normal", 2D) = "bump" {}
        _BlendAmount("Blend Amount", Range(0,1)) = 0.5
        _EdgeSharpness("Edge Sharpness", Range(0,1)) = 0.5
        _Distortion("Distortion", Range(0,1)) = 0.5
        _BloodAmount("Blood Amount", Range(0,1)) = 0.5
    }

    SubShader
    {
        ZWrite Off
        ZTest Always
        Blend Off
        Cull Off

        Pass
        {
            Name "BloodDistortion"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            // Additional textures
            TEXTURE2D(_BlendTex);
            SAMPLER(sampler_BlendTex);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            // Uniform properties
            float4 _BlendColor;
            float4 _OverlayColor;
            float _BlendAmount;
            float _EdgeSharpness;
            float _Distortion;
            float _BloodAmount;

            half4 Frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Sample camera color
                float4 blendColor = SAMPLE_TEXTURE2D(_BlendTex, sampler_BlendTex, i.texcoord);
                float4 bumpColor = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.texcoord);

                // Adjust alpha to control blend edges
                blendColor.a = blendColor.a + (_BlendAmount * 2.0 - 1.0);
                blendColor.a = saturate(blendColor.a * _EdgeSharpness - (_EdgeSharpness - 1.0) * 0.5);

                // Unpack normal (RG channels) for distortion
                half2 bump = UnpackNormal(bumpColor).rg;
                float2 distortedtexcoord = i.texcoord + bump * blendColor.a * _Distortion;

                float4 mainColor = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, distortedtexcoord);
                float4 overlayColor = blendColor;

                overlayColor.rgb = mainColor.rgb * (blendColor.rgb + 0.5) * 1.2;
                blendColor = lerp(blendColor, overlayColor, 0.3);
                mainColor.rgb *= (1.0 - blendColor.a * 0.5);

                float4 overlay = lerp(float4(1,1,1,1), _OverlayColor, _BloodAmount);
                return lerp(mainColor, blendColor * _BlendColor, blendColor.a) * overlay;
            }

            ENDHLSL
        }
    }
}
