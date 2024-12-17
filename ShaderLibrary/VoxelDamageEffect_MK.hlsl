#ifndef VOXEL_DAMAGE_EFF_INC
#define VOXEL_DAMAGE_EFF_INC

#include "./VoxelDamageEffectCBuffer.hlsl"

uniform sampler2D _VoxelNoise;

//GIST ee439d5e7388f3aafc5296005c8c3f33 by keijiro
// Rotation with angle (in radians) and axis
float3x3 AngleAxis3x3(float angle, float3 axis)
{
	float c, s;
	sincos(angle, s, c);

	float t = 1 - c;
	float x = axis.x;
	float y = axis.y;
	float z = axis.z;

	return float3x3(
		t * x * x + c, t * x * y - s * z, t * x * z + s * y,
		t * x * y + s * z, t * y * y + c, t * y * z - s * x,
		t * x * z - s * y, t * y * z + s * x, t * z * z + c
	);
}


float ease_in_cubic(float x)
{
	float t = x;
	float b = 0;
	float c = 1;
	float d = 1;
	return c * (t /= d) * t * t + b;
}

float ease_in_out_cubic(float x)
{
	float t = x;
	float b = 0;
	float c = 1;
	float d = 1;
	if ((t /= d / 2) < 1) return c / 2 * t * t * t + b;
	return c / 2 * ((t -= 2) * t * t + 2) + b;
}

float ease_in_out_quart(float x)
{
	float t = x;
	float b = 0;
	float c = 1;
	float d = 1;
	if ((t /= d / 2) < 1) return c / 2 * t * t * t * t + b;
	return -c / 2 * ((t -= 2) * t * t * t - 2) + b;
}

float ease_in_quad(float x)
{
	float t = x;
	float b = 0;
	float c = 1;
	float d = 1;
	return c * (t /= d) * t + b;
}

float ease_out_quad(float x)
{
	float t = x;
	float b = 0;
	float c = 1;
	float d = 1;
	return -c * (t /= d) * (t - 2) + b;
}

float VoxelDamageValue()
{
	return saturate(
		1. - ease_out_quad(
			voxelDamageEffectOffset + saturate((_Time.y - _VoxelDamageEffect) * voxelDamageEffectDuration)
		)
	);
	// return 1;
}


float3 VoxelDamageEmissive()
{
	return VoxelDamageValue() * voxelDamageEmissiveColor.xyz;
}

///  3 out, 3 in...
float3 hash33(float3 p3)
{
	p3 = frac(p3 * float3(.1031, .1030, .0973));
	p3 += dot(p3, p3.yxz + 33.33);
	return frac((p3.xxy + p3.yxx) * p3.zyx);
}

void VoxelDamageVert(inout float3 pos)
{
	float val = VoxelDamageValue();
	if (val <= .00001) return;

	float3 wpos = GetObjectToWorldMatrix()._m03_m13_m23;

	float3 uv = wpos.xyz * voxelDamageTimeScale.w + _Time.yyy * voxelDamageTimeScale.xyz * val;

	float3 axis = normalize(tex2Dlod(_VoxelNoise,
	                                 float4((uv.xz + uv.y) * voxelDamageUVTileOffset.xy + voxelDamageUVTileOffset.zw,
	                                        0, 0)).xyz * 2. - 1.);

	axis = normalize(axis * voxelDamageAxisMult + voxelDamageAxisBias);

	pos = mul(AngleAxis3x3(val * PI * voxelDamageEffectRotationDegrees, axis), float4(pos, 0)).xyz;
}

#endif
