Shader "UHFPS/FearTentacles"
{
    SubShader
    {
        ZWrite Off
        ZTest Always
        Blend Off
        Cull Off

        Pass
        {
            Name "FearTentacles"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _EffectTime;
            float _EffectFade;
            float _TentaclesPosition;
            float _LayerPosition;
            float _VignetteStrength;
            int _NumOfTentacles;
            int _ShowLayer;

            float rand(float2 n) 
            {
                return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
            }

            float tent(float2 uv, float id) 
            {
                float iTime = _EffectTime;
                float tentPos = clamp(_TentaclesPosition, -0.2, 0.2);
                float offset = tentPos * sign(uv.y - 0.5);
                float rv = rand(float2(id + sign(uv.y - 0.5), id));
                float2 st = uv;

                uv.y += offset;
                uv.x += rv * 0.1;
                uv.y += 0.05;

                float r = min(0.45 + _EffectFade * 0.02, 0.48) + (rv - 0.5) * 0.0;
                r += abs(uv.y - 0.5) * (0.1 + (rv - 0.5) * 0.05);

                uv.x += sin(uv.y * (3.0 + rv) + iTime * rv * 2.0 + rv * 3.0 + id * 20.0) * (0.1 + (sin(rv) * 0.1) * 0.1);
                uv.x += uv.y * (rv - 0.5) * 0.4;
                uv.y += 0.05;
                uv.y += sin(uv.x * 20.0) * 0.05;
                uv.y += sin(st.x * rv * rv * 120.0 + iTime + rv + id) * 0.05 * rv;

                float lay = 1.0;
                if (_ShowLayer) 
                {
                    uv.y -= offset;
                    uv.y += _LayerPosition * sign(uv.y - 0.5);
                    lay = lerp(1., 0.82, smoothstep(0.57, 0.6, abs(uv.y - 0.5)));
                }

                return (1.0 - smoothstep(r, r - 0.05, abs(uv.x - 0.5) + 0.5)) * lay;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, input.texcoord);
                float2 uv = input.texcoord;
                float s = 1.0;

                for (int i = 0; i < 100; i++)
                {
                    if (i >= _NumOfTentacles) break;
                    float2 randValue = float2(rand(float2(float(i), 0.0)) - 0.5, 0.01);
                    s *= tent(uv + randValue, sin(float(i) * 200.0) * 2003.0);
                }

                uv *= 1.0 - uv.yx;
                float vig = uv.x * uv.y * 5.0;
                vig = pow(vig, _EffectFade * _VignetteStrength);

                s = lerp(1.0, s, _EffectFade);
                s *= vig;

                return color * half4(s, s, s, 1.);
            }

            ENDHLSL
        }
    }
}
