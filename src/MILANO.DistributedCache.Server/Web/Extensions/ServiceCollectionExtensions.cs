using Microsoft.Extensions.Caching.Memory;
using MILANO.DistributedCache.Server.Application.Cache;
using MILANO.DistributedCache.Server.Application.Security;
using MILANO.DistributedCache.Server.Infrastructure.Cache;
using MILANO.DistributedCache.Server.Infrastructure.Options;
using MILANO.DistributedCache.Server.Infrastructure.Security;
using MILANO.DistributedCache.Server.Web.Filters;
using MILANO.DistributedCache.Server.Web.Grpc;

namespace MILANO.DistributedCache.Server.Web.Extensions
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
			services.Configure<CacheOptions>(configuration.GetSection("Cache"));
			services.Configure<ApiKeyStoreOptions>(configuration.GetSection("ApiKeyStore"));

			// Cache
			services.AddMemoryCache();
			services.AddSingleton<ExpiredEntryCollection>();
			services.AddSingleton<InMemoryCacheService>();
			services.AddSingleton<ICacheService>(sp => sp.GetRequiredService<InMemoryCacheService>());
			services.AddHostedService<BackgroundCleanupService>();

			// API key store and validation
			services.AddSingleton<FileApiKeyStore>();
			services.AddSingleton<CachedApiKeyStore>(sp =>
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
