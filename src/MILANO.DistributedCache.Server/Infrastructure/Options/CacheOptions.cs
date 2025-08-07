namespace MILANO.DistributedCache.Server.Infrastructure.Options
{
	/// <summary>
	/// Represents configuration options for the in-memory cache.
	/// </summary>
	public sealed class CacheOptions
	{
		/// <summary>
		/// Gets or sets the maximum allowed size of a cache value in bytes.
		/// </summary>
		public int MaxPayloadSizeBytes { get; set; } = 1_000_000;

		/// <summary>
		/// Gets or sets the default expiration time (in seconds) for cache entries if none is provided.
		/// A value of null disables default expiration.
		/// </summary>
		public int? DefaultExpirationSeconds { get; set; } = null;

		/// <summary>
		/// Gets or sets whether expired entries should be automatically cleaned up.
		/// </summary>
		public bool EnableAutoCleanup { get; set; } = false;
	}
}
