namespace VoxelBox.World
{
	using Chunk;
	using Chunk.Components;
	using Unity.Burst;
	using Unity.Entities;
	using static Unity.Entities.SystemAPI;

	[BurstCompile]
	[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
	partial struct VoxelWorldSystem : ISystem
	{
		EntityArchetype chunkArchetype;

		public void OnCreate(ref SystemState state)
		{
			chunkArchetype = state.EntityManager.CreateArchetype(
				ComponentType.ReadWrite<VoxelChunk>(),
				ComponentType.ReadWrite<ChunkHasProceduralVoxels>(),
				ComponentType.ReadWrite<ChunkIsDirty>(),
				ComponentType.ReadWrite<LinkedEntityGroup>()
			);
			state.EntityManager.CreateSingleton(VoxelWorld.Create(chunkArchetype));
		}

		public void OnDestroy(ref SystemState state) => GetSingleton<VoxelWorld>().Dispose();
	}
}
