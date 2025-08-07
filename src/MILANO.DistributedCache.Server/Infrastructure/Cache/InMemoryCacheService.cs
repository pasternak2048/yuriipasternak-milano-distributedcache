using MILANO.DistributedCache.Server.Application.Cache;
using MILANO.DistributedCache.Server.Application.Cache.Dto;
using MILANO.DistributedCache.Server.Application.Cache.Exceptions;
using System.Collections.Concurrent;

namespace MILANO.DistributedCache.Server.Infrastructure.Cache
{
	/// <summary>
	/// In-memory implementation of <see cref="ICacheService"/> with optional expiration support and background cleanup.
	/// </summary>
	public sealed class InMemoryCacheService : ICacheService, IDisposable
	{
		private readonly ConcurrentDictionary<string, CacheEntry> _store = new();
		private readonly int _maxPayloadSizeBytes;
		private int _validCount = 0;
		private readonly Timer _cleanupTimer;
		private readonly TimeSpan _cleanupInterval = TimeSpan.FromMinutes(1);

		public InMemoryCacheService(int maxPayloadSizeBytes = 1_000_000)
		{
			_maxPayloadSizeBytes = maxPayloadSizeBytes;

			// Start the timer to run CleanupExpiredEntries every cleanup interval
			_cleanupTimer = new Timer(_ => CleanupExpiredEntries(), null, _cleanupInterval, _cleanupInterval);
		}

		/// <summary>
		/// Retrieves a cache entry by key if it exists and is not expired.
		/// If expired, removes it from the cache.
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
					// Remove expired entry and adjust valid count
					RemoveExpiredEntry(request.Key, entry);
				}
			}

			return Task.FromResult(CacheResponse.Miss(request.Key));
		}

		/// <summary>
		/// Adds or updates a cache entry with optional expiration.
		/// Throws exception if the value size exceeds the max allowed.
		/// </summary>
		public Task SetAsync(CacheSetRequest request)
		{
			if (request.Value.Length > _maxPayloadSizeBytes)
				throw new CacheEntryTooLargeException(request.Value.Length, _maxPayloadSizeBytes);

			var expiration = request.ExpirationSeconds.HasValue && request.ExpirationSeconds.Value > 0
				? DateTimeOffset.UtcNow.AddSeconds(request.ExpirationSeconds.Value)
				: (DateTimeOffset?)null;

			var entry = new CacheEntry(request.Value, expiration);

			// Try to add new entry
			if (_store.TryAdd(request.Key, entry))
			{
				// New key added, increment valid count
				Interlocked.Increment(ref _validCount);
			}
			else
			{
				// Key exists, update entry and increment valid count if the old entry was expired
				_store.AddOrUpdate(request.Key, entry, (key, existing) =>
				{
					if (existing.IsExpired(DateTimeOffset.UtcNow))
					{
						Interlocked.Increment(ref _validCount);
					}
					return entry;
				});
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Checks if a valid (non-expired) cache entry exists for the specified key.
		/// </summary>
		public Task<bool> ExistsAsync(string key)
		{
			var now = DateTimeOffset.UtcNow;
			var exists = _store.TryGetValue(key, out var entry) && !entry.IsExpired(now);
			return Task.FromResult(exists);
		}

		/// <summary>
		/// Removes a cache entry by key.
		/// Adjusts the valid count if a valid entry was removed.
		/// </summary>
		public Task<bool> RemoveAsync(string key)
		{
			var removed = _store.TryRemove(key, out var removedEntry);

			if (removed && removedEntry != null && !removedEntry.IsExpired(DateTimeOffset.UtcNow))
			{
				Interlocked.Decrement(ref _validCount);
			}

			return Task.FromResult(removed);
		}

		/// <summary>
		/// Returns the number of valid (non-expired) cache entries.
		/// </summary>
		public Task<int> CountAsync()
		{
			return Task.FromResult(Volatile.Read(ref _validCount));
		}

		/// <summary>
		/// Returns all cache entries as a dictionary, optionally including expired entries.
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

		/// <summary>
		/// Background cleanup of expired cache entries.
		/// Removes expired entries and updates the valid count accordingly.
		/// </summary>
		private void CleanupExpiredEntries()
		{
			var now = DateTimeOffset.UtcNow;
			var keysToRemove = new List<string>();

			// Find expired keys
			foreach (var kvp in _store)
			{
				if (kvp.Value.IsExpired(now))
				{
					keysToRemove.Add(kvp.Key);
				}
			}

			// Remove expired keys
			foreach (var key in keysToRemove)
			{
				if (_store.TryRemove(key, out var removedEntry) && removedEntry != null)
				{
					Interlocked.Decrement(ref _validCount);
				}
			}
		}

		/// <summary>
		/// Removes a single expired cache entry if present and updates valid count.
		/// </summary>
		private void RemoveExpiredEntry(string key, CacheEntry expiredEntry)
		{
			if (_store.TryRemove(key, out var removedEntry) && removedEntry != null)
			{
				Interlocked.Decrement(ref _validCount);
			}
		}

		/// <summary>
		/// Disposes of the cleanup timer.
		/// </summary>
		public void Dispose()
		{
			_cleanupTimer?.Dispose();
		}

		/// <summary>
		/// Internal cache entry containing the value and optional expiration.
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

			/// <summary>
			/// Checks if the cache entry is expired compared to the provided time.
			/// </summary>
			public bool IsExpired(DateTimeOffset now)
			{
				return Expiration.HasValue && now >= Expiration.Value;
			}
		}
	}
}
