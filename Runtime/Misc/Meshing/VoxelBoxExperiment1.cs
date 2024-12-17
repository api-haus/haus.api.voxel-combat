namespace VoxelBox.Misc.Meshing
{
	using System;
	using System.Diagnostics;
	using Atlas;
	using Combine;
	using Common;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;
	using UnityEngine;
	using Debug = UnityEngine.Debug;

	[BurstCompile]
	public struct PrepareMeshingJob : IJob
	{
		NativeArray<(int, float4x4)> instancePlacements;
		int lod;

		MeshLODAtlas<VertexStream1UV> atlas;
		NativeList<MeshCombineSubTask> tasks;

		public void Execute()
		{
			foreach (var (instanceId, transform) in instancePlacements)
			{
				if (atlas.TryGetEntry(instanceId, lod, out var entry))
				{
					tasks.Add(new MeshCombineSubTask
					{
						StartingVertex = entry.startingVertex,
						StartingIndex = entry.startingIndex,
						VertexCount = entry.vertexCount,
						IndexCount = entry.indexCount,
						Transform = transform,
						Bounds = entry.localBounds,
						// ReadableMeshIndex = 0,
					});
				}
			}
		}
	}

	public class VoxelBoxExperiment1 : MonoBehaviour, IDisposable
	{
		public MeshAtlasDefinition meshAtlasDef;
		public Transform compositionRoot;

		MeshLODAtlas<VertexStream1UV> atlas;

		async void OnEnable()
		{
			var sw = Stopwatch.StartNew();

			atlas = await meshAtlasDef.Build();

			long dur = sw.ElapsedMilliseconds;
			Debug.Log(
				$"Took {dur / 1000.0:F2}sec for {atlas.Entries.Count()} objects, total {atlas.Vertices.Length} verts");
		}

		void OnDisable() => Dispose();

		public void Dispose()
		{
			atlas.Dispose();
		}
	}
}
