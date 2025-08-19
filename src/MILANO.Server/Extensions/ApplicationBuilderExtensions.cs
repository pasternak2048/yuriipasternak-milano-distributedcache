using MILANO.Server.Web.Middleware;

namespace MILANO.Server.Extensions
{
	public static class ApplicationBuilderExtensions
	{
		public static void ConfigureMilanoApp(this WebApplication app)
		{
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseMilanoMiddleware();
			app.MapMilanoEndpoints();
		}

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

		public static IApplicationBuilder UseMilanoMiddleware(this IApplicationBuilder app)
		{
			// Add per-endpoint API key validation
			app.UseApiKeyAuthFor("/cache", "GET", "get");
			app.UseApiKeyAuthFor("/cache", "POST", "set");

			return app;
		}
	}
}
