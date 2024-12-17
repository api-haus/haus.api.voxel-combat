#pragma once

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

float4x4 Scale(float x, float y, float z)
{
	return float4x4(x, 0.0, 0.0, 0.0,
	                0.0, y, 0.0, 0.0,
	                0.0, 0.0, z, 0.0,
	                0.0, 0.0, 0.0, 1.0);
}
