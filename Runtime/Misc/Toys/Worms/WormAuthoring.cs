namespace VoxelBox.Misc.Toys.Worms
{
	using UnityEngine;

	public class WormAuthoring : MonoBehaviour
	{
		public Bounds box;

		void OnDrawGizmosSelected() => Gizmos.DrawWireCube(box.center, box.size);
	}
}
