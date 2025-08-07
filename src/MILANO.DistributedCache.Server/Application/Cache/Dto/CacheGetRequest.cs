namespace MILANO.DistributedCache.Server.Application.Cache.Dto
{
	/// <summary>
	/// Represents a request to retrieve a value from the cache by key.
	/// </summary>
	public sealed record CacheGetRequest
	{
		/// <summary>
		/// Gets or sets the cache key to retrieve.
		/// </summary>
		public required string Key { get; init; }
	}
}
