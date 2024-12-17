namespace VoxelBox.Editor.Baking
{
	using System;
	using Unity.Entities;
	using Unity.Mathematics;
	using Misc.Toys.Worms;
	using Random = Unity.Mathematics.Random;

	public class WormAuthoringBaker : Baker<WormAuthoring>
	{
		public override void Bake(WormAuthoring authoring)
		{
			var ent = GetEntity(authoring, TransformUsageFlags.Dynamic);

			AddComponent(ent,
				new Worm
				{
					Direction = math.left(),
					Random = Random.CreateFromIndex((uint)(authoring.GetInstanceID() + DateTime.Now.Ticks)),
					Box = authoring.box,
				});
		}
	}
}
