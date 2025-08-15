using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MILANO.Server.Application.Cache;
using MILANO.Server.Application.Security;
using MILANO.Server.Infrastructure.Cache;
using MILANO.Server.Infrastructure.Options;
using MILANO.Server.Infrastructure.Security;
using MILANO.Server.Web.Filters;
using MILANO.Server.Web.Grpc;

namespace MILANO.Server.Web.Extensions
{
	/// <summary>
	/// Provides extension methods for registering services used by the MILANO distributed cache.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds all core services required for the distributed cache server.
		/// </summary>
		/// <param name="services">The service collection.</param>
		/// <param name="configuration">The application configuration.</param>
		/// <returns>The same service collection instance.</returns>
		public static IServiceCollection AddMilanoDistributedCache(this IServiceCollection services, IConfiguration configuration)
		{
			// Options
			services.Configure<CacheOptions>(configuration.GetSection("CacheOptions"));
			services.Configure<ApiKeyStoreOptions>(configuration.GetSection("ApiKeyStore"));

			// Cache
			services.AddMemoryCache();
			services.AddSingleton<ExpiredEntryCollection>();
			services.AddSingleton<IShardingStrategy, HashModuloShardingStrategy>();

			services.AddSingleton<ICacheService>(provider =>
			{
				var expiredEntries = provider.GetRequiredService<ExpiredEntryCollection>();
				var strategy = provider.GetRequiredService<IShardingStrategy>();
				var options = provider.GetRequiredService<IOptions<CacheOptions>>().Value;

				var shardCount = options.ShardCount;
				var payloadLimit = options.MaxPayloadSizeBytes;

				return new ShardedCacheService(
					shardCount,
					shardIndex => new InMemoryCacheService(expiredEntries, payloadLimit),
					strategy
				);
			});
			services.AddHostedService<BackgroundCleanupService>();

			// API key store and validation
			services.AddSingleton<FileApiKeyStore>();
			services.AddSingleton(sp =>
			{
				var fileStore = sp.GetRequiredService<FileApiKeyStore>();
				var cache = sp.GetRequiredService<IMemoryCache>();
				var logger = sp.GetRequiredService<ILogger<CachedApiKeyStore>>();
				return new CachedApiKeyStore(fileStore, cache, logger);
			});
			services.AddSingleton<IApiKeyStore>(sp => sp.GetRequiredService<CachedApiKeyStore>());
			services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

			// gRPC
			services.AddScoped<GrpcPermissionInterceptor>();
			services.AddGrpc(options =>
			{
				options.Interceptors.Add<GrpcPermissionInterceptor>();
			});

			// Controllers
			services.AddControllers(options =>
			{
				options.Filters.Add<ApiExceptionFilter>();
			});

			return services;
		}
	}
}
