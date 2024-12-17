namespace VoxelBox.Palette
{
	using System;
	using Unity.Mathematics;

	[Serializable]
	public readonly partial struct VoxelPrototypeId : IEquatable<VoxelPrototypeId>
	{
		public readonly uint4 Value;

		public bool Equals(VoxelPrototypeId other) => Value.Equals(other.Value);

		public override int GetHashCode() => Value.GetHashCode();

		public VoxelPrototypeId(uint4 id) =>
			Value = id;
	}
}
