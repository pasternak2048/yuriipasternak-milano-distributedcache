using MILANO.Shared.Dtos.Cache;

namespace MILANO.Server.Application.Cache
{
	/// <summary>
	/// Defines operations for interacting with the distributed cache.
	/// </summary>
	public interface ICacheService
	{
		/// <summary>
		/// Gets a value from the cache by key.
		/// </summary>
		/// <param name="request">The cache get request.</param>
		/// <returns>The result of the cache lookup, including key, value and found flag.</returns>
		Task<CacheResponse> GetAsync(CacheGetRequest request);

		/// <summary>
		/// Sets a value in the cache for the specified key.
		/// </summary>
		/// <param name="request">The cache set request, including optional expiration.</param>
		/// <returns>A task representing the asynchronous operation.</returns>
		Task SetAsync(CacheSetRequest request);

		/// <summary>
		/// Determines whether a key exists in the cache.
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <returns>True if the key exists and is not expired; otherwise, false.</returns>
		Task<bool> ExistsAsync(string key);

		/// <summary>
		/// Removes a key from the cache.
		/// </summary>
		/// <param name="key">The cache key to remove.</param>
		/// <returns>True if the key was removed; false if it was not found.</returns>
		Task<bool> RemoveAsync(string key);

		/// <summary>
		/// Gets the number of active (non-expired) entries in the cache.
		/// </summary>
		/// <returns>The number of cache entries.</returns>
		Task<int> CountAsync();

		/// <summary>
		/// Gets a dictionary snapshot of all cache entries, including expired if specified.
		/// </summary>
		/// <param name="includeExpired">Whether to include expired entries.</param>
		/// <returns>A dictionary of keys and values.</returns>
		Task<IDictionary<string, string>> DumpAsync(bool includeExpired = false);
	}
}
