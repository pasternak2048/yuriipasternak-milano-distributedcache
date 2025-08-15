namespace MILANO.DistributedCache.Server.Application.Cache.Exceptions
{
	/// <summary>
	/// Exception thrown when a cache entry exceeds the configured maximum allowed size.
	/// </summary>
	[Serializable]
	public sealed class CacheEntryTooLargeException : Exception
	{
		/// <summary>
		/// Gets the maximum allowed size in bytes.
		/// </summary>
		public int MaxAllowedSizeBytes { get; }

		/// <summary>
		/// Gets the actual size of the attempted cache entry in bytes.
		/// </summary>
		public int ActualSizeBytes { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheEntryTooLargeException"/> class.
		/// </summary>
		/// <param name="actualSizeBytes">The actual size of the cache entry.</param>
		/// <param name="maxAllowedSizeBytes">The configured maximum allowed size.</param>
		public CacheEntryTooLargeException(int actualSizeBytes, int maxAllowedSizeBytes)
			: base($"Cache entry too large: {actualSizeBytes} bytes (max allowed: {maxAllowedSizeBytes} bytes).")
		{
			ActualSizeBytes = actualSizeBytes;
			MaxAllowedSizeBytes = maxAllowedSizeBytes;
		}
	}
}
