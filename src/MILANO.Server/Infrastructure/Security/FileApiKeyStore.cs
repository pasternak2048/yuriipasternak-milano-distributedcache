using Microsoft.Extensions.Options;
using MILANO.Server.Application.Security;
using MILANO.Server.Application.Security.Models;
using MILANO.Server.Infrastructure.Options;
using System.Text.Json;

namespace MILANO.Server.Infrastructure.Security
{
	/// <summary>
	/// File-based implementation of <see cref="IApiKeyStore"/> that loads keys from a JSON file.
	/// </summary>
	public sealed class FileApiKeyStore : IApiKeyStore
	{
		private readonly string _filePath;
		private readonly ILogger<FileApiKeyStore> _logger;

		public FileApiKeyStore(IOptions<ApiKeyStoreOptions> options, ILogger<FileApiKeyStore> logger)
		{
			_filePath = options.Value.FilePath ?? throw new ArgumentNullException(nameof(options.Value.FilePath));
			_logger = logger;
		}

		public async Task<IReadOnlyCollection<ApiKeyDefinition>> GetAllAsync()
		{
			try
			{
				if (!File.Exists(_filePath))
				{
					_logger.LogWarning("API key file not found at {FilePath}", _filePath);
					return Array.Empty<ApiKeyDefinition>();
				}

				await using var stream = File.OpenRead(_filePath);
				var result = await JsonSerializer.DeserializeAsync<List<ApiKeyDefinition>>(stream, new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true
				});

				return result?.Where(k => !string.IsNullOrWhiteSpace(k.Key)).ToList()
					   ?? new List<ApiKeyDefinition>();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to read API keys from file.");
				return Array.Empty<ApiKeyDefinition>();
			}
		}

		public async Task<ApiKeyDefinition?> FindByKeyAsync(string rawKey)
		{
			var all = await GetAllAsync();
			return all.FirstOrDefault(k => k.Key.Equals(rawKey, StringComparison.Ordinal));
		}
	}
}
