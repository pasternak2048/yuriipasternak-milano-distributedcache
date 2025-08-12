namespace MILANO.DistributedCache.Server.Infrastructure.Cache
{
	/// <summary>
	/// Background service responsible for periodically cleaning up expired cache entries.
	/// It operates independently of user requests and avoids blocking the main thread,
	/// ensuring minimal performance impact on active cache operations.
	/// </summary>
	public class BackgroundCleanupService : BackgroundService
	{
		private readonly ILogger<BackgroundCleanupService> _logger;
		private readonly ExpiredEntryCollection _expiredEntries;
		private readonly InMemoryCacheService _cache;

		/// <summary>
		/// Interval between cleanup runs. Can be tuned based on system load or TTL density.
		/// </summary>
		private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

		/// <summary>
		/// Initializes a new instance of the <see cref="BackgroundCleanupService"/> class.
		/// </summary>
		public BackgroundCleanupService(
			ILogger<BackgroundCleanupService> logger,
			ExpiredEntryCollection expiredEntries,
			InMemoryCacheService cache)
		{
			_logger = logger;
			_expiredEntries = expiredEntries;
			_cache = cache;
		}

		/// <summary>
		/// Main background execution loop. Scans for expired keys and removes them from cache.
		/// Runs until the application is stopped or cancelled.
		/// </summary>
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					var now = DateTime.UtcNow;

					// Retrieve expired keys based on current UTC time
					var expiredKeys = _expiredEntries.GetExpiredKeys(now);

					if (expiredKeys.Count > 0)
					{
						// Remove each expired key from the cache
						foreach (var key in expiredKeys)
						{
							_cache.Remove(key);
						}

						// Clean up the expired keys from the TTL collection
						_expiredEntries.RemoveKeys(expiredKeys);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Error during background cleanup.");
				}

				// Wait for the next cycle with respect to cancellation token
				await Task.Delay(_interval, stoppingToken).ConfigureAwait(false);
			}
		}
	}
}
