using Grpc.Core;
using MILANO.DistributedCache.Server.Application.Cache;
using MILANO.DistributedCache.Server.Application.Cache.Exceptions;
using MILANO.DistributedCache.Shared.Dtos.Cache;
using MILANO.DistributedCache.Shared.Protos;


namespace MILANO.DistributedCache.Server.Web.Grpc
{
	public sealed class CacheGrpcService : CacheService.CacheServiceBase
	{
		private readonly ICacheService _cacheService;
		private readonly ILogger<CacheGrpcService> _logger;

		public CacheGrpcService(ICacheService cacheService, ILogger<CacheGrpcService> logger)
		{
			_cacheService = cacheService;
			_logger = logger;
		}

		public override async Task<GrpcCacheGetResponse> Get(GrpcCacheGetRequest request, ServerCallContext context)
		{
			var appRequest = ToAppRequest(request);
			var result = await _cacheService.GetAsync(appRequest);

			return new GrpcCacheGetResponse
			{
				Key = result.Key,
				Value = result.Value ?? string.Empty,
				Found = result.Found
			};
		}

		public override async Task<GrpcCacheSetResponse> Set(GrpcCacheSetRequest request, ServerCallContext context)
		{
			var appRequest = ToAppRequest(request);

			try
			{
				await _cacheService.SetAsync(appRequest);
				return new GrpcCacheSetResponse { Success = true };
			}
			catch (CacheEntryTooLargeException ex)
			{
				_logger.LogWarning("Payload too large: {Size}", ex.ActualSizeBytes);
				throw new RpcException(new Status(StatusCode.InvalidArgument, ex.Message));
			}
		}

		public override async Task<GrpcCacheRemoveResponse> Remove(GrpcCacheRemoveRequest request, ServerCallContext context)
		{
			var success = await _cacheService.RemoveAsync(request.Key);
			return new GrpcCacheRemoveResponse { Success = success };
		}

		public override async Task<GrpcCacheExistsResponse> Exists(GrpcCacheExistsRequest request, ServerCallContext context)
		{
			var exists = await _cacheService.ExistsAsync(request.Key);
			return new GrpcCacheExistsResponse { Exists = exists };
		}

		public override async Task<GrpcCacheCountResponse> Count(GrpcCacheCountRequest request, ServerCallContext context)
		{
			var count = await _cacheService.CountAsync();
			return new GrpcCacheCountResponse { Count = count };
		}

		public override async Task<GrpcCacheDumpResponse> Dump(GrpcCacheDumpRequest request, ServerCallContext context)
		{
			var entries = await _cacheService.DumpAsync(includeExpired: request.IncludeExpired);
			var response = new GrpcCacheDumpResponse();
			response.Entries.Add(entries);
			return response;
		}

		private static CacheGetRequest ToAppRequest(GrpcCacheGetRequest grpcRequest) => new()
		{
			Key = grpcRequest.Key
		};

		private static CacheSetRequest ToAppRequest(GrpcCacheSetRequest grpcRequest) => new()
		{
			Key = grpcRequest.Key,
			Value = grpcRequest.Value,
			ExpirationSeconds = grpcRequest.ExpirationSeconds
		};
	}
}
