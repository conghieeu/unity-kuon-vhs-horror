// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
// Modified version of: https://www.shadertoy.com/view/4dfBWn

Shader "UHFPS/UIHeartbeat"
{
    Properties
    {
        [PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}

        _LineColor("Line Color", Color) = (0.0, 1.0, 0.7, 1.5)
        _DefaultPulse("Default Pulse", Float) = 0.4
        _PulseMultiplier("Pulse Multiplier", Float) = 1
        _PulseFade("Pulse Fade", Float) = 0.25
        _PulseWidth("Pulse Width", Float) = 1
        _PulseRate("Pulse Rate", Float) = 1.3
        _LineThickness("Line Thickness", Float) = 10
        _GridThickness("Grid Thickness", Float) = 0.01

        [Toggle(USE_BACKGROUND)] _UseBackground("Use Background", Float) = 0
        _Color("Background Color", Color) = (1,1,1,1)

        [Toggle(USE_GRID)] _UseGrid("Use Grid", Float) = 0
        _GridColor("Grid Color", Color) = (1.0, 0.0, 0.0, 1.0)

        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255

        _GraphResolution("Graph Resolution", Float) = 512
        _ColorMask("Color Mask", Float) = 15

        _GridOffset("Grid Offset", Vector) = (0,0,0,0)
        _GridTiling("Grid Tiling", Vector) = (1,1,0,0)
        _GraphOffset("Graph Offset", Vector) = (0,0,0,0)
        _GraphTiling("Graph Tiling", Vector) = (1,1,1,1)

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
    }

        SubShader
        {
            Tags
            {
                "Queue" = "Transparent"
                "IgnoreProjector" = "True"
                "RenderType" = "Transparent"
                "PreviewType" = "Plane"
                "CanUseSpriteAtlas" = "True"
            }

            Stencil
            {
                Ref[_Stencil]
                Comp[_StencilComp]
                Pass[_StencilOp]
                ReadMask[_StencilReadMask]
                WriteMask[_StencilWriteMask]
            }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask[_ColorMask]

            Pass
            {
                Name "Default"
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma target 2.0

                #include "UnityCG.cginc"
                #include "UnityUI.cginc"

                #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
                #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

                #pragma shader_feature ZERO_PULSE
                #pragma shader_feature USE_BACKGROUND
                #pragma shader_feature USE_GRID

                #define AA_FALLOFF 1.2		    // AA falloff in pixels, must be > 0, affects all drawing
                #define FUNC_SAMPLE_STEP 0.05	// function sample step size in pixels
                #define SCOPE_RATE 0.5			// default oscilloscope refresh rate

                struct appdata_t
                {
                    float4 vertex   : POSITION;
                    float4 color    : COLOR;
                    float2 texcoord : TEXCOORD0;
                    UNITY_VERTEX_INPUT_INSTANCE_ID
                };

                struct v2f
                {
                    float4 vertex   : SV_POSITION;
                    fixed4 color : COLOR;
                    float2 texcoord  : TEXCOORD0;
                    float4 worldPosition : TEXCOORD1;
                    UNITY_VERTEX_OUTPUT_STEREO
                };

                sampler2D _MainTex;
                fixed4 _Color;
                fixed4 _LineColor;
                fixed4 _GridColor;
                float _DefaultPulse;
                float _PulseMultiplier;
                float _PulseFade;
                float _PulseWidth;
                float _PulseRate;
                float _LineThickness;
                float _GridThickness;

                fixed4 _TextureSampleAdd;
                float4 _ClipRect;
                float4 _MainTex_ST;

                float _GraphResolution;
                float4 _GridOffset;
                float4 _GridTiling;
                float4 _GraphOffset;
                float4 _GraphTiling;

                float pp;

                v2f vert(appdata_t v)
                {
                    v2f OUT;
                    UNITY_SETUP_INSTANCE_ID(v);
                    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                    OUT.worldPosition = v.vertex;
                    OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);

                    OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                    OUT.texcoord = OUT.texcoord;

                    #ifdef USE_BACKGROUND
                    OUT.color = v.color * _Color;
                    #else
                    _Color.a = 0;
                    OUT.color = v.color * _Color;
                    #endif

                    return OUT;
                }

                float sinc(float x)
                {
                    return (x == 0.0) ? 1.0 : sin(x) / x;
                }

                float triIsolate(float x)
                {
                    return abs(-1.0 + frac(clamp(x, -0.5, 0.5)) * 2.0);
                }

                float aaStep(float a, float b, float x)
                {
                    x = clamp(x, a, b);
                    return (x - a) / (b - a);
                }

                // heartbeat wave function
                float heartbeat(float x)
                {
                    float prebeat = -sinc((x - 0.4) * 40.0) * 0.6 * triIsolate((x - 0.4) * 1.0);
                    float mainbeat = (sinc((x - 0.5) * 60.0)) * 1.2 * triIsolate((x - 0.5) * 0.7);
                    float postbeat = sinc((x - 0.5) * 15.0) * 0.5 * triIsolate((x - 0.85) * 0.6);
                    return (prebeat + mainbeat + postbeat) * triIsolate((x - 0.625) * 0.8); // width 1.25
                }

                float drawCircle(float2 uv, float2 center, float radius) {
                    float r = length(uv-center);
                    float c = 1.0 - aaStep(0.0, radius + pp * AA_FALLOFF, r);
                    return c * c;
                }

                float func(float x)
                {
                    #ifdef ZERO_PULSE
                    return 0;
                    #else
                    return 0.5 * heartbeat(fmod((x * _PulseWidth), _PulseRate));
                    #endif
                }

                float drawGrid(float2 uv, float stepSize) {
                    float hlw = _GridThickness * pp * 0.5;
                    float mul = 1.0 / stepSize;
                    float2 gf = abs(float2(-0.5,-0.5) + frac((uv + float2(stepSize, stepSize) * 0.5) * mul));
                    return 1.0 - aaStep(hlw * mul, (hlw + pp * AA_FALLOFF) * mul, min(gf.x, gf.y));
                }

                float drawFunc(float2 uv) {
                    float hlw = _LineThickness * pp * 0.5;
                    float left = uv.x - hlw - pp * AA_FALLOFF;
                    float right = uv.x + hlw + pp * AA_FALLOFF;
                    float closest = 100000.0;

                    for (float x = left; x <= right; x += pp * FUNC_SAMPLE_STEP) {
                        float2 diff = float2(x, func(x)) - uv;
                        float dSqr = dot(diff, diff);
                        closest = min(dSqr, closest);
                    }

                    float c = 1.0 - aaStep(0.0, hlw + pp * AA_FALLOFF, sqrt(closest));
                    return c * c * c;
                }

                fixed4 frag(v2f IN) : SV_Target
                {
                    half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                    float graphRange = 0.4 + pow(1.2, _GraphOffset.z * _GraphOffset.z * _GraphOffset.z);
                    pp = graphRange / _GraphResolution;

                    // grid
                    #ifdef USE_GRID
                    float gridValue = drawGrid((IN.texcoord * _GridTiling) + _GridOffset, 0.1);
                    color = lerp(color, _GridColor, gridValue);
                    color.a = 1;
                    #endif

                    float rate = SCOPE_RATE * _DefaultPulse * _PulseMultiplier;
                    float pulse = frac(_Time.y * rate) * _GraphTiling.x;

                    float2 lineXY = _GraphOffset + IN.texcoord * _GraphTiling;
                    float fade = pulse - lineXY.x;

                    if (fade < 0.0) fade += _GraphTiling.x;
                    fade *= max(0.1, _PulseFade);
                    fade = clamp(fade / rate * max(1, _DefaultPulse * _PulseMultiplier), 0.0, 1.0);
                    fade = 1.0 - fade;
                    fade = fade * fade * fade;
                    fade *= step(0, lineXY.x) * step(lineXY.x, _GraphTiling.x);

                    float graphValue = drawFunc(lineXY);
                    color = lerp(color, _LineColor, graphValue * fade);

                    float circleValue = drawCircle(lineXY, float2(pulse, func(pulse)), _LineThickness * pp);
                    color = lerp(color, _LineColor, circleValue);

                    #ifdef UNITY_UI_CLIP_RECT
                    color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                    #endif

                    #ifdef UNITY_UI_ALPHACLIP
                    clip(color.a - 0.001);
                    #endif

                    return color;
                }
            ENDCG
            }
        }
}