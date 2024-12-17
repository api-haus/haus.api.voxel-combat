namespace VoxelBox.Misc.Meshing.Atlas
{
	using Common;
	using Unity.Collections;
	using Unity.Mathematics;
	using UnityEngine;

	public static class LODMeshExt
	{
		public static Mesh[] ToMeshArray(this LODMeshEntry[] entries)
		{
			var meshes = new Mesh[entries.Length];

			for (int i = 0; i < entries.Length; i++)
				meshes[i] = entries[i].mesh;

			return meshes;
		}

		public static NativeParallelHashMap<int2, MeshLODAtlas<TVertex>.MeshAtlasEntry> ToMeshAtlasEntries<TVertex>(
			this LODMeshEntry[] lodMeshEntries, Allocator alloc) where TVertex : struct, IVertexTransform
		{
			NativeParallelHashMap<int2, MeshLODAtlas<TVertex>.MeshAtlasEntry> Entries
				= new(lodMeshEntries.Length, alloc);

			uint totalVertices = 0;
			uint totalIndices = 0;

			foreach (var entry in lodMeshEntries)
			{
				uint indexCount = entry.mesh.GetIndexCount(0);
				uint vertexCount = (uint)entry.mesh.vertexCount;

				Entries.Add(new int2(entry.instanceId, entry.lod), new MeshLODAtlas<TVertex>.MeshAtlasEntry
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
			}

			return Entries;
		}
	}
}
