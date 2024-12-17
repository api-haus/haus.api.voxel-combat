namespace VoxelBox.World.Streaming.Awareness
{
	using Unity.Mathematics;
	using UnityEngine;
	using static Chunk.ChunkUtility;

	public class VoxelAwarenessBoxAuthoring : MonoBehaviour
	{
		public float3 size = 100f;

		void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireCube(transform.position, size);

			var bounds = new Bounds(transform.position, size);

			var minChunk = GetChunkCoord(bounds.min);
			var maxChunk = GetChunkCoord(bounds.max);

			foreach (var chunkCoord in ChunksMinMaxYield(minChunk, maxChunk))
			{
				var chunkBounds = ChunkBounds(chunkCoord);

				Gizmos.DrawWireCube(chunkBounds.center, chunkBounds.size);
			}
		}
	}
}
