// Copyright (c) Uno Platform Inc. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.

namespace Uno.Gallery.Helpers;

/// <summary>
/// Thin opt-in startup instrumentation layer.
/// Active when <c>PERF_MEASUREMENTS</c> is defined (Debug, canary, UITest, or opt-in Release builds).
/// In non-instrumented builds all methods are inlined no-ops and <see cref="ExportJson"/> returns <c>"[]"</c>.
/// No logging, telemetry, or user data is collected or transmitted.
/// </summary>
/// <remarks>
/// <para>
/// When building with <c>-p:EnablePerformanceMeasurements=true</c> in a Release configuration (without
/// <c>IS_CANARY_BUILD</c>, <c>DEBUG</c>, or <c>USE_UITESTS</c>), the Diagnostics/Canary UI page remains
/// hidden — the Canary category is filtered out in <see cref="App.BuildShell"/> for such configurations.
/// Marks are still recorded and, on WASM, are forwarded to the browser PerformanceTimeline; they can be
/// read externally via <c>performance.getEntriesByType('mark')</c> without any marshaled export method.
/// </para>
/// </remarks>
internal static class PerformanceMarks
{
	// Canonical mark names — shared by C# call sites, WASM JS interop, and JSON output.
	/// <summary>
	/// First managed mark. The Stopwatch backing all marks starts when this type is first accessed
	/// (typically at App constructor time), so <c>app.constructed</c> is the relative anchor for
	/// all subsequent marks — it is <em>not</em> process-start time. On WASM the corresponding
	/// browser <c>PerformanceMark.startTime</c> is relative to the Navigation Timing
	/// <c>performance.timeOrigin</c>.
	/// </summary>
	public const string Constructed          = "app.constructed";
	public const string ResourcesInitialized = "app.resources_initialized";
	public const string ShellBuilt           = "app.shell_built";
	public const string CatalogReady         = "app.catalog_ready";
	public const string WindowActivated      = "app.window_activated";
	public const string ShellLoaded          = "app.shell_loaded";
	public const string VisualReady          = "app.visual_ready";
	public const string FirstInput           = "app.first_input";
	public const string SearchRendered       = "app.search_rendered";
	public const string NavigationRendered   = "app.navigation_rendered";

#if PERF_MEASUREMENTS
	// Stopwatch is started when the type is first accessed (typically at App ctor time).
	private static readonly System.Diagnostics.Stopwatch _sw = System.Diagnostics.Stopwatch.StartNew();
	private static readonly object _lock = new();

	// Fixed-length arrays in canonical order — index in _names maps 1:1 to _ms.
	private static readonly string[] _names =
	[
		Constructed, ResourcesInitialized, ShellBuilt, CatalogReady,
		WindowActivated, ShellLoaded, VisualReady, FirstInput,
	];
	private static readonly double?[] _ms = new double?[_names.Length];
#if VISUAL_REGRESSION
	private static int _visualReadySequence;
#endif

	/// <summary>
	/// Records a mark by name with the elapsed time relative to helper initialization.
	/// Subsequent calls for the same <paramref name="name"/> are silently ignored (dedup).
	/// Unrecognized names are silently ignored.
	/// </summary>
	public static void Record(string name)
	{
		// Capture time before acquiring the lock for maximum accuracy.
		double elapsedMs = _sw.Elapsed.TotalMilliseconds;
		int idx = Array.IndexOf(_names, name);
		if (idx < 0)
			return;
		lock (_lock)
		{
			if (_ms[idx].HasValue
#if VISUAL_REGRESSION
				&& name != VisualReady
#endif
			)
				return;
			_ms[idx] = elapsedMs;
		}

#if __WASM__
#if VISUAL_REGRESSION
		Wasm.PerformanceMarksInterop.Mark(
			name == VisualReady
				? $"{VisualReady}.{System.Threading.Interlocked.Increment(ref _visualReadySequence)}"
				: name);
#else
		Wasm.PerformanceMarksInterop.Mark(name);
#endif
#endif
	}

	/// <summary>
	/// Records a duration in the browser PerformanceTimeline. Duration entries are
	/// intentionally not added to <see cref="ExportJson"/>, whose ordered startup-mark
	/// contract is consumed by existing diagnostics and tests.
	/// </summary>
	public static void RecordDuration(string name, long startTimestamp)
	{
		if (name is not SearchRendered and not NavigationRendered)
		{
			return;
		}

		var durationMs = System.Diagnostics.Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
#if __WASM__
		Wasm.PerformanceMarksInterop.Measure(name, durationMs);
#endif
	}

	/// <summary>
	/// Returns a JSON array of all recorded marks in canonical order.
	/// Format: <c>[{"name":"app.constructed","ms":1.234}, ...]</c>
	/// Unrecorded marks are omitted. Thread-safe snapshot.
	/// </summary>
	public static string ExportJson()
	{
		double?[] snapshot;
		lock (_lock)
		{
			snapshot = (double?[])_ms.Clone();
		}

		var sb = new System.Text.StringBuilder("[");
		bool first = true;
		for (int i = 0; i < _names.Length; i++)
		{
			if (!snapshot[i].HasValue)
				continue;
			if (!first)
				sb.Append(',');
			sb.Append("{\"name\":\"")
			  .Append(_names[i])
			  .Append("\",\"ms\":")
			  .Append(snapshot[i]!.Value.ToString("F3", System.Globalization.CultureInfo.InvariantCulture))
			  .Append('}');
			first = false;
		}
		sb.Append(']');
		return sb.ToString();
	}

#else
	/// <summary>No-op in non-instrumented builds.</summary>
	[System.Runtime.CompilerServices.MethodImpl(
		System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
	public static void Record(string name) { }

	/// <summary>Returns an empty JSON array in non-instrumented builds.</summary>
	public static string ExportJson() => "[]";
#endif
}
