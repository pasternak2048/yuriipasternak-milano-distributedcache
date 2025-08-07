using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MILANO.DistributedCache.Server.Application.Cache.Exceptions;

namespace MILANO.DistributedCache.Server.Web.Filters
{
	/// <summary>
	/// Global filter that catches unhandled exceptions and converts them to HTTP responses.
	/// </summary>
	public sealed class ApiExceptionFilter : IExceptionFilter
	{
		private readonly ILogger<ApiExceptionFilter> _logger;

		public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
		{
			_logger = logger;
		}

		/// <inheritdoc />
		public void OnException(ExceptionContext context)
		{
			var ex = context.Exception;

			_logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);

			context.Result = ex switch
			{
				CacheEntryTooLargeException tooLarge => new ObjectResult(new
				{
					error = "Cache entry too large",
					actual = tooLarge.ActualSizeBytes,
					limit = tooLarge.MaxAllowedSizeBytes
				})
				{
					StatusCode = StatusCodes.Status413PayloadTooLarge
				},

				CacheKeyConflictException conflict => new ObjectResult(new
				{
					error = "Key conflict",
					key = conflict.Key
				})
				{
					StatusCode = StatusCodes.Status409Conflict
				},

				ArgumentException arg => new BadRequestObjectResult(new
				{
					error = "Bad request",
					message = arg.Message
				}),

				_ => new ObjectResult(new
				{
					error = "Internal server error",
					message = "An unexpected error occurred."
				})
				{
					StatusCode = StatusCodes.Status500InternalServerError
				}
			};

			context.ExceptionHandled = true;
		}
	}
}
