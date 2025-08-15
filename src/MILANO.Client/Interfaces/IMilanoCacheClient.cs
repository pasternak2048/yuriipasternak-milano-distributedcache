namespace MILANO.Client.Interfaces
{
	/// <summary>
	/// Represents the core contract for interacting with the MILANO distributed cache server.
	/// Abstracts away transport (HTTP or gRPC).
	/// </summary>
	public interface IMilanoCacheClient
	{
		/// <summary>
		/// Attempts to get a cached value by key.
		/// Returns null if the key does not exist or is expired.
		/// </summary>
		Task<string?> GetAsync(string key, CancellationToken ct = default);

		/// <summary>
		/// Sets a string value into the cache under the given key.
		/// Optional expiration can be specified.
		/// </summary>
		Task<bool> SetAsync(string key, string value, TimeSpan? expiration = null, CancellationToken ct = default);

		/// <summary>
		/// Removes a value from the cache by key.
		/// Returns true if the key existed and was removed.
		/// </summary>
		Task<bool> RemoveAsync(string key, CancellationToken ct = default);

		/// <summary>
		/// Checks whether the specified key exists in the cache.
		/// </summary>
		Task<bool> ExistsAsync(string key, CancellationToken ct = default);

		/// <summary>
		/// Returns the total number of items in the cache.
		/// May be slow on large datasets.
		/// </summary>
		Task<int> CountAsync(CancellationToken ct = default);
	}
}
