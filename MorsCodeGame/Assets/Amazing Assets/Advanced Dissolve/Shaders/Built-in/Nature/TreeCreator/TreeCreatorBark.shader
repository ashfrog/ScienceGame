// Advanced Dissolve <https://u3d.as/16cX>
// Copyright (c) Amazing Assets <https://amazingassets.world>
 
Shader "Amazing Assets/Advanced Dissolve/Nature/Tree Creator Bark" 
{
    Properties 
    {

//[HideInInspector][CurvedWorldBendSettings] _CurvedWorldBendSettings("0|1|1", Vector) = (0, 0, 0, 0)

    _Color ("Main Color", Color) = (1,1,1,1)
    [PowerSlider(5.0)] _Shininess ("Shininess", Range (0.01, 1)) = 0.078125
    _MainTex ("Base (RGB) Alpha (A)", 2D) = "white" {}
    _BumpMap ("Normalmap", 2D) = "bump" {}
    _GlossMap ("Gloss (A)", 2D) = "black" {}

    // These are here only to provide default values
    _SpecColor ("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
    [HideInInspector] _TreeInstanceColor ("TreeInstanceColor", Vector) = (1,1,1,1)
    [HideInInspector] _TreeInstanceScale ("TreeInstanceScale", Vector) = (1,1,1,1)
    [HideInInspector] _SquashAmount ("Squash", Float) = 1


//Advanced Dissolve Properties Start////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

//Cutout
[HideInInspector]                                                   _AdvancedDissolveCutoutStandardClip("", Range(0,1)) = 0.5

[HideInInspector]											        _AdvancedDissolveCutoutStandardMap1("", 2D) = "white" { }
[HideInInspector]											        _AdvancedDissolveCutoutStandardMap1Tiling("", Vector) = (1, 1, 1, 0)
[HideInInspector]											        _AdvancedDissolveCutoutStandardMap1Offset("", Vector) = (0, 0, 0, 0)
[HideInInspector]					                                _AdvancedDissolveCutoutStandardMap1Scroll("", Vector) = (0, 0, 0, 0)
[HideInInspector]											        _AdvancedDissolveCutoutStandardMap1Intensity("", Range(0, 1)) = 1
[HideInInspector][Enum(Red, 0, Green, 1, Blue, 2, Alpha, 3)]        _AdvancedDissolveCutoutStandardMap1Channel("", INT) = 3
[HideInInspector][AdvancedDissolveToggleFloat]				        _AdvancedDissolveCutoutStandardMap1Invert("", INT) = 0
[HideInInspector]											        _AdvancedDissolveCutoutStandardMap2("", 2D) = "white" { }
[HideInInspector]											        _AdvancedDissolveCutoutStandardMap2Tiling("", Vector) = (1, 1, 1, 0)
[HideInInspector]											        _AdvancedDissolveCutoutStandardMap2Offset("", Vector) = (0, 0, 0, 0)
[HideInInspector]					                                _AdvancedDissolveCutoutStandardMap2Scroll("", Vector) = (0, 0, 0, 0)
[HideInInspector]											        _AdvancedDissolveCutoutStandardMap2Intensity("", Range(0, 1)) = 1
[HideInInspector][Enum(Red, 0, Green, 1, Blue, 2, Alpha, 3)]        _AdvancedDissolveCutoutStandardMap2Channel("", INT) = 3
[HideInInspector][AdvancedDissolveToggleFloat]				        _AdvancedDissolveCutoutStandardMap2Invert("", INT) = 0
[HideInInspector]											        _AdvancedDissolveCutoutStandardMap3("", 2D) = "white" { }
[HideInInspector]											        _AdvancedDissolveCutoutStandardMap3Tiling("", Vector) = (1, 1, 1, 0)
[HideInInspector]											        _AdvancedDissolveCutoutStandardMap3Offset("", Vector) = (0, 0, 0, 0)
[HideInInspector]					                                _AdvancedDissolveCutoutStandardMap3Scroll("", Vector) = (0, 0, 0, 0)
[HideInInspector]											        _AdvancedDissolveCutoutStandardMap3Intensity("", Range(0, 1)) = 1
[HideInInspector][Enum(Red, 0, Green, 1, Blue, 2, Alpha, 3)]        _AdvancedDissolveCutoutStandardMap3Channel("", INT) = 3
[HideInInspector][AdvancedDissolveToggleFloat]				        _AdvancedDissolveCutoutStandardMap3Invert("", INT) = 0

[HideInInspector][Enum(Multiply, 0, Add, 1)]				        _AdvancedDissolveCutoutStandardMapsBlendType("", Float) = 0
[HideInInspector][Enum(World, 0, Local, 1)]					        _AdvancedDissolveCutoutStandardMapsTriplanarMappingSpace("", Float) = 0	
[HideInInspector][Enum(Constant, 0, Camera Relative, 1)]            _AdvancedDissolveCutoutStandardMapsScreenSpaceUVScale("", Float) = 0
[HideInInspector][AdvancedDissolveToggleFloat]				        _AdvancedDissolveCutoutStandardBaseInvert("", INT) = 0

//Geometric
[HideInInspector][AdvancedDissolveToggleFloat]			    	    _AdvancedDissolveCutoutGeometricInvert("", Float) = 0
[HideInInspector]										    	    _AdvancedDissolveCutoutGeometricNoise("", Float) = 0.1	

[HideInInspector][Enum(X, 0, Y, 1, Z, 2)]                           _AdvancedDissolveCutoutGeometricXYZAxis("", Float) = 0
[HideInInspector][Enum(Linear, 0, Symmetrical, 1)]                  _AdvancedDissolveCutoutGeometricXYZStyle("", Float) = 0 
[HideInInspector][Enum(World, 0, Local, 1)]                         _AdvancedDissolveCutoutGeometricXYZSpace("", Float) = 0	 
[HideInInspector]											        _AdvancedDissolveCutoutGeometricXYZRollout("", Float) = 0
[HideInInspector]											        _AdvancedDissolveCutoutGeometricXYZPosition("", Vector) = (0, 0, 0, 0)

[HideInInspector]										    	    _AdvancedDissolveCutoutGeometric1Position("", Vector) = (0,0,0,0)
[HideInInspector]										    	    _AdvancedDissolveCutoutGeometric1Normal("", Vector) = (1,0,0,0)
[HideInInspector]										    	    _AdvancedDissolveCutoutGeometric1Radius("", Float) = 1
[HideInInspector]										    	    _AdvancedDissolveCutoutGeometric1Height("", Float) = 1

[HideInInspector]										    	    _AdvancedDissolveCutoutGeometric2Position("", Vector) = (0,0,0,0)
[HideInInspector]									    		    _AdvancedDissolveCutoutGeometric2Normal("", Vector) = (1,0,0,0)
[HideInInspector]									    		    _AdvancedDissolveCutoutGeometric2Radius("", Float) = 1
[HideInInspector]									    		    _AdvancedDissolveCutoutGeometric2Height("", Float) = 1
 
[HideInInspector]									    		    _AdvancedDissolveCutoutGeometric3Position("", Vector) = (0,0,0,0)
[HideInInspector]									    		    _AdvancedDissolveCutoutGeometric3Normal("", Vector) = (1,0,0,0)
[HideInInspector]									    		    _AdvancedDissolveCutoutGeometric3Radius("", Float) = 1
[HideInInspector]										    	    _AdvancedDissolveCutoutGeometric3Height("", Float) = 1

[HideInInspector]										    	    _AdvancedDissolveCutoutGeometric4Position("", Vector) = (0,0,0,0)
[HideInInspector]											        _AdvancedDissolveCutoutGeometric4Normal("", Vector) = (1,0,0,0)
[HideInInspector]											        _AdvancedDissolveCutoutGeometric4Radius("", Float) = 1
[HideInInspector]											        _AdvancedDissolveCutoutGeometric4Height("", Float) = 1

//Edge
[HideInInspector]										    	    _AdvancedDissolveEdgeBaseWidthStandard("", Range(0,1)) = 0.1 
[HideInInspector]										    	    _AdvancedDissolveEdgeBaseWidthGeometric("", Range(0,1)) = 0.1 
[HideInInspector][Enum(Solid, 0, Smooth, 1, Smoother, 2)]           _AdvancedDissolveEdgeBaseShape("", INT) = 0
[HideInInspector][AdvancedDissolveColorRGB]  				        _AdvancedDissolveEdgeBaseColor("", Color) = (0,1,0,1)
[HideInInspector]											        _AdvancedDissolveEdgeBaseColorTransparency("", Range(0, 1)) = 1
[HideInInspector][AdvancedDissolveExponental]                       _AdvancedDissolveEdgeBaseColorIntensity("", Vector) = (0, 0, 0, 0)		

[HideInInspector][AdvancedDissolveColorRGB]					        _AdvancedDissolveEdgeAdditionalColor("", color) = (1, 0, 0, 1)
[HideInInspector]											        _AdvancedDissolveEdgeAdditionalColorTransparency("", Range(0, 1)) = 1
[HideInInspector][AdvancedDissolveExponental]			            _AdvancedDissolveEdgeAdditionalColorIntensity("", Vector) = (0, 0, 0, 0)
[HideInInspector]								                    _AdvancedDissolveEdgeAdditionalColorMap("", 2D) = "white" { }
[HideInInspector]					                                _AdvancedDissolveEdgeAdditionalColorMapTiling("", Vector) = (1, 1, 1, 0)
[HideInInspector]					                                _AdvancedDissolveEdgeAdditionalColorMapOffset("", Vector) = (0, 0, 0, 0)
[HideInInspector]					                                _AdvancedDissolveEdgeAdditionalColorMapScroll("", Vector) = (0, 0, 0, 0)
[HideInInspector][AdvancedDissolveToggleFloat]				        _AdvancedDissolveEdgeAdditionalColorMapReverse("", FLOAT) = 0
[HideInInspector]											        _AdvancedDissolveEdgeAdditionalColorMapMipmap("", Range(0, 10)) = 1	
[HideInInspector]											        _AdvancedDissolveEdgeAdditionalColorPhaseOffset("", FLOAT) = 0
[HideInInspector]											        _AdvancedDissolveEdgeAdditionalColorAlphaOffset("", Range(-1, 1)) = 0	
[HideInInspector][AdvancedDissolveToggleFloat]				        _AdvancedDissolveEdgeAdditionalColorClipInterpolation("", Float) = 0


[HideInInspector]								                    _AdvancedDissolveEdgeUVDistortionMap("", 2D) = "black" { }
[HideInInspector]					                                _AdvancedDissolveEdgeUVDistortionMapTiling("", Vector) = (1, 1, 1, 0)
[HideInInspector]					                                _AdvancedDissolveEdgeUVDistortionMapOffset("", Vector) = (0, 0, 0, 0)
[HideInInspector]					                                _AdvancedDissolveEdgeUVDistortionMapScroll("", Vector) = (0, 0, 0, 0)
[HideInInspector]				                                    _AdvancedDissolveEdgeUVDistortionStrength("", Float) = 0

[HideInInspector][AdvancedDissolvePositiveFloat]			        _AdvancedDissolveEdgeGIMetaPassMultiplier("", Float) = 1

//Keywords
[HideInInspector][AdvancedDissolveKeywordState]                     _AdvancedDissolveKeywordState("", INT) = 0
[HideInInspector][AdvancedDissolveKeywordCutoutStandardSource]      _AdvancedDissolveKeywordCutoutStandardSource("", INT) = 0
[HideInInspector][AdvancedDissolveKeywordCutoutStandardMappingType] _AdvancedDissolveKeywordCutoutStandardSourceMapsMappingType("", INT) = 0
[HideInInspector][AdvancedDissolveKeywordCutoutGeometricType]       _AdvancedDissolveKeywordCutoutGeometricType("", INT) = 0
[HideInInspector][AdvancedDissolveKeywordCutoutGeometricCount]      _AdvancedDissolveKeywordCutoutGeometricCount("", INT) = 0
[HideInInspector][AdvancedDissolveKeywordEdgeBaseSource]            _AdvancedDissolveKeywordEdgeBaseSource("", INT) = 0
[HideInInspector][AdvancedDissolveKeywordEdgeAdditionalColorSource] _AdvancedDissolveKeywordEdgeAdditionalColorSource("", INT) = 0
[HideInInspector][AdvancedDissolveKeywordEdgeUVDistortionSource]    _AdvancedDissolveKeywordEdgeUVDistortionSource("", INT) = 0
[HideInInspector][AdvancedDissolveKeywordGlobalControlID]           _AdvancedDissolveKeywordGlobalControlID("", INT) = 0

//BakedKeywords
[HideInInspector]                                                   _AdvancedDissolveBakedKeywords("", Vector) = (0,0,0,0)	

//Advanced Dissolve Properties End////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

}

SubShader {
    Tags { "IgnoreProjector"="True" "RenderType"="TreeBark" }
    LOD 200


	// ------------------------------------------------------------
	// Surface shader code generated out of a CGPROGRAM block:
	

	// ---- forward rendering base pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardBase" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile_instancing
#pragma multi_compile_fog
#pragma multi_compile_fwdbase noshadowmask nodynlightmap nolightmap
#pragma target 3.0

#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"

#include "UnityCG.cginc"
//Shader does not support lightmap thus we always want to fallback to SH.
#undef UNITY_SHOULD_SAMPLE_SH
#define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_PREPASSBASE) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))



//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"


//Advanced Dissolve Keywords Start///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma shader_feature_local   _AD_STATE_ENABLED
#pragma shader_feature_local _ _AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA				  _AD_CUTOUT_STANDARD_SOURCE_CUSTOM_MAP                     _AD_CUTOUT_STANDARD_SOURCE_TWO_CUSTOM_MAPS _AD_CUTOUT_STANDARD_SOURCE_THREE_CUSTOM_MAPS _AD_CUTOUT_STANDARD_SOURCE_USER_DEFINED
#pragma shader_feature_local _ _AD_CUTOUT_STANDARD_SOURCE_MAPS_MAPPING_TYPE_TRIPLANAR _AD_CUTOUT_STANDARD_SOURCE_MAPS_MAPPING_TYPE_SCREEN_SPACE
#pragma shader_feature_local _ _AD_CUTOUT_GEOMETRIC_TYPE_XYZ						  _AD_CUTOUT_GEOMETRIC_TYPE_PLANE                           _AD_CUTOUT_GEOMETRIC_TYPE_SPHERE           _AD_CUTOUT_GEOMETRIC_TYPE_CUBE               _AD_CUTOUT_GEOMETRIC_TYPE_CAPSULE       _AD_CUTOUT_GEOMETRIC_TYPE_CONE_SMOOTH
#pragma shader_feature_local _ _AD_CUTOUT_GEOMETRIC_COUNT_TWO					      _AD_CUTOUT_GEOMETRIC_COUNT_THREE                          _AD_CUTOUT_GEOMETRIC_COUNT_FOUR
#pragma shader_feature_local _ _AD_EDGE_BASE_SOURCE_CUTOUT_STANDARD                   _AD_EDGE_BASE_SOURCE_CUTOUT_GEOMETRIC                     _AD_EDGE_BASE_SOURCE_ALL
#pragma shader_feature_local _ _AD_EDGE_ADDITIONAL_COLOR_BASE_COLOR                   _AD_EDGE_ADDITIONAL_COLOR_CUSTOM_MAP                      _AD_EDGE_ADDITIONAL_COLOR_GRADIENT_MAP     _AD_EDGE_ADDITIONAL_COLOR_GRADIENT_COLOR     _AD_EDGE_ADDITIONAL_COLOR_USER_DEFINED
#pragma shader_feature_local   _AD_EDGE_UV_DISTORTION_SOURCE_CUSTOM_MAP    
#pragma shader_feature_local _ _AD_GLOBAL_CONTROL_ID_ONE                              _AD_GLOBAL_CONTROL_ID_TWO                                 _AD_GLOBAL_CONTROL_ID_THREE                _AD_GLOBAL_CONTROL_ID_FOUR
//Advanced Dissolve Keywords End/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#define ADVANCED_DISSOLVE_BUILTIN_RENDER_PIPELINE

#include "../../Core/Limits.cginc"
#include "../../../cginc/Core.cginc"
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



#include "UnityBuiltin3xTreeLibrary.cginc"

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _GlossMap;
half _Shininess;

struct Input {
    float2 uv_MainTex;
    fixed4 color : COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
    o.Albedo = c.rgb * IN.color.rgb * IN.color.a;
    o.Gloss = tex2D(_GlossMap, IN.uv_MainTex).a;
    o.Alpha = c.a;
    o.Specular = _Shininess;
    o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
}


// vertex-to-fragment interpolation data
// no lightmaps:
#ifndef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _MainTex
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD4; // SH
  #endif
  UNITY_LIGHTING_COORDS(5,6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO


//Advanced Dissolve
ADVANCED_DISSOLVE_UV(7)

};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _MainTex
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  fixed4 color : COLOR0;
  #if UNITY_SHOULD_SAMPLE_SH
  half3 sh : TEXCOORD4; // SH
  #endif
  UNITY_FOG_COORDS(5)
  UNITY_SHADOW_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO

//Advanced Dissolve
ADVANCED_DISSOLVE_UV(7)

};
#endif
#endif
// with lightmaps:
#ifdef LIGHTMAP_ON
// half-precision fragment shader registers:
#ifdef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
#define FOG_COMBINED_WITH_TSPACE
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _MainTex
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD4;
  UNITY_LIGHTING_COORDS(5,6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO

//Advanced Dissolve
ADVANCED_DISSOLVE_UV(7)

};
#endif
// high-precision fragment shader registers:
#ifndef UNITY_HALF_PRECISION_FRAGMENT_SHADER_REGISTERS
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _MainTex
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD4;
  UNITY_FOG_COORDS(5)
  UNITY_SHADOW_COORDS(6)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO

//Advanced Dissolve
ADVANCED_DISSOLVE_UV(7)

};
#endif
#endif
float4 _MainTex_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  TreeVertBark (v);

  //Curved World
    #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
        #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
            CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v.vertex, v.normal, v.tangent)
        #else
            CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)
        #endif
    #endif


  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  #ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #endif

  // SH/ambient and vertex lights
  #ifndef LIGHTMAP_ON
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      // Approximated illumination from non-important point lights
      #ifdef VERTEXLIGHT_ON
        o.sh += Shade4PointLights (
          unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
          unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
          unity_4LightAtten0, worldPos, worldNormal);
      #endif
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
  #endif // !LIGHTMAP_ON

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_TRANSFER_FOG_COMBINED_WITH_TSPACE(o,o.pos); // pass fog coordinates to pixel shader
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_TRANSFER_FOG_COMBINED_WITH_WORLD_POS(o,o.pos); // pass fog coordinates to pixel shader
  #else
    UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader
  #endif


//Advanced Dissolve 
ADVANCED_DISSOLVE_INIT_UV(o, v.texcoord.xy, o.pos)


  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);



//Advanced Dissolve////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
float3 positionWS = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
float3 normalWS = float3(IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z);

#if defined(_AD_STATE_ENABLED)

  float4 dissolveBase = 0;
  #if defined(_AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA) || defined(_AD_EDGE_ADDITIONAL_COLOR_BASE_COLOR)
        dissolveBase = tex2D(_MainTex, surfIN.uv_MainTex.xy);
        dissolveBase.rgb *= _Color.rgb;    
  #endif


  ADVANCED_DISSOLVE_SETUP_CUTOUT_SOURCE_USING_WS(IN, dissolveBase, positionWS, normalWS)
  AdvancedDissolveClip(cutoutSource);
    	
  float3 dissolveAlbedo = 0; 
  float3 dissolveEmission = 0;
  float dissolveBlend = AdvancedDissolveAlbedoEmission(cutoutSource, dissolveBase, dissolveAlbedo, dissolveEmission, surfIN.uv_MainTex.xy);

#endif
//Advanced Dissolve/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



  // call surface function
  surf (surfIN, o);


//Advanced Dissolve///////////
#if defined(_AD_STATE_ENABLED)
    o.Albedo = lerp(o.Albedo, dissolveAlbedo, dissolveBlend);
    o.Emission = lerp(o.Emission, dissolveEmission, dissolveBlend);
#endif



  // compute lighting & shadowing factor
  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingBlinnPhong_GI(o, giInput, gi);

  // realtime lighting: call lighting function
  c += LightingBlinnPhong (o, worldViewDir, gi);


  c.rgb += o.Emission;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}


ENDCG

}

	// ---- forward rendering additive lights pass:
	Pass {
		Name "FORWARD"
		Tags { "LightMode" = "ForwardAdd" }
		ZWrite Off Blend One One

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile_instancing
#pragma multi_compile_fog
#pragma skip_variants INSTANCING_ON
#pragma multi_compile_fwdadd noshadowmask nodynlightmap nolightmap
#pragma target 3.0

#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"

#include "UnityCG.cginc"
//Shader does not support lightmap thus we always want to fallback to SH.
#undef UNITY_SHOULD_SAMPLE_SH
#define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_PREPASSBASE) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
#include "Lighting.cginc"
#include "AutoLight.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))


//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"


//Advanced Dissolve Keywords Start///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma shader_feature_local   _AD_STATE_ENABLED
#pragma shader_feature_local _ _AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA				  _AD_CUTOUT_STANDARD_SOURCE_CUSTOM_MAP                     _AD_CUTOUT_STANDARD_SOURCE_TWO_CUSTOM_MAPS _AD_CUTOUT_STANDARD_SOURCE_THREE_CUSTOM_MAPS _AD_CUTOUT_STANDARD_SOURCE_USER_DEFINED
#pragma shader_feature_local _ _AD_CUTOUT_STANDARD_SOURCE_MAPS_MAPPING_TYPE_TRIPLANAR _AD_CUTOUT_STANDARD_SOURCE_MAPS_MAPPING_TYPE_SCREEN_SPACE
#pragma shader_feature_local _ _AD_CUTOUT_GEOMETRIC_TYPE_XYZ						  _AD_CUTOUT_GEOMETRIC_TYPE_PLANE                           _AD_CUTOUT_GEOMETRIC_TYPE_SPHERE           _AD_CUTOUT_GEOMETRIC_TYPE_CUBE               _AD_CUTOUT_GEOMETRIC_TYPE_CAPSULE       _AD_CUTOUT_GEOMETRIC_TYPE_CONE_SMOOTH
#pragma shader_feature_local _ _AD_CUTOUT_GEOMETRIC_COUNT_TWO					      _AD_CUTOUT_GEOMETRIC_COUNT_THREE                          _AD_CUTOUT_GEOMETRIC_COUNT_FOUR
#pragma shader_feature_local _ _AD_EDGE_BASE_SOURCE_CUTOUT_STANDARD                   _AD_EDGE_BASE_SOURCE_CUTOUT_GEOMETRIC                     _AD_EDGE_BASE_SOURCE_ALL
#pragma shader_feature_local _ _AD_EDGE_ADDITIONAL_COLOR_BASE_COLOR                   _AD_EDGE_ADDITIONAL_COLOR_CUSTOM_MAP                      _AD_EDGE_ADDITIONAL_COLOR_GRADIENT_MAP     _AD_EDGE_ADDITIONAL_COLOR_GRADIENT_COLOR     _AD_EDGE_ADDITIONAL_COLOR_USER_DEFINED
#pragma shader_feature_local   _AD_EDGE_UV_DISTORTION_SOURCE_CUSTOM_MAP    
#pragma shader_feature_local _ _AD_GLOBAL_CONTROL_ID_ONE                              _AD_GLOBAL_CONTROL_ID_TWO                                 _AD_GLOBAL_CONTROL_ID_THREE                _AD_GLOBAL_CONTROL_ID_FOUR
//Advanced Dissolve Keywords End/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#define ADVANCED_DISSOLVE_BUILTIN_RENDER_PIPELINE
#include "../../Core/Limits.cginc"
#include "../../../cginc/Core.cginc"
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



#include "UnityBuiltin3xTreeLibrary.cginc"

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _GlossMap;
half _Shininess;

struct Input {
    float2 uv_MainTex;
    fixed4 color : COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
    o.Albedo = c.rgb * IN.color.rgb * IN.color.a;
    o.Gloss = tex2D(_GlossMap, IN.uv_MainTex).a;
    o.Alpha = c.a;
    o.Specular = _Shininess;
    o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
}


// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _MainTex
  float3 tSpace0 : TEXCOORD1;
  float3 tSpace1 : TEXCOORD2;
  float3 tSpace2 : TEXCOORD3;
  float3 worldPos : TEXCOORD4;
  fixed4 color : COLOR0;
  UNITY_LIGHTING_COORDS(5,6)
  UNITY_FOG_COORDS(7)
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO

//Advanced Dissolve
ADVANCED_DISSOLVE_UV(8)

};
float4 _MainTex_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  TreeVertBark (v);

  //Curved World
    #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
        #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
            CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v.vertex, v.normal, v.tangent)
        #else
            CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)
        #endif
    #endif


  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float3(worldTangent.x, worldBinormal.x, worldNormal.x);
  o.tSpace1 = float3(worldTangent.y, worldBinormal.y, worldNormal.y);
  o.tSpace2 = float3(worldTangent.z, worldBinormal.z, worldNormal.z);
  o.worldPos.xyz = worldPos;
  o.color = v.color;

  UNITY_TRANSFER_LIGHTING(o,v.texcoord1.xy); // pass shadow and, possibly, light cookie coordinates to pixel shader
  UNITY_TRANSFER_FOG(o,o.pos); // pass fog coordinates to pixel shader


//Advanced Dissolve 
ADVANCED_DISSOLVE_INIT_UV(o, v.texcoord.xy, o.pos)


  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);


  //Advanced Dissolve////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
float3 positionWS = IN.worldPos.xyz;
float3 normalWS = float3(IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z);

#if defined(_AD_STATE_ENABLED)

  float4 dissolveBase = 0;
  #if defined(_AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA) || defined(_AD_EDGE_ADDITIONAL_COLOR_BASE_COLOR)
        dissolveBase = tex2D(_MainTex, surfIN.uv_MainTex.xy);
        dissolveBase.rgb *= _Color.rgb;    
  #endif


  ADVANCED_DISSOLVE_SETUP_CUTOUT_SOURCE_USING_WS(IN, dissolveBase, positionWS, normalWS)
  AdvancedDissolveClip(cutoutSource);
    	
  float3 dissolveAlbedo = 0; 
  float3 dissolveEmission = 0;
  float dissolveBlend = AdvancedDissolveAlbedoEmission(cutoutSource, dissolveBase, dissolveAlbedo, dissolveEmission, surfIN.uv_MainTex.xy);

#endif
//Advanced Dissolve/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


  // call surface function
  surf (surfIN, o);

  //Advanced Dissolve///////////
#if defined(_AD_STATE_ENABLED)
    o.Albedo = lerp(o.Albedo, dissolveAlbedo, dissolveBlend);
#endif


  UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
  fixed4 c = 0;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = _LightColor0.rgb;
  gi.light.dir = lightDir;
  gi.light.color *= atten;
  c += LightingBlinnPhong (o, worldViewDir, gi);
  c.a = 0.0;
  UNITY_APPLY_FOG(_unity_fogCoord, c); // apply fog
  UNITY_OPAQUE_ALPHA(c.a);
  return c;
}



ENDCG

}


	// ---- deferred shading pass:
	Pass {
		Name "DEFERRED"
		Tags { "LightMode" = "Deferred" }

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile_instancing
#pragma exclude_renderers nomrt
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_prepassfinal noshadowmask nodynlightmap nolightmap
#pragma target 3.0
#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"

#include "UnityCG.cginc"
//Shader does not support lightmap thus we always want to fallback to SH.
#undef UNITY_SHOULD_SAMPLE_SH
#define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_PREPASSBASE) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
#include "Lighting.cginc"

#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))


//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"


//Advanced Dissolve Keywords Start///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma shader_feature_local   _AD_STATE_ENABLED
#pragma shader_feature_local _ _AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA				  _AD_CUTOUT_STANDARD_SOURCE_CUSTOM_MAP                     _AD_CUTOUT_STANDARD_SOURCE_TWO_CUSTOM_MAPS _AD_CUTOUT_STANDARD_SOURCE_THREE_CUSTOM_MAPS _AD_CUTOUT_STANDARD_SOURCE_USER_DEFINED
#pragma shader_feature_local _ _AD_CUTOUT_STANDARD_SOURCE_MAPS_MAPPING_TYPE_TRIPLANAR _AD_CUTOUT_STANDARD_SOURCE_MAPS_MAPPING_TYPE_SCREEN_SPACE
#pragma shader_feature_local _ _AD_CUTOUT_GEOMETRIC_TYPE_XYZ						  _AD_CUTOUT_GEOMETRIC_TYPE_PLANE                           _AD_CUTOUT_GEOMETRIC_TYPE_SPHERE           _AD_CUTOUT_GEOMETRIC_TYPE_CUBE               _AD_CUTOUT_GEOMETRIC_TYPE_CAPSULE       _AD_CUTOUT_GEOMETRIC_TYPE_CONE_SMOOTH
#pragma shader_feature_local _ _AD_CUTOUT_GEOMETRIC_COUNT_TWO					      _AD_CUTOUT_GEOMETRIC_COUNT_THREE                          _AD_CUTOUT_GEOMETRIC_COUNT_FOUR
#pragma shader_feature_local _ _AD_EDGE_BASE_SOURCE_CUTOUT_STANDARD                   _AD_EDGE_BASE_SOURCE_CUTOUT_GEOMETRIC                     _AD_EDGE_BASE_SOURCE_ALL
#pragma shader_feature_local _ _AD_EDGE_ADDITIONAL_COLOR_BASE_COLOR                   _AD_EDGE_ADDITIONAL_COLOR_CUSTOM_MAP                      _AD_EDGE_ADDITIONAL_COLOR_GRADIENT_MAP     _AD_EDGE_ADDITIONAL_COLOR_GRADIENT_COLOR     _AD_EDGE_ADDITIONAL_COLOR_USER_DEFINED
#pragma shader_feature_local   _AD_EDGE_UV_DISTORTION_SOURCE_CUSTOM_MAP    
#pragma shader_feature_local _ _AD_GLOBAL_CONTROL_ID_ONE                              _AD_GLOBAL_CONTROL_ID_TWO                                 _AD_GLOBAL_CONTROL_ID_THREE                _AD_GLOBAL_CONTROL_ID_FOUR
//Advanced Dissolve Keywords End/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#define ADVANCED_DISSOLVE_BUILTIN_RENDER_PIPELINE

#include "../../Core/Limits.cginc"
#include "../../../cginc/Core.cginc"
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#include "UnityBuiltin3xTreeLibrary.cginc"

sampler2D _MainTex;
sampler2D _BumpMap;
sampler2D _GlossMap;
half _Shininess;

struct Input {
    float2 uv_MainTex;
    fixed4 color : COLOR;
};

void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
    o.Albedo = c.rgb * IN.color.rgb * IN.color.a;
    o.Gloss = tex2D(_GlossMap, IN.uv_MainTex).a;
    o.Alpha = c.a;
    o.Specular = _Shininess;
    o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_MainTex));
}


// vertex-to-fragment interpolation data
struct v2f_surf {
  UNITY_POSITION(pos);
  float2 pack0 : TEXCOORD0; // _MainTex
  float4 tSpace0 : TEXCOORD1;
  float4 tSpace1 : TEXCOORD2;
  float4 tSpace2 : TEXCOORD3;
  fixed4 color : COLOR0;
  float4 lmap : TEXCOORD4;
#ifndef LIGHTMAP_ON
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    half3 sh : TEXCOORD5; // SH
  #endif
#else
  #ifdef DIRLIGHTMAP_OFF
    float4 lmapFadePos : TEXCOORD5;
  #endif
#endif
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO

  //Advanced Dissolve
ADVANCED_DISSOLVE_UV(6)
};
float4 _MainTex_ST;

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  TreeVertBark (v);


  //Curved World
    #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
        #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
            CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v.vertex, v.normal, v.tangent)
        #else
            CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)
        #endif
    #endif


  o.pos = UnityObjectToClipPos(v.vertex);
  o.pack0.xy = TRANSFORM_TEX(v.texcoord, _MainTex);
  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  fixed3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
  fixed tangentSign = v.tangent.w * unity_WorldTransformParams.w;
  fixed3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
  o.tSpace0 = float4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
  o.tSpace1 = float4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
  o.tSpace2 = float4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
  o.color = v.color;
  o.lmap.zw = 0;
#ifdef LIGHTMAP_ON
  o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
  #ifdef DIRLIGHTMAP_OFF
    o.lmapFadePos.xyz = (mul(unity_ObjectToWorld, v.vertex).xyz - unity_ShadowFadeCenterAndType.xyz) * unity_ShadowFadeCenterAndType.w;
    o.lmapFadePos.w = (-UnityObjectToViewPos(v.vertex).z) * (1.0 - unity_ShadowFadeCenterAndType.w);
  #endif
#else
  o.lmap.xy = 0;
    #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
      o.sh = 0;
      o.sh = ShadeSHPerVertex (worldNormal, o.sh);
    #endif
#endif


//Advanced Dissolve 
ADVANCED_DISSOLVE_INIT_UV(o, v.texcoord.xy, o.pos)



  return o;
}
#ifdef LIGHTMAP_ON
float4 unity_LightmapFade;
#endif
fixed4 unity_Ambient;

// fragment shader
void frag_surf (v2f_surf IN,
    out half4 outGBuffer0 : SV_Target0,
    out half4 outGBuffer1 : SV_Target1,
    out half4 outGBuffer2 : SV_Target2,
    out half4 outEmission : SV_Target3
#if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    , out half4 outShadowMask : SV_Target4
#endif
) {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  Input surfIN;
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_RECONSTRUCT_TBN(IN);
  #else
    UNITY_EXTRACT_TBN(IN);
  #endif
  UNITY_INITIALIZE_OUTPUT(Input,surfIN);
  surfIN.uv_MainTex.x = 1.0;
  surfIN.color.x = 1.0;
  surfIN.uv_MainTex = IN.pack0.xy;
  float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
  surfIN.color = IN.color;
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);
  o.Normal = fixed3(0,0,1);


//Advanced Dissolve////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
float3 positionWS = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);
float3 normalWS = float3(IN.tSpace0.z, IN.tSpace1.z, IN.tSpace2.z);

#if defined(_AD_STATE_ENABLED)

  float4 dissolveBase = 0;
  #if defined(_AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA) || defined(_AD_EDGE_ADDITIONAL_COLOR_BASE_COLOR)
        dissolveBase = tex2D(_MainTex, surfIN.uv_MainTex.xy);
        dissolveBase.rgb *= _Color.rgb;    
  #endif


  ADVANCED_DISSOLVE_SETUP_CUTOUT_SOURCE_USING_WS(IN, dissolveBase, positionWS, normalWS)
  AdvancedDissolveClip(cutoutSource);
    	
  float3 dissolveAlbedo = 0; 
  float3 dissolveEmission = 0;
  float dissolveBlend = AdvancedDissolveAlbedoEmission(cutoutSource, dissolveBase, dissolveAlbedo, dissolveEmission, surfIN.uv_MainTex.xy);

#endif
//Advanced Dissolve/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


  // call surface function
  surf (surfIN, o);

//Advanced Dissolve///////////
#if defined(_AD_STATE_ENABLED)
    o.Albedo = lerp(o.Albedo, dissolveAlbedo, dissolveBlend);
    o.Emission = lerp(o.Emission, dissolveEmission, dissolveBlend);
#endif


fixed3 originalNormal = o.Normal;
  float3 worldN;
  worldN.x = dot(_unity_tbn_0, o.Normal);
  worldN.y = dot(_unity_tbn_1, o.Normal);
  worldN.z = dot(_unity_tbn_2, o.Normal);
  worldN = normalize(worldN);
  o.Normal = worldN;
  half atten = 1;

  // Setup lighting environment
  UnityGI gi;
  UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
  gi.indirect.diffuse = 0;
  gi.indirect.specular = 0;
  gi.light.color = 0;
  gi.light.dir = half3(0,1,0);
  // Call GI (lightmaps/SH/reflections) lighting function
  UnityGIInput giInput;
  UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
  giInput.light = gi.light;
  giInput.worldPos = worldPos;
  giInput.worldViewDir = worldViewDir;
  giInput.atten = atten;
  #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
    giInput.lightmapUV = IN.lmap;
  #else
    giInput.lightmapUV = 0.0;
  #endif
  #if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
    giInput.ambient = IN.sh;
  #else
    giInput.ambient.rgb = 0.0;
  #endif
  giInput.probeHDR[0] = unity_SpecCube0_HDR;
  giInput.probeHDR[1] = unity_SpecCube1_HDR;
  #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
    giInput.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
  #endif
  #ifdef UNITY_SPECCUBE_BOX_PROJECTION
    giInput.boxMax[0] = unity_SpecCube0_BoxMax;
    giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
    giInput.boxMax[1] = unity_SpecCube1_BoxMax;
    giInput.boxMin[1] = unity_SpecCube1_BoxMin;
    giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
  #endif
  LightingBlinnPhong_GI(o, giInput, gi);

  // call lighting function to output g-buffer
  outEmission = LightingBlinnPhong_Deferred (o, worldViewDir, gi, outGBuffer0, outGBuffer1, outGBuffer2);
  #if defined(SHADOWS_SHADOWMASK) && (UNITY_ALLOWED_MRT_COUNT > 4)
    outShadowMask = UnityGetRawBakedOcclusions (IN.lmap.xy, worldPos);
  #endif
  #ifndef UNITY_HDR_ON
  outEmission.rgb = exp2(-outEmission.rgb);
  #endif
}



ENDCG

}

	// ---- shadow caster pass:
	Pass {
		Name "ShadowCaster"
		Tags { "LightMode" = "ShadowCaster" }
		ZWrite On ZTest LEqual

CGPROGRAM
// compile directives
#pragma vertex vert_surf
#pragma fragment frag_surf
#pragma multi_compile_instancing
#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
#pragma multi_compile_shadowcaster noshadowmask nodynlightmap nolightmap
#pragma target 3.0

#include "HLSLSupport.cginc"
#define UNITY_INSTANCED_LOD_FADE
#define UNITY_INSTANCED_SH
#define UNITY_INSTANCED_LIGHTMAPSTS
#include "UnityShaderVariables.cginc"
#include "UnityShaderUtilities.cginc"

#include "UnityCG.cginc"
//Shader does not support lightmap thus we always want to fallback to SH.
#undef UNITY_SHOULD_SAMPLE_SH
#define UNITY_SHOULD_SAMPLE_SH (!defined(UNITY_PASS_FORWARDADD) && !defined(UNITY_PASS_PREPASSBASE) && !defined(UNITY_PASS_SHADOWCASTER) && !defined(UNITY_PASS_META))
#include "Lighting.cginc"

#define INTERNAL_DATA
#define WorldReflectionVector(data,normal) data.worldRefl
#define WorldNormalVector(data,normal) normal



//#define CURVEDWORLD_BEND_TYPE_CLASSICRUNNER_X_POSITIVE
//#define CURVEDWORLD_BEND_ID_1
//#pragma shader_feature_local CURVEDWORLD_DISABLED_ON
//#pragma shader_feature_local CURVEDWORLD_NORMAL_TRANSFORMATION_ON
//#include "Assets/Amazing Assets/Curved World/Shaders/Core/CurvedWorldTransform.cginc"


//Advanced Dissolve Keywords Start///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma shader_feature_local   _AD_STATE_ENABLED
#pragma shader_feature_local _ _AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA				  _AD_CUTOUT_STANDARD_SOURCE_CUSTOM_MAP                     _AD_CUTOUT_STANDARD_SOURCE_TWO_CUSTOM_MAPS _AD_CUTOUT_STANDARD_SOURCE_THREE_CUSTOM_MAPS _AD_CUTOUT_STANDARD_SOURCE_USER_DEFINED
#pragma shader_feature_local _ _AD_CUTOUT_STANDARD_SOURCE_MAPS_MAPPING_TYPE_TRIPLANAR _AD_CUTOUT_STANDARD_SOURCE_MAPS_MAPPING_TYPE_SCREEN_SPACE
#pragma shader_feature_local _ _AD_CUTOUT_GEOMETRIC_TYPE_XYZ						  _AD_CUTOUT_GEOMETRIC_TYPE_PLANE                           _AD_CUTOUT_GEOMETRIC_TYPE_SPHERE           _AD_CUTOUT_GEOMETRIC_TYPE_CUBE               _AD_CUTOUT_GEOMETRIC_TYPE_CAPSULE       _AD_CUTOUT_GEOMETRIC_TYPE_CONE_SMOOTH
#pragma shader_feature_local _ _AD_CUTOUT_GEOMETRIC_COUNT_TWO					      _AD_CUTOUT_GEOMETRIC_COUNT_THREE                          _AD_CUTOUT_GEOMETRIC_COUNT_FOUR
#pragma shader_feature_local _ _AD_EDGE_BASE_SOURCE_CUTOUT_STANDARD                   _AD_EDGE_BASE_SOURCE_CUTOUT_GEOMETRIC                     _AD_EDGE_BASE_SOURCE_ALL
#pragma shader_feature_local _ _AD_EDGE_ADDITIONAL_COLOR_BASE_COLOR                   _AD_EDGE_ADDITIONAL_COLOR_CUSTOM_MAP                      _AD_EDGE_ADDITIONAL_COLOR_GRADIENT_MAP     _AD_EDGE_ADDITIONAL_COLOR_GRADIENT_COLOR     _AD_EDGE_ADDITIONAL_COLOR_USER_DEFINED
#pragma shader_feature_local   _AD_EDGE_UV_DISTORTION_SOURCE_CUSTOM_MAP    
#pragma shader_feature_local _ _AD_GLOBAL_CONTROL_ID_ONE                              _AD_GLOBAL_CONTROL_ID_TWO                                 _AD_GLOBAL_CONTROL_ID_THREE                _AD_GLOBAL_CONTROL_ID_FOUR
//Advanced Dissolve Keywords End/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


#define ADVANCED_DISSOLVE_BUILTIN_RENDER_PIPELINE

#include "../../Core/Limits.cginc"
#include "../../../cginc/Core.cginc"
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



#include "UnityBuiltin3xTreeLibrary.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;




// vertex-to-fragment interpolation data
struct v2f_surf {
  V2F_SHADOW_CASTER;
  float3 worldPos : TEXCOORD1;
  float2 pack0 : TEXCOORD2;
  UNITY_VERTEX_INPUT_INSTANCE_ID
  UNITY_VERTEX_OUTPUT_STEREO


  //Advanced Dissolve
	float3 normalWS    : TEXCOORD3;
	ADVANCED_DISSOLVE_UV(4)
};

// vertex shader
v2f_surf vert_surf (appdata_full v) {
  UNITY_SETUP_INSTANCE_ID(v);
  v2f_surf o;
  UNITY_INITIALIZE_OUTPUT(v2f_surf,o);
  UNITY_TRANSFER_INSTANCE_ID(v,o);
  UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
  TreeVertBark (v);

  //Curved World
    #if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON)
        #ifdef CURVEDWORLD_NORMAL_TRANSFORMATION_ON
            CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(v.vertex, v.normal, v.tangent)
        #else
            CURVEDWORLD_TRANSFORM_VERTEX(v.vertex)
        #endif
    #endif


  float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
  float3 worldNormal = UnityObjectToWorldNormal(v.normal);
  o.worldPos.xyz = worldPos;
  o.pack0 = TRANSFORM_TEX(v.texcoord, _MainTex);
  TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)


  //Advanced Dissolve 
  o.normalWS = UnityObjectToWorldNormal(v.normal);
  ADVANCED_DISSOLVE_INIT_UV(o, v.texcoord.xy, o.pos)

  return o;
}

// fragment shader
fixed4 frag_surf (v2f_surf IN) : SV_Target {
  UNITY_SETUP_INSTANCE_ID(IN);
  // prepare and unpack data
  #ifdef FOG_COMBINED_WITH_TSPACE
    UNITY_EXTRACT_FOG_FROM_TSPACE(IN);
  #elif defined (FOG_COMBINED_WITH_WORLD_POS)
    UNITY_EXTRACT_FOG_FROM_WORLD_POS(IN);
  #else
    UNITY_EXTRACT_FOG(IN);
  #endif

  float3 worldPos = IN.worldPos.xyz;
  #ifndef USING_DIRECTIONAL_LIGHT
    fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
  #else
    fixed3 lightDir = _WorldSpaceLightPos0.xyz;
  #endif
  #ifdef UNITY_COMPILER_HLSL
  SurfaceOutput o = (SurfaceOutput)0;
  #else
  SurfaceOutput o;
  #endif
  o.Albedo = 0.0;
  o.Emission = 0.0;
  o.Specular = 0.0;
  o.Alpha = 0.0;
  o.Gloss = 0.0;
  fixed3 normalWorldVertex = fixed3(0,0,1);


  //Advanced Dissolve////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#if defined(_AD_STATE_ENABLED)
 
    float4 dissolveBase = 0;
    #if defined(_AD_CUTOUT_STANDARD_SOURCE_BASE_ALPHA) || defined(_AD_EDGE_ADDITIONAL_COLOR_BASE_COLOR)
        dissolveBase = tex2D(_MainTex, IN.pack0.xy);
        dissolveBase.rgb *= _Color.rgb;
    #endif

	ADVANCED_DISSOLVE_SETUP_CUTOUT_SOURCE_USING_WS(IN, dissolveBase, IN.worldPos.xyz, IN.normalWS.xyz)

    AdvancedDissolveClip(cutoutSource);  
#endif
//Advanced Dissolve/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////



  SHADOW_CASTER_FRAGMENT(IN)
}



ENDCG

}


}

Dependency "OptimizedShader" = "Hidden/Nature/Tree Creator Bark Optimized"
FallBack "Diffuse"
CustomEditor "AmazingAssets.AdvancedDissolve.Editor.DefaultUnlitShaderGUI"
}
