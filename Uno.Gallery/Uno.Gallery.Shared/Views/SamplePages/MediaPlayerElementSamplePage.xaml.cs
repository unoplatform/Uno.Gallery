using Uno.Gallery.Views.NestedPages;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.UIComponents, "MediaPlayerElement", Description = "Represents an object that uses a MediaPlayer to render audio and video to the display.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.mediaplayerelement")]
	public sealed partial class MediaPlayerElementSamplePage : Page
	{
		public MediaPlayerElementSamplePage()
		{
			this.InitializeComponent();
		}

		private void LaunchMediaPlayerElementSample1(object sender, RoutedEventArgs e)
		{
			Shell.GetForCurrentView().ShowNestedSample<MediaPlayerElementSample_NestedPage1>(clearStack: true);
		}

		private void LaunchMediaPlayerElementSample2(object sender, RoutedEventArgs e)
		{
			Shell.GetForCurrentView().ShowNestedSample<MediaPlayerElementSample_NestedPage2>(clearStack: true);
		}

		private void LaunchMediaPlayerElementSample3(object sender, RoutedEventArgs e)
		{
			Shell.GetForCurrentView().ShowNestedSample<MediaPlayerElementSample_NestedPage3>(clearStack: true);
		}

		private void LaunchMediaPlayerElementSample4(object sender, RoutedEventArgs e)
		{
			Shell.GetForCurrentView().ShowNestedSample<MediaPlayerElementSample_NestedPage4>(clearStack: true);
		}

		private void LaunchMediaPlayerElementSample5(object sender, RoutedEventArgs e)
		{
			Shell.GetForCurrentView().ShowNestedSample<MediaPlayerElementSample_NestedPage5>(clearStack: true);
		}
	}
}
