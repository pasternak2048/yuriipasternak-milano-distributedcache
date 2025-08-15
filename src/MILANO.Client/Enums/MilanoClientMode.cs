namespace MILANO.Client.Enums
{
	/// <summary>
	/// Specifies the communication mode for the MILANO client.
	/// </summary>
	public enum MilanoClientMode
	{
		/// <summary>
		/// Use standard REST over HTTP.
		/// </summary>
		Http,

		/// <summary>
		/// Use gRPC for communication.
		/// </summary>
		Grpc
	}
}
