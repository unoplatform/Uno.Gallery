using System;
using Uno.Gallery.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Devices.Sensors;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Magnetometer", Description = "Represents an magnetic sensor. This sensor allows measuring magnetic force affecting the device.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.devices.sensors.magnetometer")]
	public sealed partial class MagnetometerSamplePage : Page
	{
		public MagnetometerSamplePage()
		{
			this.InitializeComponent();
		}

		private void ObserveReadingChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as Button)?.DataContext is MagnetometerSamplePageViewModel viewModel)
			{
				if (!viewModel.ObservingReadingChange)
				{
					viewModel.StartObserveReadingChange();
				}
				else
				{
					viewModel.StopObservingReadingChange();
				}
			}
		}
	}

	[Microsoft.UI.Xaml.Data.Bindable]
	public class MagnetometerSamplePageViewModel : ViewModelBase
	{
		private const string _startObservingContent = "Start observing magnetometer reading changes";
		private const string _stopObservingContent = "Stop observing magnetometer reading changes";
		private const string _magnetometerNotAvailableContent = "Magnetometer is not available on this device/platform";

		private readonly Magnetometer _magnetometer;

		public uint? ReportInterval
		{
			get => GetProperty<uint?>();
			set
			{
				if (_magnetometer != null && value != null)
					SetProperty<uint?>((uint)value);
			}
		}

		public DateTimeOffset? LastReadTimestamp
		{
			get => GetProperty<DateTimeOffset?>();
			set => SetProperty<DateTimeOffset?>(value);
		}

		public string ButtonContent
		{
			get => GetProperty<string>();
			set => SetProperty<string>(value);
		}

		public bool ObservingReadingChange
		{
			get => GetProperty<bool>();
			set => SetProperty<bool>(value);
		}

		public float MagneticFieldX
		{
			get => GetProperty<float>();
			set => SetProperty<float>(value);
		}

		public float MagneticFieldY
		{
			get => GetProperty<float>();
			set => SetProperty<float>(value);
		}

		public float MagneticFieldZ
		{
			get => GetProperty<float>();
			set => SetProperty<float>(value);
		}

		public MagnetometerAccuracy DirectionalAccuracy
		{
			get => GetProperty<MagnetometerAccuracy>();
			set => SetProperty<MagnetometerAccuracy>(value);
		}

		public bool MagnetometerAvailable => _magnetometer != null;

		public MagnetometerSamplePageViewModel()
		{
			_magnetometer = Magnetometer.GetDefault();

			if (_magnetometer != null)
			{
				ButtonContent = _startObservingContent;
				ReportInterval = 60;
			}
			else
			{
				ButtonContent = _magnetometerNotAvailableContent;
			}
		}

		public void StartObserveReadingChange()
		{
			_magnetometer.ReadingChanged += Magnetometer_ReadingChanged;
			ButtonContent = _stopObservingContent;
			ObservingReadingChange = true;
		}

		public void StopObservingReadingChange()
		{
			_magnetometer.ReadingChanged -= Magnetometer_ReadingChanged;
			ButtonContent = _startObservingContent;
			ObservingReadingChange = false;
		}

		private async void Magnetometer_ReadingChanged(Magnetometer sender, MagnetometerReadingChangedEventArgs args)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				MagneticFieldX = args.Reading.MagneticFieldX;
				MagneticFieldY = args.Reading.MagneticFieldY;
				MagneticFieldZ = args.Reading.MagneticFieldZ;
				DirectionalAccuracy = args.Reading.DirectionalAccuracy;
				LastReadTimestamp = args.Reading.Timestamp;
			});
		}
	}
}
