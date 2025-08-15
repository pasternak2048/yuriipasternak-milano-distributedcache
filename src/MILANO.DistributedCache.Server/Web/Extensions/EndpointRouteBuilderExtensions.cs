namespace MILANO.DistributedCache.Server.Web.Extensions
{
	/// <summary>
	/// Provides extension methods for mapping endpoints.
	/// </summary>
	public static class EndpointRouteBuilderExtensions
	{
		/// <summary>
		/// Maps all endpoints for the MILANO distributed cache service.
		/// </summary>
		/// <param name="endpoints">The endpoint route builder.</param>
		public static void MapMilanoEndpoints(this IEndpointRouteBuilder endpoints)
		{
			endpoints.MapControllers();

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
