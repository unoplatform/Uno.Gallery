namespace Uno.Gallery.RuntimeTests;

/// <summary>
/// Tells how the app was launched with regard to the runtime tests.
/// </summary>
/// <remarks>
/// Adapted from Uno.Toolkit (samples/Uno.Toolkit.Samples/RuntimeTesting/RuntimeTestModeDetector.cs).
/// </remarks>
internal static class RuntimeTestModeDetector
{
	// `--rtt=1` on the command line, `?rtt=1` in the WASM query string
	private const string EnabledKey = "rtt";

	// Set by the engine's runners (desktop script, uno-runtimetests-wasm), read by the engine itself
	private const string EngineAutorunVariable = "UNO_RUNTIME_TESTS_RUN_TESTS";
	// Set by the Android and iOS scripts, read by MobileRuntimeTestsAutostart
	private const string MobileAutorunVariable = "UITEST_RUNTIME_AUTOSTART_RESULT_FILE";

	/// <summary>
	/// Whether the runtime tests sample page should be listed in a Release build.
	/// </summary>
	public static bool IsEnabled { get; private set; }

	/// <summary>
	/// Whether the app was started by a CI script to run the runtime tests and exit.
	/// </summary>
	public static bool IsAutomatedRun { get; private set; }

	public static void Initialize(string? launchArguments)
	{
		var arguments = ParseArguments(launchArguments).Concat(ParseArguments(Environment.GetCommandLineArgs())).ToArray();

		IsEnabled = arguments.Any(x => x.Key.Equals(EnabledKey, StringComparison.OrdinalIgnoreCase) && IsTrue(x.Value));
		IsAutomatedRun =
			IsAutorunValue(Environment.GetEnvironmentVariable(EngineAutorunVariable)) ||
			!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(MobileAutorunVariable)) ||
			arguments.Any(x => x.Key.Equals(EngineAutorunVariable, StringComparison.OrdinalIgnoreCase) && IsAutorunValue(x.Value));
	}

	private static bool IsTrue(string? value) => value is "1" || bool.TryParse(value, out var result) && result;

	// The engine treats "false" as "don't run", and anything else (true, a filter or a JSON config) as "run"
	private static bool IsAutorunValue(string? value) =>
		!string.IsNullOrWhiteSpace(value) && !(bool.TryParse(value, out var result) && !result);

	private static IEnumerable<KeyValuePair<string, string?>> ParseArguments(string? arguments) =>
		string.IsNullOrWhiteSpace(arguments)
			? []
			: ParseArguments(arguments.Split(['?', '&', ' '], StringSplitOptions.RemoveEmptyEntries));

	private static IEnumerable<KeyValuePair<string, string?>> ParseArguments(IEnumerable<string> arguments) =>
		arguments.Select(KeyValuePair<string, string?> (argument) =>
		{
			var token = argument.TrimStart('-');
			var separator = token.IndexOf('=');

			return separator < 0
				? new(token, null)
				: new(token[..separator], Uri.UnescapeDataString(token[(separator + 1)..]));
		});
}
