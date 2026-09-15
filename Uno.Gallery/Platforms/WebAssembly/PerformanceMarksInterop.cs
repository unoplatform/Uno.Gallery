#if PERF_MEASUREMENTS
using System.Runtime.InteropServices.JavaScript;

namespace Uno.Gallery.Wasm;

/// <summary>
/// Managed proxy for <c>Uno.Gallery.Wasm.PerformanceMarks</c> (PerformanceMarks.ts).
/// Wraps browser Performance API calls for WASM instrumented builds.
/// Only compiled when <c>PERF_MEASUREMENTS</c> is defined; invisible in Release-without-flag builds.
/// </summary>
/// <remarks>
/// The browser PerformanceTimeline (<c>performance.getEntriesByType('mark')</c>) is the canonical
/// read path for marks in production. No marshaled export method is provided; this interop only
/// writes marks into the browser timeline via <see cref="Mark"/>.
/// </remarks>
internal static partial class PerformanceMarksInterop
{
	private const string JsType = "globalThis.Uno.Gallery.Wasm.PerformanceMarks";

	[JSImport($"{JsType}.mark")]
	internal static partial void Mark(string name);

	[JSImport($"{JsType}.measure")]
	internal static partial void Measure(string name, double durationMs);
}
#endif
