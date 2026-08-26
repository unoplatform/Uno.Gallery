using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "AnimatedIcon",
		Description = Description,
		DocumentationLink = "https://learn.microsoft.com/windows/apps/design/controls/animated-icon",
		Slug = "animated-icon",
		Tags = new[] { "icon", "animation", "lottie", "visual" },
		Status = SampleStatus.Preview,
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "Icon" })]
	public sealed partial class AnimatedIconSamplePage : Page
	{
		private const string Description =
			"AnimatedIcon plays a Lottie animation in response to state changes driven by " +
			"the AnimatedIcon.State attached property. Production use requires an " +
			"IAnimatedVisualSource2 class generated from a Lottie JSON file via the LottieGen tool. " +
			"This page demonstrates AnimatedVisualPlayer — the underlying animation primitive — " +
			"with the same Lottie source.";

		public AnimatedIconSamplePage()
		{
			this.InitializeComponent();
		}

		private async void OnPlayAnimationClick(object sender, RoutedEventArgs e)
		{
			if (sender is not Button btn) return;

			// AnimPlayer lives inside the Button's Content StackPanel (first child).
			var contentPanel = btn.Content as StackPanel;
			var player = contentPanel?.Children.OfType<AnimatedVisualPlayer>().FirstOrDefault();
			if (player is null) return;

			// AnimationStatusText is a sibling of the Button in the outer StackPanel.
			var statusText = (btn.Parent as FrameworkElement)?.FindName("AnimationStatusText") as TextBlock;

			if (player.IsPlaying)
			{
				player.Stop();
				if (statusText != null) statusText.Text = "Status: Stopped";
			}
			else
			{
				if (statusText != null) statusText.Text = "Status: Playing…";
				await player.PlayAsync(0, 1, looped: false);
				if (statusText != null) statusText.Text = "Status: Stopped";
			}
		}
	}
}
