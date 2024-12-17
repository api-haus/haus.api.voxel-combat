#if WITH_ALINE && VOXELBOX_DEBUG
namespace VoxelBox
{
	using Authoring;
	using Drawing;
	using JetBrains.Annotations;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using UnityEngine;
	using World;
	using World.Chunk;
	using World.Interaction;
	using World.Interaction.DamageZones;
	using static Unity.Entities.SystemAPI;

	[UpdateInGroup(typeof(PresentationSystemGroup))]
	public partial class VoxelDebuggingSystem : SystemBase
	{
		[BurstCompile]
		struct DebugVoxelsJob : IJob
		{
			public CommandBuilder Builder;
			public NativeParallelHashSet<Voxel>.Enumerator Voxels;

			public void Execute()
			{
				while (Voxels.MoveNext())
				{
					var voxel = Voxels.Current;
					Builder.Line(voxel.Bounds.Center, voxel.Bounds.Center + math.up(), Color.magenta);
					Builder.WireBox((Bounds)voxel.Bounds, Color.magenta);
					Builder.Cross(voxel.Bounds.Center, Color.magenta);
					Builder.Label2D(voxel.Bounds.Center, voxel.protoId.ToString());
				}
			}
		}

		protected override void OnCreate() => RequireForUpdate<VoxelWorld>();

		protected override void OnUpdate()
		{
			var builder = DrawingManager.GetBuilder(true);
			var voxelWorld = SystemAPI.GetSingleton<VoxelWorld>();
			var events = voxelWorld.DamageEvents.AsReadOnly();

			builder.PushLineWidth(3);

			foreach (var voxelChunkRef in Query<RefRW<VoxelChunk>>())
			{
				ref var voxelChunk = ref voxelChunkRef.ValueRW;
				builder.WireBox(voxelChunk.Bounds, Color.cyan);

				var voxels = voxelChunk.VoxelHash.GetEnumerator();

				Dependency = new DebugVoxelsJob
				{
					Voxels = voxels, //
					Builder = builder,
				}.Schedule(Dependency);
			}

			foreach (var voxelDamagedEvent in events)
			{
				var from = voxelDamagedEvent.ImpactPosition;
				var to = from + math.up();
				builder.PushDuration(1f);
				builder.Label2D(to, $"{voxelDamagedEvent.Damage}");
				builder.Arrow(from, to, Color.yellow);
				builder.PopDuration();
			}

			foreach (var cmd in Query<RefRO<DamageVoxelsInSphereZone>>())
			{
				// builder.WireBox(ChunkUtility.ChunkBounds(chunkCoord.ChunkCoord), Color.yellow);

				builder.SphereOutline(cmd.ValueRO.Sphere.Center, cmd.ValueRO.Sphere.Radius, Color.red);
				builder.WireBox((Bounds)cmd.ValueRO.FullAABB, Color.red);
			}

			foreach (var cmd in Query<RefRO<DamageVoxelsInAABBZone>>())
			{
				// builder.WireBox(ChunkUtility.ChunkBounds(chunkCoord.ChunkCoord), Color.yellow);
				builder.WireBox((Bounds)cmd.ValueRO.FullAABB, Color.red);
			}

			builder.PopLineWidth();
			builder.DisposeAfter(Dependency);
		}
	}
}
#endif
