namespace VoxelBox.Misc.Toys.Worms
{
	using NativeTrees;
	using Unity.Entities;
	using Unity.Mathematics;

	public struct Worm : IComponentData
	{
		public Random Random;
		public float3 Direction;
		public float TimeToSwitch;
		public float Speed;
		public AABB Box;
	}
}
