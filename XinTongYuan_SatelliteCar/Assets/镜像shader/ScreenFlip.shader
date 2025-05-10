Shader "Hidden/ScreenFlip" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _FlipHorizontal ("Flip Horizontal", Float) = 0
        _FlipVertical ("Flip Vertical", Float) = 0
    }
    SubShader {
        Pass {
            ZTest Always Cull Off ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            float _FlipHorizontal;
            float _FlipVertical;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                // 应用翻转
                if (_FlipHorizontal > 0.5)
                    o.uv.x = 1.0 - o.uv.x;
                if (_FlipVertical > 0.5)
                    o.uv.y = 1.0 - o.uv.y;
                    
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
