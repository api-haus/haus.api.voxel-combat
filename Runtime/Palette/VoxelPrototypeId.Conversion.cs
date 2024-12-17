namespace VoxelBox.Palette
{
	using System.Runtime.CompilerServices;
	using Unity.Mathematics;

	public readonly partial struct VoxelPrototypeId
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator uint4(VoxelPrototypeId id) => id.Value;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static implicit operator VoxelPrototypeId(uint4 id) => new(id);
	}
}
