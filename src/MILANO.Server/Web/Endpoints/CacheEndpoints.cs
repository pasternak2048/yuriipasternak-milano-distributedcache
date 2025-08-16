using MILANO.Common.Dtos.Cache;
using MILANO.Server.Application.Cache;
using MILANO.Server.Web.Configuration;
using static MILANO.Common.Constants;

namespace MILANO.Server.Web.Endpoints
{
	/// <summary>
	/// Defines minimal API endpoints for cache operations.
	/// </summary>
	public static class CacheEndpoints
	{
		public static void MapCacheEndpoints(this IEndpointRouteBuilder app)
		{
			var group = app.MapGroup("/cache").WithTags("Cache");

			// GET /cache/{key}
			group.MapGet("/{key}", async (
				HttpContext context,
				string key,
				ICacheService cacheService) =>
			{
				if (!TryAuthorize(context, "get", out var error, out var apiKeyContext))
					return error;

				var request = new CacheGetRequest { Key = key };

				var result = await cacheService.GetAsync(request);
				if (!result.Found)
					return Results.NotFound();

				return Results.Text(result.Value, "text/plain");
			});

			// POST /cache
			group.MapPost("/", async (
				HttpContext context,
				CacheSetRequest request,
				ICacheService cacheService) =>
			{
				if (!TryAuthorize(context, "set", out var error, out var apiKeyContext))
					return error;

				await cacheService.SetAsync(request);
				return Results.NoContent();
			});
		}

		private static bool TryAuthorize(
			HttpContext context,
			string requiredPermission,
			out IResult? failureResult,
			out ApiKeyContext? apiKeyContext)
		{
			apiKeyContext = context.Items[Metadata.ApiKeyContextItem] as ApiKeyContext;

			if (apiKeyContext is null)
			{
				failureResult = Results.Unauthorized();
				return false;
			}

			if (!apiKeyContext.HasPermission(requiredPermission))
			{
				failureResult = Results.Forbid();
				return false;
			}

			failureResult = null;
			return true;
		}
	}
}
