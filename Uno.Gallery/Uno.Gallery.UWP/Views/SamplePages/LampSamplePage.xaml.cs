using System;
using System.Threading.Tasks;
using Uno.Gallery.ViewModels;
using Windows.Devices.Lights;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Lamp", Description = "Represents a lamp device. Allows you to turn the phone's camera flashlight on and off.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/Windows.Devices.Lights.Lamp", DataType = typeof(LampSamplePageViewModel))]
#if !WINDOWS_UWP && !IS_WINUI && !__ANDROID__ && !__IOS__
	[SampleConditional(SampleConditionals.Always, Reason = "API is not supported")]
#endif
	public sealed partial class LampSamplePage : Page
	{
		public LampSamplePage()
		{
			this.InitializeComponent();
		}

		private void ChangeLampState_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as Button)?.DataContext is LampSamplePageViewModel viewModel)
			{
				if (!viewModel.IsEnabled)
				{
					viewModel.Enable();
				}
				else
				{
					viewModel.Disable();
				}
			}
		}
	}

	public class LampSamplePageViewModel : ViewModelBase, IDisposable
	{
		private const string _enable = "Enable flashlight";
		private const string _disable = "Disable flashlight";
		private const string _unavailable = "Flashlight is not available on this device or platform";
		private Lamp _lamp;

		public string ButtonContent
		{
			get => GetProperty<string>();
			set => SetProperty<string>(value);
		}

		public bool IsEnabled
		{
			get => GetProperty<bool>();
			set => SetProperty<bool>(value);
		}

		public bool IsBrightnessSupported
		{
			get => GetProperty<bool>();
			set => SetProperty<bool>(value);
		}

		public bool IsAvailable
		{
			get => GetProperty<bool>();
			set => SetProperty<bool>(value);
		}

		public float BrightnessLevel
		{
			get => GetProperty<float>();
			set => SetProperty<float>(value);
		}

		public LampSamplePageViewModel()
		{
			ButtonContent = _enable;
			IsAvailable = true;
			IsBrightnessSupported = false;
			BrightnessLevel = 1;
#if __ANDROID__ || __IOS__
			IsBrightnessSupported = true;
#endif
			Task.Run(async () =>
			{
				_lamp = await Lamp.GetDefaultAsync();
				IsAvailable = _lamp != null;
				if (!IsAvailable)
					ButtonContent = _unavailable;
			});
		}

		public void Enable()
		{
			if (_lamp != null)
			{
				ButtonContent = _disable;
				_lamp.BrightnessLevel = BrightnessLevel;
				_lamp.IsEnabled = true;
				IsEnabled = true;
			}
			else
			{
				IsAvailable = false;
				ButtonContent = _unavailable;
			}
		}

		public void Disable()
		{
			if (_lamp != null)
			{
				_lamp.IsEnabled = false;
				IsEnabled = false;
				ButtonContent = _enable;
			}
		}

		public void Dispose()
		{
			if (_lamp != null)
				_lamp.Dispose();
		}
	}
}
