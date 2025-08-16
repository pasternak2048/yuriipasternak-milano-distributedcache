namespace MILANO.Benchmark.Configuration
{
	/// <summary>
	/// Represents configuration options for a single benchmark session.
	/// </summary>
	public class BenchmarkOptions
	{
		/// <summary>
		/// Total number of requests per run.
		/// </summary>
		public int TotalRequests { get; init; } = 120_000;

		/// <summary>
		/// Number of concurrent threads (parallel tasks).
		/// </summary>
		public int Concurrency { get; init; } = 12;

		/// <summary>
		/// Number of full runs (iterations).
		/// </summary>
		public int NumberOfRuns { get; init; } = 2;

		/// <summary>
		/// Whether to show progress in the console.
		/// </summary>
		public bool ShowProgress { get; init; } = true;
	}
}
