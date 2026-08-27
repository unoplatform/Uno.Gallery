using System.Diagnostics;
using NUnit.Framework;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

[Category("Smoke")]
public sealed class Given_AllSamplesSmoke : UITestBase
{
	private const string SmokeDesign = "Material";
	private static readonly TimeSpan HostLoadTimeout = TimeSpan.FromSeconds(30);

	[Test]
	public void All_target_compatible_stable_samples_load()
	{
		var manifest = RetrieveManifest();
		var samples = SmokeCatalog.ParseStableSamples(manifest);
		var failures = new List<SmokeFailure>();

		Assert.That(samples, Is.Not.Empty, "The target manifest must contain stable samples.");
		TestContext.Progress.WriteLine(
			$"Catalog smoke: {samples.Count} target-compatible stable samples, design={SmokeDesign}.");

		foreach (var sample in samples)
		{
			SmokeSample(sample, failures);
		}

		Assert.That(failures, Is.Empty, SmokeCatalog.FormatFailures(failures));
	}

	private string RetrieveManifest()
	{
		App.WaitForElement("UITestBackdoorResponse", timeout: TimeSpan.FromSeconds(10));
		var chunks = new List<string>();
		int chunkCount;

		for (var index = 0; ; index++)
		{
			var token = Guid.NewGuid().ToString("N");
			SetBackdoorCommand("uitest:manifest:" + token + ":" + index);
			var response = WaitForBackdoorResponse(token);
			var separator = response.IndexOf('\n');
			if (separator < 1 ||
				!int.TryParse(response[..separator], out chunkCount) ||
				chunkCount < 1)
			{
				throw new InvalidDataException(
					$"Invalid target manifest chunk {index} response.");
			}

			chunks.Add(response[(separator + 1)..]);
			if (chunks.Count == chunkCount)
			{
				return string.Concat(chunks);
			}
		}
	}

	private void SmokeSample(SmokeSample sample, ICollection<SmokeFailure> failures)
	{
		var expectedMarker = sample.Slug + "\n" + SmokeDesign;
		Exception? harnessFailure = null;

		try
		{
			ResetUnhandledException();
			var token = Guid.NewGuid().ToString("N");
			SendBackdoorCommand($"uitest:navigate:{sample.Slug}:{SmokeDesign}:{token}", token);
			WaitForHostMarker(expectedMarker);

			var unhandled = GetUITestState("get-error");
			if (!string.IsNullOrWhiteSpace(unhandled))
			{
				failures.Add(new SmokeFailure(sample.Slug, SmokeDesign, unhandled));
			}
		}
		catch (Exception exception)
		{
			harnessFailure = exception;
			var appException = TryGetUITestState("get-error");
			var details = string.IsNullOrWhiteSpace(appException)
				? exception.ToString()
				: exception + Environment.NewLine + "App exception:" + Environment.NewLine + appException;
			failures.Add(new SmokeFailure(sample.Slug, SmokeDesign, details));
		}
		finally
		{
			try
			{
				TakeScreenshot($"smoke_{sample.Slug}_{SmokeDesign}");
			}
			catch (Exception screenshotException)
			{
				if (harnessFailure is null)
				{
					failures.Add(new SmokeFailure(
						sample.Slug,
						SmokeDesign,
						"Diagnostic screenshot failed: " + screenshotException));
				}
			}
		}
	}

	private void WaitForHostMarker(string expected)
	{
		var stopwatch = Stopwatch.StartNew();
		while (stopwatch.Elapsed < HostLoadTimeout)
		{
			var marker = TryGetUITestState("get-marker");
			if (string.Equals(marker, expected, StringComparison.Ordinal))
			{
				return;
			}

			var exception = TryGetUITestState("get-error");
			if (!string.IsNullOrWhiteSpace(exception))
			{
				throw new InvalidOperationException(
					$"The app reported an exception before host marker '{expected}':{Environment.NewLine}{exception}");
			}

			Thread.Sleep(TimeSpan.FromMilliseconds(100));
		}

		throw new TimeoutException(
			$"Timed out after {HostLoadTimeout.TotalSeconds:0}s waiting for sample host marker '{expected}'. " +
			$"Last marker was '{TryGetUITestState("get-marker")}'.");
	}

	private void ResetUnhandledException()
	{
		var token = Guid.NewGuid().ToString("N");
		SendBackdoorCommand("uitest:reset:" + token, token);
	}

	private string WaitForBackdoorResponse(string token)
	{
		var prefix = token + ":";
		var stopwatch = Stopwatch.StartNew();
		var lastValue = string.Empty;
		while (stopwatch.Elapsed < TimeSpan.FromSeconds(5))
		{
			try
			{
				lastValue = new QueryEx(x => x.All().Marked("UITestBackdoorResponse"))
					.GetDependencyPropertyValue<string>("Text") ?? string.Empty;
				if (lastValue.StartsWith(prefix, StringComparison.Ordinal))
				{
					return System.Text.Encoding.UTF8.GetString(
						Convert.FromBase64String(lastValue[prefix.Length..]));
				}
			}
			catch
			{
				// The response marker can be absent during the first layout pass.
			}

			Thread.Sleep(TimeSpan.FromMilliseconds(50));
		}

		throw new TimeoutException(
			$"Timed out waiting for UITest backdoor response token '{token}'. Last response: '{lastValue}'.");
	}

	private string SendBackdoorCommand(string command, string token)
	{
		SetBackdoorCommand(command);
		return WaitForBackdoorResponse(token);
	}

	private string GetUITestState(string state)
	{
		var token = Guid.NewGuid().ToString("N");
		return SendBackdoorCommand($"uitest:{state}:{token}", token);
	}

	private string TryGetUITestState(string state)
	{
		try
		{
			return GetUITestState(state);
		}
		catch
		{
			return string.Empty;
		}
	}

	private void SetBackdoorCommand(string command)
		=> new QueryEx(x => x.All().Marked("AppShell"))
			.SetDependencyPropertyValue("CurrentSampleBackdoor", command);
}
