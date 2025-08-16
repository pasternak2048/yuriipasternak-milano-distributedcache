namespace MILANO.Benchmark.Models
{
	/// <summary>
	/// Represents the result of a single benchmark run,
	/// including latency statistics, error counts, and throughput.
	/// </summary>
	public sealed record RunResult(
		int Index,
		TimeSpan Duration,
		double AvgMs,
		double MaxMs,
		double MinMs,
		double MedianMs,
		double P99Ms,
		int MoreThan100ms,
		int MoreThan50ms,
		int MoreThan10ms,
		int MoreThan1ms,
		double Throughput,
		int ErrorCount
	);
}
