namespace VoxelBox.Editor.Settings
{
	using System.Linq;
	using UnityEditor;
	using UnityEditor.Build;
	using UnityEditor.Build.Reporting;

	public sealed class VoxelSettingsLoaderBuildPlayer : IPreprocessBuildWithReport
	{
		public int callbackOrder => 0;

		public void OnPreprocessBuild(BuildReport report)
		{
			var settings = VoxelDamageEffectSettingsProvider.CurrentSettings;
			var settingsType = settings.GetType();
			var preloadedAssets = PlayerSettings.GetPreloadedAssets().ToList();
			// Removes all references from VoxelDamageEffectSettings.
			preloadedAssets.RemoveAll(st => st.GetType() == settingsType);
			// Adds the Current VoxelDamageEffectSettings.
			preloadedAssets.Add(settings);

			PlayerSettings.SetPreloadedAssets(preloadedAssets.ToArray());
		}
	}
}
