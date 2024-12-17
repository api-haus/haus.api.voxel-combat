namespace VoxelBox.Editor.Baking
{
	using Unity.Collections;
	using Unity.Entities;
	using Authoring;

	[UpdateInGroup(typeof(PostBakingSystemGroup))]
	[WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
	[RequireMatchingQueriesForUpdate]
	partial class PostBakingVoxelSystem : SystemBase
	{
		protected override void OnUpdate()
		{
			var ecb = new EntityCommandBuffer(Allocator.TempJob);
			foreach (var linked in SystemAPI.Query<DynamicBuffer<LinkedEntityGroup>>()
				         .WithAll<IsVoxel>()
				         .WithOptions(EntityQueryOptions.IncludePrefab))
			{
				foreach (var linkedEntityGroup in linked)
				{
					ecb.AddComponent(linkedEntityGroup.Value,
						new VoxelDamageEffect { RecordedTime = 0, });
					ecb.AddComponent(linkedEntityGroup.Value,
						new VoxelTotalDamage { Value = 0, });
				}
			}

			ecb.Playback(EntityManager);
			ecb.Dispose();
		}
	}
}
