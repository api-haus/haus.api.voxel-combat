namespace VoxelBox.World.Chunk
{
	using Authoring;
	using NativeTrees;
	using Unity.Collections;

	public static class NativeTreeExt
	{
		public static NativeOctree<Voxel> Clone(this NativeOctree<Voxel> tree, Allocator allocator)
		{
			var tClone =
				new NativeOctree<Voxel>(tree.Bounds, tree.ObjectsPerNode, tree.MaxDepth, allocator, tree.Capacity);

			tClone.CopyFrom(tree);

			return tClone;
		}
	}
}
