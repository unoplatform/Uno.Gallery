using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.NestedPages
{
    public sealed partial class MediaPlayerElementSample_NestedPage4 : Page
    {
        public MediaPlayerElementSample_NestedPage4()
        {
            this.InitializeComponent();
			Unloaded += MediaPlayerElementSample_NestedPage4_Unloaded;
        }

		private void NavigateBack(object sender, RoutedEventArgs e)
		{
			MediaPlayerElementSample4.MediaPlayer.Pause();
			Shell.GetForCurrentView().BackNavigateFromNestedSample();
		}

		private void MediaPlayerElementSample_NestedPage4_Unloaded(object sender, RoutedEventArgs e)
		{
			MediaPlayerElementSample4.MediaPlayer.Pause();
		}
	}
}
