namespace VoxelBox.Editor.Baking
{
	using System.Collections.Generic;
	using System.Linq;
	using Unity.Entities;
	using Unity.Mathematics;
	using UnityEditor;
	using UnityEngine;
	using Authoring;
	using Misc.Common;
	using World.Chunk;
	using World.Chunk.Components;
	using World.Procedural;
	using World.Streaming;
	using World.Streaming.SceneVolumes;

	class VoxelVolumeBaker : Baker<VoxelVolumeAuthoring>
	{
		class ChunkBuild
		{
			public Entity ChunkEntity;
			public DynamicBuffer<Voxel> VoxelBuffer;
		}

		public override void Bake(VoxelVolumeAuthoring authoring)
		{
			DependsOnComponentsInChildren<Transform>();
			DependsOnComponentsInChildren<MeshFilter>();
			DependsOnComponentsInChildren<VoxelAuthoring>();

			var logGroups = GetComponentsInChildren<LODGroup>();
			var tmpChunks = new Dictionary<int3, ChunkBuild>();

			foreach (var lodGroup in logGroups)
			{
				DependsOn(lodGroup);
				var voxelEntity = GetEntity(lodGroup.gameObject, TransformUsageFlags.Renderable);
				var voxelAuthoring = lodGroup.GetComponent<VoxelAuthoring>();
				voxelAuthoring.EnsureHasReferenceToPrefab();

				var prefabAsset = voxelAuthoring.referenceToPrefab;
				if (!prefabAsset)
				{
					Debug.Log(prefabAsset, lodGroup.gameObject);
					continue;
				}

				RegisterPrefabForBaking(prefabAsset);
				var prefabEntity = GetEntity(prefabAsset, TransformUsageFlags.None);

				var voxelBounds = voxelAuthoring.LocalBounds().Translate(lodGroup.transform.position);

				var chunkCoord = ChunkUtility.GetChunkCoord(voxelBounds.center);

				if (!tmpChunks.TryGetValue(chunkCoord, out var chunk))
				{
					var chunkEntity = CreateAdditionalEntity(TransformUsageFlags.None, false, $"VC[{chunkCoord}]");
					var voxelBuffer = AddBuffer<Voxel>(chunkEntity);

					chunk = new ChunkBuild
					{
						ChunkEntity = chunkEntity, //
						VoxelBuffer = voxelBuffer,
					};

					tmpChunks.Add(chunkCoord, chunk);
				}

				var voxel = new Voxel
				{
					Bounds = voxelBounds, //
					Entity = voxelEntity,
					protoId = voxelAuthoring.GetPrototypeID(),
					PrefabEntity = prefabEntity,
				};
				chunk.VoxelBuffer.Add(voxel);
			}

			foreach (var (coords, chunkBuild) in tmpChunks)
			{
				AddComponent(chunkBuild.ChunkEntity, new VoxelVolumeChunkCoord
				{
					Coord = coords, //
				});
			}
		}
	}
}
