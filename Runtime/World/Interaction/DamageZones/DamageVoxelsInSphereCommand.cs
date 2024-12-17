namespace VoxelBox.World.Interaction.DamageZones
{
	using System;
	using NativeTrees;
	using Unity.Entities;

	[Serializable]
	public struct DamageVoxelsInAABBZone : IDamageZone, IComponentData, IEnableableComponent
	{
		public EDamageZoneType Type => EDamageZoneType.AABB;

		public DamageVoxelsInAABBZone(int damage, float elapsedTime, AABB fullAABB, Entity creator = default,
			int payload = -1)
		{
			Damage = damage;
			Issuer = creator;
			Payload = payload;
			FullAABB = fullAABB;
			ElapsedTime = elapsedTime;
		}

		public Entity Issuer { get; }

		public int Damage { get; }
		public int Payload { get; }
		public AABB FullAABB { get; }
		public float ElapsedTime { get; }
		public bool Overlaps(AABB voxelBounds) => FullAABB.Overlaps(voxelBounds);
	}
}
