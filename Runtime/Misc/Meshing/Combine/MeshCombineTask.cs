namespace VoxelBox.Misc.Meshing.Combine
{
	using Common;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Jobs;
	using Unity.Mathematics;
	using UnityEngine;
	using UnityEngine.Rendering;

	public struct MeshCombineSubTask
	{
		public uint StartingIndex;
		public uint StartingVertex;

		public uint VertexCount;
		public uint IndexCount;

		public float4x4 Transform;
		public Bounds Bounds;

		public int ReadableMeshIndex;
	}

	public partial class MeshCombineTask
	{
		public JobHandle Schedule(Mesh.MeshDataArray readableMeshDataArray,
			Mesh.MeshDataArray writeableMeshDataArray,
			NativeArray<MeshCombineSubTask> subTasks, uint totalVertices, uint totalIndices,
			JobHandle dependency = default)
		{
			WriteableMeshDataArray = writeableMeshDataArray;
			ReadableMeshDataArray = readableMeshDataArray;

			var om = WriteableMeshDataArray[0];
			om.SetVertexBufferParams((int)totalVertices, VertexStream1UV.Attributes);
			om.SetIndexBufferParams((int)totalIndices, IndexFormat.UInt32);

			var job = new MeshCombineJob<VertexStream1UV>(
				ReadableMeshDataArray,
				om,
				subTasks
			);

			const int innerloopBatchCount = 4;

			dependency = job.ScheduleParallel(subTasks.Length, innerloopBatchCount, dependency);
			dependency = new PostProcessMeshJob(om, totalIndices).Schedule(dependency);
			return dependency;
		}
	}

	public partial class MeshCombineTask
	{
		public Bounds Bounds;
		public Mesh.MeshDataArray ReadableMeshDataArray;
		public Mesh.MeshDataArray WriteableMeshDataArray;

		public Mesh ApplyAndDispose()
		{
			var mesh = new Mesh { name = "Combined Mesh", bounds = Bounds, };

			Mesh.ApplyAndDisposeWritableMeshData(WriteableMeshDataArray, mesh);
			ReadableMeshDataArray.Dispose();

			return mesh;
		}
	}

	[BurstCompile]
	struct PostProcessMeshJob : IJob
	{
		[WriteOnly] Mesh.MeshData outputMeshData;
		readonly uint totalIndices;

		public PostProcessMeshJob(Mesh.MeshData outputMeshData, uint totalIndices)
		{
			this.outputMeshData = outputMeshData;
			this.totalIndices = totalIndices;
		}

		public void Execute()
		{
			var sm = new SubMeshDescriptor(0, (int)totalIndices);
			outputMeshData.subMeshCount = 1;
			outputMeshData.SetSubMesh(0, sm);
		}
	}

	[BurstCompile]
	struct PostProcessMeshArrayJob : IJob
	{
		Mesh.MeshDataArray outputMeshData;
		readonly int meshIndex;
		readonly uint totalIndices;

		public PostProcessMeshArrayJob(Mesh.MeshDataArray outputMeshData, uint totalIndices, int meshIndex = 0)
		{
			this.outputMeshData = outputMeshData;
			this.totalIndices = totalIndices;
			this.meshIndex = meshIndex;
		}

		public void Execute()
		{
			var om = outputMeshData[meshIndex];
			var sm = new SubMeshDescriptor(0, (int)totalIndices);
			om.subMeshCount = 1;
			om.SetSubMesh(0, sm);
		}
	}

	[BurstCompile]
	struct MeshCombineJob<TVertex> : IJobFor where TVertex : struct, IVertexTransform
	{
		[ReadOnly] Mesh.MeshDataArray readableMeshData;

		Mesh.MeshData outputMesh;
		[DeallocateOnJobCompletion] [ReadOnly] NativeArray<MeshCombineSubTask> subTasks;

		public MeshCombineJob(Mesh.MeshDataArray readableMeshData, Mesh.MeshData outputMesh,
			NativeArray<MeshCombineSubTask> subTasks)
		{
			this.readableMeshData = readableMeshData;
			this.outputMesh = outputMesh;
			this.subTasks = subTasks;
		}

		public void Execute(int index)
		{
			var task = subTasks[index];
			var meshData = readableMeshData[task.ReadableMeshIndex];

			var outputVertices = outputMesh.GetVertexData<TVertex>();
			var meshVertices = meshData.GetVertexData<TVertex>();

			for (uint i = 0; i < task.VertexCount; i++)
			{
				var vtx = meshVertices[(int)i];
				vtx.Transform(task.Transform);
				outputVertices[(int)(i + task.StartingVertex)] = vtx;
			}

			var outputIndices = outputMesh.GetIndexData<uint>();
			if (meshData.indexFormat == IndexFormat.UInt16)
			{
				var meshIndices = meshData.GetIndexData<ushort>();
				for (uint i = 0; i < task.IndexCount; i++)
					outputIndices[(int)(i + task.StartingIndex)] = task.StartingVertex + meshIndices[(int)i];
			}
			else
			{
				var meshIndices = meshData.GetIndexData<uint>();
				for (uint i = 0; i < task.IndexCount; i++)
					outputIndices[(int)(i + task.StartingIndex)] = task.StartingVertex + meshIndices[(int)i];
			}
		}
	}
}
