using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

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
		if (sender is AnimatedIcon icon)
		{
			_transitionCount = 0;
			AnimatedIcon.SetState(icon, "NormalOff");
			var status = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "AnimatedIconStatus");
			if (status is not null)
			{
				status.Text = "State: NormalOff; transitions: 0";
			}
		}
	}

	private void SetAccepted_Click(object sender, RoutedEventArgs e) => SetIconState("NormalOn");

	private void Reset_Click(object sender, RoutedEventArgs e)
	{
		_transitionCount = -1;
		SetIconState("NormalOff");
	}

	private void SetIconState(string state)
	{
		var icon = SamplePageLayoutRoot.GetSampleChild<AnimatedIcon>(Design.Agnostic, "AcceptIcon");
		var status = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "AnimatedIconStatus");
		if (icon is null || status is null)
		{
			return;
		}

		AnimatedIcon.SetState(icon, state);
		_transitionCount++;
		status.Text = $"State: {AnimatedIcon.GetState(icon)}; transitions: {_transitionCount}";
	}
}
