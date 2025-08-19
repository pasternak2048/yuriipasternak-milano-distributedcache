namespace MILANO.Server.Extensions
{
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
