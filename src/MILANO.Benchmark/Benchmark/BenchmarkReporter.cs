using MILANO.Benchmark.Common;
using MILANO.Benchmark.Models;

namespace MILANO.Benchmark.Benchmark;

/// <summary>
/// Responsible for printing benchmark results and summaries.
/// </summary>
public static class BenchmarkReporter
{
	public static void PrintReportHeader()
	{
		ConsoleExtensions.PrintBoxed("🚀 MILANO Load Test Report");
	}

	public static void PrintBenchmarkStart(int index)
	{
		ConsoleExtensions.PrintBoxed($"🏁 BENCHMARK #{index} STARTED");
	}

	public static void PrintBenchmarkEnd(int index, TimeSpan duration)
	{
		ConsoleExtensions.PrintBoxedLines([
			$"✅ BENCHMARK SESSION #{index} COMPLETE",
			$"🕒 Finished at {DateTime.Now:HH:mm:ss} | Duration: {duration.TotalSeconds:F2}s"
		]);
	}

	/// <summary>
	/// Enhanced report using raw values (original signature).
	/// </summary>
	public static void PrintEnhancedReport(
		TimeSpan totalTime,
		int totalRequests,
		int concurrency,
		double avgMs,
		double maxMs,
		double minMs,
		double median,
		double p99,
		int moreThan100ms,
		int moreThan50ms,
		int moreThan10ms,
		int moreThan1ms,
		double rps,
		int errorCount,
		IReadOnlyCollection<string> errorDetails,
		int totalTimings)
	{
		Console.WriteLine($"\n🔢 Total Requests:     {totalRequests:N0}");
		Console.WriteLine($"🧵 Concurrency Level:  {concurrency}");
		Console.WriteLine($"⏱  Total Time:         {totalTime.Hours}h {totalTime.Minutes}m {totalTime.Seconds}s {totalTime.Milliseconds}ms\n");

		Console.WriteLine("📊 Time per Request:");
		Console.WriteLine("   Avg: {0,10:F5} ms  | {1,10:F2} μs  | {2,13:N0} ns", avgMs, avgMs * 1000, avgMs * 1_000_000);
		Console.WriteLine("   Max: {0,10:F5} ms  | {1,10:F2} μs  | {2,13:N0} ns", maxMs, maxMs * 1000, maxMs * 1_000_000);
		Console.WriteLine("   Min: {0,10:F5} ms  | {1,10:F2} μs  | {2,13:N0} ns", minMs, minMs * 1000, minMs * 1_000_000);
		Console.WriteLine();

		Console.WriteLine($"📉 Median Time:        {median:F3} ms");
		Console.WriteLine($"💎 99th Percentile:    {p99:F3} ms");

		if (totalTimings > 0)
		{
			SetColorByThreshold(moreThan100ms); Console.WriteLine($"🐢 > 100ms requests:  {moreThan100ms} ({(moreThan100ms * 100.0 / totalTimings):F2}%)");
			SetColorByThreshold(moreThan50ms); Console.WriteLine($"🐢 > 50ms  requests:  {moreThan50ms} ({(moreThan50ms * 100.0 / totalTimings):F2}%)");
			SetColorByThreshold(moreThan10ms); Console.WriteLine($"🐢 > 10ms  requests:  {moreThan10ms} ({(moreThan10ms * 100.0 / totalTimings):F2}%)");
			SetColorByThreshold(moreThan1ms); Console.WriteLine($"🐢 > 1ms   requests:  {moreThan1ms} ({(moreThan1ms * 100.0 / totalTimings):F2}%)");
		}
		else
		{
			Console.WriteLine("No timings recorded.");
		}

		Console.ResetColor();
		Console.ForegroundColor = ConsoleColor.Green;
		Console.WriteLine($"📈 Throughput:         {rps:N0} req/sec");
		Console.ResetColor();

		Console.ForegroundColor = errorCount > 0 ? ConsoleColor.Red : ConsoleColor.Gray;
		Console.WriteLine($"\n❌ Errors Detected:    {errorCount}");
		Console.ResetColor();

		if (errorDetails.Count > 0)
		{
			Console.WriteLine("\n=== Error Details (first 20) ===");
			foreach (var err in errorDetails.Take(20))
				Console.WriteLine("   • " + err);
			if (errorDetails.Count > 20)
				Console.WriteLine($"...and {errorDetails.Count - 20} more.");
		}
	}

	/// <summary>
	/// Overload that accepts RunResult + ErrorDetails.
	/// </summary>
	public static void PrintEnhancedReport(
		RunResult result,
		int totalRequests,
		int concurrency,
		IReadOnlyCollection<string> errorDetails,
		int totalTimings)
	{
		PrintEnhancedReport(
			result.Duration,
			totalRequests,
			concurrency,
			result.AvgMs,
			result.MaxMs,
			result.MinMs,
			result.MedianMs,
			result.P99Ms,
			result.MoreThan100ms,
			result.MoreThan50ms,
			result.MoreThan10ms,
			result.MoreThan1ms,
			result.Throughput,
			result.ErrorCount,
			errorDetails,
			totalTimings
		);
	}

	public static void PrintSummaryTable(List<RunResult> results)
	{
		Console.WriteLine("\n════════════════════ 📊 Summary Table 📊 ════════════════════════════════════════════════════════════════════════════");
		Console.WriteLine("│ Run │   Time   │ Avg (ms) │ Max (ms) │ Min (ms) │ P99 (ms) │ >100ms │ >50ms  │ >10ms  │ >1ms   │ Errors │   RPS   │");
		Console.WriteLine("├─────┼──────────┼──────────┼──────────┼──────────┼──────────┼────────┼────────┼────────┼────────┼────────┼─────────┤");

		foreach (var r in results)
		{
			Console.WriteLine($"│ #{r.Index,2} │ {r.Duration.TotalSeconds,7:F2}s │" +
							  $" {r.AvgMs,8:F1} │ {r.MaxMs,8:F1} │ {r.MinMs,8:F1} │ {r.P99Ms,8:F1} │" +
							  $" {r.MoreThan100ms,6} │ {r.MoreThan50ms,6} │ {r.MoreThan10ms,6} │ {r.MoreThan1ms,6} │ {r.ErrorCount,6} │ {r.Throughput,7:N0} │");
		}

		Console.WriteLine("═════════════════════════════════════════════════════════════════════════════════════════════════════════════════════");
	}

	public static void PrintFinalStats(List<RunResult> results)
	{
		var best = results.OrderByDescending(r => r.Throughput).First();
		var fastest = results.OrderBy(r => r.Duration).First();
		var lowestAvg = results.OrderBy(r => r.AvgMs).First();

		Console.WriteLine("\n🏁 Best RPS:      {0:N0} (Run #{1})", best.Throughput, best.Index);
		Console.WriteLine("⏱  Fastest Run:  #{0} ({1:F2}s)", fastest.Index, fastest.Duration.TotalSeconds);
		Console.WriteLine("📉 Lowest Avg:    {0:F3} ms (Run #{1})", lowestAvg.AvgMs, lowestAvg.Index);
	}

	private static void SetColorByThreshold(int count)
	{
		if (count > 0)
			Console.ForegroundColor = ConsoleColor.Yellow;
		else
			Console.ForegroundColor = ConsoleColor.Green;
	}
}
