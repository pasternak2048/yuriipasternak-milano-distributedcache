using Microsoft.Extensions.Caching.Memory;
using MILANO.Server.Application.Security;
using MILANO.Server.Application.Security.Models;
using MILANO.Shared;

namespace MILANO.Server.Infrastructure.Security
{
	/// <summary>
	/// Caching decorator for <see cref="IApiKeyStore"/> that stores API keys in memory
	/// to avoid frequent file/database access.
	/// </summary>
	public sealed class CachedApiKeyStore : IApiKeyStore
	{
		private readonly IApiKeyStore _inner;
		private readonly IMemoryCache _cache;
		private readonly ILogger<CachedApiKeyStore> _logger;

		private const string CacheKey = Constants.Metadata.CacheApiKey;

		/// <summary>
		/// Duration to keep keys in memory before reloading from the source.
		/// </summary>
		private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

		/// <summary>
		/// Initializes a new instance of the <see cref="CachedApiKeyStore"/> class.
		/// </summary>
		/// <param name="inner">The actual API key store (e.g., file or DB).</param>
		/// <param name="cache">In-memory cache instance.</param>
		/// <param name="logger">Logger for diagnostics.</param>
		public CachedApiKeyStore(IApiKeyStore inner, IMemoryCache cache, ILogger<CachedApiKeyStore> logger)
		{
			_inner = inner;
			_cache = cache;
			_logger = logger;
		}

		/// <inheritdoc />
		public async Task<IReadOnlyCollection<ApiKeyDefinition>> GetAllAsync()
		{
			var all = await GetCacheAsync();
			return all.Values.ToList();
		}

		/// <inheritdoc />
		public async Task<ApiKeyDefinition?> FindByKeyAsync(string rawKey)
		{
			var all = await GetCacheAsync();
			return all.TryGetValue(rawKey, out var keyDef) ? keyDef : null;
		}

		/// <summary>
		/// Retrieves the current cached API keys, or loads them if not present in memory.
		/// </summary>
		private async Task<Dictionary<string, ApiKeyDefinition>> GetCacheAsync()
		{
			// Try to return keys from memory if available
			if (_cache.TryGetValue(CacheKey, out Dictionary<string, ApiKeyDefinition> cached))
				return cached;

			// Otherwise, load and cache fresh data
			var fresh = await LoadAndCacheAsync();
			return fresh;
		}

		/// <summary>
		/// Loads API keys from the underlying store and updates the memory cache.
		/// </summary>
		private async Task<Dictionary<string, ApiKeyDefinition>> LoadAndCacheAsync()
		{
			var all = await _inner.GetAllAsync();

			var dict = all
				.Where(x => !string.IsNullOrWhiteSpace(x.Key))
				.ToDictionary(x => x.Key, StringComparer.Ordinal);

			_cache.Set(CacheKey, dict, CacheDuration);
			_logger.LogInformation("Cached {Count} API keys", dict.Count);

			return dict;
		}

		/// <summary>
		/// Forces cache reload from the inner store (optional external trigger).
		/// Useful for admin endpoints or scheduled refreshes.
		/// </summary>
		public async Task RefreshAsync()
		{
			await LoadAndCacheAsync();
		}
	}
}
