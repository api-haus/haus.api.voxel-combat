namespace VoxelBox.Misc.Meshing.Common
{
	using Unity.Mathematics;

	public interface IVertexTransform
	{
		void Transform(float4x4 trs);
	}
}
