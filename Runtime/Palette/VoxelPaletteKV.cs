namespace VoxelBox.Palette
{
	using System;
	using Authoring;
	using Unity.Entities;
	using UnityEngine;

	[Serializable]
	public struct VoxelPaletteItem
	{
		public VoxelPrototypeId protoId;
		public Entity VoxelPrefab;
		public VoxelData voxelData;
		public Bounds bounds;
		public VoxelFlags voxelFlags;
		public bool IsNull => VoxelPrefab == Entity.Null;
	}

	[Serializable]
	public struct VoxelPaletteKV : IBufferElementData
	{
		public VoxelPrototypeId key;
		public VoxelPaletteItem value;
	}
}
