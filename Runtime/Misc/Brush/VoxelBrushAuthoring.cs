namespace VoxelBox.Misc.Brush
{
	using UnityEngine;

	public class VoxelBrushAuthoring : MonoBehaviour
	{
		public float radius = 10;

		void OnDrawGizmos()
		{
			Gizmos.color = new Color(1, 1, 1, .5f);
			Gizmos.DrawSphere(transform.position, radius * transform.localScale.x);
		}
	}
}
