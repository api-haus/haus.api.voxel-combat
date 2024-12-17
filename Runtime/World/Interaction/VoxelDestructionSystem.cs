namespace VoxelBox.World.Interaction
{
	using Chunk;
	using Chunk.Components;
	using Chunk.Jobs;
	using DamageZones;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Mathematics;
	using static Unity.Entities.SystemAPI;

	public struct VoxelDamagedEvent
	{
		public Entity DamageZone;
		public Entity Creator;
		public Entity Voxel;
		public int Damage;
		public bool IsVoxelAlive;
		public float3 ImpactPosition;
		public int Payload;
	}

	[BurstCompile]
	[RequireMatchingQueriesForUpdate]
	[UpdateInGroup(typeof(InitializationSystemGroup))]
	[UpdateAfter(typeof(VoxelDestructionSystem))]
	partial struct VoxelCleanupSystem : ISystem
	{
		EntityStorageInfoLookup entityLookup;

		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
			entityLookup = state.GetEntityStorageInfoLookup();
		}

		public void OnUpdate(ref SystemState state)
		{
			entityLookup.Update(ref state);

			var endInit = GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
			var ecb = endInit.CreateCommandBuffer(state.WorldUnmanaged);

			foreach (var (voxelChunkRef, chunkEntity) in Query<RefRW<VoxelChunk>>()
				         .WithEntityAccess()
				         .WithAll<ChunkIsDirty>())
			{
				ref var chunk = ref voxelChunkRef.ValueRW;

				using var pairsToRemove = new NativeList<DamageEntityPair>(Allocator.TempJob);

				foreach (var uniqueDamagePair in chunk.UniqueDamagePairs)
				{
					if (!entityLookup.Exists(uniqueDamagePair.Issuer))
					{
						pairsToRemove.Add(uniqueDamagePair);
					}
				}

				foreach (var damageEntityPair in pairsToRemove)
				{
					chunk.UniqueDamagePairs.Remove(damageEntityPair);
				}

				ecb.SetComponentEnabled<ChunkIsDirty>(chunkEntity, false);
			}
		}
	}

	[BurstCompile]
	[RequireMatchingQueriesForUpdate]
	[UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
	partial struct VoxelDestructionSystem : ISystem
	{
		public void OnCreate(ref SystemState state)
		{
			state.RequireForUpdate<VoxelWorld>();
			state.RequireForUpdate<EndInitializationEntityCommandBufferSystem.Singleton>();
			state.RequireForUpdate<BeginInitializationEntityCommandBufferSystem.Singleton>();
		}

		public void OnUpdate(ref SystemState state)
		{
			var endInit = GetSingleton<EndInitializationEntityCommandBufferSystem.Singleton>();
			ref var voxelWorld = ref GetSingletonRW<VoxelWorld>().ValueRW;

			// voxelWorld.ChunkJobs.Clear();
			voxelWorld.DamageEvents.Clear();

			var ecb = endInit.CreateCommandBuffer(state.WorldUnmanaged);

			foreach (var (cmdRef, cmdEntity)
			         in Query<RefRO<DamageVoxelsInSphereZone>>()
				         .WithEntityAccess())
			{
				voxelWorld.ApplyDamage(ref state, cmdRef.ValueRO, cmdEntity, ref endInit, voxelWorld.ChunkJobs);
				ecb.DestroyEntity(cmdEntity);
			}

			foreach (var (cmdRef, cmdEntity)
			         in Query<RefRO<DamageVoxelsInAABBZone>>()
				         .WithEntityAccess())
			{
				voxelWorld.ApplyDamage(ref state, cmdRef.ValueRO, cmdEntity, ref endInit, voxelWorld.ChunkJobs);
				ecb.DestroyEntity(cmdEntity);
			}

			voxelWorld.CompleteJobs();
		}
	}
}
