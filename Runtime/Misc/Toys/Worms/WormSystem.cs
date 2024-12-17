namespace VoxelBox.Misc.Toys.Worms
{
	using Unity.Entities;
	using Unity.Mathematics;
	using Unity.Transforms;
	using World.Interaction.DamageZones;
	using World.Interaction.DamageZones.Geometry;

	partial class WormSystem : SystemBase
	{
		EndSimulationEntityCommandBufferSystem Barrier =>
			World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();

		EntityArchetype destroyVoxelCommandArch;

		protected override void OnCreate() =>
			destroyVoxelCommandArch =
				EntityManager.CreateArchetype(
					typeof(DamageVoxelsInSphereZone)
				);

		protected override void OnUpdate()
		{
			float deltaTime = SystemAPI.Time.DeltaTime;
			float elapsedTime = (float)SystemAPI.Time.ElapsedTime;

			var ecb = Barrier.CreateCommandBuffer();

			foreach (var (_worm, _lt) in SystemAPI
				         .Query<RefRW<Worm>, RefRW<LocalTransform>>())
			{
				ref var ltt = ref _lt.ValueRW;
				ref var worm = ref _worm.ValueRW;

				ltt.Position += worm.Direction * worm.Speed * deltaTime;

				bool isOut = !worm.Box.Contains(ltt.Position);
				var n = math.normalize(worm.Box.ClosestPoint(ltt.Position) - ltt.Position);

				if (isOut || (worm.TimeToSwitch -= deltaTime) <= 0)
				{
					worm.Direction = isOut ? math.reflect(worm.Direction, n) : worm.Random.NextFloat3Direction();
					worm.TimeToSwitch = worm.Random.NextFloat(3);
					worm.Speed = worm.Random.NextFloat(5, 6);
				}

				var ent = ecb.CreateEntity(destroyVoxelCommandArch);
				ecb.SetComponent(ent,
					new DamageVoxelsInSphereZone(int.MaxValue / 50, elapsedTime,
						new Sphere(ltt.Position, ltt.Scale * .5f)));
			}
		}
	}
}
