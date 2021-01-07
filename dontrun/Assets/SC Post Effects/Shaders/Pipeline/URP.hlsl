// SC Post Effects
// Staggart Creations
// http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

//Libraries
#define URP
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "../Blending.hlsl"


#ifdef REQUIRE_DEPTH
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
#endif
#ifdef REQUIRE_DEPTH_NORMALS
TEXTURE2D_X(_CameraDepthNormalsTexture);
#endif

#define Clamp sampler_LinearClamp
#define Repeat sampler_LinearRepeat

#define _BlitTex _MainTex //Ensures multipass effects work using built-in blit
#define sampler_MainTex sampler_LinearClamp
TEXTURE2D_X(_MainTex); /* Always included */
float4 _MainTex_TexelSize;

//Tex sampling
#define DECLARE_RT(textureName) TEXTURE2D_X(textureName);
#define DECLARE_TEX(textureName) TEXTURE2D(textureName);
#define SAMPLE_RT(textureName, samplerName, uv) SAMPLE_TEXTURE2D_X(textureName, samplerName, uv)
#define SAMPLE_RT_LOD(textureName, samplerName, uv, mip) SAMPLE_TEXTURE2D_X_LOD(textureName, samplerName, uv, mip)
#define SAMPLE_TEX(textureName, samplerName, uv) SAMPLE_TEXTURE2D_LOD(textureName, samplerName, uv, 0)

#define SAMPLE_DEPTH(uv) SampleSceneDepth(uv) //Use function from DeclareDepthTexture, uses a float sampler
#define LINEAR_DEPTH(depth) Linear01Depth(depth, _ZBufferParams)
#define LINEAR_EYE_DEPTH(depth) LinearEyeDepth(depth, _ZBufferParams)
#define SAMPLE_DEPTH_NORMALS(uv) SAMPLE_RT(_CameraDepthNormalsTexture, Clamp, uv)

//Param and Arg usage is swapped
#define TEX_PARAM(textureName, samplerName) TEXTURE2D_ARGS(textureName, samplerName)
#define TEX_ARG(textureName, samplerName) TEXTURE2D_PARAM(textureName, samplerName)
#define RT_PARAM(textureName, samplerName) TEXTURE2D_X_ARGS(textureName, samplerName)
#define RT_ARG(textureName, samplerName) TEXTURE2D_X_PARAM(textureName, samplerName)

#define SCREEN_COLOR(uv) SAMPLE_RT(_MainTex, Clamp, uv);
//Shorthand for sampling MainTex
float4 ScreenColor(float2 uv) {
	return SAMPLE_RT(_MainTex, sampler_MainTex, uv);
}

//Generic functions
#include "../SCPE.hlsl"

//Stereo rendering
//#define UNITY_SETUP_INSTANCE_ID(v) UNITY_SETUP_INSTANCE_ID(v)
#define STEREO_EYE_INDEX_POST_VERTEX(input) UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
//#define UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o) UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o)

//Vertex
#define OBJECT_TO_CLIP(v) TransformObjectToHClip(v.positionOS.xyz)
#define GET_TRIANGLE_UV(v) v.uv
#define GET_TRIANGLE_UV_VR(uv, id) UnityStereoTransformScreenSpaceTex(uv)
float2 FlipUV(float2 uv) {
	return uv;
}
//Fragment
#define UV i.uv
#define UV_VR UnityStereoTransformScreenSpaceTex(i.uv)

struct Varyings2 {
	float4 positionCS : POSITION;
	float4 texcoord[2] : TEXCOORD0;
	float2 texcoordStereo : TEXCOORD2;
#if STEREO_INSTANCING_ENABLED
	uint stereoTargetEyeIndex : SV_RenderTargetArrayIndex;
#endif
};

struct Varyings3 {
	float4 positionCS : POSITION;
	float4 texcoord[3] : TEXCOORD0;
	float2 texcoordStereo : TEXCOORD3;
#if STEREO_INSTANCING_ENABLED
	uint stereoTargetEyeIndex : SV_RenderTargetArrayIndex;
#endif
};

#define EPSILON         1.0e-4

// Temporarly added so Fog shader compiles without error
// Quadratic color thresholding
// curve = (threshold - knee, knee * 2, 0.25 / knee)
//
half4 QuadraticThreshold(half4 color, half threshold, half3 curve)
{
	// Pixel brightness
	half br = Max3(color.r, color.g, color.b);

	// Under-threshold part: quadratic curve
	half rq = clamp(br - curve.x, 0.0, curve.y);
	rq = curve.z * rq * rq;

	// Combine and apply the brightness response curve.
	color *= max(rq, br - threshold) / max(br, EPSILON);

	return color;
}