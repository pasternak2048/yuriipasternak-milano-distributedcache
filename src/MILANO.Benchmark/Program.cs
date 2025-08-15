using MILANO.Common;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;

const string apiKey = "test-key-full-access";
const string apiHeader = Constants.Headers.ApiKey;
const string address = "https://localhost:7011/cache/";

using var http = new HttpClient();
http.BaseAddress = new Uri(address);
http.DefaultRequestHeaders.Add(apiHeader, apiKey);
http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

Console.Write("🔢 Enter total number of requests per run: ");
int totalRequests = int.Parse(Console.ReadLine() ?? "120000");

Console.Write("🧵 Enter number of threads (concurrency): ");
int concurrency = int.Parse(Console.ReadLine() ?? "12");

Console.Write("🔁 Enter number of runs: ");
int runs = int.Parse(Console.ReadLine() ?? "2");

var results = new List<RunResult>();

PrintBoxed("🏋️ MILANO Benchmark Session Started");
for (int i = 1; i <= runs; i++)
{
	PrintBenchmarkStart(i);
	var result = await RunLoader(http, totalRequests, concurrency, i);
	results.Add(result);
	PrintBenchmarkEnd(i, result.Duration);
	Console.WriteLine();
	Console.WriteLine();
	Console.WriteLine();
	Console.WriteLine();
	Console.WriteLine();
	Console.WriteLine();

}

PrintSummaryTable(results);
PrintFinalStats(results);

// === MAIN LOADER ===

async Task<RunResult> RunLoader(HttpClient http, int totalRequests, int concurrency, int index)
{
	var stopwatch = Stopwatch.StartNew();
	var errorCount = 0;
	var timings = new ConcurrentBag<double>();
	var errorDetails = new ConcurrentBag<string>();

	int done = 0;
	void ReportProgress()
	{
		int current = Interlocked.Increment(ref done);
		if (current % 5000 == 0 || current == totalRequests)
		{
			double percent = current * 100.0 / totalRequests;
			Console.Write($"\r📦 Progress: {percent:F1}% ({current}/{totalRequests})     ");
		}
	}

	var tasks = new Task[concurrency];
	for (int i = 0; i < concurrency; i++)
	{
		int taskNum = i;
		tasks[i] = Task.Run(async () =>
		{
			int iterations = totalRequests / concurrency;
			for (int j = 0; j < iterations; j++)
			{
				var ticks = DateTime.UtcNow.Ticks;
				var key = $"key_{index}{taskNum}_{j}{ticks}";
				var value = $"value_{index}{taskNum}_{j}{ticks}";

				var setPayload = JsonSerializer.Serialize(new
				{
					Key = key,
					Value = value,
					ExpirationSeconds = 60
				});

				var setContent = new StringContent(setPayload, Encoding.UTF8, "application/json");

				try
				{
					var localSw = Stopwatch.StartNew();

					// SET
					var setResp = await http.PostAsync("", setContent);
					if (!setResp.IsSuccessStatusCode)
					{
						errorDetails.Add($"❌ SET FAIL [{key}]: {setResp.StatusCode}");
						Interlocked.Increment(ref errorCount);
						continue;
					}

					// GET
					var getResp = await http.GetAsync($"{Uri.EscapeDataString(key)}");
					if (!getResp.IsSuccessStatusCode)
					{
						errorDetails.Add($"❌ GET FAIL [{key}]: {getResp.StatusCode}");
						Interlocked.Increment(ref errorCount);
						continue;
					}

					var returnedValue = JsonSerializer.Deserialize<string>(await getResp.Content.ReadAsStringAsync());

					localSw.Stop();
					timings.Add(localSw.Elapsed.TotalMilliseconds);

					if (returnedValue != value)
					{
						errorDetails.Add($"❌ MISMATCH [{key}]: got={returnedValue}");
						Interlocked.Increment(ref errorCount);
					}
				}
				catch (Exception ex)
				{
					errorDetails.Add($"❌ EXCEPTION [{key}]: {ex.Message}");
					Interlocked.Increment(ref errorCount);
				}

				ReportProgress();
			}
		});
	}

	await Task.WhenAll(tasks);
	stopwatch.Stop();
	Console.WriteLine();

	// Aggregate Results
	var elapsed = stopwatch.Elapsed;
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
	double rps = totalRequests / elapsed.TotalSeconds;

	PrintReportHeader();
	Console.WriteLine();
	PrintEnhancedReport(
		elapsed, totalRequests, concurrency,
		avg, max, min, median, p99,
		moreThan100ms, moreThan50ms, moreThan10ms, moreThan1ms,
		rps, errorCount, errorDetails, ordered.Length
	);

	return new RunResult(index, elapsed, avg, max, min, median, p99,
		moreThan100ms, moreThan50ms, moreThan10ms, moreThan1ms,
		rps, errorCount);
}

// === OUTPUT HELPERS ===


void PrintBoxed(string text)
{
	const int width = 60;
	string border = new string('═', width);
	Console.WriteLine($"╔{border}╗");
	Console.WriteLine($"║{CenterText(text, width)}║");
	Console.WriteLine($"╚{border}╝");
}

void PrintBoxedLines(IEnumerable<string> lines)
{
	const int width = 60;
	string border = new string('═', width);
	Console.WriteLine($"╔{border}╗");
	foreach (var line in lines)
		Console.WriteLine($"║{CenterText(line, width)}║");
	Console.WriteLine($"╚{border}╝");
}

void PrintBenchmarkStart(int index) => PrintBoxed($"🏁 BENCHMARK #{index} STARTED");

void PrintBenchmarkEnd(int index, TimeSpan duration) =>
	PrintBoxedLines([
		$"✅ BENCHMARK SESSION #{index} COMPLETE",
		$"🕒 Finished at {DateTime.Now:HH:mm:ss} | Duration: {duration.TotalSeconds:F2}s"
	]);

void PrintReportHeader() => PrintBoxed("🚀 MILANO Load Test Report");

string CenterText(string text, int width)
{
	int visualLength = GetVisualLength(text);
	int padding = Math.Max(0, (width - visualLength) / 2);
	return new string(' ', padding) + text + new string(' ', width - padding - visualLength);
}

int GetVisualLength(string input)
{
	int width = 0;
	var enumerator = StringInfo.GetTextElementEnumerator(input);
	while (enumerator.MoveNext())
	{
		string element = enumerator.GetTextElement();
		char c = element[0];
		width += IsWideChar(c) ? 2 : 1;
	}
	return width;
}

bool IsWideChar(char c) => c >= 0x1100;

void PrintEnhancedReport(TimeSpan totalTime, int totalRequests, int concurrency, double avgMs, double maxMs, double minMs, double median, double p99, int moreThan100ms, int moreThan50ms, int moreThan10ms, int moreThan1ms, double rps, int errorCount, ConcurrentBag<string> errorDetails, int totalTimings)
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

	Console.ForegroundColor = moreThan100ms > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
	Console.ForegroundColor = moreThan50ms > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
	Console.ForegroundColor = moreThan10ms > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;
	Console.ForegroundColor = moreThan1ms > 0 ? ConsoleColor.Yellow : ConsoleColor.Green;

	if (totalTimings > 0)
	{
		Console.WriteLine($"🐢 > 100ms requests:  {moreThan100ms} ({(moreThan100ms * 100.0 / totalTimings):F2}%)");
		Console.WriteLine($"🐢 > 50ms  requests:  {moreThan50ms} ({(moreThan50ms * 100.0 / totalTimings):F2}%)");
		Console.WriteLine($"🐢 > 10ms  requests:  {moreThan10ms} ({(moreThan10ms * 100.0 / totalTimings):F2}%)");
		Console.WriteLine($"🐢 > 1ms   requests:  {moreThan1ms} ({(moreThan1ms * 100.0 / totalTimings):F2}%)");
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

	if (!errorDetails.IsEmpty)
	{
		Console.WriteLine("\n=== Error Details (first 20) ===");
		foreach (var err in errorDetails.Take(20))
			Console.WriteLine("   • " + err);
		if (errorDetails.Count > 20)
			Console.WriteLine($"...and {errorDetails.Count - 20} more.");
	}
}

void PrintSummaryTable(List<RunResult> results)
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

void PrintFinalStats(List<RunResult> results)
{
	var best = results.OrderByDescending(r => r.Throughput).First();
	var fastest = results.OrderBy(r => r.Duration).First();
	var lowestAvg = results.OrderBy(r => r.AvgMs).First();

	Console.WriteLine("\n🏁 Best RPS:      {0:N0} (Run #{1})", best.Throughput, best.Index);
	Console.WriteLine("⏱  Fastest Run:  #{0} ({1:F2}s)", fastest.Index, fastest.Duration.TotalSeconds);
	Console.WriteLine("📉 Lowest Avg:    {0:F3} ms (Run #{1})", lowestAvg.AvgMs, lowestAvg.Index);
}

record RunResult(
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
	int ErrorCount);
