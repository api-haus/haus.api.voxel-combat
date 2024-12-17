namespace VoxelBox.Misc.Meshing.Common
{
	using System.Runtime.CompilerServices;
	using Unity.Burst;
	using Unity.Mathematics;
	using UnityEngine.Rendering;

	[BurstCompile]
	public struct VertexStream2UV : IVertexTransform, ITransformVertex<VertexStream1UV>
	{
		public float3 Position;
		public float3 Normal;
		public float4 Tangent;
		public float2 Texcoord0;
		public float2 Texcoord1;

		public static readonly VertexAttributeDescriptor[] Attributes =
		{
			new(VertexAttribute.Position),
			new(VertexAttribute.Normal),
			new(VertexAttribute.Tangent, VertexAttributeFormat.Float32, 4),
			new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
			new(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2),
		};

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Transform(float4x4 trs)
		{
			Position = math.transform(trs, Position);
			Normal = math.rotate(trs, Normal);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public VertexStream1UV Transform() =>
			new()
			{
				Position = Position, //
				Normal = Normal,
				Tangent = Tangent,
				Texcoord0 = Texcoord0,
			};
	}
}
