using MILANO.Client.Interfaces;
using MILANO.Shared.Dtos.Cache;
using System.Net.Http.Json;
using System.Text.Json;

namespace MILANO.Client.Services
{
	/// <summary>
	/// MILANO cache client over HTTP transport.
	/// </summary>
	public sealed class HttpMilanoCacheClient : IMilanoCacheClient
	{
		private readonly HttpClient _http;
		private readonly JsonSerializerOptions _jsonOptions;

		public HttpMilanoCacheClient(HttpClient http)
		{
			_http = http;
			_jsonOptions = new JsonSerializerOptions
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			};
		}

		public async Task<string?> GetAsync(string key, CancellationToken ct = default)
		{
			var response = await _http.GetAsync($"/cache/{key}", ct);

			if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
				return null;

			response.EnsureSuccessStatusCode();

			var value = await response.Content.ReadAsStringAsync(ct);
			return value;
		}

		public async Task<bool> SetAsync(string key, string value, TimeSpan? expiration = null, CancellationToken ct = default)
		{
			var request = new CacheSetRequest
			{
				Key = key,
				Value = value,
				ExpirationSeconds = expiration.HasValue ? (int)expiration.Value.TotalSeconds : 0
			};

			var httpResponse = await _http.PostAsJsonAsync("/cache", request, _jsonOptions, ct);
			return httpResponse.IsSuccessStatusCode;
		}

		//TODO:
		public Task<bool> RemoveAsync(string key, CancellationToken ct = default)
		{
			return Task.FromResult(false);
		}

		//TODO:
		public Task<bool> ExistsAsync(string key, CancellationToken ct = default)
		{
			return Task.FromResult(false);
		}

		//TODO:
		public Task<int> CountAsync(CancellationToken ct = default)
		{
			return Task.FromResult(-1);
		}
	}
}
