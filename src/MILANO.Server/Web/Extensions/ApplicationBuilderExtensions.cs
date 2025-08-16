using MILANO.Server.Web.Middleware;

namespace MILANO.Server.Web.Extensions
{
	/// <summary>
	/// Provides extension methods for configuring the application middleware pipeline.
	/// </summary>
	public static class ApplicationBuilderExtensions
	{
		/// <summary>
		/// Adds API key authorization middleware to specific path + method.
		/// </summary>
		public static IApplicationBuilder UseApiKeyAuthFor(
			this IApplicationBuilder app,
			PathString path,
			string httpMethod,
			string requiredPermission)
		{
			if (string.IsNullOrWhiteSpace(httpMethod))
				throw new ArgumentException("HTTP method cannot be null or empty.", nameof(httpMethod));

			if (string.IsNullOrWhiteSpace(requiredPermission))
				throw new ArgumentException("Required permission cannot be null or empty.", nameof(requiredPermission));

			app.UseWhen(
				context =>
					context.Request.Path.StartsWithSegments(path, StringComparison.OrdinalIgnoreCase) &&
					context.Request.Method.Equals(httpMethod, StringComparison.OrdinalIgnoreCase),
				appBuilder => appBuilder.UseMiddleware<ApiKeyAuthMiddleware>(requiredPermission));

			return app;
		}

		/// <summary>
		/// Adds all required middleware for the MILANO distributed cache service.
		/// </summary>
		public static IApplicationBuilder UseMilanoMiddleware(this IApplicationBuilder app)
		{
			// Optional: Add global exception middleware here in the future
			// app.UseMiddleware<ApiExceptionMiddleware>();

			// Add per-endpoint API key validation
			app.UseApiKeyAuthFor("/cache", "GET", "get");
			app.UseApiKeyAuthFor("/cache", "POST", "set");

			return app;
		}
	}
}
