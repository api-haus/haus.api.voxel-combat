namespace VoxelBox.World.Chunk
{
	using System.Collections.Generic;
	using Unity.Mathematics;
	using UnityEngine;

	public static class ChunkUtility
	{
		public static int3 GetChunkCoord(float3 positionInChunk) =>
			(int3)math.floor(positionInChunk / VoxelWorld.ChunkSize);

		public static IEnumerable<int3> ChunksMinMaxYield(int3 min, int3 max)
		{
			int3 c;
			for (c.x = min.x; c.x <= max.x; c.x++)
			{
				for (c.y = min.y; c.y <= max.y; c.y++)
				{
					for (c.z = min.z; c.z <= max.z; c.z++)
					{
						yield return c;
					}
				}
			}
		}

		public static ushort GetVoxelIndexInChunk(this VoxelChunk chunk, Bounds voxelBounds)
		{
			var coord = (int3)(float3)(voxelBounds.center - voxelBounds.min);

			coord = math.clamp(coord, 0, VoxelWorld.ChunkSize - 1);

			int index = (coord.z * VoxelWorld.ChunkSize2) + (coord.y * VoxelWorld.ChunkSize) + coord.x;

			return (ushort)index;
		}

		public static Bounds ChunkBounds(int3 chunkCoord)
		{
			var min = chunkCoord * VoxelWorld.ChunkSize;
			var max = min + VoxelWorld.ChunkSize;

			var bounds = new Bounds { min = (float3)min, max = (float3)max, };

			return bounds;
		}
	}
}
