#pragma once

float4 packedNormal(half4 norm)
{
	return float4(norm.xy, 0, 1);
}

//  1 out, 3 in...
float hash13(float3 p3)
{
	p3 = frac(p3 * .1031);
	p3 += dot(p3, p3.yzx + 33.33);
	return frac((p3.x + p3.y) * p3.z);
}

// Define 24 rotation matrices for a cube
static float3 flips[8] = {
	float3(1, 1, 1),
	float3(-1, 1, 1),
	float3(1, -1, 1),
	float3(1, 1, -1),
	float3(-1, -1, 1),
	float3(-1, 1, -1),
	float3(1, -1, -1),
	float3(-1, -1, -1)
};

// Define a set of rotation matrices.
static float3x3 rotations[8] = {
	float3x3(1, 0, 0, 0, 1, 0, 0, 0, 1), // Identity matrix
	float3x3(0, -1, 0, 1, 0, 0, 0, 0, 1), // 90-degree rotation around Z
	float3x3(0, 1, 0, -1, 0, 0, 0, 0, 1), // -90-degree rotation around Z
	float3x3(1, 0, 0, 0, 0, -1, 0, 1, 0), // 90-degree rotation around X
	float3x3(1, 0, 0, 0, 0, 1, 0, -1, 0), // -90-degree rotation around X
	float3x3(0, 0, 1, 0, 1, 0, -1, 0, 0), // 90-degree rotation around Y
	float3x3(0, 0, -1, 0, 1, 0, 1, 0, 0), // -90-degree rotation around Y
	float3x3(-1, 0, 0, 0, -1, 0, 0, 0, 1) // 180-degree rotation around Z
};

#define NUM_PERTURB 8

void randomFlip(inout float3 wp)
{
	// Generate a pseudo-random value based on the world position.
	float3 WorldPosition = GetObjectToWorldMatrix()._m03_m13_m23;
	float h = hash13(WorldPosition);
	// uint perturb = (uint)floor(NUM_PERTURB * h);
	// uint perturb2 = (uint)floor(NUM_PERTURB * hash13(WorldPosition * 4.214));

	// wp = WorldPosition - wp;

	// wp = mul(rotations[perturb2], float4(wp, 0.0)).xyz;
	// wp=abs(wp+10.);
	// wp *= flips[perturb];
	wp += h;
	// Apply the selected rotation matrix to the coordinates.
}

inline void TriplanarSamplingPacked(TEXTURE2D (topTexMap), SAMPLER (samplertopTexMap), float3 worldPos,
                                    float3 worldNormal, float falloff, float2 tiling, float3 normalScale,
                                    out float3 normTS, out float propagate, out float mask)
{
	float3 projNormal = (pow(abs(worldNormal), falloff));
	projNormal /= (projNormal.x + projNormal.y + projNormal.z) + 0.00001;
	float3 nsign = sign(worldNormal);
	half4 xSmp;
	half4 ySmp;
	half4 zSmp;
	xSmp = SAMPLE_TEXTURE2D(topTexMap, samplertopTexMap, tiling * worldPos.zy * float2(nsign.x, 1.0));
	ySmp = SAMPLE_TEXTURE2D(topTexMap, samplertopTexMap, tiling * worldPos.xz * float2(nsign.y, 1.0));
	zSmp = SAMPLE_TEXTURE2D(topTexMap, samplertopTexMap, tiling * worldPos.xy * float2(-nsign.z, 1.0));

	half4 xNorm;
	half4 yNorm;
	half4 zNorm;
	xNorm.xyz = half3(UnpackNormalScale(packedNormal(xSmp), normalScale.y).xy * float2(nsign.x, 1.0) + worldNormal.zy,
	                  worldNormal.x).zyx;
	yNorm.xyz = half3(UnpackNormalScale(packedNormal(ySmp), normalScale.x).xy * float2(nsign.y, 1.0) + worldNormal.xz,
	                  worldNormal.y).xzy;
	zNorm.xyz = half3(UnpackNormalScale(packedNormal(zSmp), normalScale.y).xy * float2(-nsign.z, 1.0) + worldNormal.xy,
	                  worldNormal.z).xyz;

	normTS.xyz = normalize(xNorm.xyz * projNormal.x + yNorm.xyz * projNormal.y + zNorm.xyz * projNormal.z);

	half4 smp = xSmp * projNormal.x + ySmp * projNormal.y + zSmp * projNormal.z;

	propagate = saturate(smp.b);
	mask = saturate(smp.a);
}
