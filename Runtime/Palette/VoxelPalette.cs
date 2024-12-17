namespace VoxelBox.Palette
{
	using System.Collections.Generic;
	using Authoring;
	using UnityEngine;

	[CreateAssetMenu(menuName = "VoxelBox 🧃/Voxel Palette 🎨")]
	public class VoxelPalette : ScriptableObject
	{
		public GameObject[] voxelPrefabs;

		void OnValidate()
		{
			var prefs = new List<GameObject>(voxelPrefabs);

			var hs = new HashSet<string>();
			foreach (var voxelPrefab in voxelPrefabs)
			{
				var name = voxelPrefab.name;

				if (!voxelPrefab.TryGetComponent(out VoxelAuthoring auth))
				{
					Debug.LogWarning($"no voxel authoring on {name}", voxelPrefab);
					prefs.Remove(voxelPrefab);
				}
				else if (!auth.Validate())
				{
					Debug.LogWarning($"no valid voxel authoring on {name}", voxelPrefab);
					prefs.Remove(voxelPrefab);
				}

				if (!hs.Add(name))
				{
					Debug.LogWarning($"duplicate name in voxel palette {name}", voxelPrefab);
					prefs.Remove(voxelPrefab);
				}
			}

			voxelPrefabs = prefs.ToArray();
		}
	}
}
