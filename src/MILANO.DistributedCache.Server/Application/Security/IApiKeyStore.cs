using MILANO.DistributedCache.Server.Application.Security.Models;

namespace MILANO.DistributedCache.Server.Application.Security
{
	/// <summary>
	/// Defines a store for retrieving API keys and their permissions.
	/// </summary>
	public interface IApiKeyStore
	{
		/// <summary>
		/// Gets all configured API keys.
		/// </summary>
		/// <returns>A collection of API keys with associated permissions.</returns>
		Task<IReadOnlyCollection<ApiKeyDefinition>> GetAllAsync();

		/// <summary>
		/// Finds a specific API key by its raw string value.
		/// </summary>
		/// <param name="rawKey">The raw API key string (as provided by the client).</param>
		/// <returns>The matching API key definition, or null if not found.</returns>
		Task<ApiKeyDefinition?> FindByKeyAsync(string rawKey);
	}
}
