namespace VoxelBox.World.Interaction.DamageZones
{
	using NativeTrees;
	using Unity.Entities;

	public interface IDamageZone
	{
		EDamageZoneType Type { get; }
		int Damage { get; }
		int Payload { get; }
		AABB FullAABB { get; }
		float ElapsedTime { get; }
		public Entity Issuer { get; }
		bool Overlaps(AABB voxelBounds);
	}
}
