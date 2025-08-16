using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MILANO.Client.Configuration;
using MILANO.Client.Interfaces;
using MILANO.Client.Services;
using MILANO.Common;

namespace MILANO.Client.Extensions
{
	/// <summary>
	/// Provides extension methods to register the MILANO cache client in the DI container.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		private const string HttpClientName = "MilanoHttp";

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
			return services.AddMilanoCacheClientCore();
		}

		/// <summary>
		/// Adds and configures the MILANO cache client using manual delegate configuration.
		/// </summary>
		public static IServiceCollection AddMilanoCacheClient(
			this IServiceCollection services,
			Action<MilanoClientOptions> configure)
		{
			services.Configure(configure);
			return services.AddMilanoCacheClientCore();
		}

		/// <summary>
		/// Adds the MILANO cache client and configures HTTP client with base address, API key and timeout.
		/// </summary>
		private static IServiceCollection AddMilanoCacheClientCore(this IServiceCollection services)
		{
			services.AddHttpClient<IMilanoCacheClient, HttpMilanoCacheClient>(HttpClientName, (provider, client) =>
			{
				var options = provider.GetRequiredService<IOptions<MilanoClientOptions>>().Value;
				client.BaseAddress = new Uri(options.ServerHost);
				client.Timeout = options.Timeout;
				client.DefaultRequestHeaders.Add(Constants.Headers.ApiKey, options.ApiKey);
			});

			return services;
		}
	}
}
