using MILANO.Client.Interfaces;
using MILANO.Shared.Protos;

namespace MILANO.Client.Services
{
	/// <summary>
	/// MILANO cache client over gRPC transport.
	/// </summary>
	public sealed class GrpcMilanoCacheClient : IMilanoCacheClient
	{
		private readonly CacheService.CacheServiceClient _client;

		public GrpcMilanoCacheClient(CacheService.CacheServiceClient client)
		{
			_client = client;
		}

		public async Task<string?> GetAsync(string key, CancellationToken ct = default)
		{
			var request = new GrpcCacheGetRequest { Key = key };
			var response = await _client.GetAsync(request, cancellationToken: ct);
			return response.Found ? response.Value : null;
		}

		public async Task<bool> SetAsync(string key, string value, TimeSpan? expiration = null, CancellationToken ct = default)
		{
			var request = new GrpcCacheSetRequest
			{
				Key = key,
				Value = value,
				ExpirationSeconds = expiration.HasValue ? (int)expiration.Value.TotalSeconds : 0
			};

			var response = await _client.SetAsync(request, cancellationToken: ct);
			return response.Success;
		}

		public async Task<bool> RemoveAsync(string key, CancellationToken ct = default)
		{
			var request = new GrpcCacheRemoveRequest { Key = key };
			var response = await _client.RemoveAsync(request, cancellationToken: ct);
			return response.Success;
		}

		public async Task<bool> ExistsAsync(string key, CancellationToken ct = default)
		{
			var request = new GrpcCacheExistsRequest { Key = key };
			var response = await _client.ExistsAsync(request, cancellationToken: ct);
			return response.Exists;
		}

		public async Task<int> CountAsync(CancellationToken ct = default)
		{
			var response = await _client.CountAsync(new GrpcCacheCountRequest(), cancellationToken: ct);
			return response.Count;
		}
	}
}
