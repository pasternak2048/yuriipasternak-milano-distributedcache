using MILANO.Benchmark.Models;
using MILANO.Client.Interfaces;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace MILANO.Benchmark.Benchmark
{
	/// <summary>
	/// Executes a single benchmark run against the MILANO cache client.
	/// </summary>
	public class BenchmarkRunner
	{
		private readonly IMilanoCacheClient _client;
		private readonly int _totalRequests;
		private readonly int _concurrency;
		private readonly int _runIndex;
		private readonly bool _showProgress;

		private readonly ConcurrentBag<string> _errorDetails = new();
		private readonly ConcurrentBag<double> _timings = new();
		private int _errorCount = 0;

		/// <summary>
		/// List of error messages that occurred during the run.
		/// </summary>
		public IReadOnlyCollection<string> ErrorDetails => _errorDetails;

		/// <summary>
		/// Number of successful timing measurements.
		/// </summary>
		public int TimingCount => _timings.Count;

		public BenchmarkRunner(IMilanoCacheClient client, int totalRequests, int concurrency, int runIndex, bool showProgress = true)
		{
			_client = client;
			_totalRequests = totalRequests;
			_concurrency = concurrency;
			_runIndex = runIndex;
			_showProgress = showProgress;
		}

		public async Task<RunResult> RunAsync()
		{
			var stopwatch = Stopwatch.StartNew();

			int done = 0;
			void ReportProgress()
			{
				if (_showProgress)
				{
					int current = Interlocked.Increment(ref done);
					if (current % 5000 == 0 || current == _totalRequests)
					{
						double percent = current * 100.0 / _totalRequests;
						Console.Write($"\r📦 Progress: {percent:F1}% ({current}/{_totalRequests})     ");
					}
				}
			}

			var tasks = new Task[_concurrency];
			for (int i = 0; i < _concurrency; i++)
			{
				int taskNum = i;
				tasks[i] = Task.Run(async () =>
				{
					int iterations = _totalRequests / _concurrency;
					for (int j = 0; j < iterations; j++)
					{
						var ticks = DateTime.UtcNow.Ticks;
						var key = $"key_{_runIndex}_{taskNum}_{j}_{ticks}";
						var value = $"value_{_runIndex}_{taskNum}_{j}_{ticks}";

						try
						{
							var localSw = Stopwatch.StartNew();

							// SET
							var setOk = await _client.SetAsync(key, value, TimeSpan.FromSeconds(60));
							if (!setOk)
							{
								_errorDetails.Add($"❌ SET FAIL [{key}]");
								Interlocked.Increment(ref _errorCount);
								continue;
							}

							// GET
							var returnedValue = await _client.GetAsync(key);
							if (returnedValue == null)
							{
								_errorDetails.Add($"❌ GET FAIL [{key}]: null");
								Interlocked.Increment(ref _errorCount);
								continue;
							}

							localSw.Stop();
							_timings.Add(localSw.Elapsed.TotalMilliseconds);

							if (returnedValue != value)
							{
								_errorDetails.Add($"❌ MISMATCH [{key}]: got={returnedValue}");
								Interlocked.Increment(ref _errorCount);
							}
						}
						catch (Exception ex)
						{
							_errorDetails.Add($"❌ EXCEPTION [{key}]: {ex.Message}");
							Interlocked.Increment(ref _errorCount);
						}

						ReportProgress();
					}
				});
			}

			await Task.WhenAll(tasks);
			stopwatch.Stop();
			Console.WriteLine();

			// Aggregate results
			var elapsed = stopwatch.Elapsed;
			var ordered = _timings.OrderBy(x => x).ToArray();

			double avg = ordered.Length > 0 ? ordered.Average() : 0;
			double max = ordered.Length > 0 ? ordered.Max() : 0;
			double min = ordered.Length > 0 ? ordered.Min() : 0;
			double median = ordered.Length > 0 ? ordered[ordered.Length / 2] : 0;
			double p99 = ordered.Length > 0 ? ordered[(int)(ordered.Length * 0.99)] : 0;
			int moreThan100ms = ordered.Count(x => x > 100);
			int moreThan50ms = ordered.Count(x => x > 50);
			int moreThan10ms = ordered.Count(x => x > 10);
			int moreThan1ms = ordered.Count(x => x > 1);
			double rps = _totalRequests / elapsed.TotalSeconds;

			return new RunResult(_runIndex, elapsed, avg, max, min, median, p99,
				moreThan100ms, moreThan50ms, moreThan10ms, moreThan1ms,
				rps, _errorCount);
		}
	}
}
