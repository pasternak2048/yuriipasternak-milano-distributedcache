using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MILANO.Client.Configuration;
using MILANO.Client.Interfaces;
using MILANO.Client.Services;

namespace MILANO.Client.Extensions
{
	/// <summary>
	/// Provides extension methods to register the MILANO cache client in the DI container.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds and configures the MILANO cache client using strongly typed options from configuration.
		/// Example: configuration.GetSection("MilanoClient") → MilanoClientOptions
		/// </summary>
		public static IServiceCollection AddMilanoCacheClient(
		this IServiceCollection services,
		IConfiguration configuration,
		string sectionName = "MilanoClient")
		{
			services.Configure<MilanoClientOptions>(configuration.GetSection(sectionName));

			return services.AddMilanoCacheClient();
		}

		/// <summary>
		/// Adds and configures the MILANO cache client using manual delegate configuration.
		/// </summary>
		public static IServiceCollection AddMilanoCacheClient(
			this IServiceCollection services,
			Action<MilanoClientOptions> configure)
		{
			services.Configure(configure);
			return services.AddMilanoCacheClient();
		}

		/// <summary>
		/// Adds the MILANO cache client factory and client implementation.
		/// </summary>
		private static IServiceCollection AddMilanoCacheClient(this IServiceCollection services)
		{
			services.AddHttpClient("MilanoHttp");

			services.AddSingleton<MilanoCacheClientFactory>();
			services.AddSingleton<IMilanoCacheClient>(sp =>
			{
				var factory = sp.GetRequiredService<MilanoCacheClientFactory>();
				return factory.Create();
			});

			return services;
		}
	}
}
