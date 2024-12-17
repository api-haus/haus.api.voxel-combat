namespace VoxelBox.Misc.Meshing.Atlas
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading.Tasks;
	using Combine;
	using Common;
	using Unity.Collections;
	using Unity.Jobs;
	using UnityEngine;
	using UnityEngine.Rendering;

	[Serializable]
	public struct LODMeshEntry
	{
		public int instanceId;
		public int lod;
		public Mesh mesh;
	}

	[CreateAssetMenu(fileName = "VoxelBox 🧃/Mesh Atlas 🌐", menuName = "Mesh Atlas")]
	public class MeshAtlasDefinition : ScriptableObject
	{
		public GameObject[] sourcePrefabs;

		public async Task<MeshLODAtlas<VertexStream1UV>> Build()
		{
			var entries = GetEntries();

			entries = await PreprocessEntries(entries);

			var mla = new MeshLODAtlas<VertexStream1UV>();

			var d = mla.AllocateAndBuild(entries);

			while (!d.IsCompleted)
				await Task.Yield();
			d.Complete();

			mla.Indices = mla.WritableMeshData[0].GetIndexData<uint>();
			mla.Vertices = mla.WritableMeshData[0].GetVertexData<VertexStream1UV>();

			// TODO: move out?
			mla.ReadableMeshData.Dispose();

			return mla;
		}

		public async Task<LODMeshEntry[]> PreprocessEntries(LODMeshEntry[] entries)
		{
			var meshes = entries.ToMeshArray();

			var readable = Mesh.AcquireReadOnlyMeshData(meshes);
			var writable = Mesh.AllocateWritableMeshData(meshes.Length);

			using var uv1Tasks = new NativeList<int>(0, Allocator.TempJob);
			using var uv2Tasks = new NativeList<int>(0, Allocator.TempJob);
			using var uv3Tasks = new NativeList<int>(0, Allocator.TempJob);

			for (int i = 0; i < meshes.Length; i++)
			{
				var md = readable[i];
				bool uv0 = md.HasVertexAttribute(VertexAttribute.TexCoord0);
				bool uv1 = md.HasVertexAttribute(VertexAttribute.TexCoord1);
				bool uv2 = md.HasVertexAttribute(VertexAttribute.TexCoord2);

				int vertexCount = md.vertexCount;
				int indexCount = md.GetSubMesh(0).indexCount;

				var om = writable[i];
				om.SetVertexBufferParams(vertexCount, VertexStream1UV.Attributes);
				om.SetIndexBufferParams(indexCount, IndexFormat.UInt32);

				if (uv2 && uv1 && uv0)
				{
					uv3Tasks.Add(i);
					continue;
				}

				if (uv1 && uv0)
				{
					uv2Tasks.Add(i);
					continue;
				}

				if (uv0)
				{
					uv1Tasks.Add(i);
					continue;
				}

				throw new InvalidDataException("unsupported vertex attributes");
			}

			JobHandle dep = default;
			if (uv3Tasks.Length > 0)
				dep = new PreprocessMeshJob<VertexStream3UV, VertexStream1UV>(readable, writable, uv3Tasks)
					.Schedule(uv3Tasks.Length, dep);

			while (!dep.IsCompleted)
				await Task.Yield();
			dep.Complete();

			if (uv2Tasks.Length > 0)
				dep = new PreprocessMeshJob<VertexStream2UV, VertexStream1UV>(readable, writable, uv2Tasks)
					.Schedule(uv2Tasks.Length, dep);

			while (!dep.IsCompleted)
				await Task.Yield();
			dep.Complete();

			if (uv1Tasks.Length > 0)
				dep = new PreprocessMeshJob<VertexStream1UV, VertexStream1UV>(readable, writable, uv1Tasks)
					.Schedule(uv1Tasks.Length, dep);

			while (!dep.IsCompleted)
				await Task.Yield();
			dep.Complete();

			for (int i = 0; i < meshes.Length; i++)
			{
				dep = new PostProcessMeshArrayJob(writable, (uint)readable[i].GetSubMesh(0).indexCount, i).Schedule(dep);
			}

			while (!dep.IsCompleted)
				await Task.Yield();
			dep.Complete();

			for (int i = 0; i < meshes.Length; i++)
			{
				meshes[i] = new Mesh
				{
					name = $"{meshes[i].name} (preprocessed)", //
					bounds = meshes[i].bounds,
				};
			}

			Mesh.ApplyAndDisposeWritableMeshData(writable, meshes);
			readable.Dispose();

			var newEntries = new LODMeshEntry[entries.Length];

			for (int i = 0; i < newEntries.Length; i++)
			{
				var e = entries[i];

				e.mesh = meshes[i];

				newEntries[i] = e;
			}

			return newEntries;
		}

		public LODMeshEntry[] GetEntries()
		{
			var lodMeshes = new List<LODMeshEntry>();

			foreach (var sourcePrefab in sourcePrefabs)
			{
				var lodGroup = sourcePrefab.GetComponentInChildren<LODGroup>();
				if (lodGroup)
				{
					var lods = lodGroup.GetLODs();
					for (int lodIndex = 0; lodIndex < lods.Length; lodIndex++)
					{
						var lod = lods[lodIndex];
						foreach (var lodRenderer in lod.renderers)
						{
							AddFromMeshFilter(lodRenderer.gameObject, lodIndex);
						}
					}

					continue;
				}

				AddFromMeshFilter(sourcePrefab);
			}

			return lodMeshes.ToArray();

			bool AddFromMeshFilter(GameObject sourceObject, int lod = 0)
			{
				if (sourceObject.TryGetComponent(out MeshFilter mf))
				{
					lodMeshes.Add(new LODMeshEntry
					{
						instanceId = sourceObject.GetInstanceID(), //
						lod = lod,
						mesh = mf.sharedMesh,
					});
					return true;
				}

				return false;
			}
		}
	}
}
