//
// This file was automatically generated. Please don't edit by hand. Execute Editor command [ Edit > Rendering > Generate Shader Includes ] instead
//

#ifndef VOXELDAMAGEEFFECTCBUFFER_HLSL
#define VOXELDAMAGEEFFECTCBUFFER_HLSL
// Generated from VoxelBox.Rendering.VoxelDamageEffectCBuffer
// PackingRules = Exact
CBUFFER_START(VoxelDamageEffectCBuffer)
    float4 voxelDamageAxisMult;
    float4 voxelDamageAxisBias;
    float4 voxelDamageUVTileOffset;
    float4 voxelDamageTimeScale;
    float4 voxelDamageEmissiveColor; // x: r y: g z: b w: a 
    float4 voxelData;
    float4 cracksCenterTiling;
    float4 cracksCenter;
    float4 cracksColor; // x: r y: g z: b w: a 
    float4 cracksData;
CBUFFER_END


#endif
