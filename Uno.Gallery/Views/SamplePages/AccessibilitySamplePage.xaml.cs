using System;
using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.Accessibility, "Accessibility",
	Description = "Keyboard focus, semantic names and states, live announcements, contrast, reduced motion, and renderer-specific accessibility expectations.",
	DocumentationLink = "https://platform.uno/docs/articles/features/working-with-accessibility.html",
	Slug = "accessibility",
	Tags = new[] { "accessibility", "keyboard", "screen-reader", "contrast", "reduced-motion" },
	Status = SampleStatus.Stable,
	ContractVersion = 1,
	SupportedDesigns = SampleDesigns.Agnostic,
	SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
	Requirements = new[] { SampleContractDefaults.NoExternalRequirements },
	AccessibilityNotes = new[] { "Use the keyboard, a screen reader, high contrast, and reduced-motion settings to exercise the documented semantics." },
	ResetBehavior = SampleContractDefaults.ReloadToReset,
	Variants = new[] { "Focus order and accessible naming", "Live announcements", "Contrast and high contrast", "Reduced motion" },
	Owner = "unoplatform",
	ReviewedOn = "2026-08-26",
	RelatedSamples = new[] { "localization-rtl" })]
public sealed partial class AccessibilitySamplePage : Page
{
	private int _announcementCount;
	private bool _motionAtEnd;

	public AccessibilitySamplePage()
	{
		InitializeComponent();
	}

	private void SaveProfile_Click(object sender, RoutedEventArgs e)
	{
		if (SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "AnnouncementStatus") is { } status)
		{
			_announcementCount++;
			AccessibilityHelper.Announce(status, string.Format(
				CultureInfo.CurrentCulture,
				LocalizationHelper.GetString(
					"AccessibilityAnnouncementSaved",
					"Profile saved. Announcement {0}."),
				_announcementCount));
		}
	}

	private void MoveTarget_Click(object sender, RoutedEventArgs e)
	{
		var target = SamplePageLayoutRoot.GetSampleChild<Border>(Design.Agnostic, "MotionTarget");
		var toggle = SamplePageLayoutRoot.GetSampleChild<ToggleSwitch>(Design.Agnostic, "ReduceMotionToggle");
		var status = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "MotionStatus");
		if (target?.RenderTransform is not TranslateTransform transform || toggle is null || status is null)
		{
			throw new InvalidOperationException("The reduced-motion sample is not loaded.");
		}

		_motionAtEnd = !_motionAtEnd;
		var destination = _motionAtEnd ? 120d : 0d;
		if (toggle.IsOn)
		{
			transform.X = destination;
			status.Tag = "Reduced";
			status.Text = string.Format(
				CultureInfo.CurrentCulture,
				LocalizationHelper.GetString(
					"AccessibilityMotionReduced",
					"Motion reduced: target updated instantly to {0:0}."),
				destination);
			return;
		}

		var animation = new DoubleAnimation
		{
			To = destination,
			Duration = new Duration(TimeSpan.FromMilliseconds(350)),
			EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut }
		};
		Storyboard.SetTarget(animation, transform);
		Storyboard.SetTargetProperty(animation, nameof(TranslateTransform.X));
		var storyboard = new Storyboard();
		storyboard.Children.Add(animation);
		storyboard.Begin();
		status.Tag = "Animated";
		status.Text = string.Format(
			CultureInfo.CurrentCulture,
			LocalizationHelper.GetString(
				"AccessibilityMotionEnabled",
				"Motion enabled: target animating to {0:0}."),
			destination);
	}

	private void RendererStatus_Loaded(object sender, RoutedEventArgs e)
	{
		var platform =
#if __WASM__
			"WebAssembly";
#elif WINDOWS
			"Windows WinAppSDK";
#elif __ANDROID__
			"Android";
#elif __MACCATALYST__
			"macOS Catalyst";
#elif __IOS__
			"iOS";
#elif __DESKTOP__
			"Desktop";
#else
			"Unknown platform";
#endif

		((TextBlock)sender).Text =
			$"{platform} | {BuildInfo.Renderer}. Consult the accessibility renderer matrix for automated and manual coverage.";
	}
}
