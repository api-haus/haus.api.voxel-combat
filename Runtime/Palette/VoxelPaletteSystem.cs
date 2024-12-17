namespace VoxelBox.Palette
{
	using System;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using UnityEngine;
	using static Unity.Entities.SystemAPI;

	[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
	[RequireMatchingQueriesForUpdate]
	[BurstCompile]
	public partial struct VoxelPaletteSystem : ISystem
	{
		public struct Singleton : IComponentData, IDisposable
		{
			public const int MaxItems = 2048;

			public NativeParallelHashMap<VoxelPrototypeId, VoxelPaletteItem> VoxelPalette;

			public Singleton(Allocator allocator) =>
				VoxelPalette = new(MaxItems, allocator);

			public void Dispose() => VoxelPalette.Dispose();
		}

		public void OnCreate(ref SystemState state)
		{
			state.EntityManager.CreateSingleton(new Singleton(Allocator.Persistent));
			state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
			state.RequireForUpdate<Singleton>();
		}

		public void OnUpdate(ref SystemState state)
		{
			ref var paletteST = ref GetSingletonRW<Singleton>().ValueRW;

			var barrier = GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
			var ecb = barrier.CreateCommandBuffer(state.WorldUnmanaged);

			foreach (var (voxelPaletteBuffers, paletteEntity) in
			         Query<DynamicBuffer<VoxelPaletteKV>>()
				         .WithEntityAccess())
			{
				foreach (var voxelPaletteBuffer in voxelPaletteBuffers)
					paletteST.VoxelPalette.TryAdd(
						voxelPaletteBuffer.key,
						voxelPaletteBuffer.value
					);
				ecb.DestroyEntity(paletteEntity);
			}
		}

		public void OnDestroy(ref SystemState state) => GetSingleton<Singleton>().Dispose();
	}
}
