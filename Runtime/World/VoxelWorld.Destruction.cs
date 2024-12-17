namespace VoxelBox.World
{
	using Chunk;
	using Chunk.Components;
	using Interaction.DamageZones;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;

	public partial struct VoxelWorld
	{
		public void ApplyDamage(
			ref SystemState state,
			IDamageZone cmd, Entity cmdEntity,
			ref EndInitializationEntityCommandBufferSystem.Singleton ecbSingleton, // todo: reuse ecb per-chunk maybe?
			NativeParallelHashMap<int3, JobHandle> jobsInChunk)
		{
			var bounds = cmd.FullAABB;

			var minChunk = ChunkUtility.GetChunkCoord(bounds.min);
			var maxChunk = ChunkUtility.GetChunkCoord(bounds.max);

			var chunkEcb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

			foreach (var chunkCoord in ChunkUtility.ChunksMinMaxYield(minChunk, maxChunk))
			{
				if (TryGetLoadedChunk(chunkCoord, out var chunk))
				{
					if (!jobsInChunk.ContainsKey(chunkCoord))
						jobsInChunk[chunkCoord] = default;

					var voxelEcb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

					jobsInChunk[chunkCoord] = chunk.ApplyDamage(cmd, cmdEntity, voxelEcb, jobsInChunk[chunkCoord]);
					chunkEcb.SetComponentEnabled<ChunkIsDirty>(chunk.Entity, true);
				}
			}
		}
	}
}
