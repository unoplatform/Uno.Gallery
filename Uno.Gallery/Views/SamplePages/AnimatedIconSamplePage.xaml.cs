using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Gallery.Helpers;

namespace Uno.Gallery.Views.Samples;

[SamplePage(SampleCategory.UIComponents, "AnimatedIcon",
	Description = "AnimatedIcon maps semantic control states to marker-based transitions in an IAnimatedVisualSource and provides a static fallback icon when animation is unavailable.",
	DocumentationLink = "https://learn.microsoft.com/windows/apps/design/controls/animated-icon",
	Slug = "animated-icon",
	Tags = new[] { "animation", "icon", "state", "fallback" },
	Status = SampleStatus.Stable,
	Owner = "unoplatform",
	ReviewedOn = "2026-08-27",
	RelatedSamples = new[] { "animated-visual-player", "icon" })]
public sealed partial class AnimatedIconSamplePage : Page
{
	private int _transitionCount;

	public AnimatedIconSamplePage()
	{
		InitializeComponent();
	}

	private void AnimatedIcon_Loaded(object sender, RoutedEventArgs e)
	{
		var icon = (AnimatedIcon)sender;
		_transitionCount = 0;
		AnimatedIcon.SetState(icon, "NormalOff");
		UpdateStatus(icon);
	}

	private void SetAccepted_Click(object sender, RoutedEventArgs e) => SetIconState("NormalOn");

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		var icon = GetRequiredChild<AnimatedIcon>("AcceptIcon");
		if (AnimatedIcon.GetState(icon) != "NormalOff")
		{
			AnimatedIcon.SetState(icon, "NormalOff");
		}
		_transitionCount = 0;
		UpdateStatus(icon);
	}

	private void SetIconState(string state)
	{
		var icon = GetRequiredChild<AnimatedIcon>("AcceptIcon");
		if (AnimatedIcon.GetState(icon) != state)
		{
			AnimatedIcon.SetState(icon, state);
			_transitionCount++;
		}

		UpdateStatus(icon);
	}

	private void UpdateStatus(AnimatedIcon icon)
		=> AccessibilityHelper.Announce(
			GetRequiredChild<TextBlock>("AnimatedIconStatus"),
			$"State: {AnimatedIcon.GetState(icon)}; transitions: {_transitionCount}");

	private T GetRequiredChild<T>(string name) where T : FrameworkElement
		=> SamplePageLayoutRoot.GetSampleChild<T>(Design.Agnostic, name)
			?? throw new InvalidOperationException($"AnimatedIcon sample child '{name}' is not loaded.");
}
