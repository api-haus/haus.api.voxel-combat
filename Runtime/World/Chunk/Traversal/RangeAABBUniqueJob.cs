namespace VoxelBox.World.Chunk.Traversal
{
	using NativeTrees;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;
	using Authoring;

	[BurstCompile]
	public struct RangeAABBUniqueJob : IJob
	{
		AABB rangeAABB;
		[ReadOnly] NativeOctree<Voxel> tree;
		NativeParallelHashSet<Voxel> results;

		public void Execute() =>
			tree.RangeAABBUnique(rangeAABB, results);

		public static JobHandle Schedule(NativeOctree<Voxel> tree, AABB rangeAABB, NativeParallelHashSet<Voxel> results,
			JobHandle dependency) =>
			new RangeAABBUniqueJob
			{
				tree = tree, //
				results = results,
				rangeAABB = rangeAABB,
			}.Schedule(dependency);
	}
}
