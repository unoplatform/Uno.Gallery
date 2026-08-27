using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "WebView",
		Description = "Hosts deterministic offline HTML and reports navigation success or failure.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.webview2",
		Slug = "webview",
		Tags = new[] { "web", "html", "offline", "platform" },
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
				UpdateStatus("Offline HTML navigation started.");
			}
			catch (Exception ex)
			{
				UpdateStatus($"WebView unavailable: {ex.GetType().Name}.");
			}
		}

		private void WebView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
			=> UpdateStatus(args.IsSuccess
				? "Offline HTML loaded successfully."
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
