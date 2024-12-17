namespace VoxelBox.Editor.Baking
{
	using Authoring;
	using UnityEditor;
	using UnityEngine;
	using World.Streaming;
	using World.Streaming.SceneVolumes;

	[CustomEditor(typeof(VoxelVolumeAuthoring))]
	class VoxelVolumeInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (GUILayout.Button("EnsureAllLODGroupsHaveVoxelAuthoring"))
			{
				var tgt = (VoxelVolumeAuthoring)target;

				tgt.EnsureAllLODGroupsHaveVoxelAuthoring();
			}
		}
	}

	static class VoxelVolumeExt
	{
		internal static void EnsureHasReferenceToPrefab(this VoxelAuthoring vox)
		{
			string prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(vox.gameObject);
			var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);

			vox.referenceToPrefab = prefabAsset;
		}

		internal static void EnsureAllLODGroupsHaveVoxelAuthoring(this VoxelVolumeAuthoring vol)
		{
			var logGroups = vol.GetComponentsInChildren<LODGroup>();
			foreach (var lodGroup in logGroups)
			{
				if (!lodGroup.gameObject.TryGetComponent(out VoxelAuthoring va))
				{
					va = lodGroup.gameObject.AddComponent<VoxelAuthoring>();
					EditorUtility.SetDirty(lodGroup.gameObject);
				}
				va.EnsureHasReferenceToPrefab();
				EditorUtility.SetDirty(va);
			}
		}
	}
}
