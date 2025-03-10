Shader "Custom/TVNoise"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _SignalStrength ("Signal Strength", Range(0.0, 1.0)) = 0.5
        _NoiseSpeed ("Noise Speed", Range(0.0, 10.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Pass
        {
            Cull Off ZWrite Off ZTest Always
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _SignalStrength;
            float _NoiseSpeed;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            // 传入 uv 和时间，生成随时间变化的伪随机值
            float pseudoRandom2D(float2 uv, float time)
            {
                // 使用简单哈希生成噪声值，注意增大 time 可以产生不断变化的噪声
                return frac(sin(dot(uv + time, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _NoiseSpeed;

                // 采样原图
                fixed4 col = tex2D(_MainTex, i.uv);

                // 生成噪声
                float noiseVal = pseudoRandom2D(i.uv, time);

                // 使用信号强度控制噪声的强度，同时让噪点随机出现
                float3 noise = lerp(col.rgb, float3(noiseVal, noiseVal, noiseVal), _SignalStrength);

                return fixed4(noise, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}