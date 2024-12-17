namespace VoxelBox.Rendering
{
	using UnityEngine;

	static class ShaderPropertyID
	{
		internal static readonly int VoxelNoise = Shader.PropertyToID("_VoxelNoise");
		internal static readonly int CracksPackedMap = Shader.PropertyToID("_CracksPackedMap");
		internal static readonly int VoxelDamageBuffer = Shader.PropertyToID(nameof(VoxelDamageEffectCBuffer));
	}
}
