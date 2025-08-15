using Microsoft.AspNetCore.Mvc;
using MILANO.DistributedCache.Shared.Dtos.Cache;
using MILANO.Server.Application.Cache;
using MILANO.Server.Application.Common;
using MILANO.Server.Web.Context;
using MILANO.Shared.Dtos.Cache;

namespace MILANO.Server.Web.Controllers
{
	/// <summary>
	/// Provides endpoints for interacting with the distributed cache.
	/// </summary>
	[ApiController]
	[Route("cache")]
	public sealed class CacheController : ControllerBase
	{
		private readonly ICacheService _cacheService;

		public CacheController(ICacheService cacheService)
		{
			_cacheService = cacheService;
		}

		/// <summary>
		/// Gets a value from the cache by key.
		/// </summary>
		[HttpGet("{key}")]
		public async Task<IActionResult> GetAsync(string key)
		{
			var apiKeyContext = HttpContext.Items[Constants.Metadata.ApiKeyContextItem] as ApiKeyContext;

			if (apiKeyContext is null || !apiKeyContext.HasPermission("get"))
				return Forbid("API key does not have 'get' permission.");

			var request = new CacheGetRequest
			{
				Key = key
			};

			var result = await _cacheService.GetAsync(request);

			if (!result.Found)
				return NotFound();

			return Ok(result.Value);
		}

		/// <summary>
		/// Sets a cache value for a given key.
		/// </summary>
		[HttpPost]
		public async Task<IActionResult> SetAsync([FromBody] CacheSetRequest request)
		{
			var apiKeyContext = HttpContext.Items[Constants.Metadata.ApiKeyContextItem] as ApiKeyContext;

			if (apiKeyContext is null || !apiKeyContext.HasPermission("set"))
				return Forbid("API key does not have 'set' permission.");

			await _cacheService.SetAsync(request);

			return NoContent();
		}
	}
}
