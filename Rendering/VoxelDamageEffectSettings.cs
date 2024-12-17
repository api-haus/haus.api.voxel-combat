namespace VoxelBox.Rendering
{
	using System;
	using UnityEngine;

	public class VoxelDamageEffectSettings : ScriptableObject
	{
		public const string ConfigName = "land.pala.voxelbox.settings";

		private static VoxelDamageEffectSettings instance;

		private void OnEnable()
		{
			if (instance == null) instance = this;
		}

		public static VoxelDamageEffectSettings Load()
		{
			if (instance) return instance;
#if UNITY_EDITOR
			if (!UnityEditor.EditorBuildSettings.TryGetConfigObject(ConfigName, out instance))
			{
				const string path = "Assets/VoxelBoxSettings.asset";
				var settings = CreateInstance<VoxelDamageEffectSettings>();
				UnityEditor.AssetDatabase.CreateAsset(settings, path);
				UnityEditor.AssetDatabase.SaveAssets();
				UnityEditor.EditorBuildSettings.AddConfigObject(ConfigName, settings, overwrite: true);
			}
#else
        // Loads from the memory.
        instance = FindObjectOfType<VoxelDamageEffectSettings>();
#endif
			instance.IsDirty = true;
			return instance;
		}

		public VoxelDamageEffectCBuffer settings = VoxelDamageEffectCBuffer.Default();
		public Texture2D voxelNoise;
		public Texture2D packedCracksMap;

		[field: NonSerialized] public bool IsDirty { get; private set; }

		void OnValidate() => IsDirty = true;

		public bool MustUpdate()
		{
			if (IsDirty)
			{
				IsDirty = false;
				return true;
			}

			return false;
		}

		public void MakeDirty() => IsDirty = true;
	}
}
