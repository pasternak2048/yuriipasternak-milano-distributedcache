namespace MILANO.Server.Infrastructure.Cache
{
	/// <summary>
	/// Represents a single in-memory cache entry, including its string value and optional expiration time.
	/// </summary>
	public sealed class CacheEntry
	{
		/// <summary>
		/// The actual cached value as a UTF-8 string.
		/// </summary>
		public string Value { get; }

		/// <summary>
		/// The optional expiration time (UTC) for the entry. If null, the entry does not expire.
		/// </summary>
		public DateTimeOffset? Expiration { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheEntry"/> class.
		/// </summary>
		/// <param name="value">The string value to store in the cache.</param>
		/// <param name="expiration">The UTC expiration time. Null means no expiration.</param>
		public CacheEntry(string value, DateTimeOffset? expiration)
		{
			Value = value;
			Expiration = expiration;
		}

		/// <summary>
		/// Determines whether the cache entry is expired compared to the specified point in time.
		/// </summary>
		/// <param name="now">The current UTC time for comparison.</param>
		/// <returns>True if the entry is expired; otherwise, false.</returns>
		public bool IsExpired(DateTimeOffset now)
		{
			return Expiration.HasValue && now >= Expiration.Value;
		}
	}
}
