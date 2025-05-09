﻿// Shader created with Shader Forge v1.38 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.38;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,cgin:,lico:1,lgpr:1,limd:1,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,imps:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:0,bsrc:3,bdst:7,dpts:2,wrdp:False,dith:0,atcv:False,rfrpo:True,rfrpn:Refraction,coma:15,ufog:True,aust:True,igpj:True,qofs:0,qpre:3,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,atwp:False,stva:128,stmr:255,stmw:255,stcp:6,stps:0,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False,fsmp:False;n:type:ShaderForge.SFN_Final,id:4013,x:32719,y:32712,varname:node_4013,prsc:2|emission-4322-RGB,alpha-4393-R;n:type:ShaderForge.SFN_Tex2d,id:4393,x:32494,y:32922,ptovrint:False,ptlb:node_1746_copy,ptin:_MainTex,varname:_MainTex,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;n:type:ShaderForge.SFN_Tex2d,id:4322,x:32454,y:32682,ptovrint:False,ptlb:node_1746_copy_copy,ptin:_MainTexA,varname:_MainTexA,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,ntxv:0,isnm:False;proporder:4393-4322;pass:END;sub:END;*/

Shader "AVProVideo/Lit/custom0" {
    Properties{
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
        SubShader{
            Tags {
                "IgnoreProjector" = "True"
                "Queue" = "Transparent"
                "RenderType" = "Transparent"
            }
            Pass {
                Name "FORWARD"
                Tags {
                    "LightMode" = "ForwardBase"
                }
                Blend SrcAlpha OneMinusSrcAlpha
                ZWrite Off

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #define UNITY_PASS_FORWARDBASE
                #include "UnityCG.cginc"
                #pragma multi_compile_fwdbase
                #pragma multi_compile_fog
            //#pragma only_renderers d3d9 d3d11 glcore gles 
            #pragma target 3.0
            uniform sampler2D _MainTex; uniform float4 _MainTex_ST;
            uniform sampler2D _FilterTex; uniform float4 _FilterTex_ST;
            uniform sampler2D _MovTex1; uniform float4 _MovTex1_ST;
            uniform sampler2D _FilterTex1; uniform float4 _FilterTex1_ST;
            uniform sampler2D _MovTex2; uniform float4 _MovTex2_ST;
            uniform sampler2D _FilterTex2; uniform float4 _FilterTex2_ST;
            uniform sampler2D _MovTex3; uniform float4 _MovTex3_ST;
            uniform sampler2D _FilterTex3; uniform float4 _FilterTex3_ST;
            uniform sampler2D _MovTex4; uniform float4 _MovTex4_ST;
            uniform sampler2D _FilterTex4; uniform float4 _FilterTex4_ST;
            uniform sampler2D _MovTex5; uniform float4 _MovTex5_ST;
            uniform sampler2D _FilterTex5; uniform float4 _FilterTex5_ST;

            struct VertexInput {
                float4 vertex : POSITION;
                float2 texcoord0 : TEXCOORD0;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float2 uv0 : TEXCOORD0;
                UNITY_FOG_COORDS(1)
            };
            VertexOutput vert(VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.uv0 = v.texcoord0;
                o.pos = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i) : COLOR {
                ////// Lighting:
                ////// Emissive:

                                float4 _MainTex_var = tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));
                                float3 emissive = _MainTex_var.rgb;
                                float3 finalColor = emissive;
                                float4 _FilterTex_var = tex2D(_FilterTex, TRANSFORM_TEX(i.uv0, _FilterTex));
                                float4 _MovTex1_var = tex2D(_MovTex1, TRANSFORM_TEX(i.uv0, _MovTex1));
                                float4 _FilterTex1_var = tex2D(_FilterTex1, TRANSFORM_TEX(i.uv0, _FilterTex1));
                                float4 _MovTex2_var = tex2D(_MovTex2, TRANSFORM_TEX(i.uv0, _MovTex2));
                                float4 _FilterTex2_var = tex2D(_FilterTex2, TRANSFORM_TEX(i.uv0, _FilterTex2));
                                float4 _MovTex3_var = tex2D(_MovTex3, TRANSFORM_TEX(i.uv0, _MovTex3));
                                float4 _FilterTex3_var = tex2D(_FilterTex3, TRANSFORM_TEX(i.uv0, _FilterTex3));
                                float4 _MovTex4_var = tex2D(_MovTex4, TRANSFORM_TEX(i.uv0, _MovTex4));
                                float4 _FilterTex4_var = tex2D(_FilterTex4, TRANSFORM_TEX(i.uv0, _FilterTex4));
                                float4 _MovTex5_var = tex2D(_MovTex5, TRANSFORM_TEX(i.uv0, _MovTex5));
                                float4 _FilterTex5_var = tex2D(_FilterTex5, TRANSFORM_TEX(i.uv0, _FilterTex5));

                                fixed4 finalRGBA = fixed4(finalColor, _FilterTex_var.r);
                                finalRGBA = lerp(finalRGBA, _MovTex1_var, _FilterTex1_var.r);
                                finalRGBA = lerp(finalRGBA, _MovTex2_var, _FilterTex2_var.r);
                                finalRGBA = lerp(finalRGBA, _MovTex3_var, _FilterTex3_var.r);
                                finalRGBA = lerp(finalRGBA, _MovTex4_var, _FilterTex4_var.r);
                                finalRGBA = lerp(finalRGBA, _MovTex5_var, _FilterTex5_var.r);


                                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                                return finalRGBA;
                            }
                            ENDCG
                        }
        }
            FallBack "Diffuse"
                                //CustomEditor "ShaderForgeMaterialInspector"
}
