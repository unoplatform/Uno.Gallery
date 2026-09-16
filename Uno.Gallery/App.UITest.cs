#if USE_UITESTS
namespace Uno.Gallery;

public partial class App
{
	private const int UITestManifestChunkSize = 1_000;

	private void InitializeUITestHooks()
		=> UnhandledException += OnUITestUnhandledException;

	private static bool TryHandleUITestBackdoor(Shell shell)
	{
		var command = shell.CurrentSampleBackdoor;
		if (!command.StartsWith("uitest:", StringComparison.Ordinal))
		{
			return false;
		}

		var parts = command.Split(':');
		var responseToken = string.Equals(parts.ElementAtOrDefault(1), "navigate", StringComparison.Ordinal)
			? parts.ElementAtOrDefault(4)
			: parts.ElementAtOrDefault(2);

		try
		{
			switch (parts.ElementAtOrDefault(1))
			{
				case "begin-smoke" when parts.Length == 3:
					shell.UITestUnhandledExceptionState = string.Empty;
					shell.UITestSampleHostLoadedState = string.Empty;
					shell.UITestSmokeCaptureEnabled = true;
					ReturnUITestResponse(shell, parts[2], "ok");
					break;

				case "end-smoke" when parts.Length == 3:
					shell.UITestSmokeCaptureEnabled = false;
					ReturnUITestResponse(shell, parts[2], "ok");
					break;

				case "manifest" when parts.Length == 4 && int.TryParse(parts[3], out var chunkIndex):
					ReturnUITestManifestChunk(shell, parts[2], chunkIndex);
					break;

				case "navigate" when parts.Length == 5:
					NavigateFromUITest(shell, parts[2], parts[3]);
					ReturnUITestResponse(shell, parts[4], "ok");
					break;

				case "reset" when parts.Length == 3:
					shell.UITestUnhandledExceptionState = string.Empty;
					ReturnUITestResponse(shell, parts[2], "ok");
					break;

				case "get-error" when parts.Length == 3:
					ReturnUITestResponse(shell, parts[2], shell.UITestUnhandledExceptionState);
					break;

				case "get-marker" when parts.Length == 3:
					ReturnUITestResponse(shell, parts[2], shell.UITestSampleHostLoadedState);
					break;

				default:
					throw new InvalidOperationException($"Unknown UITest backdoor command '{command}'.");
			}
		}
		catch (Exception exception)
		{
			RecordUITestException(shell, exception);
			if (!string.IsNullOrEmpty(responseToken))
			{
				ReturnUITestResponse(shell, responseToken, "error\n" + exception);
			}
		}

		return true;
	}

	private static void ReturnUITestManifestChunk(Shell shell, string token, int chunkIndex)
	{
		var manifest = SampleManifest.GetJson();
		var chunkCount = (manifest.Length + UITestManifestChunkSize - 1) / UITestManifestChunkSize;
		if (chunkIndex < 0 || chunkIndex >= chunkCount)
		{
			throw new InvalidOperationException(
				$"UITest requested manifest chunk {chunkIndex}, but the target has {chunkCount} chunks.");
		}

		var offset = chunkIndex * UITestManifestChunkSize;
		var length = Math.Min(UITestManifestChunkSize, manifest.Length - offset);
		ReturnUITestResponse(
			shell,
			token,
			chunkCount + "\n" + manifest.Substring(offset, length));
	}

	private static void NavigateFromUITest(Shell shell, string slug, string designName)
	{
		shell.UITestSampleHostLoadedState = string.Empty;

		if (!Enum.TryParse<Design>(designName, ignoreCase: true, out var design))
		{
			throw new InvalidOperationException($"Unknown UITest design '{designName}'.");
		}

		var sample = shell.Navigator!.FindBySlug(slug)
			?? throw new InvalidOperationException($"Unknown target catalog slug '{slug}'.");

		SamplePageLayout.SetPreferredDesign(design);

		// Force a fresh page even when the requested sample is already current. This makes the
		// host-loaded marker a reliable acknowledgement for every catalog entry in a batch.
		shell.NavigationView.Content = null;
		shell.Navigator.NavigateTo(sample);
	}

	private static void ReturnUITestResponse(Shell shell, string token, string payload)
		=> shell.SetUITestResponse(
			token + ":" + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(payload)));

	private void OnUITestUnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs args)
	{
		var shell = MainWindow?.Content as Shell;
		if (shell?.UITestSmokeCaptureEnabled == true)
		{
			RecordUITestException(shell, args.Exception);
			// Keep the process alive only while the smoke batch is deliberately aggregating failures.
			args.Handled = true;
		}
	}

	private static void RecordUITestException(Shell shell, Exception exception)
	{
		var details = exception.ToString();
		shell.UITestUnhandledExceptionState = string.IsNullOrEmpty(shell.UITestUnhandledExceptionState)
			? details
			: shell.UITestUnhandledExceptionState + Environment.NewLine + Environment.NewLine + details;
	}
}
#endif
