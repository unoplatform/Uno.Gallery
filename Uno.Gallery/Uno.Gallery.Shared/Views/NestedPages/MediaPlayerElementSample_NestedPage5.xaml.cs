using System;
using System.Collections.Generic;
using Windows.Foundation.Metadata;
using Windows.Media.Core;
using Windows.Media.Playback;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;

namespace Uno.Gallery.Views.NestedPages
{
	public sealed partial class MediaPlayerElementSample_NestedPage5 : Page
	{
		public MediaPlayerElementSample_NestedPage5()
		{
			this.InitializeComponent();
			InitializePlaybackList();
			Unloaded += MediaPlayerElementSample_NestedPage5_Unloaded;
		}

		private void InitializePlaybackList()
		{
			
			if (ApiInformation.IsPropertyPresent("Windows.Media.Playback.MediaPlaybackList", "Items"))
			{
#pragma warning disable Uno0001 // Uno type or member is not implemented
				var mediaPlaybackList = new MediaPlaybackList();
				mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri("https://uno-assets.platform.uno/tests/videos/Mobile_Development_in_VS_Code_with_Uno_Platform_orDotNetMAUI.mp4"))));
				mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri("https://uno-assets.platform.uno/tests/audio/Getting_Started_with_Uno_Platform_and_Visual_Studio_Code.mp3"))));
				mediaPlaybackList.Items.Add(new MediaPlaybackItem(MediaSource.CreateFromUri(new Uri("https://uno-assets.platform.uno/tests/videos/Getting_Started_with_Uno_Platform_and_Visual_Studio_Code.mp4"))));
#pragma warning restore Uno0001 // Uno type or member is not implemented

				MediaPlayerElementSample5.MediaPlayer.Source = mediaPlaybackList;
			}
			else
			{
				if (MediaPlayerElementSample5.MediaPlayer == null)
				{
					MediaPlayerElementSample5.SetMediaPlayer(new Windows.Media.Playback.MediaPlayer());
				}
				MediaPlayerElementSample5.Source = MediaSource.CreateFromUri(new Uri("https://uno-assets.platform.uno/tests/videos/Mobile_Development_in_VS_Code_with_Uno_Platform_orDotNetMAUI.mp4"));
			}
		}

		private void NavigateBack(object sender, RoutedEventArgs e)
		{
			MediaPlayerElementSample5.MediaPlayer.Pause();
			Shell.GetForCurrentView().BackNavigateFromNestedSample();
		}

		private void MediaPlayerElementSample_NestedPage5_Unloaded(object sender, RoutedEventArgs e)
		{
			MediaPlayerElementSample5.MediaPlayer.Pause();
		}
	}
}
