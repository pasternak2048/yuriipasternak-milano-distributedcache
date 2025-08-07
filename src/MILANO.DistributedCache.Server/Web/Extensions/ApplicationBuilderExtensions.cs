using MILANO.DistributedCache.Server.Web.Middleware;

namespace MILANO.DistributedCache.Server.Web.Extensions
{
	/// <summary>
	/// Provides extension methods for configuring the application middleware pipeline.
	/// </summary>
	public static class ApplicationBuilderExtensions
	{
		/// <summary>
		/// Maps the API key authorization middleware to a specific path and method.
		/// </summary>
		/// <param name="app">The application builder.</param>
		/// <param name="path">The request path to match (e.g. "/cache").</param>
		/// <param name="httpMethod">The HTTP method to match (e.g. "get", "post").</param>
		/// <param name="requiredPermission">The required permission for the route.</param>
		/// <returns>The same application builder for chaining.</returns>
		public static IApplicationBuilder UseApiKeyAuthFor(
			this IApplicationBuilder app,
			PathString path,
			string httpMethod,
			string requiredPermission)
		{
			app.UseWhen(
				context =>
					context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase) &&
					context.Request.Method.Equals(httpMethod, StringComparison.OrdinalIgnoreCase),
				appBuilder => appBuilder.UseMiddleware<ApiKeyAuthMiddleware>(requiredPermission));

			return app;
		}

		/// <summary>
		/// Adds all default middleware for the MILANO distributed cache service.
		/// </summary>
		/// <param name="app">The application builder.</param>
		/// <returns>The same application builder.</returns>
		public static IApplicationBuilder UseMilanoMiddleware(this IApplicationBuilder app)
		{
			// Example: global exception handling (if needed)
			// app.UseMiddleware<ApiExceptionMiddleware>();

			// API key auth per route
			app.UseApiKeyAuthFor("/cache", "GET", "get");
			app.UseApiKeyAuthFor("/cache", "POST", "set");

			return app;
		}
	}
}
