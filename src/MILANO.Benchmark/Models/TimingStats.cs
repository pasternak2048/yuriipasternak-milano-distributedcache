namespace MILANO.Benchmark.Models
{
	/// <summary>
	/// Represents calculated latency statistics for a benchmark run.
	/// </summary>
	public sealed record TimingStats(
		double AvgMs,
		double MaxMs,
		double MinMs,
		double MedianMs,
		double P99Ms,
		int MoreThan100ms,
		int MoreThan50ms,
		int MoreThan10ms,
		int MoreThan1ms
	);
}
