using MILANO.Server.Application.Security;
using MILANO.Server.Application.Security.Models;

namespace MILANO.Server.Infrastructure.Security
{
	/// <summary>
	/// Default implementation of <see cref="IApiKeyValidator"/> that checks existence and permissions of API keys.
	/// </summary>
	public sealed class ApiKeyValidator : IApiKeyValidator
	{
		private readonly IApiKeyStore _keyStore;
		private readonly ILogger<ApiKeyValidator> _logger;

		public ApiKeyValidator(IApiKeyStore keyStore, ILogger<ApiKeyValidator> logger)
		{
			_keyStore = keyStore;
			_logger = logger;
		}

		/// <inheritdoc />
		public async Task<ApiKeyDefinition?> ValidateAsync(string rawKey, string requiredPermission)
		{
			if (string.IsNullOrWhiteSpace(rawKey))
			{
				_logger.LogWarning("API key was not provided.");
				return null;
			}

			var key = await _keyStore.FindByKeyAsync(rawKey);

			if (key is null)
			{
				_logger.LogWarning("API key not found: {Key}", rawKey);
				return null;
			}

			if (!key.HasPermission(requiredPermission))
			{
				_logger.LogWarning("API key '{Label}' does not have permission: {Permission}", key.Label ?? key.Key, requiredPermission);
				return null;
			}

			return key;
		}
	}
}
