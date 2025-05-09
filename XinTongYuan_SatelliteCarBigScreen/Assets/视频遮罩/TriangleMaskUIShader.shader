Shader "AVProVideo/Lit/custom1"
{
    Properties
    {
        _MainTex("_MainTex", 2D) = "white" {}
        _FilterTex("_FilterTex", 2D) = "white" {}
        _MovTex1("_MovTex1", 2D) = "black" {}
        _FilterTex1("_FilterTex1", 2D) = "black" {}
        _MovTex2("_MovTex2", 2D) = "black" {}
        _FilterTex2("_FilterTex2", 2D) = "black" {}
        _MovTex3("_MovTex3", 2D) = "black" {}
        _FilterTex3("_FilterTex3", 2D) = "black" {}
        _MovTex4("_MovTex4", 2D) = "black" {}
        _FilterTex4("_FilterTex4", 2D) = "black" {}
        _MovTex5("_MovTex5", 2D) = "black" {}
        _FilterTex5("_FilterTex5", 2D) = "black" {}
        [HideInInspector]_Cutoff("Alpha cutoff", Range(0,1)) = 0.5
    }

    HLSLINCLUDE
    #pragma target 4.5
    #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch

    // Include HDRP common files
    #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/FragInputs.hlsl"
    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/ShaderPass/ShaderPass.cs.hlsl"
    
    TEXTURE2D(_MainTex);       SAMPLER(sampler_MainTex);
    TEXTURE2D(_FilterTex);     SAMPLER(sampler_FilterTex);
    TEXTURE2D(_MovTex1);       SAMPLER(sampler_MovTex1);
    TEXTURE2D(_FilterTex1);    SAMPLER(sampler_FilterTex1);
    TEXTURE2D(_MovTex2);       SAMPLER(sampler_MovTex2);
    TEXTURE2D(_FilterTex2);    SAMPLER(sampler_FilterTex2);
    TEXTURE2D(_MovTex3);       SAMPLER(sampler_MovTex3);
    TEXTURE2D(_FilterTex3);    SAMPLER(sampler_FilterTex3);
    TEXTURE2D(_MovTex4);       SAMPLER(sampler_MovTex4);
    TEXTURE2D(_FilterTex4);    SAMPLER(sampler_FilterTex4);
    TEXTURE2D(_MovTex5);       SAMPLER(sampler_MovTex5);
    TEXTURE2D(_FilterTex5);    SAMPLER(sampler_FilterTex5);
    
    float4 _MainTex_ST;
    float4 _FilterTex_ST;
    float4 _MovTex1_ST;
    float4 _FilterTex1_ST;
    float4 _MovTex2_ST;
    float4 _FilterTex2_ST;
    float4 _MovTex3_ST;
    float4 _FilterTex3_ST;
    float4 _MovTex4_ST;
    float4 _FilterTex4_ST;
    float4 _MovTex5_ST;
    float4 _FilterTex5_ST;
    ENDHLSL

    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "Transparent" "Queue" = "Transparent" }
        
        Pass
        {
            Name "ForwardUnlit"
            Tags { "LightMode" = "ForwardOnly" }
            
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };
            
            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }
            
            float4 Frag(Varyings input) : SV_Target
            {
                float2 mainTexUV = input.uv * _MainTex_ST.xy + _MainTex_ST.zw;
                float2 filterTexUV = input.uv * _FilterTex_ST.xy + _FilterTex_ST.zw;
                float2 movTex1UV = input.uv * _MovTex1_ST.xy + _MovTex1_ST.zw;
                float2 filterTex1UV = input.uv * _FilterTex1_ST.xy + _FilterTex1_ST.zw;
                float2 movTex2UV = input.uv * _MovTex2_ST.xy + _MovTex2_ST.zw;
                float2 filterTex2UV = input.uv * _FilterTex2_ST.xy + _FilterTex2_ST.zw;
                float2 movTex3UV = input.uv * _MovTex3_ST.xy + _MovTex3_ST.zw;
                float2 filterTex3UV = input.uv * _FilterTex3_ST.xy + _FilterTex3_ST.zw;
                float2 movTex4UV = input.uv * _MovTex4_ST.xy + _MovTex4_ST.zw;
                float2 filterTex4UV = input.uv * _FilterTex4_ST.xy + _FilterTex4_ST.zw;
                float2 movTex5UV = input.uv * _MovTex5_ST.xy + _MovTex5_ST.zw;
                float2 filterTex5UV = input.uv * _FilterTex5_ST.xy + _FilterTex5_ST.zw;
                
                float4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, mainTexUV);
                float4 filterTex = SAMPLE_TEXTURE2D(_FilterTex, sampler_FilterTex, filterTexUV);
                float4 movTex1 = SAMPLE_TEXTURE2D(_MovTex1, sampler_MovTex1, movTex1UV);
                float4 filterTex1 = SAMPLE_TEXTURE2D(_FilterTex1, sampler_FilterTex1, filterTex1UV);
                float4 movTex2 = SAMPLE_TEXTURE2D(_MovTex2, sampler_MovTex2, movTex2UV);
                float4 filterTex2 = SAMPLE_TEXTURE2D(_FilterTex2, sampler_FilterTex2, filterTex2UV);
                float4 movTex3 = SAMPLE_TEXTURE2D(_MovTex3, sampler_MovTex3, movTex3UV);
                float4 filterTex3 = SAMPLE_TEXTURE2D(_FilterTex3, sampler_FilterTex3, filterTex3UV);
                float4 movTex4 = SAMPLE_TEXTURE2D(_MovTex4, sampler_MovTex4, movTex4UV);
                float4 filterTex4 = SAMPLE_TEXTURE2D(_FilterTex4, sampler_FilterTex4, filterTex4UV);
                float4 movTex5 = SAMPLE_TEXTURE2D(_MovTex5, sampler_MovTex5, movTex5UV);
                float4 filterTex5 = SAMPLE_TEXTURE2D(_FilterTex5, sampler_FilterTex5, filterTex5UV);
                
                // Start with main texture and base alpha
                float4 finalColor = float4(mainTex.rgb, filterTex.r);
                
                // Apply each layer based on its filter texture
                finalColor = lerp(finalColor, movTex1, filterTex1.r);
                finalColor = lerp(finalColor, movTex2, filterTex2.r);
                finalColor = lerp(finalColor, movTex3, filterTex3.r);
                finalColor = lerp(finalColor, movTex4, filterTex4.r);
                finalColor = lerp(finalColor, movTex5, filterTex5.r);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}