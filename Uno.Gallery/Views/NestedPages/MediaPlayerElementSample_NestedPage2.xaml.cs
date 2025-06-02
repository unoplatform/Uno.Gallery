using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.NestedPages
{
    public sealed partial class MediaPlayerElementSample_NestedPage2 : Page
    {
        public MediaPlayerElementSample_NestedPage2()
        {
            this.InitializeComponent();
			Unloaded += MediaPlayerElementSample_NestedPage2_Unloaded;
        }

		private void NavigateBack(object sender, RoutedEventArgs e)
		{
			MediaPlayerElementSample2.MediaPlayer.Pause();
			Shell.GetForElement(this).BackNavigateFromNestedSample();
		}
		private void MediaPlayerElementSample_NestedPage2_Unloaded(object sender, RoutedEventArgs e)
		{
			MediaPlayerElementSample2.MediaPlayer.Pause();
		}
	}
}
