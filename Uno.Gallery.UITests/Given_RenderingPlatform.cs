using System;
using NUnit.Framework;
using Uno.UITest.Helpers;
using Uno.UITest.Helpers.Queries;

namespace Uno.Gallery.UITests;

public class Given_RenderingPlatform : TestBase
{
	[Test]
	public void When_DiagnosticsLoads_RendererPlatformAndFeaturesAreReported()
	{
		NavigateToSample("Diagnostics");

		var diagnostics = GetText("Diagnostics_BuildInfo");
		Assert.That(diagnostics, Does.Contain("Renderer:"));
		Assert.That(diagnostics, Does.Contain("Backend:"));
		Assert.That(diagnostics, Does.Contain("Target framework:"));
		Assert.That(diagnostics, Does.Contain("execution:"));
		Assert.That(diagnostics, Does.Contain("Features:"));
	}

	[Test]
	public void When_CompositionStateIsApplied_PageReportsDeterministicVisualValues()
	{
		NavigateToSample("Composition Visuals");

		App.WaitThenTap("Composition_ApplyState");
		Assert.That(GetText("Composition_Status"),
			Is.EqualTo("State 1; offset: 116,32; opacity: 0.55; scale: 0.80."));
	}

	[Test]
	public void When_AppOwnedTransferRuns_PageReportsPayloadAndCount()
	{
		NavigateToSample("Drag and Drop");

		App.WaitThenTap("DragDrop_DeterministicTransfer");
		Assert.That(GetText("DragDrop_Status"),
			Is.EqualTo("Transfer 1 via deterministic action: Gallery card: deterministic payload"));
	}

	[Test]
	public void When_GeolocationDeniedStateIsPreviewed_StatusIsVisibleWithoutDeviceAccess()
	{
		NavigateToSample("Geolocator");

		App.WaitThenTap("Geolocator_PreviewDenied");
		Assert.That(GetText("Geolocator_Status"),
			Is.EqualTo("Location access denied. Enable location permission in system settings."));
	}

	[Test]
	public void When_OfflineWebViewLoads_StatusDoesNotRemainBlank()
	{
		NavigateToSample("WebView");

		var reported = PollForText("WebView_Status", "Self-contained HTML loaded successfully.", TimeSpan.FromSeconds(10)) ||
			PollForText("WebView_Status", "WebView unavailable:", TimeSpan.FromSeconds(1)) ||
			PollForText("WebView_Status", "WebView navigation failed:", TimeSpan.FromSeconds(1));
		Assert.That(reported, Is.True, "WebView must expose success, unavailable, or failure status.");
	}

	private bool PollForText(string automationId, string expectedFragment, TimeSpan timeout)
	{
		var deadline = DateTime.UtcNow + timeout;
		while (DateTime.UtcNow < deadline)
		{
			if (GetText(automationId)?.Contains(expectedFragment, StringComparison.Ordinal) == true)
			{
				return true;
			}

			App.Wait(TimeSpan.FromMilliseconds(100));
		}

		return false;
	}

	private static string GetText(string automationId)
		=> new QueryEx(q => q.All().Marked(automationId)).GetDependencyPropertyValue<string>("Text");
}

[Category("SkiaCanvas")]
public class Given_SkiaRenderingPlatform : UITestBase
{
	[Test]
	[Explicit("Requires a Skia-renderer UI-test host; DOM automation cannot exercise SKCanvasElement.")]
	public void When_SkiaCanvasRedraws_RenderOverrideOwnsCompletionStatus()
	{
		NavigateToSample("Skia Canvas");

		App.WaitThenTap("SkiaCanvas_Redraw");
		var statusQuery = new QueryEx(q => q.All().Marked("SkiaCanvas_Status"));
		var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
		string status;
		do
		{
			status = statusQuery.GetDependencyPropertyValue<string>("Text") ?? string.Empty;
			if (status.Contains("RenderOverride completed:", StringComparison.Ordinal))
			{
				Assert.That(status, Does.Contain("state: 1"));
				Assert.That(status, Does.Contain("shapes: rectangle, circle, text"));
				return;
			}
			App.Wait(TimeSpan.FromMilliseconds(100));
		}
		while (DateTime.UtcNow < deadline);

		Assert.Fail("Canvas status must be updated by a completed RenderOverride call, not by the redraw request.");
	}
}
