Shader "UHFPS/DualKawaseBlur"
{
    HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
        #include "DualKawaseBlur.hlsl"
    ENDHLSL

    SubShader
    {
        Cull Off
        ZWrite Off
        ZTest Always

        // --- Downsample Pass ---
        Pass
        {
            HLSLPROGRAM
                #pragma vertex Vert_DownSample
                #pragma fragment Frag_DownSample
            ENDHLSL
        }

        // --- Upsample Pass ---
        Pass
        {
            HLSLPROGRAM
                #pragma vertex Vert_UpSample
                #pragma fragment Frag_UpSample
            ENDHLSL
        }
    }
}