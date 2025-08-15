namespace MILANO.DistributedCache.Server.Application.Cache
{
	/// <summary>
	/// Defines a strategy for mapping cache keys to shard indices.
	/// Used by <see cref="ShardedCacheService"/> to determine which shard should handle a given key.
	/// Allows plugging in different distribution algorithms (e.g. hash modulo, rendezvous hashing).
	/// </summary>
	public interface IShardingStrategy
	{
		/// <summary>
		/// Computes the index of the target shard for a given cache key.
		/// </summary>
		/// <param name="key">The cache key to route.</param>
		/// <param name="totalShards">The total number of available shards.</param>
		/// <returns>Index of the shard responsible for the key (0-based).</returns>
		int GetShardIndex(string key, int totalShards);
	}
}
