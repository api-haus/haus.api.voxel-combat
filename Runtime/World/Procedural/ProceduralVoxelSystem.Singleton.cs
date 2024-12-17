namespace VoxelBox.World.Procedural
{
	using System;
	using FastNoise2.Bindings;
	using FastNoise2.Jobs;
	using FastNoise2.NativeTexture;
	using Unity.Burst;
	using Unity.Collections;
	using Unity.Entities;
	using Unity.Jobs;
	using Unity.Mathematics;
	using Unity.Transforms;
	using Authoring;
	using Palette;
	using Chunk;
	using Misc.Common;
	using Streaming.SceneVolumes;

	struct OverworldGenerator : IDisposable
	{
		internal FastNoise Noise;

		internal int Seed;
		internal float Frequency;

		internal VoxelPrototypeId DirtVoxelId;
		internal VoxelPrototypeId SurfaceVoxelId;

		internal VoxelPaletteItem DirtPrefab;
		internal VoxelPaletteItem SurfacePrefab;

		internal bool IsLoaded => !DirtPrefab.IsNull && !SurfacePrefab.IsNull;

		internal bool FindPrefabs(VoxelPaletteSystem.Singleton dhm)
		{
			if (!dhm.VoxelPalette.TryGetValue(DirtVoxelId, out DirtPrefab))
			{
				// Debug.Log($"cannot find dirt {DirtVoxelId}");
				return false;
			}

			if (!dhm.VoxelPalette.TryGetValue(SurfaceVoxelId, out SurfacePrefab))
			{
				// Debug.Log($"cannot find surface {SurfaceVoxelId}");
				return false;
			}

			return true;
		}

		internal static OverworldGenerator Create() =>
			new()
			{
				Noise = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAgAAAAAAD8AAAAAAA=="),
				Seed = 14242,
				Frequency = .06f,
				DirtVoxelId = "dirt_cube_2",
				SurfaceVoxelId = "ground_cube_grass",
			};

		public void Dispose() => Noise.Dispose();
	}

	struct OverworldChunk2D : IDisposable
	{
		const int ChunkSize = VoxelWorld.ChunkSize;

		public NativeTexture2D<float> OverworldLevels;
		public JobHandle JobHandle;

		public int2 Coord { get; private set; }
		public int2 VoxelCoord { get; private set; }

		public void Dispose() => OverworldLevels.Dispose();

		public static OverworldChunk2D Create(int2 chunkCoord) =>
			new()
			{
				Coord = chunkCoord,
				VoxelCoord = chunkCoord * ChunkSize, //
				OverworldLevels = new(ChunkSize, Allocator.Persistent), //
			};
	}

	partial struct ProceduralVoxelSystem
	{
		struct Singleton : IComponentData, IDisposable
		{
			const int MaxLoadedOverworldChunks = 32;

			NativeHashMap<int2, OverworldChunk2D> overworldChunks;
			NativeParallelHashMap<int3, JobHandle> chunkJobs;
			OverworldGenerator overworldGenerator;
			EntityArchetype volumeArchetype;

			public static Singleton Create(EntityArchetype volumeArchetype) => new()
			{
				volumeArchetype = volumeArchetype,
				overworldGenerator = OverworldGenerator.Create(), //
				overworldChunks = new(MaxLoadedOverworldChunks, Allocator.Persistent),
				chunkJobs = new(VoxelWorld.MaxLoadedChunks, Allocator.Persistent),
			};

			public bool Load(VoxelPaletteSystem.Singleton voxelPalette) =>
				overworldGenerator.IsLoaded || overworldGenerator.FindPrefabs(voxelPalette);

			public void Dispose()
			{
				foreach (var overworldChunk in overworldChunks)
					overworldChunk.Value.Dispose();
				overworldGenerator.Dispose();
				overworldChunks.Dispose();
				chunkJobs.Dispose();
			}

			public void Clear() => chunkJobs.Clear();

			[BurstCompile]
			struct ProceduralPlacementJob : IJobFor
			{
				internal EntityCommandBuffer.ParallelWriter ECB;
				[ReadOnly] internal OverworldGenerator OverworldGenerator;
				[ReadOnly] internal EntityArchetype VolumeArchetype;
				[ReadOnly] internal OverworldChunk2D OverworldChunk;
				[ReadOnly] internal int3 ChunkCoord;

				public void Execute(int i)
				{
					if (ChunkCoord.y == 0)
					{
						SurfaceVoxels(i);
						return;
					}

					if (ChunkCoord.y < 0)
					{
						UndergroundVoxels(i);
						return;
					}
				}

				void UndergroundVoxels(int i)
				{
					float pixelValue = OverworldChunk.OverworldLevels.ReadPixel(i, out var pixelCoord);

					var startCoord = ChunkCoord * VoxelWorld.ChunkSize;

					for (int y = 0; y < VoxelWorld.ChunkSize; y++)
					{
						var position = new float3(pixelCoord.x, pixelValue + y, pixelCoord.y) + startCoord;

						MakeVoxel(i, position, OverworldGenerator.DirtPrefab);
					}
				}

				void SurfaceVoxels(int i)
				{
					float pixelValue = OverworldChunk.OverworldLevels.ReadPixel(i, out var pixelCoord);

					var startCoord = ChunkCoord * VoxelWorld.ChunkSize;
					var position = new float3(pixelCoord.x, pixelValue, pixelCoord.y) + startCoord;

					MakeVoxel(i, position, OverworldGenerator.SurfacePrefab);
				}

				void MakeVoxel(int sortKey, float3 position, VoxelPaletteItem prefab)
				{
					var voxelEntity = ECB.Instantiate(sortKey, prefab.VoxelPrefab);
					ECB.SetComponent(sortKey, voxelEntity,
						new LocalTransform
						{
							Position = position, //
							Scale = 1,
							Rotation = quaternion.identity,
						});

					var volumeEntity = ECB.CreateEntity(sortKey, VolumeArchetype);
					ECB.SetComponent(sortKey, volumeEntity, new VoxelVolumeChunkCoord { Coord = ChunkCoord, });
					ECB.AppendToBuffer(sortKey, volumeEntity,
						new Voxel
						{
							protoId = prefab.protoId,
							Bounds = prefab.bounds.Translate(position),
							Entity = voxelEntity,
							PrefabEntity = prefab.VoxelPrefab,
						});
				}
			}

			internal JobHandle MakeOverworldNoise(
				ref OverworldChunk2D chunk,
				JobHandle dependency = default)
			{
				if (chunk.JobHandle.Equals(default))
				{
					dependency =
						overworldGenerator.Noise.GenUniformGrid2D(
							chunk.OverworldLevels,
							overworldGenerator.Seed,
							chunk.VoxelCoord,
							overworldGenerator.Frequency,
							dependency
						);

					chunk.JobHandle = dependency;
				}

				return chunk.JobHandle;
			}

			public void Generate(VoxelChunk chunk, EntityCommandBuffer ecb)
			{
				JobHandle job = default;

				if (!overworldChunks.TryGetValue(chunk.Coord.xz, out var overworldChunk))
				{
					overworldChunk = OverworldChunk2D.Create(chunk.Coord.xz);
					overworldChunks[chunk.Coord.xz] = overworldChunk;
				}

				job = MakeOverworldNoise(ref overworldChunk, job);
				job = new ProceduralPlacementJob
				{
					ECB = ecb.AsParallelWriter(),
					OverworldGenerator = overworldGenerator,
					VolumeArchetype = volumeArchetype,
					OverworldChunk = overworldChunk,
					ChunkCoord = chunk.Coord,
				}.ScheduleParallel(overworldChunk.OverworldLevels.Length, 64, job);

				overworldChunks[chunk.Coord.xz] = overworldChunk;
				chunkJobs[chunk.Coord] = job;
			}

			public void CompleteJobs()
			{
				JobHandle dep = default;

				foreach (var keyValue in chunkJobs)
					dep = JobHandle.CombineDependencies(dep, keyValue.Value);

				dep.Complete();
			}
		}
	}
}
