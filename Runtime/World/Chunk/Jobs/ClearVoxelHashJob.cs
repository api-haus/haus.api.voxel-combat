namespace VoxelBox.World.Chunk.Jobs
{
	using Authoring;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;

	[BurstCompile]
	struct ClearVoxelHashJob : IJob
	{
		NativeParallelHashSet<Voxel> toDamage;
		NativeParallelHashSet<Voxel> toDestroy;

		public void Execute()
		{
			toDamage.Clear();
			toDestroy.Clear();
		}

		public static JobHandle Schedule(VoxelChunk chunk, JobHandle dependency) =>
			new ClearVoxelHashJob
			{
				toDamage = chunk.ToDamage, //
				toDestroy = chunk.ToDestroy, //
			}.Schedule(dependency);
	}
}
