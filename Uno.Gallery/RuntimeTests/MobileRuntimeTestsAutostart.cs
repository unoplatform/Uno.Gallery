#if __ANDROID__ || __IOS__
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.UI.Dispatching;
using Uno.UI.RuntimeTests;

namespace Uno.Gallery.RuntimeTests;

/// <summary>
/// Runs the runtime tests on Android and iOS when the CI scripts ask for it, and writes the NUnit results to a file they collect.
/// </summary>
/// <remarks>
/// Ported from uno.extensions (src/Uno.Extensions.RuntimeTests/.../MobileRuntimeTestsAutostart.cs).
/// The desktop and WebAssembly heads use the engine's own runner (UNO_RUNTIME_TESTS_RUN_TESTS), but its module initializer
/// hooks Console.CancelKeyPress, which throws in a sandboxed iOS app. So the mobile heads use the UITEST_RUNTIME_* variables instead:
/// the Android script passes them as intent extras (bridged in MainActivity), the iOS script through SIMCTL_CHILD_*.
/// </remarks>
internal static class MobileRuntimeTestsAutostart
{
	private const string ResultFileVariable = "UITEST_RUNTIME_AUTOSTART_RESULT_FILE";
	private const string FilterVariable = "UITEST_RUNTIME_TESTS_FILTER";

	private static int _started;

	public static void StartIfRequested(Window window)
	{
		var resultFile = Environment.GetEnvironmentVariable(ResultFileVariable);

		// Always logged: a silent return looks the same as a hung run in the device log.
		Console.WriteLine($"MobileRuntimeTestsAutostart: {ResultFileVariable}={(string.IsNullOrEmpty(resultFile) ? "<not set>, autostart disabled" : resultFile)}");

		if (string.IsNullOrEmpty(resultFile) || Interlocked.Exchange(ref _started, 1) == 1)
		{
			return;
		}

		_ = Task.Run(() => RunAsync(window, resultFile))
			.ContinueWith(run => Exit(run, resultFile), TaskScheduler.Default);
	}

	private static void Exit(Task run, string resultFile)
	{
		if (run.IsCompletedSuccessfully)
		{
			Console.WriteLine($"MobileRuntimeTestsAutostart: wrote the results to {resultFile}");
			Environment.Exit(0);
		}

		try
		{
			Console.Error.WriteLine($"MobileRuntimeTestsAutostart failed: {run.Exception?.GetBaseException() ?? (object)run.Status}");
		}
		finally
		{
			Environment.Exit(1);
		}
	}

	private static async Task RunAsync(Window window, string resultFile)
	{
		var dispatcher = window.DispatcherQueue ?? throw new InvalidOperationException("The window has no DispatcherQueue.");

		// Let the app finish starting before replacing its content.
		await Task.Delay(TimeSpan.FromSeconds(5)).ConfigureAwait(false);

		// Same as the engine's desktop runner: the test control replaces the window content.
		var unitTests = await OnDispatcherAsync(dispatcher, () =>
		{
			var control = new UnitTestsControl();
			window.Content = control;
			return Task.FromResult(control);
		}).ConfigureAwait(false);

		var deadline = DateTimeOffset.UtcNow + TimeSpan.FromSeconds(30);
		while (!await OnDispatcherAsync(dispatcher, () => Task.FromResult(unitTests.IsLoaded)).ConfigureAwait(false))
		{
			if (DateTimeOffset.UtcNow > deadline)
			{
				throw new TimeoutException("The UnitTestsControl did not load within 30s.");
			}

			await Task.Delay(200).ConfigureAwait(false);
		}

		var results = await OnDispatcherAsync(dispatcher, async () =>
		{
			await unitTests.RunTests(CancellationToken.None, BuildConfig()).ConfigureAwait(true);
			return unitTests.NUnitTestResultsDocument;
		}).ConfigureAwait(false);

		Directory.CreateDirectory(Path.GetDirectoryName(resultFile) ?? ".");
		await File.WriteAllTextAsync(resultFile, results, Encoding.UTF8).ConfigureAwait(false);
	}

	/// <summary>
	/// Runs <paramref name="callback"/> on the dispatcher and hands its result, or what it threw, back to the caller.
	/// </summary>
	private static Task<T> OnDispatcherAsync<T>(DispatcherQueue dispatcher, Func<Task<T>> callback)
	{
		var tcs = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);

		if (!dispatcher.TryEnqueue(async () =>
		{
			try
			{
				tcs.TrySetResult(await callback().ConfigureAwait(true));
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		}))
		{
			// Without this the caller would wait out the CI script's timeout instead of failing.
			throw new InvalidOperationException("The DispatcherQueue rejected the work item; the dispatcher is shutting down.");
		}

		return tcs.Task;
	}

	private static UnitTestEngineConfig BuildConfig()
	{
		// The filter is base64-encoded by the CI scripts, because `adb shell am start -e` and simctl mangle spaces and pipes.
		// '!' stands in for the '=' padding for the same reason.
		var rawFilter = Environment.GetEnvironmentVariable(FilterVariable);
		var filter = string.IsNullOrWhiteSpace(rawFilter)
			? null
			: Encoding.UTF8.GetString(Convert.FromBase64String(rawFilter.Replace('!', '=')));

		return new UnitTestEngineConfig
		{
			Filter = filter,
			Attempts = 1,
		};
	}
}
#endif
