using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Uno.Gallery.Views.NestedPages
{
    public sealed partial class MediaPlayerElementSample_NestedPage1 : Page
    {
        public MediaPlayerElementSample_NestedPage1()
        {
            this.InitializeComponent();
			Unloaded += MediaPlayerElementSample_NestedPage1_Unloaded;
		}

		private void NavigateBack(object sender, RoutedEventArgs e)
		{
			MediaPlayerElementSample1.MediaPlayer.Pause();
			Shell.GetForElement(this).BackNavigateFromNestedSample();
		}

		private void MediaPlayerElementSample_NestedPage1_Unloaded(object sender, RoutedEventArgs e)
		{
			MediaPlayerElementSample1.MediaPlayer.Pause();
		}
	}
}
