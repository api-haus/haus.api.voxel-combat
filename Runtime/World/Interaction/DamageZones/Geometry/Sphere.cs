namespace VoxelBox.World.Interaction.DamageZones.Geometry
{
	using System.Runtime.CompilerServices;
	using NativeTrees;
	using Unity.Mathematics;
	using UnityEngine;
	using static Unity.Mathematics.math;

	public readonly struct Sphere
	{
		public float Radius { get; }
		public float3 Center { get; }
		public Bounds FullBounds => new(Center, (float3)(Radius * 2f));

		public float RadiusSq { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Overlaps(AABB objBounds) =>
			distancesq(objBounds.ClosestPoint(Center), Center) <= RadiusSq;

		public Sphere(float3 center, float radius)
		{
			Center = center;
			Radius = radius;
			RadiusSq = radius * radius;
		}
	}
}
