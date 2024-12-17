namespace VoxelBox.World
{
	using System;
	using Chunk;
	using Chunk.Components;
	using Interaction;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;

	public partial struct VoxelWorld : IComponentData, IDisposable
	{
		public const int ChunkSize = 4;
		public const int ChunkSize2 = ChunkSize * ChunkSize;
		public const int ChunkSize3 = ChunkSize * ChunkSize * ChunkSize;
		public const int ObjectsPerNode = 6;

		public const int MaxLoadedChunks = 256;
		public const int MaxDamageEventsPerFrame = 1024;

		public EntityArchetype ChunkArchetype;
		public NativeHashMap<int3, VoxelChunk> Chunks;
		public NativeList<VoxelDamagedEvent> DamageEvents;
		public NativeParallelHashMap<int3, JobHandle> ChunkJobs;

		public JobHandle CombineJobs()
		{
			JobHandle job = default;
			foreach (var chunkJob in ChunkJobs)
				job = JobHandle.CombineDependencies(job, chunkJob.Value);
			return job;
		}

		public void CompleteJobs()
		{
			CombineJobs().Complete();
			ChunkJobs.Clear();
		}

		bool isDisposed;

		public static VoxelWorld Create(EntityArchetype chunkArchetype) =>
			new()
			{
				Chunks = new(MaxLoadedChunks, Allocator.Persistent), //
				ChunkJobs = new(MaxLoadedChunks, Allocator.Persistent),
				DamageEvents = new(MaxDamageEventsPerFrame, Allocator.Persistent),
				ChunkArchetype = chunkArchetype,
			};

		public void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;

			foreach (var kvPair in Chunks)
				kvPair.Value.Dispose();

			Chunks.Dispose();
			ChunkJobs.Dispose();
			DamageEvents.Dispose();
		}

		public bool TryUnloadChunk(ref SystemState state, int3 coord, out Entity chunkEntity)
		{
			if (Chunks.TryGetValue(coord, out var chunk))
			{
				chunkEntity = chunk.Entity;
				state.EntityManager.DestroyEntity(chunkEntity);
				Chunks.Remove(coord);
				chunk.Dispose();
				return true;
			}

			chunkEntity = Entity.Null;
			return false;
		}

		public bool TryLoadChunk(ref SystemState state, int3 coord, out VoxelChunk chunk)
		{
			if (!Chunks.TryGetValue(coord, out chunk))
			{
				var chunkEntity = state.EntityManager.CreateEntity(ChunkArchetype);
// #if UNITY_EDITOR
				// state.EntityManager.SetName(chunkEntity, coord.ToString());
// #endif
				chunk = VoxelChunk.Allocate(this, coord, chunkEntity);
				state.EntityManager.GetBuffer<LinkedEntityGroup>(chunkEntity)
					.Add(new LinkedEntityGroup() { Value = chunkEntity });
				state.EntityManager.SetComponentData(chunkEntity, chunk);
				state.EntityManager.SetComponentEnabled<ChunkHasProceduralVoxels>(chunkEntity, false);
				Chunks.Add(coord, chunk);
				return true;
			}

			return false;
		}

		public bool TryGetLoadedChunk(int3 coord, out VoxelChunk chunk)
		{
			if (Chunks.TryGetValue(coord, out chunk))
			{
				if (chunk.IsAlive)
					return true;
			}

			return false;
		}

		public bool HasChunk(int3 chunkCoord) =>
			Chunks.ContainsKey(chunkCoord);
	}
}
