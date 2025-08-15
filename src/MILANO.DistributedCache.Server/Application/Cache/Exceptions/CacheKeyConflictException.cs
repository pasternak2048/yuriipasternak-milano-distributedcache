using System.Runtime.Serialization;

namespace MILANO.DistributedCache.Server.Application.Cache.Exceptions
{
	/// <summary>
	/// Exception thrown when a cache key conflict occurs (e.g. key already exists and overwrite is disabled).
	/// </summary>
	[Serializable]
	public sealed class CacheKeyConflictException : Exception
	{
		/// <summary>
		/// Gets the conflicting cache key.
		/// </summary>
		public string Key { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="CacheKeyConflictException"/> class.
		/// </summary>
		/// <param name="key">The cache key that caused the conflict.</param>
		public CacheKeyConflictException(string key)
			: base($"Cache key conflict: '{key}' already exists.")
		{
			Key = key;
		}
	}
}
