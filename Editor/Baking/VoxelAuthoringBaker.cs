namespace VoxelBox.Editor.Baking
{
	using Unity.Entities;
	using Authoring;

	public class VoxelAuthoringBaker : Baker<VoxelAuthoring>
	{
		public override void Bake(VoxelAuthoring authoring)
		{
			var entity = GetEntity(authoring, TransformUsageFlags.None);

			AddComponent<IsVoxel>(entity);
			AddBuffer<VoxelDamageEventsBuffer>(entity);
		}
	}
}
