using MILANO.Benchmark.Models;

namespace MILANO.Benchmark.Benchmark
{
	/// <summary>
	/// Provides helper methods for calculating timing statistics from latency measurements.
	/// </summary>
	public static class TimingStatsHelper
	{
		public static TimingStats Calculate(double[] timings)
		{
			if (timings == null || timings.Length == 0)
			{
				return new TimingStats(0, 0, 0, 0, 0, 0, 0, 0, 0);
			}

			var ordered = timings.OrderBy(x => x).ToArray();

			double avg = ordered.Average();
			double max = ordered.Max();
			double min = ordered.Min();
			double median = ordered[ordered.Length / 2];
			double p99 = ordered[(int)(ordered.Length * 0.99)];
			int moreThan100ms = ordered.Count(x => x > 100);
			int moreThan50ms = ordered.Count(x => x > 50);
			int moreThan10ms = ordered.Count(x => x > 10);
			int moreThan1ms = ordered.Count(x => x > 1);

			return new TimingStats(avg, max, min, median, p99, moreThan100ms, moreThan50ms, moreThan10ms, moreThan1ms);
		}
	}
}
