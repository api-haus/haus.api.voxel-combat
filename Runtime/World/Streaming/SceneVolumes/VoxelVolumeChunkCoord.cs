namespace VoxelBox.World.Streaming.SceneVolumes
{
	using Unity.Entities;
	using Unity.Mathematics;

	public struct VoxelVolumeChunkCoord : IComponentData
	{
		public int3 Coord;
	}
}
