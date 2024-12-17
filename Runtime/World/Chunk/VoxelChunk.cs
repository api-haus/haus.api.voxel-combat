namespace VoxelBox.World.Chunk
{
	using System;
	using System.Runtime.InteropServices;
	using Authoring;
	using Interaction;
	using Jobs;
	using NativeTrees;
	using Palette;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using UnityEngine;
	using static ChunkUtility;
	using static Unity.Mathematics.math;

	[StructLayout(LayoutKind.Sequential)]
	public partial struct VoxelChunk : IComponentData, IDisposable
	{
		public const int ObjectsPerNode = VoxelWorld.ObjectsPerNode;

		public Bounds Bounds => ChunkBounds(Coord);

		public bool IsAlive => !isDisposed;

		internal int3 Coord;
		internal Entity Entity;
		internal NativeOctree<Voxel> Tree;
		internal NativeParallelHashSet<Voxel> VoxelHash;
		internal NativeParallelHashMap<Voxel, VoxelData> VoxelDataMap;

		internal NativeParallelHashSet<DamageEntityPair> UniqueDamagePairs;
		internal NativeList<VoxelDamagedEvent> EventWriter;

		internal NativeParallelHashSet<Voxel> ToDamage;
		internal NativeParallelHashSet<Voxel> ToDestroy;

		bool isDisposed;

		public void Dispose()
		{
			if (isDisposed) return;
			isDisposed = true;
			Tree.Dispose();
			ToDamage.Dispose();
			ToDestroy.Dispose();
			VoxelHash.Dispose();
			VoxelDataMap.Dispose();
			UniqueDamagePairs.Dispose();
		}

		public JobHandle Load(NativeArray<Voxel>.ReadOnly voxels, NativeParallelHashMap<VoxelPrototypeId, VoxelPaletteItem>.ReadOnly voxelPalette,
			EntityCommandBuffer ecb,
			JobHandle dependency)
		{
			dependency = LoadVoxelsFromVolumeJob.Schedule(this, ecb, voxels, voxelPalette, dependency);

			return dependency;
		}

		public static VoxelChunk Allocate(VoxelWorld world, int3 coord, Entity chunkEntity)
		{
			const int capacity = VoxelWorld.ChunkSize3;
			const int maxDamagePairs = VoxelWorld.MaxDamageEventsPerFrame;
			const int maxZoneDestruction = capacity; // up to 100% of the chunk allowed to destroy by one damage zone

			int maxDepth = (int)ceil(log(capacity) / log(4)) + 1;

			maxDepth = min(10, maxDepth);

			return new VoxelChunk
			{
				Coord = coord,
				Entity = chunkEntity,
				EventWriter = world.DamageEvents,
				isDisposed = false,
				UniqueDamagePairs = new(maxDamagePairs,
					Allocator.Persistent),
				ToDamage = new(maxZoneDestruction,
					Allocator.Persistent),
				ToDestroy = new(maxZoneDestruction,
					Allocator.Persistent),
				VoxelDataMap = new(capacity,
					Allocator.Persistent),
				VoxelHash = new(capacity,
					Allocator.Persistent),
				Tree = new(
					ChunkBounds(coord),
					ObjectsPerNode,
					maxDepth,
					Allocator.Persistent),
			};
		}
	}
}
