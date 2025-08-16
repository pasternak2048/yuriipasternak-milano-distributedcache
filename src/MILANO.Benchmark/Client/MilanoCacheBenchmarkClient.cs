using MILANO.Client.Interfaces;

namespace MILANO.Benchmark.Client
{
	/// <summary>
	/// Wrapper over IMilanoCacheClient to abstract benchmark logic from transport.
	/// </summary>
	public class MilanoCacheBenchmarkClient
	{
		private readonly IMilanoCacheClient _client;

		public MilanoCacheBenchmarkClient(IMilanoCacheClient client)
		{
			_client = client;
		}

		/// <summary>
		/// Attempts to set a key-value pair with optional expiration.
		/// </summary>
		public async Task<bool> SetAsync(string key, string value, TimeSpan? expiration = null)
		{
			return await _client.SetAsync(key, value, expiration ?? TimeSpan.FromSeconds(60));
		}

		/// <summary>
		/// Attempts to get a string value by key.
		/// Returns null if key not found or expired.
		/// </summary>
		public async Task<string?> GetAsync(string key)
		{
			return await _client.GetAsync(key);
		}
	}
}
