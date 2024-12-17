namespace VoxelBox.World.Interaction.DamageZones
{
	using System;
	using Geometry;
	using NativeTrees;
	using Unity.Entities;

	[Serializable]
	public struct DamageVoxelsInSphereZone : IDamageZone, IComponentData, IEnableableComponent
	{
		public EDamageZoneType Type => EDamageZoneType.Sphere;

		public Sphere Sphere { get; }

		public DamageVoxelsInSphereZone(int damage, float elapsedTime, Sphere sphere, Entity creator = default,
			int payload = -1)
		{
			Sphere = sphere;
			Damage = damage;
			Issuer = creator;
			Payload = payload;
			ElapsedTime = elapsedTime;
		}

		public Entity Issuer { get; }

		public int Damage { get; }
		public int Payload { get; }
		public AABB FullAABB => Sphere.FullBounds;
		public float ElapsedTime { get; }
		public bool Overlaps(AABB voxelBounds) => Sphere.Overlaps(voxelBounds);
	}
}
