namespace VoxelBox.Editor.Settings
{
	using Rendering;
	using UnityEditor;
	using UnityEngine;
	using UnityEngine.UIElements;

	public sealed class VoxelDamageEffectSettingsProvider : AssetSettingsProvider
	{
		private string searchContext;
		private VisualElement rootElement;

		/// <summary>
		/// The current VoxelDamageEffectSettings used for this project.
		/// <para>
		/// This instance will be available in editor while playing and in any other player builds.
		/// To do so, this CurrentSettings needs to be an asset to be added to the Preloaded Assets
		/// list during the build process.
		/// </para>
		/// <para>See how this is done on <see cref="VoxelSettingsLoaderBuildPlayer"/>.</para>
		/// </summary>
		public static VoxelDamageEffectSettings CurrentSettings
		{
			get
			{
				EditorBuildSettings.TryGetConfigObject(VoxelDamageEffectSettings.ConfigName,
					out VoxelDamageEffectSettings settings);
				return settings;
			}
			set
			{
				bool remove = value == null;
				if (remove)
				{
					EditorBuildSettings.RemoveConfigObject(VoxelDamageEffectSettings.ConfigName);
				}
				else
				{
					EditorBuildSettings.AddConfigObject(VoxelDamageEffectSettings.ConfigName, value, overwrite: true);
				}
			}
		}

		public VoxelDamageEffectSettingsProvider()
			: base("Project/VoxelBox", () => CurrentSettings)
		{
			CurrentSettings = FindVoxelDamageEffectSettings();
			keywords = GetSearchKeywordsFromGUIContentProperties<VoxelDamageEffectSettings>();
		}

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			this.rootElement = rootElement;
			this.searchContext = searchContext;
			base.OnActivate(searchContext, rootElement);
		}

		public override void OnGUI(string searchContext)
		{
			DrawCurrentSettingsGUI();
			EditorGUILayout.Space();

			bool invalidSettings = CurrentSettings == null;
			if (invalidSettings) DisplaySettingsCreationGUI();
			else base.OnGUI(searchContext);
		}

		private void DrawCurrentSettingsGUI()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUI.indentLevel++;
			var settings = EditorGUILayout.ObjectField("Current Settings", CurrentSettings,
				typeof(VoxelDamageEffectSettings), allowSceneObjects: false) as VoxelDamageEffectSettings;
			if (settings) DrawCurrentSettingsMessage();
			EditorGUI.indentLevel--;

			bool newSettings = EditorGUI.EndChangeCheck();
			if (newSettings)
			{
				CurrentSettings = settings;
				RefreshEditor();
			}
		}

		private void RefreshEditor() => base.OnActivate(searchContext, rootElement);

		private void DisplaySettingsCreationGUI()
		{
			const string message = "You have no VoxelBox Effect Settings. Would you like to create one?";
			EditorGUILayout.HelpBox(message, MessageType.Info, wide: true);
			bool openCreationdialog = GUILayout.Button("Create");
			if (openCreationdialog)
			{
				CurrentSettings = SaveSceneLoaderAsset();
			}
		}

		private void DrawCurrentSettingsMessage()
		{
			const string message = "This is the current VoxelBox Effect Settings and " +
			                       "it will be automatically included into any builds.";
			EditorGUILayout.HelpBox(message, MessageType.Info, wide: true);
		}

		private static VoxelDamageEffectSettings FindVoxelDamageEffectSettings()
		{
			string filter = $"t:{typeof(VoxelDamageEffectSettings).Name}";
			string[] guids = AssetDatabase.FindAssets(filter);
			bool hasGuids = guids.Length > 0;
			string path = hasGuids ? AssetDatabase.GUIDToAssetPath(guids[0]) : string.Empty;

			return AssetDatabase.LoadAssetAtPath<VoxelDamageEffectSettings>(path);
		}

		private static VoxelDamageEffectSettings SaveSceneLoaderAsset()
		{
			string path = EditorUtility.SaveFilePanelInProject(
				title: "Save VoxelBox Effect Settings", defaultName: "DefaultVoxelDamageEffectSettings", extension: "asset",
				message: "Please enter a filename to save the projects VoxelBox settings.");
			bool invalidPath = string.IsNullOrEmpty(path);
			if (invalidPath) return null;

			var settings = ScriptableObject.CreateInstance<VoxelDamageEffectSettings>();
			AssetDatabase.CreateAsset(settings, path);
			AssetDatabase.SaveAssets();

			Selection.activeObject = settings;
			return settings;
		}

		[SettingsProvider]
		private static SettingsProvider CreateProjectSettingsMenu() => new VoxelDamageEffectSettingsProvider();
	}
}
