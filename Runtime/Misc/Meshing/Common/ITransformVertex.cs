namespace VoxelBox.Misc.Meshing.Common
{
	public interface ITransformVertex<out TVertexOut> where TVertexOut : struct
	{
		TVertexOut Transform();
	}
}
