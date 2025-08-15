namespace MILANO.Server.Application.Security.Models
{
	/// <summary>
	/// Represents an API key and its associated permissions.
	/// </summary>
	public sealed class ApiKeyDefinition
	{
		/// <summary>
		/// Gets or sets the raw API key string used by clients.
		/// </summary>
		public string Key { get; init; } = default!;

		/// <summary>
		/// Gets or sets a human-readable name or label for this key (for admin purposes).
		/// </summary>
		public string? Label { get; init; }

		/// <summary>
		/// Gets or sets the set of permissions granted to this API key (e.g., "get", "set").
		/// </summary>
		public HashSet<string> Permissions { get; init; } = new();

		/// <summary>
		/// Checks if the API key has a given permission (case-insensitive).
		/// </summary>
		/// <param name="permission">The permission to check (e.g. "get").</param>
		/// <returns>True if the key has the permission; otherwise, false.</returns>
		public bool HasPermission(string permission)
			=> Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
	}
}
