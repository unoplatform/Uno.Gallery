using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Phone.Devices.Notification;

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
				vibrationDevice.Vibrate(TimeSpan.FromMilliseconds(1000));
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
