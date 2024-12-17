namespace VoxelBox.World.Streaming.Awareness
{
	using Unity.Entities;
	using Unity.Mathematics;

	public struct VoxelAwarenessBox : IComponentData
	{
		public float3 Size;
	}
}
