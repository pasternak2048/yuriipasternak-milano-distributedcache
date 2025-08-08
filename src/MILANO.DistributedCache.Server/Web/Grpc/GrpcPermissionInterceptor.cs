using Grpc.Core;
using Grpc.Core.Interceptors;
using MILANO.DistributedCache.Server.Application.Security;

namespace MILANO.DistributedCache.Server.Web.Grpc
{
	/// <summary>
	/// Intercepts incoming gRPC requests and validates API keys against required permissions.
	/// This ensures that only clients with valid keys and proper access rights can call specific gRPC methods.
	/// </summary>
	public sealed class GrpcPermissionInterceptor : Interceptor
	{
		private readonly IApiKeyValidator _validator;
		private readonly ILogger<GrpcPermissionInterceptor> _logger;

		/// <summary>
		/// Initializes a new instance of the <see cref="GrpcPermissionInterceptor"/> class.
		/// </summary>
		/// <param name="validator">Service that validates API keys and permissions.</param>
		/// <param name="logger">Logger instance for diagnostic output.</param>
		public GrpcPermissionInterceptor(IApiKeyValidator validator, ILogger<GrpcPermissionInterceptor> logger)
		{
			_validator = validator;
			_logger = logger;
		}

		/// <summary>
		/// Intercepts unary gRPC calls and applies API key validation.
		/// </summary>
		public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
			TRequest request,
			ServerCallContext context,
			UnaryServerMethod<TRequest, TResponse> continuation)
		{
			var method = context.Method; // Example: "/Cache.CacheService/Get"
			var apiKey = context.RequestHeaders.GetValue("x-api-key");

			if (string.IsNullOrWhiteSpace(apiKey))
			{
				_logger.LogWarning("Missing API key for gRPC method {Method}", method);
				throw new RpcException(new Status(StatusCode.Unauthenticated, "Missing API key"));
			}

			// Define permission dynamically based on method name
			var requiredPermission = ExtractPermissionFromMethod(method);

			var result = await _validator.ValidateAsync(apiKey, requiredPermission);

			if (result is null)
			{
				_logger.LogWarning("Access denied for API key on method {Method} with permission '{Permission}'", method, requiredPermission);
				throw new RpcException(new Status(StatusCode.PermissionDenied, "Invalid API key or insufficient permissions"));
			}

			// All good — continue to the handler
			return await continuation(request, context);
		}

		/// <summary>
		/// Extracts a simplified permission string from the gRPC method path.
		/// </summary>
		private static string ExtractPermissionFromMethod(string methodPath)
		{
			// Example: "/Cache.CacheService/Get" → "get"
			return methodPath.Split('/').LastOrDefault()?.ToLowerInvariant() ?? "unknown";
		}
	}
}
