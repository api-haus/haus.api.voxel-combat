namespace VoxelBox.Rendering
{
	using System.Collections.Generic;
	using UnityEngine;
	using UnityEngine.Rendering;
	using static Unity.Collections.LowLevel.Unsafe.UnsafeUtility;
	using static UnityEngine.GraphicsBuffer;

	public static class VoxelSettingsSystem
	{
		static GraphicsBuffer cbuffer;
		static VoxelDamageEffectSettings settings;

		[RuntimeInitializeOnLoadMethod]
		private static void Initialize() => Load();

		static void AllocateIfNeeded()
		{
			if (cbuffer == null || !cbuffer.IsValid())
				cbuffer = new(Target.Constant, 1, SizeOf<VoxelDamageEffectCBuffer>());
		}

		public static void Load()
		{
			Destroy();

			settings = VoxelDamageEffectSettings.Load();
			settings.MakeDirty();

// #if !UNITY_EDITOR
			Application.quitting -= Destroy;
			Application.quitting += Destroy;
// #endif
			RenderPipelineManager.beginContextRendering -= BeginContextRendering;
			RenderPipelineManager.beginContextRendering += BeginContextRendering;
		}

		static void Destroy()
		{
			Shader.SetGlobalConstantBuffer(ShaderPropertyID.VoxelDamageBuffer, (GraphicsBuffer)null, 0,
				SizeOf<VoxelDamageEffectCBuffer>());
			cbuffer?.Release();
			cbuffer = null;
		}

		static void BeginContextRendering(ScriptableRenderContext ctx, List<Camera> cameras)
		{
			AllocateIfNeeded();

			var cmd = CommandBufferPool.Get();

#if UNITY_EDITOR
			if (!Application.isPlaying || settings.MustUpdate())
#else
			if (settings.MustUpdate())
#endif
			{
				var set = settings.settings;

				var vd = set.voxelData;
				vd.x *= Mathf.Deg2Rad;
				set.voxelData = vd;

				set.cracksCenterTiling = set.cracksCenter / set.cracksData.y;

				cmd.SetBufferData(cbuffer, new[] { set });
			}

			cmd.SetGlobalConstantBuffer(cbuffer, ShaderPropertyID.VoxelDamageBuffer, 0,
				SizeOf<VoxelDamageEffectCBuffer>());
			cmd.SetGlobalTexture(ShaderPropertyID.VoxelNoise, settings.voxelNoise);
			cmd.SetGlobalTexture(ShaderPropertyID.CracksPackedMap, settings.packedCracksMap);

			ctx.ExecuteCommandBuffer(cmd);
			CommandBufferPool.Release(cmd);
		}
	}
}
