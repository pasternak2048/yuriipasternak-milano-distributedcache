using MILANO.Server.Application.Security;
using MILANO.Server.Web.Context;
using MILANO.Shared;

namespace MILANO.Server.Web.Middleware
{
	/// <summary>
	/// Middleware that validates the incoming API key and its permissions before allowing access to protected endpoints.
	/// </summary>
	public sealed class ApiKeyAuthMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IServiceProvider _provider;
		private readonly ILogger<ApiKeyAuthMiddleware> _logger;
		private readonly string _requiredPermission;

		/// <summary>
		/// Initializes a new instance of the <see cref="ApiKeyAuthMiddleware"/> class.
		/// </summary>
		/// <param name="next">The next middleware delegate.</param>
		/// <param name="provider">The root service provider (used to create a scoped validator).</param>
		/// <param name="logger">Logger instance.</param>
		/// <param name="requiredPermission">The permission required to access this route (e.g. "get", "set").</param>
		public ApiKeyAuthMiddleware(
			RequestDelegate next,
			IServiceProvider provider,
			ILogger<ApiKeyAuthMiddleware> logger,
			string requiredPermission)
		{
			_next = next;
			_provider = provider;
			_logger = logger;
			_requiredPermission = requiredPermission;
		}

		/// <summary>
		/// Executes the API key validation logic for the incoming request.
		/// </summary>
		public async Task InvokeAsync(HttpContext context)
		{
			var rawKey = context.Request.Headers[Constants.Headers.ApiKey].FirstOrDefault();

			if (string.IsNullOrWhiteSpace(rawKey))
			{
				_logger.LogWarning("Missing API key header.");
				await WriteErrorAsync(context, StatusCodes.Status401Unauthorized, "Missing API key.");
				return;
			}

			using var scope = _provider.CreateScope();
			var validator = scope.ServiceProvider.GetRequiredService<IApiKeyValidator>();

			var result = await validator.ValidateAsync(rawKey, _requiredPermission);
			if (result is null)
			{
				_logger.LogWarning("Invalid API key or insufficient permission: {Permission}", _requiredPermission);
				await WriteErrorAsync(context, StatusCodes.Status403Forbidden, "Forbidden: Invalid API key or insufficient permission.");
				return;
			}

			context.Items[Constants.Metadata.ApiKeyContextItem] = new ApiKeyContext(result);

			await _next(context);
		}

		private static async Task WriteErrorAsync(HttpContext context, int statusCode, string message)
		{
			context.Response.StatusCode = statusCode;
			context.Response.ContentType = "text/plain";
			await context.Response.WriteAsync(message);
		}
	}
}
