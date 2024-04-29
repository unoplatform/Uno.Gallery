using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

#if HAS_UNO
using Windows.Phone.Devices.Notification;
#endif

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Vibration", Description = "Activates the vibration device for a certain amount of time.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.devices.haptics.vibrationdevice")]
#if !HAS_UNO
	[SampleConditional(SampleConditionals.Disabled, Reason = "API is not supported")]
#endif
	public sealed partial class VibrationDeviceSamplePage : Page
	{
        public VibrationDeviceSamplePage()
		{
			this.InitializeComponent();
		}

		private void Vibrate100_Click(object sender, RoutedEventArgs args)
		{
#if HAS_UNO
			var vibrationDevice = VibrationDevice.GetDefault();
			if (vibrationDevice != null)
			{
				vibrationDevice.Vibrate(TimeSpan.FromMilliseconds(100));
			}
#endif
		}

		private void Vibrate1000_Click(object sender, RoutedEventArgs args)
		{
#if HAS_UNO
			var vibrationDevice = VibrationDevice.GetDefault();
			if (vibrationDevice != null)
			{
				vibrationDevice.Vibrate(TimeSpan.FromMilliseconds(1000));
			}
#endif
		}
	}
}
