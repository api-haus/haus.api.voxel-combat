namespace VoxelBox.World.Interaction
{
	using Authoring;
	using Unity.Burst;
	using Unity.Entities;
	using static Unity.Mathematics.math;

	[UpdateInGroup(typeof(PresentationSystemGroup), OrderFirst = true)]
	[RequireMatchingQueriesForUpdate]
	[BurstCompile]
	partial struct VoxelDamageEffectPropagationSystem : ISystem
	{
		internal ComponentLookup<VoxelDamageEffect> VoxelDamageEffectLookup;
		internal ComponentLookup<VoxelTotalDamage> VoxelTotalDamageLookup;

		[BurstCompile]
		partial struct PropagateJob : IJobEntity
		{
			internal ComponentLookup<VoxelDamageEffect> VoxelDamageEffectLookup;
			internal ComponentLookup<VoxelTotalDamage> VoxelTotalDamageLookup;

			void Execute(DynamicBuffer<LinkedEntityGroup> linked, DynamicBuffer<VoxelDamageEventsBuffer> dmgEvents)
			{
				foreach (var voxelDamageEvent in dmgEvents)
				{
					foreach (var linkedEntityGroup in linked)
					{
						if (VoxelDamageEffectLookup.HasComponent(linkedEntityGroup.Value))
						{
							VoxelDamageEffectLookup[linkedEntityGroup.Value] = new VoxelDamageEffect
							{
								RecordedTime = voxelDamageEvent.RecordedAt,
							};
						}

						if (VoxelTotalDamageLookup.HasComponent(linkedEntityGroup.Value))
						{
							VoxelTotalDamageLookup[linkedEntityGroup.Value] = new VoxelTotalDamage
							{
								Value = 1f - saturate((float)voxelDamageEvent.RemainingHealth / (float)int.MaxValue),
							};
						}
					}
				}

				dmgEvents.Clear();
			}
		}

		public void OnCreate(ref SystemState state)
		{
			VoxelDamageEffectLookup = state.GetComponentLookup<VoxelDamageEffect>();
			VoxelTotalDamageLookup = state.GetComponentLookup<VoxelTotalDamage>();
		}

		public void OnUpdate(ref SystemState state)
		{
			VoxelDamageEffectLookup.Update(ref state);
			VoxelTotalDamageLookup.Update(ref state);

			state.Dependency = new PropagateJob
			{
				VoxelDamageEffectLookup = VoxelDamageEffectLookup, //
				VoxelTotalDamageLookup = VoxelTotalDamageLookup,
			}.Schedule(state.Dependency);
		}
	}
}
