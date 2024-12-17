namespace VoxelBox.Authoring
{
	using System;

	[Flags]
	public enum VoxelFlags : ushort
	{
		None = 0,
		IsSolid = 1 << 0,
		Gravity = 1 << 1,
		Destructible = 1 << 2,
		Default = IsSolid | Destructible,
	}
}
