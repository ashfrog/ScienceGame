Shader "UI/UISnowMask"
{
    Properties
    {
        _SignalStrength ("Signal Strength", Range(0,1)) = 0.5
        _NoiseSpeed ("Noise Speed", Range(0,10)) = 1.0
        _ColorTint ("Mask Color Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        // 为了让材质能够在 UI 中使用，设置 Queue="Transparent" 和 RenderType="Transparent"
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanvasOverlay"="True" }
        LOD 100

        Pass
        {
            Cull Off ZWrite Off ZTest Always
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _ColorTint;
            float _SignalStrength;
            float _NoiseSpeed;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                o.position = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            // 简单的 2D 噪声函数
            float pseudoRandom2D(float2 uv, float time)
            {
                return frac(sin(dot(uv + time, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _NoiseSpeed;
                float rnd  = pseudoRandom2D(i.uv, time);

                // 假设：信号好的部分（rnd > _SignalStrength）让画面透过，信号差的部分变成遮罩。
                // alpha = 1 表示可见（透过），alpha = 0 表示遮罩/隐藏。
                float alphaMask = (rnd > _SignalStrength) ? 1.0 : 0.0;

                // 将颜色设为可调的 tint，仅用于可视化；主要依赖 alphaMask
                float4 col = _ColorTint;
                col.a = alphaMask;

                return col;
            }
            ENDCG
        }
    }
    FallBack "Unlit/Transparent"
}