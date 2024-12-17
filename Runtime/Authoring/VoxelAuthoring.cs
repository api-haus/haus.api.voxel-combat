namespace VoxelBox.Authoring
{
	using System;
	using System.Linq;
	using Misc.Common;
	using Palette;
	using Unity.Entities.Hybrid.Baking;
	using UnityEngine;

	[DisallowMultipleComponent]
	[RequireComponent(typeof(LinkedEntityGroupAuthoring))]
	public class VoxelAuthoring : MonoBehaviour
	{
		public VoxelFlags flags = VoxelFlags.Default;

		public VoxelData data = VoxelData.Default();

		public VoxelPrototypeId GetPrototypeID() => gameObject.name;

		[SerializeField] [HideInInspector] public GameObject referenceToPrefab;

		public Bounds LocalBounds()
		{
			if (TryGetComponent(out LODGroup lg))
			{
				var ren = lg.GetLODs().First().renderers.First();
				return ren.localBounds.Scale(ren.transform);
			}

			if (TryGetComponent(out MeshRenderer mf))
			{
				return mf.localBounds.Scale(transform);
			}

			return default;
		}

		public void OnDrawGizmosSelected()
		{
			var b = LocalBounds().Translate(transform.position);
			Gizmos.DrawWireCube(b.center, b.size);
		}

		public bool Validate() => LocalBounds() != default;
	}
}
