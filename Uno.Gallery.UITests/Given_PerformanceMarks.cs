using System;
using System.Globalization;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests
{
	/// <summary>
	/// Verifies that the performance instrumentation layer records marks correctly.
	/// Requires a PERF_MEASUREMENTS-enabled build (USE_UITESTS ⇒ PERF_MEASUREMENTS is always set).
	/// </summary>
	public class Given_PerformanceMarks : TestBase
	{
		/// <summary>
		/// Tapping the Refresh button is the first real pointer input in the test, so it
		/// records app.first_input via PointerPressed → OnFirstPointerInput before Click fires
		/// RefreshMarks_Click → ExportJson. The resulting JSON must contain all early marks
		/// (constructed … window_activated, shell_loaded) plus app.first_input, each exactly
		/// once, and in ascending ms order.
		/// </summary>
		[Test]
		public void When_DiagnosticsPage_PerformanceMarks_AreOrderedAndUnique()
		{
			// Navigate via backdoor — no pointer event, so first_input is not yet recorded.
			NavigateToSample("Diagnostics");

			// First real pointer tap:
			//   1. PointerPressed bubbles to Shell → OnFirstPointerInput → Record(FirstInput)
			//   2. Click → RefreshMarks_Click → ExportJson (first_input is already recorded)
			App.WaitThenTap("PerfMarks_RefreshButton");

			var text = new QueryEx(x => x.All().Marked("PerfMarksOutput"))
				.GetDependencyPropertyValue<string>("Text");

			TakeScreenshot("PerfMarks_AfterFirstTap");

			Assert.That(text, Is.Not.Null.And.Not.Empty,
				"PerfMarksOutput must not be empty");
			Assert.That(text, Is.Not.EqualTo("[]"),
				"Performance marks JSON must be non-empty in a PERF_MEASUREMENTS build");

			// All early marks must be present.
			Assert.That(text, Does.Contain("\"app.constructed\""),      "app.constructed missing");
			Assert.That(text, Does.Contain("\"app.resources_initialized\""), "app.resources_initialized missing");
			Assert.That(text, Does.Contain("\"app.shell_built\""),      "app.shell_built missing");
			Assert.That(text, Does.Contain("\"app.catalog_ready\""),    "app.catalog_ready missing");
			Assert.That(text, Does.Contain("\"app.window_activated\""), "app.window_activated missing");
			Assert.That(text, Does.Contain("\"app.shell_loaded\""),     "app.shell_loaded missing");
			Assert.That(text, Does.Contain("\"app.first_input\""),
				"app.first_input must be present after the tap on the Refresh button");

			// No duplicate entries — each canonical name appears at most once.
			foreach (var name in new[]
			{
				"app.constructed", "app.resources_initialized", "app.shell_built",
				"app.catalog_ready", "app.window_activated", "app.shell_loaded", "app.first_input"
			})
			{
				int count = CountOccurrences(text, $"\"name\":\"{name}\"");
				Assert.That(count, Is.EqualTo(1),
					$"Mark '{name}' must appear exactly once in ExportJson (found {count})");
			}

			// Ordering: app.constructed ms < app.first_input ms.
			double constructedMs = ExtractMs(text, "app.constructed");
			double firstInputMs  = ExtractMs(text, "app.first_input");
			Assert.That(constructedMs, Is.Not.NaN, "app.constructed ms must be parseable");
			Assert.That(firstInputMs,  Is.Not.NaN, "app.first_input ms must be parseable");
			Assert.That(constructedMs, Is.LessThan(firstInputMs),
				"app.constructed must be recorded before app.first_input");

			// Second tap — first_input is already recorded; ExportJson must not duplicate it.
			App.WaitThenTap("PerfMarks_RefreshButton");
			var text2 = new QueryEx(x => x.All().Marked("PerfMarksOutput"))
				.GetDependencyPropertyValue<string>("Text");

			TakeScreenshot("PerfMarks_AfterSecondTap");

			int firstInputCount = CountOccurrences(text2, "\"name\":\"app.first_input\"");
			Assert.That(firstInputCount, Is.EqualTo(1),
				"app.first_input must still appear exactly once after a second tap (dedup)");
		}

		private static int CountOccurrences(string text, string pattern)
		{
			int count = 0, idx = 0;
			while ((idx = text.IndexOf(pattern, idx, StringComparison.Ordinal)) >= 0)
			{
				count++;
				idx += pattern.Length;
			}
			return count;
		}

		private static double ExtractMs(string json, string markName)
		{
			var search = $"\"name\":\"{markName}\",\"ms\":";
			int idx = json.IndexOf(search, StringComparison.Ordinal);
			if (idx < 0) return double.NaN;
			int start = idx + search.Length;
			int end = json.IndexOf('}', start);
			if (end < 0) return double.NaN;
			return double.TryParse(
				json.Substring(start, end - start),
				NumberStyles.Float,
				CultureInfo.InvariantCulture,
				out double ms) ? ms : double.NaN;
		}
	}
}
