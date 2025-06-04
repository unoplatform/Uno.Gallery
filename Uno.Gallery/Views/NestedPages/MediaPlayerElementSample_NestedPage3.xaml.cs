using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.NestedPages
{
    public sealed partial class MediaPlayerElementSample_NestedPage3 : Page
    {
        public MediaPlayerElementSample_NestedPage3()
        {
            this.InitializeComponent();
			Unloaded += MediaPlayerElementSample_NestedPage3_Unloaded;
        }

		private void NavigateBack(object sender, RoutedEventArgs e)
		{
			MediaPlayerElementSample3.MediaPlayer.Pause();
			Shell.GetForElement(this).BackNavigateFromNestedSample();
		}

		private void MediaPlayerElementSample_NestedPage3_Unloaded(object sender, RoutedEventArgs e)
		{
			MediaPlayerElementSample3.MediaPlayer.Pause();
		}
	}
}
