Shader "Projector/Light_Flipped_SelfIllum_NoFalloff" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
        _ShadowTex ("Cookie", 2D) = "" {}
        // 保留但不使用 FallOff 纹理，防止引用错误
        _FalloffTex ("FallOff", 2D) = "white" {}
        [Toggle]_FlipY("Flip Y", Float) = 0
        _Brightness("亮度 (Brightness)", Range(0.1, 3.0)) = 1.0
        _Saturation("饱和度 (Saturation)", Range(0.0, 2.0)) = 1.0
        _Contrast("对比度 (Contrast)", Range(0.5, 3.0)) = 1.0
        _BlackLevel("黑色深度 (Black Level)", Range(-0.5, 0.5)) = 0.0
        [Toggle]_UseStandardColor("标准颜色模式 (Standard Color Mode)", Float) = 0
    }
    SubShader {
        Tags {
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Pass {
            ZWrite Off
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature _               
            #include "UnityCG.cginc"
            struct v2f {
                float4 uvShadow : TEXCOORD0;
                float4 pos : SV_POSITION;
            };
            float4x4 unity_Projector;
            float _FlipY;
            float _Brightness;
            float _Saturation;
            float _Contrast;
            float _BlackLevel;
            float _UseStandardColor;
            
            v2f vert (float4 vertex : POSITION)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(vertex);
                o.uvShadow = mul(unity_Projector, vertex);
                // Flip Y if _FlipY is 1
                o.uvShadow.y = lerp(o.uvShadow.y, -o.uvShadow.y + o.uvShadow.w, _FlipY);
                return o;
            }
            
            sampler2D _ShadowTex;
            fixed4 _Color;
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainTexColor = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(i.uvShadow)) * _Color;
                
                // Apply black level adjustment
                mainTexColor.rgb = saturate(mainTexColor.rgb - _BlackLevel.xxx);
                
                // Apply brightness adjustment to the RGB channels
                mainTexColor.rgb *= _Brightness;
                
                // Apply contrast enhancement
                mainTexColor.rgb = saturate((mainTexColor.rgb - 0.5) * _Contrast + 0.5);
                
                // Apply saturation adjustment
                float luminance = dot(mainTexColor.rgb, float3(0.2126, 0.7152, 0.0722));
                fixed3 saturatedColor = lerp(luminance.xxx, mainTexColor.rgb, _Saturation);
                
                // Apply standard color mode if enabled
                if (_UseStandardColor > 0.5) {
                    saturatedColor = saturatedColor * _Color.rgb;
                }
                
                // 直接使用主纹理的 alpha，不再乘以 falloff 的 alpha
                fixed4 finalColor = fixed4(saturatedColor, mainTexColor.a);
                return finalColor;
            }
            ENDCG
        }
    }
    Fallback "Projector/Light"
}