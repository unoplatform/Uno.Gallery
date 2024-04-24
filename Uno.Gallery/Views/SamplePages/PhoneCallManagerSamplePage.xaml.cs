using Uno.Gallery.ViewModels;
using Windows.ApplicationModel.Calls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "PhoneCallManager", Description = "Allows apps to present a phone call UI to the user to allow them to initiate a call.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.applicationmodel.calls.phonecallmanager?view=winrt-22000")]
	[SampleConditional(SampleConditionals.Mobile | SampleConditionals.Wasm, Reason = "PhoneCallManager is currently only implemented on Android, iOS and WASM")]
	public sealed partial class PhoneCallManagerSamplePage : Page
	{
		public PhoneCallManagerSamplePage()
		{
			this.InitializeComponent();
		}

		private void ShowPhoneCallUI_Click(object sender, RoutedEventArgs e)
		{
			PhoneCallManager.ShowPhoneCallUI("123456789", "Jon Doe");
		}

		private void ShowPhoneCallSettingsUI_Click(object sender, RoutedEventArgs e)
		{
			PhoneCallManager.ShowPhoneCallSettingsUI();
		}

		private void ObservePhoneCallState_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as Button)?.DataContext is PhoneCallManagerSamplePageViewModel viewModel)
			{
				if (!viewModel.IsMonitoring)
				{
					viewModel.StartMonitoring();
				}
				else
				{
					viewModel.StopMonitoring();
				}
			}
		}
	}

	public class PhoneCallManagerSamplePageViewModel : ViewModelBase
	{
		public bool IsSettingsUISupported { get; set; }
		public bool IsCallStateSupported { get; set; }

		public bool IsMonitoring { get; set; }

		public bool IsCallActive
		{
			get => GetProperty<bool>();
			set
			{
				SetProperty<bool>(value);
			}
		}

		public bool IsCallIncoming
		{
			get => GetProperty<bool>();
			set
			{
				SetProperty<bool>(value);
			}
		}

		public PhoneCallManagerSamplePageViewModel()
		{
#if __ANDROID__
			IsSettingsUISupported = true;
			IsCallStateSupported = true;
#elif __IOS__
			IsCallStateSupported = true;
#endif
		}

		public void StartMonitoring()
		{
			PhoneCallManager.CallStateChanged += PhoneCallManager_CallStateChanged;
			IsMonitoring = true;
		}

		private void PhoneCallManager_CallStateChanged(object sender, object e)
		{
			IsCallActive = PhoneCallManager.IsCallActive;
			IsCallIncoming = PhoneCallManager.IsCallIncoming;
		}

		public void StopMonitoring()
		{
			PhoneCallManager.CallStateChanged -= PhoneCallManager_CallStateChanged;
			IsMonitoring = false;
		}
	}
}
