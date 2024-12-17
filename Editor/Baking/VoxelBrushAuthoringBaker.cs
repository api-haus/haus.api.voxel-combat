namespace VoxelBox.Editor.Baking
{
	using Unity.Entities;
	using Misc.Brush;

	class VoxelBrushAuthoringBaker : Baker<VoxelBrushAuthoring>
	{
		public override void Bake(VoxelBrushAuthoring authoring)
		{
			// DependsOn(authoring.transform);
			//
			// var ent = GetEntity(authoring, TransformUsageFlags.Renderable);
			//
			// var c = new DamageVoxelsInSphereZone
			// {
			// 	Sphere = new Sphere(authoring.transform.position, authoring.radius * authoring.transform.localScale.x)
			// };
			//
			// AddComponent(ent, c);
		}
	}
}
