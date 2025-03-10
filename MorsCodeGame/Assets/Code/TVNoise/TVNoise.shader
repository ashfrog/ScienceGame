Shader "Custom/TVNoise"
{
    Properties
    {
        _MainTex ("Base Texture (Video Input)", 2D) = "white" {}
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

            // Generates pseudo-random noise based on uv/time
            float pseudoRandom2D(float2 uv, float time)
            {
                return frac(sin(dot(uv + time, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _NoiseSpeed;

                // Sample the input texture (AVProVideo output)
                fixed4 col = tex2D(_MainTex, i.uv);

                // Generate noise
                float noiseVal = pseudoRandom2D(i.uv, time);

                // Blend noise based on signal strength
                float3 noise = lerp(col.rgb, float3(noiseVal, noiseVal, noiseVal), _SignalStrength);

                return fixed4(noise, 1.0);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}