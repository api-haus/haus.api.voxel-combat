namespace VoxelBox.World.Chunk.Traversal
{
	using NativeTrees;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;
	using Authoring;
	using Interaction.DamageZones;

	[BurstCompile]
	public struct RangeAABBDamageZoneJob<TDamageZone> : IJob where TDamageZone : IDamageZone
	{
		public TDamageZone Command;

		[ReadOnly] public NativeOctree<Voxel> Tree;
		public NativeParallelHashSet<Voxel> Results;

		public void Execute()
		{
			var visitor = new RangeAABBDamageZoneVisitor
			{
				Results = Results, //
				Command = Command,
			};

			Tree.Range(Command.FullAABB, ref visitor);
		}

		[BurstCompile]
		public struct RangeAABBDamageZoneVisitor : IOctreeRangeVisitor<Voxel>
		{
			public NativeParallelHashSet<Voxel> Results;
			public TDamageZone Command;

			public bool OnVisit(Voxel obj, AABB objBounds, AABB queryRange)
			{
				if (Command.Overlaps(objBounds))
					Results.Add(obj);

				return true; // always keep iterating, we want to catch all objects
			}
		}

	}
}
