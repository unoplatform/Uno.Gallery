using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "WebView",
		Description = "Hosts deterministic self-contained HTML and reports navigation success or failure.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.webview2",
		Slug = "webview",
		Tags = new[] { "web", "html", "offline", "platform" },
		Status = SampleStatus.Stable,
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Fluent,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { "Requires a WebView implementation on the host; the demonstrated HTML is inline and performs no network requests." },
		AccessibilityNotes = new[] { "The WebView has an accessible name and navigation success, unavailability, and failure are exposed as text." },
		ResetBehavior = "Reload the sample to create a new WebView and navigate the same self-contained HTML.",
		Variants = new[] { "Self-contained HTML", "Navigation completion", "Unavailable or failed host status" },
		KnownLimitations = new[] { "The embedded browser engine, security policy, and web-platform support vary by target." },
		Owner = "unoplatform",
		ReviewedOn = "2026-08-27")]
	public sealed partial class WebViewSamplePage : Page
	{
		public WebViewSamplePage()
		{
			this.InitializeComponent();
		}

		private void WebView_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				((WebView2)sender).NavigateToString(
					"<html><body style='font-family:sans-serif'><h1>Uno Gallery</h1><p>Offline WebView content.</p></body></html>");
				UpdateStatus("Self-contained HTML navigation started.");
			}
			catch (Exception ex)
			{
				UpdateStatus($"WebView unavailable: {ex.GetType().Name}.");
			}
		}

		private void WebView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
			=> UpdateStatus(args.IsSuccess
				? "Self-contained HTML loaded successfully."
				: $"WebView navigation failed: {args.WebErrorStatus}.");

		private void UpdateStatus(string message)
		{
			if (LocalSamplePageLayout.GetSampleChild<TextBlock>(Design.Fluent, "WebViewStatus") is { } status)
			{
				AccessibilityHelper.Announce(status, message);
			}
		}
	}
}
