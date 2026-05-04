Shader "UHFPS/Scanlines"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _Intensity("Intensity", Range(0,1)) = 0.1
        _Frequency("Frequency", Range(0,100)) = 10
        _ScrollSpeed("Scroll Speed", Range(0,1)) = 1
        [Toggle(GLITCHY)] _Glitchy("Use Glitchy Effect", Int) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            #pragma shader_feature GLITCHY

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _Intensity;
            float _Frequency;
            float _ScrollSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                float scroll = _Time.y * _ScrollSpeed;
                float scanlines = sin((i.uv.y + scroll) * _Frequency * 2.0 * 3.1416);

                #ifdef GLITCHY
                float2 offset = float2(sin(i.uv.y * _Frequency), cos(i.uv.y * _Frequency)) * _Intensity;
                fixed4 colR = tex2D(_MainTex, i.uv + float2(offset.x, 0.0));
                fixed4 colG = tex2D(_MainTex, i.uv + float2(0.0, offset.y));
                fixed4 colB = tex2D(_MainTex, i.uv - offset);

                col.r = lerp(col.r, colR.r * scanlines, _Intensity);
                col.g = lerp(col.g, colG.g * scanlines, _Intensity);
                col.b = lerp(col.b, colB.b * scanlines, _Intensity);
                #else
                col.rgb = lerp(col.rgb, col.rgb * scanlines, _Intensity);
                #endif

                return col;
            }
            ENDCG
        }
    }
}