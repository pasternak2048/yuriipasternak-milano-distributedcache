namespace MILANO.DistributedCache.Server.Application.Cache.Dto
{
	/// <summary>
	/// Represents a request to set a value in the cache by key.
	/// </summary>
	public sealed record CacheSetRequest
	{
		/// <summary>
		/// Gets or sets the cache key to set.
		/// </summary>
		public required string Key { get; init; }

		/// <summary>
		/// Gets or sets the value to associate with the specified key.
		/// </summary>
		public required string Value { get; init; }

		/// <summary>
		/// Gets or sets the optional expiration time in seconds for the cache entry.
		/// If null or zero, the value will persist until explicitly removed or overwritten.
		/// </summary>
		public int? ExpirationSeconds { get; init; }
	}
}
