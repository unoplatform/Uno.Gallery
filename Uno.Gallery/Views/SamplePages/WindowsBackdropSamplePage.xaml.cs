using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Helpers;

#if WINDOWS
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Media;
using WinRT.Interop;
#endif

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIFeatures, "Windows Backdrops",
	Description = "Windows-only SystemBackdrop selection and AppWindow availability for the existing Gallery window.",
	DocumentationLink = "https://learn.microsoft.com/windows/apps/windows-app-sdk/system-backdrop-controller",
	Slug = "windows-backdrops",
	Tags = new[] { "windows", "windowing", "mica", "acrylic", "appwindow" },
	Status = SampleStatus.Stable,
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "composition-visuals", "diagnostics" })]
[SampleConditional(SampleConditionals.Windows,
	Reason = "Mica, Desktop Acrylic, and Win32 AppWindow interop are exposed only by the Windows target.")]
public sealed partial class WindowsBackdropSamplePage : Page
{
	public WindowsBackdropSamplePage()
	{
		InitializeComponent();
		Unloaded += (_, _) => ClearBackdrop();
	}

	private void WindowStatus_Loaded(object sender, RoutedEventArgs e)
	{
#if WINDOWS
		var hwnd = WindowNative.GetWindowHandle(App.Instance.MainWindow);
		var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
		var appWindow = AppWindow.GetFromWindowId(windowId);
		((TextBlock)sender).Text =
			$"AppWindow available: {appWindow is not null}; " +
			$"Mica supported: {MicaController.IsSupported()}; " +
			$"Desktop Acrylic supported: {DesktopAcrylicController.IsSupported()}.";
#else
		((TextBlock)sender).Text = "Unavailable: this page is compiled out of non-Windows catalogs.";
#endif
	}

	private void ApplyMica_Click(object sender, RoutedEventArgs e)
	{
#if WINDOWS
		ApplyBackdrop(
			MicaController.IsSupported(),
			static () => new MicaBackdrop(),
			"Mica");
#endif
	}

	private void ApplyAcrylic_Click(object sender, RoutedEventArgs e)
	{
#if WINDOWS
		ApplyBackdrop(
			DesktopAcrylicController.IsSupported(),
			static () => new DesktopAcrylicBackdrop(),
			"Desktop Acrylic");
#endif
	}

	private void ClearBackdrop_Click(object sender, RoutedEventArgs e)
	{
		ClearBackdrop();
		UpdateStatus("SystemBackdrop cleared; AppWindow remains owned by the Gallery navigator.");
	}

#if WINDOWS
	private void ApplyBackdrop(bool supported, Func<SystemBackdrop> factory, string name)
	{
		if (!supported)
		{
			UpdateStatus($"{name} is unavailable on this Windows system.");
			return;
		}

		try
		{
			App.Instance.MainWindow.SystemBackdrop = factory();
			UpdateStatus($"{name} applied to the existing Gallery window.");
		}
		catch (Exception ex)
		{
			UpdateStatus($"{name} failed: {ex.GetType().Name}.");
		}
	}
#endif

	private void ClearBackdrop()
	{
#if WINDOWS
		App.Instance.MainWindow.SystemBackdrop = null;
#endif
	}

	private void UpdateStatus(string message)
	{
		if (SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "Backdrop_Status") is { } status)
		{
			AccessibilityHelper.Announce(status, message);
		}
	}
}
