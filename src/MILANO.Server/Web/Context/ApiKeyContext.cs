using MILANO.Server.Application.Security.Models;

namespace MILANO.Server.Web.Context
{
	/// <summary>
	/// Provides access to the currently authorized API key and its permissions.
	/// </summary>
	public sealed class ApiKeyContext
	{
		/// <summary>
		/// Gets the validated API key definition.
		/// </summary>
		public ApiKeyDefinition ApiKey { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ApiKeyContext"/> class.
		/// </summary>
		/// <param name="apiKey">The validated API key definition.</param>
		public ApiKeyContext(ApiKeyDefinition apiKey)
		{
			ApiKey = apiKey;
		}

		/// <summary>
		/// Gets the raw key string.
		/// </summary>
		public string Key => ApiKey.Key;

		/// <summary>
		/// Gets the human-friendly label for the key (if provided).
		/// </summary>
		public string? Label => ApiKey.Label;

		/// <summary>
		/// Checks whether the current key has the specified permission.
		/// </summary>
		/// <param name="permission">The permission to check.</param>
		/// <returns>True if permission is granted.</returns>
		public bool HasPermission(string permission)
			=> ApiKey.HasPermission(permission);
	}
}
