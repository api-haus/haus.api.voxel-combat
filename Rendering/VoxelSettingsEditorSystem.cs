#if UNITY_EDITOR
namespace VoxelBox.Rendering
{
	using UnityEditor;

	public static class VoxelSettingsEditorSystem
	{
		[InitializeOnLoadMethod]
		private static void Initialize()
		{
			// EditorApplication.playModeStateChanged -= EditorApplicationOnplayModeStateChanged;
			// EditorApplication.playModeStateChanged += EditorApplicationOnplayModeStateChanged;
			VoxelSettingsSystem.Load();
		}

		// static void EditorApplicationOnplayModeStateChanged(PlayModeStateChange obj)
		// {
		// 	if (obj == PlayModeStateChange.EnteredEditMode)
		// 	{
		// 		VoxelSettingsSystem.Load();
		// 	}
		// }
	}
}
#endif
