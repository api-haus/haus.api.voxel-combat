namespace VoxelBox.Palette
{
	using Authoring;
	using Unity.Entities;
	using UnityEngine;

	public class VoxelPaletteAuthoringBaker : Baker<VoxelPaletteAuthoring>
	{
		public override void Bake(VoxelPaletteAuthoring authoring)
		{
			DependsOn(authoring.palette);

			var paletteEntity = GetEntity(authoring, TransformUsageFlags.None);

			var paletteBuffer = AddBuffer<VoxelPaletteKV>(paletteEntity);

			foreach (var paletteVoxelPrefab in authoring.palette.voxelPrefabs)
			{
				RegisterPrefabForBaking(paletteVoxelPrefab);

				var prefabEntity = GetEntity(paletteVoxelPrefab, TransformUsageFlags.Dynamic);
				var voxelAuthoring = paletteVoxelPrefab.GetComponent<VoxelAuthoring>();

				if (!voxelAuthoring)
				{
					Debug.LogWarning("Voxel in Palette has no VoxelAuthoring component! Skipping...", paletteVoxelPrefab);
					continue;
				}

				var protoId = voxelAuthoring.GetPrototypeID();

				DependsOn(voxelAuthoring);
				paletteBuffer.Add(
					new VoxelPaletteKV
					{
						key = protoId,
						value = new VoxelPaletteItem
						{
							protoId = protoId,
							VoxelPrefab = prefabEntity, //
							voxelData = voxelAuthoring.data,
							bounds = voxelAuthoring.LocalBounds(),
							voxelFlags = voxelAuthoring.flags,
						}
					});
			}
		}
	}
}
