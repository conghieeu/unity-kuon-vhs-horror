Shader "UHFPS/EyeBlink"
{
    Properties
    {
        _VignetteOuterRing("Vignette Outer Ring", Range(0, 1)) = 0.4
        _VignetteInnerRing("Vignette Inner Ring", Range(0, 1)) = 0.5
        _VignetteAspectRatio("Vignette Aspect Ratio", Range(0, 1)) = 1.0
        _Blink("Blink", Range(0, 1)) = 0
    }

    SubShader
    {
        ZWrite Off
        ZTest Always
        Blend Off
        Cull Off

        Pass
        {
            Name "EyeBlink"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _VignetteOuterRing;
            float _VignetteInnerRing;
            float _VignetteAspectRatio;
            float _Blink;

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
                float aspectRatio = _VignetteAspectRatio + 1.0 / (1.0 - _Blink) - 1.0;
                float2 vd = float2(input.texcoord.x - 0.5, (input.texcoord.y - 0.5) * aspectRatio);

                float vb = 1.0 - _Blink * 2.0;
                vb = max(0.0, vb);

                float outerRing = 1.0 - _VignetteOuterRing;
                float innerRing = 1.0 - _VignetteInnerRing;

                if (innerRing >= outerRing) {
                    innerRing = outerRing - 0.0001f;
                }

                float vignetteEffect = saturate((dot(vd, vd) - outerRing) / (innerRing - outerRing));
                vignetteEffect = saturate(vignetteEffect - (_Blink * 0.5));

                float3 vcolor = lerp(color.rgb * vb, color.rgb, vignetteEffect);
                return half4(vcolor, color.a);
            }

            ENDHLSL
        }
    }
}
