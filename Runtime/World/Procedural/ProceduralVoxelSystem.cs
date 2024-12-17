namespace VoxelBox.World.Procedural
{
	using Unity.Burst;
	using Unity.Entities;
	using Authoring;
	using Palette;
	using Chunk;
	using Chunk.Components;
	using Streaming.SceneVolumes;
	using static Unity.Entities.SystemAPI;

	[BurstCompile]
	[DisableAutoCreation]
	[RequireMatchingQueriesForUpdate]
	[UpdateInGroup(typeof(PresentationSystemGroup))]
	partial struct ProceduralVoxelSystem : ISystem
	{
		public void OnCreate(ref SystemState state)
		{
			var volumeArchetype = state.EntityManager.CreateArchetype(
				ComponentType.ReadWrite<Voxel>(),
				ComponentType.ReadWrite<VoxelVolumeChunkCoord>()
			);

			state.EntityManager.CreateSingleton(Singleton.Create(volumeArchetype));

			state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
			state.RequireForUpdate<VoxelPaletteSystem.Singleton>();
			state.RequireForUpdate<Singleton>();
		}

		public void OnUpdate(ref SystemState state)
		{
			ref var st = ref GetSingletonRW<Singleton>().ValueRW;

			ref var paletteST = ref GetSingletonRW<VoxelPaletteSystem.Singleton>().ValueRW;

			if (!st.Load(paletteST))
				return;

			var barrier = GetSingleton<BeginInitializationEntityCommandBufferSystem.Singleton>();
			var ecb = barrier.CreateCommandBuffer(state.WorldUnmanaged);

			st.Clear();

			foreach (var voxelChunkRef in Query<RefRW<VoxelChunk>>().WithDisabled<ChunkHasProceduralVoxels>())
			{
				ref var chunk = ref voxelChunkRef.ValueRW;

				ecb.SetComponentEnabled<ChunkHasProceduralVoxels>(chunk.Entity, true);

				var ecb2 = barrier.CreateCommandBuffer(state.WorldUnmanaged);
				st.Generate(chunk, ecb2);
			}

			st.CompleteJobs();
		}

		public void OnDestroy(ref SystemState state) =>
			GetSingleton<Singleton>().Dispose();
	}
}
