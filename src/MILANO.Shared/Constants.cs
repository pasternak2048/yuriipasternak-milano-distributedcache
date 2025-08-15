namespace MILANO.Shared
{
	/// <summary>
	/// Provides global constants used throughout the application.
	/// </summary>
	public static class Constants
	{
		/// <summary>
		/// HTTP header names used by the API.
		/// </summary>
		public static class Headers
		{
			public const string ApiKey = "X-Milano-ApiKey";
		}

		/// <summary>
		/// Cache-related limits and defaults.
		/// </summary>
		public static class Limits
		{
			/// <summary>
			/// The default maximum allowed payload size in bytes.
			/// </summary>
			public const int MaxPayloadSizeBytes = 1_000_000;
		}

		/// <summary>
		/// Common error codes or identifiers.
		/// </summary>
		public static class Errors
		{
			public const string InvalidApiKey = "error.invalid_api_key";
			public const string EntryTooLarge = "error.entry_too_large";
			public const string KeyConflict = "error.key_conflict";
		}

		/// <summary>
		/// Keys used for internal request metadata (e.g., in middleware).
		/// </summary>
		public static class Metadata
		{
			public const string ApiKeyContextItem = "MILANO:ApiKeyContext";
			public const string CacheApiKey = "apikeys_cache";
		}
	}
}
