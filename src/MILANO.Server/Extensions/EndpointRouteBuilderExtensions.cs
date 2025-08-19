using MILANO.Server.Web.Endpoints;

namespace MILANO.Server.Extensions
{
	public static class EndpointRouteBuilderExtensions
	{
		public static void MapMilanoEndpoints(this IEndpointRouteBuilder endpoints)
		{
			endpoints.MapCacheEndpoints();

			endpoints.MapGet("/health", () => Results.Ok(new
			{
				status = "Healthy",
				timestamp = DateTime.UtcNow
			}))
			.WithName("HealthCheck")
			.WithTags("System");

			endpoints.MapGet("/version", () => Results.Ok("MILANO.DistributedCache v0.1"))
				.WithName("Version")
				.WithTags("System");
		}
	}
}
