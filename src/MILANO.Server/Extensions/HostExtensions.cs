namespace MILANO.Server.Extensions
{
	public static class HostExtensions
	{
		public static void ConfigureMilanoHost(this WebApplicationBuilder builder)
		{
			builder.WebHost.ConfigureKestrel(options =>
			{
				options.Limits.MaxConcurrentConnections = 100_000;
				options.Limits.MaxConcurrentUpgradedConnections = 10_000;
				options.Limits.MaxRequestBodySize = 1_048_576; // 1 MB
				options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(30);
				options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);
				options.AddServerHeader = false;
			});
		}
	}
}
