using System;
using Windows.System.Display;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Display Request", Description = "Represents a display request. It will keep the screen on even if the device is idle.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.system.display.displayrequest")]
    public sealed partial class DisplayRequestSamplePage : Page
	{
		private DisplayRequest _displayRequest;

        public DisplayRequestSamplePage()
		{
			this.InitializeComponent();
			_displayRequest = new DisplayRequest();
		}

		private void CheckBox_Click(object sender, RoutedEventArgs e)
		{
			var checkBox = sender as CheckBox;

			if (checkBox.IsChecked == true)
			{
				_displayRequest.RequestActive();
			}
			else
			{
				_displayRequest.RequestRelease();
			}
		}
	}
}
