using MILANO.Client.Enums;

namespace MILANO.Client.Configuration
{
	/// <summary>
	/// Configuration options for MILANO.Client.
	/// Defines the communication mode, host, API key, and optional timeout.
	/// </summary>
	public class MilanoClientOptions
	{
		/// <summary>
		/// The full base URL of the MILANO.Server (e.g., https://milano.company.com).
		/// </summary>
		public string ServerHost { get; set; } = default!;

		/// <summary>
		/// The secret API key to include in outgoing requests.
		/// This is used for simple API-level authorization.
		/// </summary>
		public string ApiKey { get; set; } = default!;

		/// <summary>
		/// Communication mode: either HTTP (via REST).
		/// </summary>
		public MilanoClientMode Mode { get; set; } = MilanoClientMode.Http;

		/// <summary>
		/// Optional request timeout. Default is 10 seconds.
		/// </summary>
		public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(10);
	}
}
