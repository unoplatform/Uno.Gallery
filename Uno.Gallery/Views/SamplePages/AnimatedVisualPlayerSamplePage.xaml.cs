using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "AnimatedVisualPlayer",
		Description = Description,
		DocumentationLink = "https://learn.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.animatedvisualplayer",
		Slug = "animated-visual-player",
		Tags = new[] { "animation", "lottie", "visual", "player" },
		Status = SampleStatus.Stable,
		ContractVersion = 1,
		SupportedDesigns = SampleDesigns.Agnostic,
		SupportedRenderers = SampleRenderers.Native | SampleRenderers.Skia | SampleRenderers.DOM,
		Requirements = new[] { "The Lottie animation is bundled with the Gallery; no network access is required." },
		AccessibilityNotes = new[] { "Playback is controlled by a named button and a text status reports stopped and completed states." },
		ResetBehavior = SampleContractDefaults.ReloadToReset,
		Variants = new[] { "Manual play and stop", "Automatic playback", "Looped and one-shot playback" },
		KnownLimitations = new[] { "Animation timing and frame cadence can differ between rendering backends." },
		Owner = "unoplatform",
		ReviewedOn = "2026-08-26",
		RelatedSamples = new[] { "lottie" })]
	public sealed partial class AnimatedVisualPlayerSamplePage : Page
	{
		private const string Description =
			"AnimatedVisualPlayer is the core animation primitive in WinUI that renders an IAnimatedVisualSource " +
			"such as a Lottie JSON animation. It supports manual play/stop via PlayAsync, looped and one-shot " +
			"playback, and position control. AnimatedIcon builds on top of it to add state-driven transitions.";

		private int _animationPlayCount;

		public AnimatedVisualPlayerSamplePage()
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

			// FindName does not cross XamlDisplay/DataTemplate scope boundaries.
			// Use GetSampleChild which walks the visual tree from the ContentPresenter.
			var statusText = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "AnimationStatusText");
			var countText = SamplePageLayoutRoot.GetSampleChild<TextBlock>(Design.Agnostic, "AnimPlayCount");

			if (player.IsPlaying)
			{
				player.Stop();
				if (statusText != null) statusText.Text = "Status: Stopped";
			}
			else
			{
				if (statusText != null) statusText.Text = "Status: Playing\u2026";
				await player.PlayAsync(0, 1, looped: false);
				_animationPlayCount++;
				if (countText != null) countText.Text = $"Completed plays: {_animationPlayCount}";
				if (statusText != null) statusText.Text = "Status: Stopped";
			}
		}
	}
}
