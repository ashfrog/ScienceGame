// Advanced Dissolve <https://u3d.as/16cX>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 


#ifndef UNITY_STANDARD_SHADOW_INCLUDED
#define UNITY_STANDARD_SHADOW_INCLUDED

// NOTE: had to split shadow functions into separate file,
// otherwise compiler gives trouble with LIGHTING_COORDS macro (in UnityStandardCore.cginc)


#include "UnityCG.cginc"
#include "UnityShaderVariables.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityStandardUtils.cginc"

#if (defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)) && defined(UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS)
    #define UNITY_STANDARD_USE_DITHER_MASK 1
#endif

// Need to output UVs in shadow caster, since we need to sample texture and do clip/dithering based on it
//#if defined(_ALPHATEST_ON) || defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
#define UNITY_STANDARD_USE_SHADOW_UVS 1     //Advanced Dissovle needs this definition
//#endif

// Has a non-empty shadow caster output struct (it's an error to have empty structs on some platforms...)
#if !defined(V2F_SHADOW_CASTER_NOPOS_IS_EMPTY) || defined(UNITY_STANDARD_USE_SHADOW_UVS)
#define UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT 1
#endif

#ifdef UNITY_STEREO_INSTANCING_ENABLED
#define UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT 1
#endif


half4       _Color;
half        _Cutoff;
sampler2D   _MainTex;
float4      _MainTex_ST;
#ifdef UNITY_STANDARD_USE_DITHER_MASK
sampler3D   _DitherMaskLOD;
#endif

// Handle PremultipliedAlpha from Fade or Transparent shading mode
half4       _SpecColor;
half        _Metallic;
#ifdef _SPECGLOSSMAP
sampler2D   _SpecGlossMap;
#endif
#ifdef _METALLICGLOSSMAP
sampler2D   _MetallicGlossMap;
#endif

#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
sampler2D   _ParallaxMap;
half        _Parallax;
#endif

half MetallicSetup_ShadowGetOneMinusReflectivity(half2 uv)
{
    half metallicity = _Metallic;
    #ifdef _METALLICGLOSSMAP
        metallicity = tex2D(_MetallicGlossMap, uv).r;
    #endif
    return OneMinusReflectivityFromMetallic(metallicity);
}

half RoughnessSetup_ShadowGetOneMinusReflectivity(half2 uv)
{
    half metallicity = _Metallic;
#ifdef _METALLICGLOSSMAP
    metallicity = tex2D(_MetallicGlossMap, uv).r;
#endif
    return OneMinusReflectivityFromMetallic(metallicity);
}

half SpecularSetup_ShadowGetOneMinusReflectivity(half2 uv)
{
    half3 specColor = _SpecColor.rgb;
    #ifdef _SPECGLOSSMAP
        specColor = tex2D(_SpecGlossMap, uv).rgb;
    #endif
    return (1 - SpecularStrength(specColor));
}

// SHADOW_ONEMINUSREFLECTIVITY(): workaround to get one minus reflectivity based on UNITY_SETUP_BRDF_INPUT
#define SHADOW_JOIN2(a, b) a##b
#define SHADOW_JOIN(a, b) SHADOW_JOIN2(a,b)
#define SHADOW_ONEMINUSREFLECTIVITY SHADOW_JOIN(UNITY_SETUP_BRDF_INPUT, _ShadowGetOneMinusReflectivity)

struct VertexInput
{
    float4 vertex   : POSITION;
    float3 normal   : NORMAL;
    float2 uv0      : TEXCOORD0;
    //#if defined(UNITY_STANDARD_USE_SHADOW_UVS) && defined(_PARALLAXMAP)
        half4 tangent   : TANGENT;	//Curved World requires tangent
    //#endif
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput
{
    UNITY_POSITION(pos);
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
struct VertexOutputShadowCaster
{
    V2F_SHADOW_CASTER_NOPOS
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        float2 tex : TEXCOORD1;

        #if defined(_PARALLAXMAP)
            half3 viewDirForParallax : TEXCOORD2;
        #endif
    #endif


    //Advanced Dissolve
	float3 positionWS    : TEXCOORD3;
    float3 normalWS      : TEXCOORD4;
	ADVANCED_DISSOLVE_UV(5)
};
#endif

#ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
struct VertexOutputStereoShadowCaster
{
    UNITY_VERTEX_OUTPUT_STEREO
};
#endif

// We have to do these dances of outputting SV_POSITION separately from the vertex shader,
// and inputting VPOS in the pixel shader, since they both map to "POSITION" semantic on
// some platforms, and then things don't go well.


void vertShadowCaster (VertexInput v
    , out VertexOutput output
    #ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
    , out VertexOutputShadowCaster o
    #endif
    #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
    , out VertexOutputStereoShadowCaster os
    #endif
)
{
    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, output);

    #ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
        o = (VertexOutputShadowCaster)0;
    #endif

    #ifdef UNITY_STANDARD_USE_STEREO_SHADOW_OUTPUT_STRUCT
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(os);
    #endif


    //Curved World
    #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
        CURVEDWORLD_TRANSFORM_VERTEX(v.vertex);
    #endif


    TRANSFER_SHADOW_CASTER_NOPOS(o, output.pos)
    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)
        o.tex = TRANSFORM_TEX(v.uv0, _MainTex);

        #ifdef _PARALLAXMAP
            TANGENT_SPACE_ROTATION;
            o.viewDirForParallax = mul (rotation, ObjSpaceViewDir(v.vertex));
        #endif
    #endif


    //Advanced Dissolve 
    #if defined(_AD_STATE_ENABLED)
        #if defined(UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT)
    
	        o.positionWS = mul(unity_ObjectToWorld, v.vertex);
            o.normalWS = UnityObjectToWorldNormal(v.normal);
	        ADVANCED_DISSOLVE_INIT_UV(o, v.uv0.xy, UnityObjectToClipPos(v.vertex))

        #endif
    #endif

}

half4 fragShadowCaster (VertexOutput input
#ifdef UNITY_STANDARD_USE_SHADOW_OUTPUT_STRUCT
    , VertexOutputShadowCaster i
#endif
) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);

    #if defined(UNITY_STANDARD_USE_SHADOW_UVS)



  //Advanced Dissolve////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    #if defined(_AD_STATE_ENABLED)

        float4 dissolveBase = 0;
        #if defined(_AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA)
            dissolveBase = tex2D(_MainTex, i.tex.xy);
        #endif

	    ADVANCED_DISSOLVE_SETUP_CUTOUT_SOURCE_USING_WS(i, dissolveBase, i.positionWS.xyz, i.normalWS.xyz)

        #if !defined(_ALPHATEST_ON)
            AdvancedDissolveClip(cutoutSource);
        #endif

    #endif
    //Advanced Dissolve/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        #if defined(_PARALLAXMAP) && (SHADER_TARGET >= 30)
            half3 viewDirForParallax = normalize(i.viewDirForParallax);
            fixed h = tex2D (_ParallaxMap, i.tex.xy).g;
            half2 offset = ParallaxOffset1Step (h, _Parallax, viewDirForParallax);
            i.tex.xy += offset;
        #endif

        #if defined(_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A)
            half alpha = _Color.a;
        #else
            half alpha = tex2D(_MainTex, i.tex.xy).a * _Color.a;
        #endif
        #if defined(_ALPHATEST_ON)
            
            float cutout = _Cutoff * 1.001;

		    //Advanced Dissolve
		    #if defined(_AD_STATE_ENABLED)
			    AdvancedDissolveCalculateAlphaAndClip(cutoutSource, alpha, cutout);
		    #endif

            clip (alpha - cutout);

        #endif
        #if defined(_ALPHABLEND_ON) || defined(_ALPHAPREMULTIPLY_ON)
            #if defined(_ALPHAPREMULTIPLY_ON)
                half outModifiedAlpha;
                PreMultiplyAlpha(half3(0, 0, 0), alpha, SHADOW_ONEMINUSREFLECTIVITY(i.tex), outModifiedAlpha);
                alpha = outModifiedAlpha;
            #endif
            #if defined(UNITY_STANDARD_USE_DITHER_MASK)
                // Use dither mask for alpha blended shadows, based on pixel position xy
                // and alpha level. Our dither texture is 4x4x16.
                #ifdef LOD_FADE_CROSSFADE
                    #define _LOD_FADE_ON_ALPHA
                    alpha *= unity_LODFade.y;
                #endif
                half alphaRef = tex3D(_DitherMaskLOD, float3(input.pos.xy*0.25,alpha*0.9375)).a;
                clip (alphaRef - 0.01);
            #else
                
                float cutout = _Cutoff * 1.01;

		        //Advanced Dissolve
		        #if defined(_AD_STATE_ENABLED)
			        AdvancedDissolveCalculateAlphaAndClip(cutoutSource, alpha, cutout);
		        #endif

                clip (alpha - cutout);

            #endif
        #endif
    #endif // #if defined(UNITY_STANDARD_USE_SHADOW_UVS)

    #ifdef LOD_FADE_CROSSFADE
        #ifdef _LOD_FADE_ON_ALPHA
            #undef _LOD_FADE_ON_ALPHA
        #else
            UnityApplyDitherCrossFade(input.pos.xy);
        #endif
    #endif

    SHADOW_CASTER_FRAGMENT(i)
}

#endif // UNITY_STANDARD_SHADOW_INCLUDED
