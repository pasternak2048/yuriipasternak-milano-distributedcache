using MILANO.DistributedCache.Server.Application.Cache;
using MILANO.DistributedCache.Server.Application.Security;
using MILANO.DistributedCache.Server.Infrastructure.Cache;
using MILANO.DistributedCache.Server.Infrastructure.Options;
using MILANO.DistributedCache.Server.Infrastructure.Security;
using MILANO.DistributedCache.Server.Web.Filters;

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

			// Core services
			services.AddSingleton<ICacheService, InMemoryCacheService>();
			services.AddScoped<IApiKeyStore, FileApiKeyStore>();
			services.AddScoped<IApiKeyValidator, ApiKeyValidator>();

			// Controllers and filters
			services.AddControllers(options =>
			{
				// Optional: add global filters like exception handling
				options.Filters.Add<ApiExceptionFilter>();
			});

			return services;
		}
	}
}
