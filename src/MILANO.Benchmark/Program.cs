using Microsoft.Extensions.DependencyInjection;
using MILANO.Benchmark.Benchmark;
using MILANO.Benchmark.Common;
using MILANO.Benchmark.Configuration;
using MILANO.Benchmark.Models;
using MILANO.Client.Extensions;
using MILANO.Client.Interfaces;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

// === MANUAL INPUT ===
Console.Write("🔢 Enter total number of requests per run: ");
int totalRequests = int.Parse(Console.ReadLine() ?? "120000");

Console.Write("🧵 Enter number of threads (concurrency): ");
int concurrency = int.Parse(Console.ReadLine() ?? "12");

Console.Write("🔁 Enter number of runs: ");
int runs = int.Parse(Console.ReadLine() ?? "2");

// === SETUP CLIENT ===
var services = new ServiceCollection();
services.AddMilanoCacheClient(options =>
{
	options.ServerHost = "http://localhost:7010/cache/";
	options.ApiKey = "test-key-full-access";
	options.Timeout = TimeSpan.FromSeconds(5);
});

var provider = services.BuildServiceProvider();
var client = provider.GetRequiredService<IMilanoCacheClient>();

// === OPTIONS ===
var benchmarkOptions = new BenchmarkOptions
{
	TotalRequests = totalRequests,
	Concurrency = concurrency,
	NumberOfRuns = runs,
	ShowProgress = true
};

ConsoleExtensions.PrintBoxed("🏋️ MILANO Benchmark Session Started");

var results = new List<RunResult>();

// === MAIN BENCHMARK LOOP ===
for (int i = 1; i <= benchmarkOptions.NumberOfRuns; i++)
{
	BenchmarkReporter.PrintBenchmarkStart(i);

	var runner = new BenchmarkRunner(
		client,
		benchmarkOptions.TotalRequests,
		benchmarkOptions.Concurrency,
		i,
		benchmarkOptions.ShowProgress
	);

	var result = await runner.RunAsync();
	results.Add(result);

	BenchmarkReporter.PrintReportHeader();

	BenchmarkReporter.PrintEnhancedReport(
		result,
		benchmarkOptions.TotalRequests,
		benchmarkOptions.Concurrency,
		runner.ErrorDetails,
		runner.TimingCount
	);

	BenchmarkReporter.PrintBenchmarkEnd(i, result.Duration);
	Console.WriteLine(); Console.WriteLine();
}

// === SUMMARY ===
BenchmarkReporter.PrintSummaryTable(results);
BenchmarkReporter.PrintFinalStats(results);