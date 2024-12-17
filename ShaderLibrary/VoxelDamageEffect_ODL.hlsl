#ifndef VOXEL_DAMAGE_EFF_INC
#define VOXEL_DAMAGE_EFF_INC

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GlobalSamplers.hlsl"
#include "./VoxelDamageEffectCBuffer.hlsl"
#include "./PackedTriplanar.hlsl"
#include "./Matrices.hlsl"
#include "./Easing.hlsl"
#include "./Biplanar.hlsl"

#define voxelDamageEffectRotationDegrees voxelData.r
#define voxelDamageEffectDuration voxelData.g
#define voxelDamageEffectOffset voxelData.b
#define cracksTriplanarFalloff cracksData.r
#define cracksTriplanarTiling cracksData.g
#define cracksNormalScale cracksData.b
#define cracksOpacity cracksData.a

TEXTURE2D(_CracksPackedMap);
uniform sampler2D _VoxelNoise;

float InterpolateDamageValue(float val)
{
	return saturate(
		1. - ease_out_quad(
			voxelDamageEffectOffset + saturate((_Time.y - val) * voxelDamageEffectDuration)
		)
	);
}

float3x3 getmtx(float timerValue)
{
	float3 wpos = GetObjectToWorldMatrix()._m03_m13_m23;

	float3 uv = hash13(wpos) + wpos.xyz * voxelDamageTimeScale.w + _Time.yyy * voxelDamageTimeScale.xyz * timerValue;

	float3 axis = normalize(tex2Dlod(_VoxelNoise,
	                                 float4((uv.xz + uv.y) * voxelDamageUVTileOffset.xy + voxelDamageUVTileOffset.zw,
	                                        0, 0)).xyz * 2. - 1.);

	axis = normalize(axis * voxelDamageAxisMult + voxelDamageAxisBias);

	// float4x4 mtx = Scale(axis);

	return AngleAxis3x3(timerValue * PI * voxelDamageEffectRotationDegrees, axis);
}

void ModifyVert(float timerValue, inout float3 pos)
{
	float3x3 mtx = getmtx(timerValue);

	pos = mul(mtx, float4(pos, 1)).xyz;
}

void ModifyVert(float timerValue, inout float3 pos, inout float3 norm)
{
	float3x3 mtx = getmtx(timerValue);

	pos = mul(mtx, float4(pos, 1)).xyz;
	norm = mul(norm, (float3x3)mtx);
}

void GetBlend(float cracksPropagationMask, float cracksAlphaMask, inout float healthPercentage)
{
	// healthPercentage = ease_out_cubic(healthPercentage);
	healthPercentage = 1. - step(cracksPropagationMask, healthPercentage);
	// healthPercentage *= cracksAlphaMask;
}

void VoxelDamageCracksTriplanarSample(in float3 posWS, in float3 normWS, out float3 cracksNorm,
                                      out float cracksPropagationMask, out float cracksAlphaMask)
{
	// posWS = TransformWorldToObject(posWS);
	// posWS -= GetObjectToWorldMatrix()._m03_m13_m23;
	// normWS = TransformWorldToObjectNormal(normWS);
	posWS += cracksCenterTiling;
	randomFlip(posWS);
	TriplanarSamplingPacked(_CracksPackedMap, sampler_LinearRepeat, posWS, normWS, cracksTriplanarFalloff,
	                        cracksTriplanarTiling, cracksNormalScale, cracksNorm, cracksPropagationMask,
	                        cracksAlphaMask);
}

void VB_NormalBlend(float3 A, float3 B, out float3 Out)
{
	Out = normalize(float3(A.rg + B.rg, A.b * B.b));
	// Out = normalize(float3(A.rg + B.rg, lerp(A.b, B.b, .5)));


	// float3 t = A.xyz + float3(0.0, 0.0, 1.0);
	// float3 u = B.xyz * float3(-1.0, -1.0, 1.0);
	// Out = (t / t.z) * dot(t, u) - u;
}

void VB_GetCracksNormalTS(float totalDamagePercent, in float3 posWS, in float3 normWS, out float3 cracksNormTS)
{
	if (totalDamagePercent > 0)
	{
		float cracksAlphaMask;
		float cracksPropagationMask;
		VoxelDamageCracksTriplanarSample(posWS, normWS, cracksNormTS, cracksPropagationMask, cracksAlphaMask);
		GetBlend(cracksPropagationMask, cracksAlphaMask, totalDamagePercent);

		// cracksNormTS = float3(cracksNormTS.rg * totalDamagePercent,
		// lerp(1, cracksNormTS.b, saturate(totalDamagePercent)));
	}
}

void VB_Frag_float(
	float totalDamagePercent,
	in float3 posWS, in float3 normWS,
	in float3 albedoIn,
	out float3 albedoOut)
{
	albedoOut = albedoIn;
	if (totalDamagePercent > 0)
	{
		float3 cracksNormTS;
		float cracksAlphaMask;
		float cracksPropagationMask;
		VoxelDamageCracksTriplanarSample(posWS, normWS, cracksNormTS, cracksPropagationMask, cracksAlphaMask);
		GetBlend(cracksPropagationMask, cracksAlphaMask, totalDamagePercent);

		albedoOut = (lerp(albedoIn, cracksColor.xyz, totalDamagePercent * cracksOpacity));
	}
}


void VB_Frag_Biplanar_float(
	float damageTimer, float totalDamagePercent,
	in float3 posWS, in float3 normWS, in float3 wtan, in float3 wbtn,
	out float cracksValue, out float3 cracksNormalTS
)
{
	float2 packedmask;
	BiplanarNormal_packed_float(_CracksPackedMap, sampler_LinearRepeat,
	                            posWS, wtan, wbtn, normWS, cracksNormalTS, packedmask
	);

	float cracksPropagationMask = packedmask.r;
	float cracksAlphaMask = packedmask.g;

	GetBlend(cracksPropagationMask, cracksAlphaMask, totalDamagePercent);

	cracksValue = totalDamagePercent * cracksOpacity;

	// Rescale normal
	totalDamagePercent *= cracksNormalScale;
	cracksNormalTS = float3(cracksNormalTS.rg * totalDamagePercent,
	                        lerp(1, cracksNormalTS.b, saturate(totalDamagePercent)));
}

void VB_Frag_float(
	float damageTimer, float totalDamagePercent,
	in float3 posWS, in float3 normWS,
	in float3 albedoIn, in float3 emissiveIn, in float3 normTSIn,
	out float3 albedoOut, out float3 emissiveOut, out float3 normTSOut)
{
	albedoOut = albedoIn;
	normTSOut = normTSIn;
	emissiveOut = emissiveIn;
	if (totalDamagePercent > 0)
	{
		float3 cracksNormTS;
		float cracksAlphaMask;
		float cracksPropagationMask;
		VoxelDamageCracksTriplanarSample(posWS, normWS, cracksNormTS, cracksPropagationMask, cracksAlphaMask);
		GetBlend(cracksPropagationMask, cracksAlphaMask, totalDamagePercent);

		albedoOut = lerp(albedoIn, cracksColor.xyz, totalDamagePercent * cracksOpacity);

		cracksNormTS = float3(cracksNormTS.rg * totalDamagePercent,
		                      lerp(1, cracksNormTS.b, saturate(totalDamagePercent)));

		// normTSOut = cracksNormTS;
		VB_NormalBlend(normTSIn, cracksNormTS, normTSOut);
	}

	damageTimer = InterpolateDamageValue(damageTimer);
	if (damageTimer > 0)
	{
		emissiveOut = damageTimer * voxelDamageEmissiveColor.xyz;
	}
}

void VB_Vert_float(float damageTimer, in float3 posIn, out float3 posOut)
{
	posOut = posIn;
	damageTimer = InterpolateDamageValue(damageTimer);
	if (damageTimer > 0)
	{
		ModifyVert(damageTimer, posOut);
	}
}

void VB_Vert_float(float damageTimer, in float3 posIn, in float3 normIn, out float3 posOut, out float3 normOut)
{
	posOut = posIn;
	normOut = normIn;
	damageTimer = InterpolateDamageValue(damageTimer);
	if (damageTimer > 0)
	{
		ModifyVert(damageTimer, posOut, normOut);
	}
}

void VB_Vert_inout_float(float damageTimer, inout float3 pos)
{
	damageTimer = InterpolateDamageValue(damageTimer);
	if (damageTimer > 0)
	{
		ModifyVert(damageTimer, pos);
	}
}

void VB_Vert_inout_float(float damageTimer, inout float3 pos, inout float3 norm)
{
	damageTimer = InterpolateDamageValue(damageTimer);
	if (damageTimer > 0)
	{
		ModifyVert(damageTimer, pos, norm);
	}
}

#endif
