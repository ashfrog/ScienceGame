Shader "Custom/MirrorEffect"
{
    Properties
    {
        [Toggle] _FlipX ("Flip Horizontal", Float) = 0
        [Toggle] _FlipY ("Flip Vertical", Float) = 0
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
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
            float _FlipX;
            float _FlipY;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // Base UV
                o.uv = v.uv;

                // Horizontal flip
                if (_FlipX > 0.5)
                {
                    o.uv.x = 1.0 - o.uv.x;
                }

                // Vertical flip
                if (_FlipY > 0.5)
                {
                    o.uv.y = 1.0 - o.uv.y;
                }

                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}