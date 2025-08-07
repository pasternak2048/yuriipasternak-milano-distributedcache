namespace MILANO.DistributedCache.Server.Web.Configuration
{
	/// <summary>
	/// Represents the raw API key provided by the client via HTTP headers.
	/// </summary>
	public sealed class ApiKey
	{
		/// <summary>
		/// Gets the raw value of the API key.
		/// </summary>
		public string? Value { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="ApiKey"/> class.
		/// </summary>
		/// <param name="value">The raw API key string.</param>
		public ApiKey(string? value)
		{
			Value = value;
		}

		/// <summary>
		/// Indicates whether the API key is present and non-empty.
		/// </summary>
		public bool IsPresent => !string.IsNullOrWhiteSpace(Value);
	}
}
