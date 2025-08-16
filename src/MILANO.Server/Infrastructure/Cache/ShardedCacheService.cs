using MILANO.Common.Dtos.Cache;
using MILANO.Server.Application.Cache;

namespace MILANO.Server.Infrastructure.Cache
{
	/// <summary>
	/// Distributed cache service that partitions data across multiple in-memory shards.
	/// Uses a pluggable sharding strategy to route requests to the appropriate shard.
	/// Designed for horizontal scalability and reduced contention under concurrent access.
	/// </summary>
	public sealed class ShardedCacheService : ICacheService
	{
		/// <summary>
		/// Collection of underlying cache shards.
		/// Each shard is an independent implementation of <see cref="ICacheService"/>.
		/// </summary>
		private readonly ICacheService[] _shards;

		/// <summary>
		/// Strategy used to map keys to shard indices.
		/// Allows flexible hashing (e.g., hash modulo, rendezvous hashing).
		/// </summary>
		private readonly IShardingStrategy _strategy;

		/// <summary>
		/// Initializes a new instance of the <see cref="ShardedCacheService"/>.
		/// </summary>
		/// <param name="shardCount">Number of shards to create.</param>
		/// <param name="shardFactory">Factory function used to instantiate each shard.</param>
		/// <param name="strategy">Sharding strategy used to distribute keys.</param>
		public ShardedCacheService(int shardCount, Func<int, ICacheService> shardFactory, IShardingStrategy strategy)
		{
			_strategy = strategy;

			_shards = Enumerable.Range(0, shardCount)
								.Select(i => shardFactory(i))
								.ToArray();
		}

		/// <summary>
		/// Determines the appropriate shard for a given cache key using the configured strategy.
		/// </summary>
		/// <param name="key">The cache key to route.</param>
		/// <returns>The shard responsible for the given key.</returns>
		private ICacheService GetShard(string key)
		{
			var index = _strategy.GetShardIndex(key, _shards.Length);
			return _shards[index];
		}

		/// <inheritdoc />
		public ValueTask<CacheResponse> GetAsync(CacheGetRequest request)
			=> GetShard(request.Key).GetAsync(request);

		/// <inheritdoc />
		public ValueTask SetAsync(CacheSetRequest request)
			=> GetShard(request.Key).SetAsync(request);

		/// <inheritdoc />
		public ValueTask<bool> ExistsAsync(string key)
			=> GetShard(key).ExistsAsync(key);

		/// <inheritdoc />
		public ValueTask<bool> RemoveAsync(string key)
			=> GetShard(key).RemoveAsync(key);

		/// <summary>
		/// Aggregates the entry count across all shards.
		/// </summary>
		public async Task<int> CountAsync()
		{
			var counts = await Task.WhenAll(_shards.Select(s => s.CountAsync()));
			return counts.Sum();
		}

		/// <summary>
		/// Aggregates key-value pairs from all shards into a single dictionary.
		/// Can optionally include expired entries.
		/// </summary>
		public async Task<IDictionary<string, string>> DumpAsync(bool includeExpired = false)
		{
			var dumps = await Task.WhenAll(_shards.Select(s => s.DumpAsync(includeExpired)));

			// Flatten all dictionaries into one
			return dumps.SelectMany(d => d).ToDictionary(pair => pair.Key, pair => pair.Value);
		}
	}
}
