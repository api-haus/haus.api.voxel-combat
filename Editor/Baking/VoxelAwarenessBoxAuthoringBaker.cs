namespace VoxelBox.Editor.Baking
{
	using Unity.Entities;
	using World.Streaming;
	using World.Streaming.Awareness;

	public class VoxelAwarenessBoxAuthoringBaker : Baker<VoxelAwarenessBoxAuthoring>
	{
		public override void Bake(VoxelAwarenessBoxAuthoring authoring)
		{
			var ent = GetEntity(authoring, TransformUsageFlags.Renderable);

			AddComponent(ent, new VoxelAwarenessBox() { Size = authoring.size });
		}
	}
}
