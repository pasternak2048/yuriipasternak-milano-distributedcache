using MILANO.DistributedCache.Server.Application.Cache;
using MILANO.DistributedCache.Server.Application.Cache.Dto;
using MILANO.DistributedCache.Server.Application.Cache.Exceptions;
using System.Collections.Concurrent;

namespace MILANO.DistributedCache.Server.Infrastructure.Cache
{
	/// <summary>
	/// In-memory implementation of <see cref="ICacheService"/> with optional expiration support.
	/// </summary>
	public sealed class InMemoryCacheService : ICacheService
	{
		private readonly ConcurrentDictionary<string, CacheEntry> _store = new();
		private readonly int _maxPayloadSizeBytes;

		public InMemoryCacheService(int maxPayloadSizeBytes = 1_000_000)
		{
			_maxPayloadSizeBytes = maxPayloadSizeBytes;
		}

		public Task<CacheResponse> GetAsync(CacheGetRequest request)
		{
			if (_store.TryGetValue(request.Key, out var entry) && !entry.IsExpired)
			{
				return Task.FromResult(CacheResponse.Hit(request.Key, entry.Value));
			}

			return Task.FromResult(CacheResponse.Miss(request.Key));
		}

		public Task SetAsync(CacheSetRequest request)
		{
			if (request.Value.Length > _maxPayloadSizeBytes)
			{
				throw new CacheEntryTooLargeException(request.Value.Length, _maxPayloadSizeBytes);
			}

			var expiration = request.ExpirationSeconds.HasValue
				? DateTimeOffset.UtcNow.AddSeconds(request.ExpirationSeconds.Value)
				: (DateTimeOffset?)null;

			var entry = new CacheEntry(request.Value, expiration);

			_store[request.Key] = entry;

			return Task.CompletedTask;
		}

		public Task<bool> ExistsAsync(string key)
		{
			return Task.FromResult(_store.TryGetValue(key, out var entry) && !entry.IsExpired);
		}

		public Task<bool> RemoveAsync(string key)
		{
			return Task.FromResult(_store.TryRemove(key, out _));
		}

		public Task<int> CountAsync()
		{
			var count = _store.Values.Count(e => !e.IsExpired);
			return Task.FromResult(count);
		}

		public Task<IDictionary<string, string>> DumpAsync(bool includeExpired = false)
		{
			var result = _store
				.Where(kvp => includeExpired || !kvp.Value.IsExpired)
				.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

			return Task.FromResult((IDictionary<string, string>)result);
		}

		/// <summary>
		/// Internal representation of a cache entry with optional expiration.
		/// </summary>
		private sealed class CacheEntry
		{
			public string Value { get; }
			public DateTimeOffset? Expiration { get; }

			public CacheEntry(string value, DateTimeOffset? expiration)
			{
				Value = value;
				Expiration = expiration;
			}

			public bool IsExpired =>
				Expiration.HasValue && DateTimeOffset.UtcNow >= Expiration.Value;
		}
	}
}
