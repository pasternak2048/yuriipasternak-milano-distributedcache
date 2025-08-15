using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MILANO.Client.Configuration;
using MILANO.Client.Enums;
using MILANO.Client.Interfaces;
using MILANO.Shared;
using MILANO.Shared.Protos;

namespace MILANO.Client.Services
{
	/// <summary>
	/// Factory responsible for creating the appropriate IMilanoCacheClient instance
	/// based on MilanoClientOptions (Http or Grpc).
	/// </summary>
	public sealed class MilanoCacheClientFactory
	{
		private readonly MilanoClientOptions _options;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly ILoggerFactory _loggerFactory;

		public MilanoCacheClientFactory(
			IOptions<MilanoClientOptions> options,
			IHttpClientFactory httpClientFactory,
			ILoggerFactory loggerFactory)
		{
			_options = options.Value;
			_httpClientFactory = httpClientFactory;
			_loggerFactory = loggerFactory;
		}

		/// <summary>
		/// Creates the appropriate IMilanoCacheClient based on configured transport.
		/// </summary>
		public IMilanoCacheClient Create()
		{
			return _options.Mode switch
			{
				MilanoClientMode.Grpc => CreateGrpcClient(),
				MilanoClientMode.Http => CreateHttpClient(),
				_ => throw new InvalidOperationException($"Unknown client mode: {_options.Mode}")
			};
		}

		private IMilanoCacheClient CreateHttpClient()
		{
			var httpClient = _httpClientFactory.CreateClient("MilanoHttp");
			httpClient.BaseAddress = new Uri(_options.ServerHost);
			httpClient.Timeout = _options.Timeout;
			httpClient.DefaultRequestHeaders.Add(Constants.Headers.ApiKey, _options.ApiKey);

			return new HttpMilanoCacheClient(httpClient);
		}

		private IMilanoCacheClient CreateGrpcClient()
		{
			var channel = GrpcChannel.ForAddress(_options.ServerHost);
			var grpcClient = new CacheService.CacheServiceClient(channel);
			return new GrpcMilanoCacheClient(grpcClient);
		}
	}
}
