namespace MILANO.Server.Infrastructure.Options
{
	/// <summary>
	/// Options for configuring the API key store file path.
	/// </summary>
	public sealed class ApiKeyStoreOptions
	{
		/// <summary>
		/// Gets or sets the path to the JSON file containing API keys.
		/// </summary>
		public string? FilePath { get; set; }
	}
}
