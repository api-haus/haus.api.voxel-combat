namespace VoxelBox.World.Streaming.SceneVolumes
{
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Authoring;
	using Palette;
	using static Unity.Entities.SystemAPI;

	[BurstCompile]
	[RequireMatchingQueriesForUpdate]
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(VoxelPaletteSystem))]
	partial struct VoxelVolumeLoadingSystem : ISystem
	{
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
			state.RequireForUpdate<VoxelPaletteSystem.Singleton>();
			state.RequireForUpdate<VoxelWorld>();
		}

		public void OnUpdate(ref SystemState state)
		{
			var barrier = GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
			ref var voxelWorld = ref GetSingletonRW<VoxelWorld>().ValueRW;
			ref var paletteST = ref GetSingletonRW<VoxelPaletteSystem.Singleton>().ValueRW;

			if (paletteST.VoxelPalette.IsEmpty)
				return;

			foreach (var (volumeChunkRef, volumeVoxels, voxelVolumeChunkEntity)
			         in Query<RefRW<VoxelVolumeChunkCoord>, DynamicBuffer<Voxel>>()
				         .WithEntityAccess())
			{
				ref var volumeChunk = ref volumeChunkRef.ValueRW;
				// if (!voxelWorld.HasChunk(volumeChunk.Coord))
				// continue;
				voxelWorld.TryLoadChunk(ref state, volumeChunk.Coord, out var chunk);

				if (!voxelWorld.ChunkJobs.ContainsKey(volumeChunk.Coord))
					voxelWorld.ChunkJobs[volumeChunk.Coord] = default;

				var voxelEcb = barrier.CreateCommandBuffer(state.WorldUnmanaged);
				voxelWorld.ChunkJobs[chunk.Coord] =
					chunk.Load(
						volumeVoxels.AsNativeArray().AsReadOnly(),
						paletteST.VoxelPalette.AsReadOnly(), voxelEcb,
						voxelWorld.ChunkJobs[chunk.Coord]);

				var ecb = barrier.CreateCommandBuffer(state.WorldUnmanaged);
				ecb.DestroyEntity(voxelVolumeChunkEntity);
			}

			voxelWorld.CompleteJobs();
		}
	}
}
