namespace MILANO.Server.Web.Extensions
{
	/// <summary>
	/// Extension methods for configuring DI services.
	/// </summary>
	public static class WebApplicationBuilderExtensions
	{
		public static void ConfigureMilanoServices(this WebApplicationBuilder builder)
		{
			builder.Services.AddMilanoDistributedCache(builder.Configuration);
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();
		}
	}
}
