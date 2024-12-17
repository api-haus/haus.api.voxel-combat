namespace VoxelBox.World.Chunk.Traversal
{
	using System;
	using NativeTrees;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;
	using Authoring;
	using Interaction.DamageZones.Geometry;

	[BurstCompile]
	public struct RangeAABBSphereJob : IJob
	{
		AABB rangeAABB;
		Sphere sphere;
		[ReadOnly] NativeOctree<Voxel> tree;
		NativeParallelHashSet<Voxel> results;

		public void Execute()
		{
			var visitor = new RangeAABBSphereUniqueVisitor<Voxel>
			{
				Results = results, //
				Sphere = sphere,
			};

			tree.Range(rangeAABB, ref visitor);
		}

		public static JobHandle Schedule(NativeOctree<Voxel> tree, AABB rangeAABB, Sphere sphere,
			NativeParallelHashSet<Voxel> results,
			JobHandle dependency) =>
			new RangeAABBSphereJob
				{
					tree = tree, //
					sphere = sphere,
					results = results,
					rangeAABB = rangeAABB,
				}
				.Schedule(dependency);
	}

	[BurstCompile]
	public struct RangeAABBSphereUniqueVisitor<T> : IOctreeRangeVisitor<T> where T : unmanaged, IEquatable<T>
	{
		public NativeParallelHashSet<T> Results;
		public Sphere Sphere;

		public bool OnVisit(T obj, AABB objBounds, AABB queryRange)
		{
			if (Sphere.Overlaps(objBounds))
				Results.Add(obj);

			return true; // always keep iterating, we want to catch all objects
		}
	}
}
