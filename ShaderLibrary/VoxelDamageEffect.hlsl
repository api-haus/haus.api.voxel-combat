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
	healthPercentage = ease_out_expo(healthPercentage);
	healthPercentage = step(cracksPropagationMask, healthPercentage - .01);
	// healthPercentage *= cracksAlphaMask;
}

void VB_Frag_Biplanar_float(
	float damageTimer, float totalDamagePercent,
	in float3 posWS, in float3 normWS, in float3 wtan, in float3 wbtn,
	out float cracksValue, out float3 cracksNormalTS
)
{
	float2 packedMask;
	float3 wp = GetObjectToWorldMatrix()._m03_m13_m23;
	float h = hash13(wp);
	posWS -= wp;
	posWS += cracksCenter;
	posWS *= cracksTriplanarTiling * (.35 + h * (1. - .35));
	// randomFlip(posWS);
	BiplanarNormal_packed_float(_CracksPackedMap, sampler_LinearRepeat,
	                            posWS, wtan, wbtn, normWS, cracksNormalTS, packedMask
	);

	float cracksPropagationMask = packedMask.r;
	float cracksAlphaMask = packedMask.g;

	GetBlend(cracksPropagationMask, cracksAlphaMask, totalDamagePercent);

	cracksValue = totalDamagePercent * cracksOpacity * cracksAlphaMask;

	// Rescale normal
	totalDamagePercent *= cracksNormalScale;
	cracksNormalTS = float3(cracksNormalTS.rg * totalDamagePercent,
	                        lerp(1, cracksNormalTS.b, saturate(totalDamagePercent)));
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
