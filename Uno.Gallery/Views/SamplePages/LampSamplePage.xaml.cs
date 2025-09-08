using System;
using System.Threading.Tasks;
using Uno.Gallery.ViewModels;
using Windows.Devices.Lights;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
#if __IOS__
using Uno.Gallery.Platforms.iOS;
#endif

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Lamp", Description = "Represents a lamp device. Allows you to turn the phone's camera flashlight on and off.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/Windows.Devices.Lights.Lamp", DataType = typeof(LampSamplePageViewModel))]
#if !WINDOWS && !IS_WINUI && !__ANDROID__ && !__IOS__
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
#if !__IOS__
		private Lamp? _lamp;
#endif

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
#if __IOS__
			IsBrightnessSupported = true;
			Task.Run(async () =>
			{
				try
				{
					var available = await FlashlightHelper.IsFlashlightAvailableAsync();
					IsAvailable = available;
					if (!available)
					{
						ButtonContent = _unavailable;
					}
				}
				catch (Exception ex)
				{
					IsAvailable = false;
					ButtonContent = _unavailable;
					System.Diagnostics.Debug.WriteLine($"Failed to check flashlight availability: {ex.Message}");
				}
			});
#else
			Task.Run(async () =>
			{
				try
				{
					_lamp = await Lamp.GetDefaultAsync();
					IsAvailable = _lamp != null;
					if (!IsAvailable)
					{
						ButtonContent = _unavailable;
					}
				}
				catch (Exception ex)
				{
					// Handle exceptions that might occur due to permission denial or other issues
					IsAvailable = false;
					ButtonContent = _unavailable;
					System.Diagnostics.Debug.WriteLine($"Failed to initialize lamp: {ex.Message}");
				}
			});
#endif
		}

		public async void Enable()
		{
#if __IOS__
			try
			{
				var success = await FlashlightHelper.SetFlashlightAsync(true, BrightnessLevel);
				if (success)
				{
					ButtonContent = _disable;
					IsEnabled = true;
				}
				else
				{
					IsAvailable = false;
					ButtonContent = _unavailable;
					IsEnabled = false;
				}
			}
			catch (Exception ex)
			{
				IsAvailable = false;
				ButtonContent = _unavailable;
				IsEnabled = false;
				System.Diagnostics.Debug.WriteLine($"Failed to enable flashlight: {ex.Message}");
			}
#else
			await Task.Run(() =>
			{
				if (_lamp != null)
				{
					try
					{
						ButtonContent = _disable;
						// Note: These APIs are not implemented in Uno Platform yet
						// _lamp.BrightnessLevel = BrightnessLevel;
						// _lamp.IsEnabled = true;
						IsEnabled = true;
					}
					catch (Exception ex)
					{
						// Handle exceptions that might occur when enabling the lamp
						IsAvailable = false;
						ButtonContent = _unavailable;
						IsEnabled = false;
						System.Diagnostics.Debug.WriteLine($"Failed to enable lamp: {ex.Message}");
					}
				}
				else
				{
					IsAvailable = false;
					ButtonContent = _unavailable;
				}
			});
#endif
		}

		public async void Disable()
		{
#if __IOS__
			try
			{
				var success = await FlashlightHelper.SetFlashlightAsync(false);
				if (success)
				{
					IsEnabled = false;
					ButtonContent = _enable;
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"Failed to disable flashlight: {ex.Message}");
			}
#else
			await Task.Run(() =>
			{
				if (_lamp != null)
				{
					try
					{
						// Note: This API is not implemented in Uno Platform yet
						// _lamp.IsEnabled = false;
						IsEnabled = false;
						ButtonContent = _enable;
					}
					catch (Exception ex)
					{
						System.Diagnostics.Debug.WriteLine($"Failed to disable lamp: {ex.Message}");
					}
				}
			});
#endif
		}

		public void Dispose()
		{
#if !__IOS__
			if (_lamp != null)
			{
				try
				{
					// Note: This API is not implemented in Uno Platform yet
					// _lamp.Dispose();
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Failed to dispose lamp: {ex.Message}");
				}
			}
#endif
		}
	}
}
