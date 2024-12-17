namespace VoxelBox.World.Chunk.Components
{
	using Unity.Entities;

	public struct ChunkIsDirty : IComponentData, IEnableableComponent
	{
	}

	public struct ChunkHasProceduralVoxels : IComponentData, IEnableableComponent
	{
	}
}
