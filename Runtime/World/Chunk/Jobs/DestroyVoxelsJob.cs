namespace VoxelBox.World.Chunk.Jobs
{
	using Authoring;
	using NativeTrees;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;

	[BurstCompile]
	struct DestroyVoxelsJob : IJob
	{
		[ReadOnly] NativeParallelHashSet<Voxel> voxelsToDestroy;
		NativeOctree<Voxel> tree;
		NativeParallelHashSet<Voxel> chunkVoxels;
		NativeParallelHashMap<Voxel, VoxelData> voxelData;

		EntityCommandBuffer ecb;

		public void Execute()
		{
			foreach (var voxel in voxelsToDestroy)
			{
				ecb.DestroyEntity(voxel.Entity);
				chunkVoxels.Remove(voxel);
				voxelData.Remove(voxel);
			}

			tree.Clear();
			foreach (var voxel in chunkVoxels)
			{
				tree.Insert(voxel, voxel.Bounds);
			}
		}

		public static JobHandle Schedule(VoxelChunk chunk,
			EntityCommandBuffer ecb,
			JobHandle dependency) =>
			new DestroyVoxelsJob
				{
					tree = chunk.Tree,
					voxelsToDestroy = chunk.ToDestroy, //
					chunkVoxels = chunk.VoxelHash,
					voxelData = chunk.VoxelDataMap,
					ecb = ecb,
				}
				.Schedule(dependency);
	}
}
