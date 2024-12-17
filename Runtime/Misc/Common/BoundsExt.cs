namespace VoxelBox.Misc.Common
{
	using Unity.Mathematics;
	using UnityEngine;

	public static class BoundsExt
	{
		public static Bounds Translate(this Bounds b, float3 pos)
		{
			b.center += (Vector3)pos;

			return b;
		}

		public static Bounds Scale(this Bounds b, Transform tr)
		{
			var min = b.min;
			var max = b.max;

			min = tr.TransformPoint(min);
			max = tr.TransformPoint(max);

			b = new Bounds { min = min, max = max };
			b.center -= tr.position;

			return b;
		}
	}
}
