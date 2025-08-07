namespace MILANO.DistributedCache.Server.Application.Cache.Dto
{
	/// <summary>
	/// Represents a response containing the result of a cache get operation.
	/// </summary>
	public sealed record CacheResponse
	{
		/// <summary>
		/// Gets or sets the cache key that was requested.
		/// </summary>
		public required string Key { get; init; }

		/// <summary>
		/// Gets or sets the value associated with the cache key.
		/// </summary>
		public string? Value { get; init; }

		/// <summary>
		/// Gets or sets a flag indicating whether the value was found in the cache.
		/// </summary>
		public bool Found { get; init; }

		/// <summary>
		/// Creates a response indicating a cache hit.
		/// </summary>
		public static CacheResponse Hit(string key, string value) => new()
		{
			Key = key,
			Value = value,
			Found = true
		};

		/// <summary>
		/// Creates a response indicating a cache miss.
		/// </summary>
		public static CacheResponse Miss(string key) => new()
		{
			Key = key,
			Value = null,
			Found = false
		};
	}
}