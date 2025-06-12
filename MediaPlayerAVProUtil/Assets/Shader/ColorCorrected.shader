// AVProVideo DisplayUGUI Color Corrected Shader
// 专门解决AVProVideo在Unity Linear Color Space下的色差问题

Shader "AVProVideo/UI/ColorCorrected" {
    Properties {
        _MainTex ("Main Texture", 2D) = "white" {}
        _FilterTex("Filter Texture", 2D) = "white" {}
        _MovTex1("Movie Texture 1", 2D) = "black" {}
        _FilterTex1("Filter Texture 1", 2D) = "black" {}
        _MovTex2("Movie Texture 2", 2D) = "black" {}
        _FilterTex2("Filter Texture 2", 2D) = "black" {}
        _MovTex3("Movie Texture 3", 2D) = "black" {}
        _FilterTex3("Filter Texture 3", 2D) = "black" {}
        _MovTex4("Movie Texture 4", 2D) = "black" {}
        _FilterTex4("Filter Texture 4", 2D) = "black" {}
        _MovTex5("Movie Texture 5", 2D) = "black" {}
        _FilterTex5("Filter Texture 5", 2D) = "black" {}
        
        // 核心颜色校正参数
        [Toggle] _ApplyColorCorrection ("Apply Color Correction", Float) = 1
        [Toggle] _ForceGammaCorrection ("Force Gamma Correction", Float) = 1
        _ColorBalance ("Color Balance (RGB)", Color) = (1,1,1,1)
        _Brightness ("Brightness", Range(0.1,3)) = 1
        _Contrast ("Contrast", Range(0.1,3)) = 1
        _Saturation ("Saturation", Range(0,2)) = 1
        _GammaCorrection ("Gamma Correction", Range(0.1,3)) = 2.2
        
        // UI相关参数
        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
        [HideInInspector] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }
    
    SubShader {
        Tags {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Stencil {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        
        Pass {
            Name "AVProVideo_UI_ColorCorrected"
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP
            
            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _FilterTex;
            float4 _FilterTex_ST;
            sampler2D _MovTex1;
            float4 _MovTex1_ST;
            sampler2D _FilterTex1;
            float4 _FilterTex1_ST;
            sampler2D _MovTex2;
            float4 _MovTex2_ST;
            sampler2D _FilterTex2;
            float4 _FilterTex2_ST;
            sampler2D _MovTex3;
            float4 _MovTex3_ST;
            sampler2D _FilterTex3;
            float4 _FilterTex3_ST;
            sampler2D _MovTex4;
            float4 _MovTex4_ST;
            sampler2D _FilterTex4;
            float4 _FilterTex4_ST;
            sampler2D _MovTex5;
            float4 _MovTex5_ST;
            sampler2D _FilterTex5;
            float4 _FilterTex5_ST;
            
            float _ApplyColorCorrection;
            float _ForceGammaCorrection;
            float4 _ColorBalance;
            float _Brightness;
            float _Contrast;
            float _Saturation;
            float _GammaCorrection;
            
            float4 _ClipRect;
            float _UseUIAlphaClip;
            
            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            v2f vert(appdata_t v) {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color;
                return OUT;
            }
            
            // 精确的颜色空间转换函数
            float3 GammaToLinearPrecise(float3 sRGB) {
                return sRGB * (sRGB * (sRGB * 0.305306011 + 0.682171111) + 0.012522878);
            }
            
            float3 LinearToGammaPrecise(float3 lin) {
                return max(1.055 * pow(max(lin, 0.0), 0.416666667) - 0.055, 0.0);
            }
            
            // 简化的转换函数（性能更好）
            float3 GammaToLinearFast(float3 sRGB) {
                return pow(abs(sRGB), 2.2);
            }
            
            float3 LinearToGammaFast(float3 lin) {
                return pow(abs(lin), 1.0/2.2);
            }
            
            // 获取亮度值
            float GetLuminance(float3 color) {
                return dot(color, float3(0.2126729, 0.7151522, 0.0721750));
            }
            
            // AVProVideo专用颜色校正
            float3 AVProVideoColorCorrection(float3 color) {
                if (_ApplyColorCorrection < 0.5) {
                    return color;
                }
                
                // 1. 如果Unity在Linear空间，但视频是sRGB编码的，需要转换
                #ifdef UNITY_COLORSPACE_GAMMA
                    // Unity在Gamma空间，不需要额外转换
                    float3 workingColor = color;
                #else
                    // Unity在Linear空间，假设输入是sRGB编码的视频
                    float3 workingColor = color;
                    if (_ForceGammaCorrection > 0.5) {
                        // 将sRGB转换为线性空间进行处理
                        workingColor = GammaToLinearFast(color);
                    }
                #endif
                
                // 2. 应用颜色平衡
                workingColor *= _ColorBalance.rgb;
                
                // 3. 应用自定义Gamma校正
                workingColor = pow(abs(workingColor), _GammaCorrection);
                
                // 4. 应用亮度调整
                workingColor *= _Brightness;
                
                // 5. 应用对比度调整
                workingColor = (workingColor - 0.5) * _Contrast + 0.5;
                
                // 6. 应用饱和度调整
                float luma = GetLuminance(workingColor);
                workingColor = lerp(float3(luma, luma, luma), workingColor, _Saturation);
                
                // 7. 限制到有效范围
                workingColor = saturate(workingColor);
                
                // 8. 如果Unity在Linear空间，转换回适当的空间
                #ifndef UNITY_COLORSPACE_GAMMA
                    if (_ForceGammaCorrection > 0.5) {
                        // 转换回sRGB用于显示
                        workingColor = LinearToGammaFast(workingColor);
                    }
                #endif
                
                return workingColor;
            }
            
            fixed4 frag(v2f IN) : SV_Target {
                // 采样主纹理
                half4 color = tex2D(_MainTex, IN.texcoord);
                
                // 采样其他纹理
                float4 _FilterTex_var = tex2D(_FilterTex, TRANSFORM_TEX(IN.texcoord, _FilterTex));
                float4 _MovTex1_var = tex2D(_MovTex1, TRANSFORM_TEX(IN.texcoord, _MovTex1));
                float4 _FilterTex1_var = tex2D(_FilterTex1, TRANSFORM_TEX(IN.texcoord, _FilterTex1));
                float4 _MovTex2_var = tex2D(_MovTex2, TRANSFORM_TEX(IN.texcoord, _MovTex2));
                float4 _FilterTex2_var = tex2D(_FilterTex2, TRANSFORM_TEX(IN.texcoord, _FilterTex2));
                float4 _MovTex3_var = tex2D(_MovTex3, TRANSFORM_TEX(IN.texcoord, _MovTex3));
                float4 _FilterTex3_var = tex2D(_FilterTex3, TRANSFORM_TEX(IN.texcoord, _FilterTex3));
                float4 _MovTex4_var = tex2D(_MovTex4, TRANSFORM_TEX(IN.texcoord, _MovTex4));
                float4 _FilterTex4_var = tex2D(_FilterTex4, TRANSFORM_TEX(IN.texcoord, _FilterTex4));
                float4 _MovTex5_var = tex2D(_MovTex5, TRANSFORM_TEX(IN.texcoord, _MovTex5));
                float4 _FilterTex5_var = tex2D(_FilterTex5, TRANSFORM_TEX(IN.texcoord, _FilterTex5));
                
                // 应用AVProVideo专用颜色校正
                color.rgb = AVProVideoColorCorrection(color.rgb);
                
                // 构建最终颜色并应用混合
                fixed4 finalColor = fixed4(color.rgb, _FilterTex_var.r);
                finalColor = lerp(finalColor, _MovTex1_var, _FilterTex1_var.r);
                finalColor = lerp(finalColor, _MovTex2_var, _FilterTex2_var.r);
                finalColor = lerp(finalColor, _MovTex3_var, _FilterTex3_var.r);
                finalColor = lerp(finalColor, _MovTex4_var, _FilterTex4_var.r);
                finalColor = lerp(finalColor, _MovTex5_var, _FilterTex5_var.r);
                
                // 应用UI顶点颜色
                finalColor *= IN.color;
                
                // UI裁剪支持
                #ifdef UNITY_UI_CLIP_RECT
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif
                
                // UI Alpha裁剪支持
                #ifdef UNITY_UI_ALPHACLIP
                clip(finalColor.a - 0.001);
                #endif
                
                return finalColor;
            }
            ENDCG
        }
    }
    
    FallBack "UI/Default"
}