namespace VoxelBox.Misc.Meshing.Atlas
{
	using Common;
	using Unity.Collections;
	using Unity.Jobs;
	using UnityEngine;
	using UnityEngine.Assertions;
	using UnityEngine.Rendering;

	struct PreprocessMeshJob<TVertexIn, TVertexOut> : IJobFor
		where TVertexIn : struct, ITransformVertex<TVertexOut> where TVertexOut : struct
	{
		[NativeDisableParallelForRestriction] [ReadOnly]
		public Mesh.MeshDataArray ReadableMeshDataArray;

		[NativeDisableParallelForRestriction] public Mesh.MeshDataArray WriteableMeshDataArray;

		[ReadOnly] public NativeList<int> Tasks;

		public PreprocessMeshJob(Mesh.MeshDataArray readable, Mesh.MeshDataArray writable, NativeList<int> tasks)
		{
			Tasks = tasks;
			ReadableMeshDataArray = readable;
			WriteableMeshDataArray = writable;
		}

		public void Execute(int index)
		{
			var task = Tasks[index];

			var readable = ReadableMeshDataArray[task];
			var writeable = WriteableMeshDataArray[task];

			var inputVertices = readable.GetVertexData<TVertexIn>();
			var outputVertices = writeable.GetVertexData<TVertexOut>();

			if (inputVertices.Length != outputVertices.Length)
			{
				throw new AssertionException("inputVertices.Length != outputVertices.Length", "");
			}

			for (int i = 0; i < inputVertices.Length; i++)
			{
				var vtx = inputVertices[i];
				outputVertices[i] = vtx.Transform();
			}

			var outputIndices = writeable.GetIndexData<uint>();
			if (readable.indexFormat == IndexFormat.UInt16)
			{
				var meshIndices = readable.GetIndexData<ushort>();
				for (int i = 0; i < meshIndices.Length; i++)
					outputIndices[i] = meshIndices[i];
			}
			else
			{
				var meshIndices = readable.GetIndexData<uint>();
				for (int i = 0; i < meshIndices.Length; i++)
					outputIndices[i] = meshIndices[i];
			}
		}
	}
}
