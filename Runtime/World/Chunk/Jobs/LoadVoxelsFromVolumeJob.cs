namespace VoxelBox.World.Chunk.Jobs
{
	using Authoring;
	using Misc.Common;
	using NativeTrees;
	using Palette;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Collections.LowLevel.Unsafe;
	using Unity.Entities;
	using Unity.Jobs;
	using UnityEngine;

	[BurstCompile]
	struct LoadVoxelsFromVolumeJob : IJob
	{
		[NativeDisableContainerSafetyRestriction]
		[ReadOnly] NativeArray<Voxel>.ReadOnly volumeVoxels;

		internal Entity ChunkEntity;
		internal NativeOctree<Voxel> Tree;
		internal NativeParallelHashSet<Voxel> VoxelHash;
		internal NativeParallelHashMap<Voxel, VoxelData> VoxelDataMap;
		[ReadOnly] internal NativeParallelHashMap<VoxelPrototypeId, VoxelPaletteItem>.ReadOnly VoxelPalette;

		EntityCommandBuffer ecb;

		public void Execute()
		{
			foreach (var srcVoxel in volumeVoxels)
			{
				if (!VoxelPalette.TryGetValue(srcVoxel.protoId, out var voxelPaletteItem))
				{
					Debug.LogWarning("cannot find voxel");
					continue;
				}

				VoxelDataMap.Add(srcVoxel, voxelPaletteItem.voxelData);
				VoxelHash.Add(srcVoxel);
				Tree.Insert(srcVoxel, srcVoxel.Bounds);

				ecb.AppendToBuffer(ChunkEntity, new LinkedEntityGroup { Value = srcVoxel.Entity, });
			}
		}

		public static JobHandle Schedule(VoxelChunk chunk, EntityCommandBuffer ecb,
			NativeArray<Voxel>.ReadOnly voxels, NativeParallelHashMap<VoxelPrototypeId, VoxelPaletteItem>.ReadOnly voxelPalette,
			JobHandle dependency) =>
			new LoadVoxelsFromVolumeJob
				{
					ecb = ecb,
					Tree = chunk.Tree,
					VoxelHash = chunk.VoxelHash,
					ChunkEntity = chunk.Entity,
					VoxelDataMap = chunk.VoxelDataMap, //
					VoxelPalette = voxelPalette,
					volumeVoxels = voxels,
				}
				.Schedule(dependency);
	}
}
