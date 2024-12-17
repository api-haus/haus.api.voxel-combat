namespace VoxelBox.Rendering
{
	using System;
	using System.Runtime.InteropServices;
	using Sirenix.OdinInspector;
	using UnityEngine;
	using UnityEngine.Rendering;

	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	[GenerateHLSL(generateCBuffer = true, needAccessors = false,
		sourcePath = "Packages/land.pala.voxelbox/ShaderLibrary/VoxelDamageEffectCBuffer")]
	public struct VoxelDamageEffectCBuffer
	{
		public Vector4 voxelDamageAxisMult;
		public Vector4 voxelDamageAxisBias;
		public Vector4 voxelDamageUVTileOffset;
		public Vector4 voxelDamageTimeScale;

		[ColorUsage(false, true)] public Color voxelDamageEmissiveColor;

		[LabelText("X = ROTATION DEGREES\nY = EFFECT DURATION\nZ = EFFECT OFFSET\nW = UNUSED")]
		[PropertySpace]
		public Vector4 voxelData;

		[HideInInspector] public Vector4 cracksCenterTiling;
		public Vector4 cracksCenter;
		[ColorUsage(false, true)] public Color cracksColor;

		[LabelText("X = FALLOFF\nY = TILING\nZ = NORMAL SCALE\nW = OPACITY")]
		[PropertySpace]
		public Vector4 cracksData;


		public static VoxelDamageEffectCBuffer Default() =>
			new()
			{
				voxelDamageAxisMult = new Vector3(1,
					1,
					1),
				voxelDamageAxisBias = new Vector3(0.25f,
					.31f,
					-0.74f),
				voxelDamageUVTileOffset = new(1,
					1,
					0,
					0),
				voxelDamageTimeScale = new(0.52f,
					.25f,
					0.61f,
					0.1f),
				voxelDamageEmissiveColor = Color.white,
				voxelData = new Vector4(
					5.98f,
					3,
					0
				),
				cracksCenter = Vector4.one * .5f,
				cracksData = new Vector4(
					100,
					.01f,
					1f,
					1f
				),
				cracksColor = Color.white,
			};
	}
}
