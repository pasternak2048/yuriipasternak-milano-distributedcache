using MILANO.Server.Application.Security.Models;

namespace MILANO.Server.Application.Security
{
	/// <summary>
	/// Provides functionality to validate API keys and their permissions.
	/// </summary>
	public interface IApiKeyValidator
	{
		/// <summary>
		/// Validates the given API key and ensures it has the required permission.
		/// </summary>
		/// <param name="rawKey">The raw API key provided by the client.</param>
		/// <param name="requiredPermission">The permission required (e.g., "get", "set").</param>
		/// <returns>The validated <see cref="ApiKeyDefinition"/> if valid; otherwise, null.</returns>
		Task<ApiKeyDefinition?> ValidateAsync(string rawKey, string requiredPermission);
	}
}
