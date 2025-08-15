using MILANO.DistributedCache.Server.Application.Cache;
using MILANO.DistributedCache.Server.Application.Cache.Exceptions;
using MILANO.DistributedCache.Shared.Dtos.Cache;
using System.Collections.Concurrent;

namespace MILANO.DistributedCache.Server.Infrastructure.Cache
{
	/// <summary>
	/// In-memory implementation of <see cref="ICacheService"/> that stores key-value pairs with optional TTL.
	/// Thread-safe and optimized for fast read/write under concurrent access.
	/// TTL expiration is handled externally by a background cleanup service.
	/// </summary>
	public sealed class InMemoryCacheService : ICacheService
	{
		private readonly ConcurrentDictionary<string, CacheEntry> _store = new();
		private readonly ExpiredEntryCollection _expiredEntries;
		private readonly int _maxPayloadSizeBytes;
		private int _validCount = 0;

		/// <summary>
		/// Initializes a new instance of the <see cref="InMemoryCacheService"/>.
		/// </summary>
		/// <param name="expiredEntries">Reference to a shared collection that tracks expirations.</param>
		/// <param name="maxPayloadSizeBytes">Maximum allowed size per value in bytes. Defaults to 1MB.</param>
		public InMemoryCacheService(ExpiredEntryCollection expiredEntries, int maxPayloadSizeBytes = 1_000_000)
		{
			_expiredEntries = expiredEntries;
			_maxPayloadSizeBytes = maxPayloadSizeBytes;
		}

		/// <summary>
		/// Retrieves a value from the cache by key if it's not expired.
		/// If expired, removes the entry immediately.
		/// </summary>
		public Task<CacheResponse> GetAsync(CacheGetRequest request)
		{
			var now = DateTimeOffset.UtcNow;

			if (_store.TryGetValue(request.Key, out var entry))
			{
				if (!entry.IsExpired(now))
				{
					return Task.FromResult(CacheResponse.Hit(request.Key, entry.Value));
				}
				else
				{
					_ = RemoveAsync(request.Key);
				}
			}

			return Task.FromResult(CacheResponse.Miss(request.Key));
		}

		/// <summary>
		/// Stores or updates a key-value pair in the cache with optional expiration.
		/// Registers the key in the expiration collection if TTL is provided.
		/// </summary>
		public Task SetAsync(CacheSetRequest request)
		{
			if (request.Value.Length > _maxPayloadSizeBytes)
				throw new CacheEntryTooLargeException(request.Value.Length, _maxPayloadSizeBytes);

			var expiration = request.ExpirationSeconds.HasValue && request.ExpirationSeconds.Value > 0
				? DateTimeOffset.UtcNow.AddSeconds(request.ExpirationSeconds.Value)
				: (DateTimeOffset?)null;

			var entry = new CacheEntry(request.Value, expiration);

			if (_store.TryAdd(request.Key, entry))
			{
				Interlocked.Increment(ref _validCount);
			}
			else
			{
				_store.AddOrUpdate(request.Key, entry, (key, existing) =>
				{
					if (existing.IsExpired(DateTimeOffset.UtcNow))
					{
						Interlocked.Increment(ref _validCount);
					}
					return entry;
				});
			}

			if (expiration.HasValue)
			{
				_expiredEntries.Add(request.Key, expiration.Value.UtcDateTime);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Checks if a valid (non-expired) cache entry exists for the given key.
		/// </summary>
		public Task<bool> ExistsAsync(string key)
		{
			var now = DateTimeOffset.UtcNow;
			var exists = _store.TryGetValue(key, out var entry) && !entry.IsExpired(now);
			return Task.FromResult(exists);
		}

		/// <summary>
		/// Removes an entry by key. If the entry is valid, decrements the internal valid counter.
		/// </summary>
		public Task<bool> RemoveAsync(string key)
		{
			var removed = _store.TryRemove(key, out var entry);
			if (removed)
			{
				Interlocked.Decrement(ref _validCount);
			}
			return Task.FromResult(removed);
		}

		/// <summary>
		/// Returns the number of currently valid (non-expired) entries in the cache.
		/// </summary>
		public Task<int> CountAsync()
		{
			return Task.FromResult(Volatile.Read(ref _validCount));
		}

		/// <summary>
		/// Returns all stored entries as a dictionary.
		/// Can optionally include expired entries for diagnostics or debugging.
		/// </summary>
		public Task<IDictionary<string, string>> DumpAsync(bool includeExpired = false)
		{
			var now = DateTimeOffset.UtcNow;

			var result = includeExpired
				? _store.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value)
				: _store.Where(kvp => !kvp.Value.IsExpired(now))
						.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

			return Task.FromResult((IDictionary<string, string>)result);
		}
	}
}
