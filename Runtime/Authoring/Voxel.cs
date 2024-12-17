namespace VoxelBox.Authoring
{
	using System;
	using NativeTrees;
	using Palette;
	using Unity.Entities;
	using Unity.Mathematics;
	using Unity.Mathematics.Geometry;
	using Unity.Rendering;
	using UnityEngine;

	[Serializable]
	public struct VoxelData
	{
		public int health;

		public static VoxelData Default() =>
			new() { health = int.MaxValue };
	}

	[MaterialProperty("_VB_DamageTimer")]
	public struct VoxelDamageEffect : IComponentData
	{
		public float RecordedTime;
	}

	[MaterialProperty("_VB_TotalDamageFactor")]
	public struct VoxelTotalDamage : IComponentData
	{
		public float Value;
	}

	public struct IsVoxel : IComponentData
	{
	}

	public struct VoxelDamageEventsBuffer : IBufferElementData
	{
		public int Damage;
		public float RecordedAt;
		public int RemainingHealth;
	}

	[Serializable]
	public struct Voxel : IBufferElementData, IEquatable<Voxel>
	{
		public static Voxel Null => new Voxel();

		public VoxelPrototypeId protoId;

		public AABB Bounds;
		public Entity Entity;
		public Entity PrefabEntity;

		public bool Equals(Voxel other) => Entity == other.Entity;

		public override int GetHashCode() => math.asint(math.hash(new int2(Entity.Index, Entity.Version)));

		// Since we use Entity as only equatable, must be able to use Entity in place of Voxel for containment checks
		public static implicit operator Voxel(Entity ent) => new() { Entity = ent };
	}
}
