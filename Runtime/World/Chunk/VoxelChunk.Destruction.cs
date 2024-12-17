namespace VoxelBox.World.Chunk
{
	using System;
	using Interaction.DamageZones;
	using Jobs;
	using Traversal;
	using Unity.Entities;
	using Unity.Jobs;

	public partial struct VoxelChunk
	{
		public JobHandle ApplyDamage(IDamageZone cmd, Entity cmdEntity, EntityCommandBuffer ecb,
			JobHandle dependency)
		{
			dependency = ClearVoxelHashJob.Schedule(this, dependency);
			dependency = RangeAABBDamageZoneJobSchedule(this, cmd, dependency);
			dependency = ApplyDamageJobSchedule(this, cmd, cmdEntity, ecb, dependency);
			dependency = DestroyVoxelsJob.Schedule(this, ecb, dependency);

			return dependency;
		}

		static JobHandle RangeAABBDamageZoneJobSchedule(VoxelChunk chunk, IDamageZone cmd,
			JobHandle dependency) =>
			cmd.Type switch
			{
				EDamageZoneType.Sphere => new RangeAABBDamageZoneJob<DamageVoxelsInSphereZone>
					{
						Tree = chunk.Tree, //
						Command = (DamageVoxelsInSphereZone)cmd,
						Results = chunk.ToDamage,
					}
					.Schedule(dependency),
				EDamageZoneType.AABB => new RangeAABBDamageZoneJob<DamageVoxelsInAABBZone>
					{
						Tree = chunk.Tree, //
						Command = (DamageVoxelsInAABBZone)cmd,
						Results = chunk.ToDamage,
					}
					.Schedule(dependency),
				_ => throw new ArgumentOutOfRangeException(),
			};

		static JobHandle ApplyDamageJobSchedule(
			VoxelChunk chunk,
			IDamageZone cmd,
			Entity cmdEntity,
			EntityCommandBuffer ecb,
			JobHandle dependency = default) =>
			cmd.Type switch
			{
				EDamageZoneType.Sphere => new ApplyDamageJob<DamageVoxelsInSphereZone>
				{
					VoxelsToDamage = chunk.ToDamage, //
					VoxelsToDestroy = chunk.ToDestroy, //
					VoxelDataMap = chunk.VoxelDataMap,
					EventWriter = chunk.EventWriter.AsParallelWriter(),
					ECB = ecb,
					Cmd = (DamageVoxelsInSphereZone)cmd,
					CmdEntity = cmdEntity,
					UniqueDamagePairs = chunk.UniqueDamagePairs,
				}.Schedule(dependency),
				EDamageZoneType.AABB => new ApplyDamageJob<DamageVoxelsInAABBZone>
				{
					VoxelsToDamage = chunk.ToDamage, //
					VoxelsToDestroy = chunk.ToDestroy, //
					VoxelDataMap = chunk.VoxelDataMap,
					EventWriter = chunk.EventWriter.AsParallelWriter(),
					ECB = ecb,
					Cmd = (DamageVoxelsInAABBZone)cmd,
					CmdEntity = cmdEntity,
					UniqueDamagePairs = chunk.UniqueDamagePairs,
				}.Schedule(dependency),
				_ => throw new ArgumentOutOfRangeException()
			};
	}
}
