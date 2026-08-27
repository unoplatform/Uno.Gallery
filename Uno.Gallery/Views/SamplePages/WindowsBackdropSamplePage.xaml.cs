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
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Native,
	Requirements = new[] { "Requires the Windows target; Mica and Desktop Acrylic additionally depend on OS and device support." },
	AccessibilityNotes = new[] { "Backdrop actions are keyboard-focusable and AppWindow capability and application results are exposed as text." },
	ResetBehavior = "Choose Clear or leave the sample to remove the backdrop and restore the Gallery background.",
	Variants = new[] { "Mica", "Desktop Acrylic", "No system backdrop", "AppWindow identity and size" },
	KnownLimitations = new[] { "Backdrop availability depends on Windows version, transparency settings, and device capabilities." },
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "composition-visuals", "diagnostics" })]
[SampleConditional(SampleConditionals.Windows,
	Reason = "Mica, Desktop Acrylic, and Win32 AppWindow interop are exposed only by the Windows target.")]
public sealed partial class WindowsBackdropSamplePage : Page
{
#if WINDOWS
	private Brush? _shellBackground;
	private Brush? _pageBackground;
	private Grid? _shellRoot;
	private bool _backdropSurfacePrepared;
#endif

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
		if (appWindow is null)
		{
			((TextBlock)sender).Text = "AppWindow is unavailable for the Gallery window.";
			return;
		}
		((TextBlock)sender).Text =
			$"AppWindow: {appWindow.Title}; size: {appWindow.Size.Width}x{appWindow.Size.Height}; " +
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
			PrepareBackdropSurface();
			App.Instance.MainWindow.SystemBackdrop = factory();
			UpdateStatus($"{name} applied to the existing Gallery window.");
		}
		catch (Exception ex)
		{
			App.Instance.MainWindow.SystemBackdrop = null;
			RestoreBackdropSurface();
			UpdateStatus($"{name} failed: {ex.GetType().Name}.");
		}
	}

	private void PrepareBackdropSurface()
	{
		if (_backdropSurfacePrepared) return;

		_shellRoot = (App.Instance.MainWindow.Content as Shell)?.Content as Grid;
		_shellBackground = _shellRoot?.Background;
		_pageBackground = BackdropPageRoot.Background;
		if (_shellRoot is not null)
		{
			_shellRoot.Background = null;
		}
		BackdropPageRoot.Background = null;
		_backdropSurfacePrepared = true;
	}

	private void RestoreBackdropSurface()
	{
		if (!_backdropSurfacePrepared) return;

		if (_shellRoot is not null)
		{
			_shellRoot.Background = _shellBackground;
		}
		BackdropPageRoot.Background = _pageBackground;
		_shellRoot = null;
		_backdropSurfacePrepared = false;
	}
#endif

	private void ClearBackdrop()
	{
#if WINDOWS
		App.Instance.MainWindow.SystemBackdrop = null;
		RestoreBackdropSurface();
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
