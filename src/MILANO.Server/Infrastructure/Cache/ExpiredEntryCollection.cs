using System.Collections.Concurrent;

namespace MILANO.Server.Infrastructure.Cache
{
	/// <summary>
	/// Thread-safe collection for tracking keys with their expiration timestamps.
	/// Used by the background cleanup service to determine which keys need removal.
	/// </summary>
	public class ExpiredEntryCollection
	{
		/// <summary>
		/// Internal queue that stores keys along with their expiration times.
		/// Entries are added in the order of arrival and scanned for expiration.
		/// </summary>
		private readonly ConcurrentQueue<(string Key, DateTime ExpiresAt)> _queue = new();

		/// <summary>
		/// Adds a new entry with expiration to the queue.
		/// Called during cache writes when TTL is present.
		/// </summary>
		public void Add(string key, DateTime expiresAt)
		{
			_queue.Enqueue((key, expiresAt));
		}

		/// <summary>
		/// Returns all keys that are expired at the specified moment in time.
		/// Does not remove entries from the queue.
		/// </summary>
		public List<string> GetExpiredKeys(DateTime now)
		{
			var expired = new List<string>();

			foreach (var (key, expiresAt) in _queue)
			{
				if (expiresAt <= now)
					expired.Add(key);
			}

			return expired;
		}

		/// <summary>
		/// Removes expired keys from the queue by rebuilding it,
		/// excluding the specified keys to delete.
		/// </summary>
		public void RemoveKeys(IEnumerable<string> keys)
		{
			var keySet = new HashSet<string>(keys);
			var newQueue = new ConcurrentQueue<(string Key, DateTime ExpiresAt)>();

			foreach (var item in _queue)
			{
				if (!keySet.Contains(item.Key))
				{
					newQueue.Enqueue(item);
				}
			}

			while (_queue.TryDequeue(out _)) { }

			foreach (var item in newQueue)
			{
				_queue.Enqueue(item);
			}
		}
	}
}
