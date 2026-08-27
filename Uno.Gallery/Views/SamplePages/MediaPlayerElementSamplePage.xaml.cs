using Uno.Gallery.Views.NestedPages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using Uno.Gallery.Helpers;
using Windows.Foundation.Metadata;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "MediaPlayerElement",
		Description = "Exercises local and remote media scenarios while visibly reporting API availability; codec and network failures remain platform-dependent.",
		DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.mediaplayerelement",
		Slug = "mediaplayerelement",
		Tags = new[] { "media", "video", "audio", "platform" },
		Owner = "unoplatform",
		ReviewedOn = "2026-08-27")]
	public sealed partial class MediaPlayerElementSamplePage : Page
	{
		public MediaPlayerElementSamplePage()
		{
			this.InitializeComponent();
			Loaded += MediaPlayerElementSamplePage_Loaded;
		}

		private void MediaPlayerElementSamplePage_Loaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
		{
			var playlistAvailable = ApiInformation.IsPropertyPresent("Windows.Media.Playback.MediaPlaybackList", "Items");
			if (!playlistAvailable)
			{
				Button buttonSampleButton5 = (Button)LocalSamplePageLayout.FindName("LaunchMediaPlayerElementSampleButton5");
				TextBlock textBlockSampleButton5 = (TextBlock)LocalSamplePageLayout.FindName("LaunchMediaPlayerElementSampleTextBlock5");
				if (buttonSampleButton5 != null && textBlockSampleButton5 != null)
				{
					buttonSampleButton5.Visibility = Visibility.Collapsed;
					textBlockSampleButton5.Visibility = Visibility.Collapsed;
				}
			}

			if ((TextBlock)LocalSamplePageLayout.FindName("MediaAvailabilityStatus") is { } status)
			{
				AccessibilityHelper.Announce(status, playlistAvailable
					? "Media APIs available. Local playback avoids network; actual codec support is supplied by the target platform."
					: "Media playback-list API unavailable. The playlist scenario is hidden rather than shown as a no-op.");
			}
		}

		private void LaunchMediaPlayerElementSample1(object sender, RoutedEventArgs e)
		{
			Shell.GetForElement(this).ShowNestedSample<MediaPlayerElementSample_NestedPage1>(clearStack: true);
		}

		private void LaunchMediaPlayerElementSample2(object sender, RoutedEventArgs e)
		{
			Shell.GetForElement(this).ShowNestedSample<MediaPlayerElementSample_NestedPage2>(clearStack: true);
		}

		private void LaunchMediaPlayerElementSample3(object sender, RoutedEventArgs e)
		{
			Shell.GetForElement(this).ShowNestedSample<MediaPlayerElementSample_NestedPage3>(clearStack: true);
		}

		private void LaunchMediaPlayerElementSample4(object sender, RoutedEventArgs e)
		{
			Shell.GetForElement(this).ShowNestedSample<MediaPlayerElementSample_NestedPage4>(clearStack: true);
		}

		private void LaunchMediaPlayerElementSample5(object sender, RoutedEventArgs e)
		{
			Shell.GetForElement(this).ShowNestedSample<MediaPlayerElementSample_NestedPage5>(clearStack: true);
		}
	}
}
