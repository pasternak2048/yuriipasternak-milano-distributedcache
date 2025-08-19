using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MILANO.Server.Application.Cache;
using MILANO.Server.Application.Security;
using MILANO.Server.Infrastructure.Cache;
using MILANO.Server.Infrastructure.Options;
using MILANO.Server.Infrastructure.Security;
using MILANO.Server.Web.Filters;

namespace MILANO.Server.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddMilanoDistributedCache(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddMilanoOptions(configuration)
				.AddMilanoCache()
				.AddMilanoApiKeyValidation()
				.AddMilanoControllers();

			return services;
		}

		private static IServiceCollection AddMilanoOptions(this IServiceCollection services, IConfiguration configuration)
		{
			services
				.AddOptions<CacheOptions>()
				.Bind(configuration.GetSection("CacheOptions"))
				.ValidateDataAnnotations()
				.ValidateOnStart();

			services
				.AddOptions<ApiKeyStoreOptions>()
				.Bind(configuration.GetSection("ApiKeyStore"))
				.ValidateDataAnnotations()
				.ValidateOnStart();

			return services;
		}

		private static IServiceCollection AddMilanoCache(this IServiceCollection services)
		{
			services.AddMemoryCache();
			services.AddSingleton<ExpiredEntryCollection>();
			services.AddSingleton<IShardingStrategy, HashModuloShardingStrategy>();

			services.AddSingleton<ICacheService>(sp =>
			{
				var expiredEntries = sp.GetRequiredService<ExpiredEntryCollection>();
				var strategy = sp.GetRequiredService<IShardingStrategy>();
				var options = sp.GetRequiredService<IOptions<CacheOptions>>().Value;

				return new ShardedCacheService(
					shardCount: options.ShardCount,
					shardFactory: _ => new InMemoryCacheService(expiredEntries, options.MaxPayloadSizeBytes),
					strategy: strategy
				);
			});

			services.AddHostedService<BackgroundCleanupService>();

			return services;
		}

		private static IServiceCollection AddMilanoApiKeyValidation(this IServiceCollection services)
		{
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

			return services;
		}

		private static IServiceCollection AddMilanoControllers(this IServiceCollection services)
		{
			services.AddControllers(options =>
			{
				options.Filters.Add<ApiExceptionFilter>();
			});

			return services;
		}
	}
}
