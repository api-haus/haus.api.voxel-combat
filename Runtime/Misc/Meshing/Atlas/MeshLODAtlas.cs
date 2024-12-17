namespace VoxelBox.Misc.Meshing.Atlas
{
	using System;
	using System.Runtime.CompilerServices;
	using Combine;
	using Common;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;
	using UnityEngine;
	using UnityEngine.Rendering;

	public struct MeshLODAtlas<TVertex> : IDisposable where TVertex : struct, IVertexTransform
	{
		[Serializable]
		public struct MeshAtlasEntry
		{
			public int instanceID;
			public int instanceLOD;
			public uint startingIndex;
			public uint startingVertex;
			public uint indexCount;
			public uint vertexCount;
			public Bounds localBounds;
		}

		public NativeArray<TVertex> Vertices;
		public NativeArray<uint> Indices;
		public NativeParallelHashMap<int2, MeshAtlasEntry> Entries;

		public Mesh.MeshDataArray WritableMeshData;
		public Mesh.MeshDataArray ReadableMeshData;

		public JobHandle AllocateAndBuild(LODMeshEntry[] lodMeshEntries, JobHandle dependency = default)
		{
			var mct = new MeshCombineTask();

			var meshes = lodMeshEntries.ToMeshArray();
			var subTasks = new NativeArray<MeshCombineSubTask>(lodMeshEntries.Length, Allocator.TempJob);

			Entries = new(lodMeshEntries.Length, Allocator.Persistent);

			uint totalVertices = 0;
			uint totalIndices = 0;

			ReadableMeshData = Mesh.AcquireReadOnlyMeshData(meshes);
			for (int i = 0; i < lodMeshEntries.Length; i++)
			{
				var entry = lodMeshEntries[i];
				var meshData = ReadableMeshData[i];

				uint vertexCount = (uint)meshData.vertexCount;
				uint indexCount = (uint)meshData.GetSubMesh(0).indexCount;

				subTasks[i] = new MeshCombineSubTask
				{
					StartingIndex = totalIndices,
					StartingVertex = totalVertices,
					VertexCount = vertexCount,
					IndexCount = indexCount,
					Transform = float4x4.identity,
					Bounds = entry.mesh.bounds,
					ReadableMeshIndex = i,
				};

				Entries.Add(new int2(entry.instanceId, entry.lod), new MeshAtlasEntry
				{
					instanceID = entry.instanceId,
					instanceLOD = entry.lod,
					startingIndex = totalIndices,
					startingVertex = totalVertices,
					indexCount = indexCount,
					vertexCount = vertexCount,
					localBounds = entry.mesh.bounds,
				});

				totalVertices += vertexCount;
				totalIndices += indexCount;

				mct.Bounds.Encapsulate(meshes[i].bounds);
			}

			WritableMeshData = Mesh.AllocateWritableMeshData(1);

			var om = WritableMeshData[0];
			om.SetVertexBufferParams((int)totalVertices, VertexStream1UV.Attributes);
			om.SetIndexBufferParams((int)totalIndices, IndexFormat.UInt32);

			const int innerloopBatchCount = 4;
			var mj = new MeshCombineJob<TVertex>(ReadableMeshData, om, subTasks);
			dependency = mj.ScheduleParallel(ReadableMeshData.Length, innerloopBatchCount, dependency);
			dependency = new PostProcessMeshJob(om, totalIndices).Schedule(dependency);
			// dependency =
			// new ConstructMeshLODAtlasJob(om,)
			// .Schedule(dependency);

			return dependency;
		}

		public (NativeSlice<TVertex>, NativeSlice<uint>) GetLODMesh(int instanceId, int lod)
		{
			var meshAtlasKey = new int2(instanceId, lod);
			if (Entries.TryGetValue(meshAtlasKey, out var entry))
			{
				var vertices = Vertices.Slice((int)entry.startingVertex, (int)entry.vertexCount);
				var indices = Indices.Slice((int)entry.startingIndex, (int)entry.indexCount);
				return (vertices, indices);
			}

			throw new ArgumentOutOfRangeException(nameof(meshAtlasKey), "Mesh does not exist for InstanceID + LOD");
		}

		public Mesh ApplyAndDispose()
		{
			var mesh = new Mesh { name = "Combined Mesh", bounds = new Bounds(Vector3.zero, Vector3.one), };

			Mesh.ApplyAndDisposeWritableMeshData(WritableMeshData, mesh);
			// ReadableMeshData.Dispose();

			return mesh;
		}

		public void Dispose()
		{
			// ReadableMeshData.Dispose();
			WritableMeshData.Dispose();
			Entries.Dispose();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool TryGetEntry(int instanceId, int lod, out MeshAtlasEntry meshAtlasEntry) =>
			Entries.TryGetValue(new int2(instanceId, lod), out meshAtlasEntry) ||
			Entries.TryGetValue(new int2(instanceId, lod + 1), out meshAtlasEntry) ||
			Entries.TryGetValue(new int2(instanceId, 0), out meshAtlasEntry);
	}
}
