namespace VoxelBox.World.Streaming.Awareness
{
	using System;
	using Unity.Burst;
	using Unity.Entities;
	using Unity.Transforms;
	using UnityEngine;
	using Unity.Collections;
	using Unity.Mathematics;
	using static Chunk.ChunkUtility;
	using static Unity.Entities.SystemAPI;

	[BurstCompile]
	[RequireMatchingQueriesForUpdate]
	[UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
	partial struct VoxelBoxAwarenessSystem : ISystem
	{
		struct Singleton : IComponentData, IDisposable
		{
			internal NativeHashSet<int3> ChunksInView;

			public Singleton(Allocator allocator) => ChunksInView = new(VoxelWorld.MaxLoadedChunks, allocator);

			public void Dispose() => ChunksInView.Dispose();
		}

		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<Singleton>();
			state.RequireForUpdate<VoxelWorld>();
			state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
			state.EntityManager.CreateSingleton(new Singleton(Allocator.Persistent));
		}

		public void OnDestroy(ref SystemState state) => GetSingleton<Singleton>().Dispose();

		public void OnUpdate(ref SystemState state)
		{
			ref var s = ref GetSingletonRW<Singleton>().ValueRW;
			ref var voxelWorld = ref GetSingletonRW<VoxelWorld>().ValueRW;

			s.ChunksInView.Clear();

			using var chunks = voxelWorld.Chunks.GetValueArray(Allocator.TempJob);

			// Find chunks in view
			foreach (var (boxRef, ltwRef) in Query<RefRW<VoxelAwarenessBox>, RefRW<LocalToWorld>>())
			{
				ref var box = ref boxRef.ValueRW;
				ref var ltw = ref ltwRef.ValueRW;

				var bounds = new Bounds(ltw.Position, box.Size);

				var minChunk = GetChunkCoord(bounds.min);
				var maxChunk = GetChunkCoord(bounds.max);

				foreach (var chunkCoord in ChunksMinMaxYield(minChunk, maxChunk))
				{
					s.ChunksInView.Add(chunkCoord);
				}
			}

			// Unload chunks not in view
			foreach (var chunk in chunks)
			{
				if (!s.ChunksInView.Contains(chunk.Coord))
				{
					voxelWorld.TryUnloadChunk(ref state, chunk.Coord, out _);
				}
			}

			// Load chunks in view
			foreach (var chunkCoord in s.ChunksInView)
			{
				voxelWorld.TryLoadChunk(ref state, chunkCoord, out _);
			}
		}
	}
}
