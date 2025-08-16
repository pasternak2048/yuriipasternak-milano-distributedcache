using System.Globalization;

namespace MILANO.Benchmark.Common
{
	/// <summary>
	/// Provides utility methods for printing styled console output, including boxes, colors, and centered text.
	/// </summary>
	public static class ConsoleExtensions
	{
		private const int BoxWidth = 60;

		public static void PrintBoxed(string text)
		{
			const int width = 60;
			string border = new string('═', width);
			Console.WriteLine($"╔{border}╗");
			Console.WriteLine($"║{CenterText(text, width - 1)}║");
			Console.WriteLine($"╚{border}╝");
		}

		public static void PrintBoxedLines(IEnumerable<string> lines)
		{
			const int width = 60;
			string border = new string('═', width);
			Console.WriteLine($"╔{border}╗");
			foreach (var line in lines)
				Console.WriteLine($"║{CenterText(line, width - 1)}║");
			Console.WriteLine($"╚{border}╝");
		}

		public static string CenterText(string text, int width)
		{
			int visualLength = GetVisualLength(text);
			int padding = Math.Max(0, (width - visualLength) / 2);
			return new string(' ', padding) + text + new string(' ', width - padding - visualLength);
		}

		/// <summary>
		/// Approximates the visual width of the string for correct centering,
		/// accounting for full-width Unicode characters (e.g. emojis, CJK).
		/// </summary>
		public static int GetVisualLength(string input)
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

		/// <summary>
		/// Detects characters that occupy 2 spaces in monospace output.
		/// Covers CJK, emojis, and some symbols.
		/// </summary>
		public static bool IsWideChar(char c)
		{
			return c >= 0x1100 && (
				c <= 0x115F ||   // Hangul Jamo
				c == 0x2329 || c == 0x232A || // angle brackets
				(c >= 0x2E80 && c <= 0xA4CF && c != 0x303F) || // CJK...
				(c >= 0xAC00 && c <= 0xD7A3) || // Hangul Syllables
				(c >= 0xF900 && c <= 0xFAFF) || // CJK Compatibility Ideographs
				(c >= 0xFE10 && c <= 0xFE19) || // Vertical forms
				(c >= 0xFE30 && c <= 0xFE6F) || // CJK Compatibility Forms
				(c >= 0xFF00 && c <= 0xFF60) || // Fullwidth Forms
				(c >= 0xFFE0 && c <= 0xFFE6) || // Halfwidth Forms
				(c >= 0x1F300 && c <= 0x1F64F) || // Emojis
				(c >= 0x1F900 && c <= 0x1F9FF) // Supplemental emojis
			);
		}

		/// <summary>
		/// Changes foreground color, executes action, and restores color.
		/// </summary>
		public static void WithColor(ConsoleColor color, Action action)
		{
			var previous = Console.ForegroundColor;
			Console.ForegroundColor = color;
			action();
			Console.ForegroundColor = previous;
		}
	}
}
