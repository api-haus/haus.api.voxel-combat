namespace VoxelBox.World.Chunk.Jobs
{
	using System;
	using Authoring;
	using Interaction;
	using Interaction.DamageZones;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Collections.LowLevel.Unsafe;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using static Unity.Mathematics.math;

	struct DamageEntityPair : IEquatable<DamageEntityPair>
	{
		public Entity Voxel;
		public Entity Issuer;

		public DamageEntityPair(Entity issuerEntity, Entity voxelEntity)
		{
			Voxel = voxelEntity;
			Issuer = issuerEntity;
		}

		public bool Equals(DamageEntityPair other) => other.GetHashCode() == GetHashCode();

		public override int GetHashCode() =>
			asint(hash(new int4(Voxel.Index, Voxel.Version, Issuer.Index, Issuer.Version)));
	}

	[BurstCompile]
	struct ApplyDamageJob<TDamageZone> : IJob where TDamageZone : IDamageZone
	{
		[ReadOnly] public NativeParallelHashSet<Voxel> VoxelsToDamage;
		public NativeParallelHashMap<Voxel, VoxelData> VoxelDataMap;

		public NativeParallelHashSet<Voxel> VoxelsToDestroy;
		public EntityCommandBuffer ECB;

		[NativeDisableContainerSafetyRestriction]
		public NativeList<VoxelDamagedEvent>.ParallelWriter EventWriter;

		public TDamageZone Cmd;
		public Entity CmdEntity;

		[NativeDisableContainerSafetyRestriction]
		internal NativeParallelHashSet<DamageEntityPair> UniqueDamagePairs;

		public void Execute()
		{
			foreach (var voxel in VoxelsToDamage)
			{
				if (Cmd.Issuer != Entity.Null)
				{
					var pair = new DamageEntityPair(Cmd.Issuer, voxel.Entity);

					if (UniqueDamagePairs.Contains(pair))
						continue;

					UniqueDamagePairs.Add(pair);
				}

				var data = VoxelDataMap[voxel];

				data.health -= Cmd.Damage;

				VoxelDataMap[voxel] = data;

				ECB.AppendToBuffer(voxel.Entity,
					new VoxelDamageEventsBuffer
					{
						Damage = Cmd.Damage, //
						RecordedAt = Cmd.ElapsedTime,
						RemainingHealth = data.health,
					});

				bool isVoxelAlive = data.health > 0;

				EventWriter.AddNoResize(new VoxelDamagedEvent
				{
					DamageZone = CmdEntity,
					Creator = Cmd.Issuer,
					Voxel = voxel.Entity,
					Damage = Cmd.Damage,
					IsVoxelAlive = isVoxelAlive,
					ImpactPosition = Cmd.FullAABB.Center,
					Payload = Cmd.Payload,
				});

				if (!isVoxelAlive)
				{
					VoxelsToDestroy.Add(voxel);
				}
			}
		}
	}
}
