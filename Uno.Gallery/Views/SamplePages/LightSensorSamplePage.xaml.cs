using System;
using System.ComponentModel;
using Uno.Gallery.ViewModels;
using Windows.Devices.Sensors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Light Sensor", Description = "Represents an ambient light sensor. The sensor returns illuminance in LUX.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/uwp/devices-sensors/use-the-light-sensor")]
	public sealed partial class LightSensorSamplePage : Page
	{
		public LightSensorSamplePage()
		{
			this.InitializeComponent();
		}

		private void ObserveReadingChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as Button)?.DataContext is LightSensorSamplePageViewModel viewModel)
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
	public class LightSensorSamplePageViewModel : ViewModelBase
	{
		private const string _startObservingContent = "Start observing light sensor reading changes";
		private const string _stopObservingContent = "Stop observing light sensor reading changes";
		private const string _sensorNotAvailable = "Light sensor is not available on this device/platform";

		private readonly LightSensor _lightSensor;

		public uint? ReportInterval
		{
			get => GetProperty<uint?>();
			set
			{
				if (_lightSensor != null && value != null)
				{
					_lightSensor.ReportInterval = (uint)value;
					SetProperty(value);
				}
			}
		}
		
		public DateTimeOffset? LastReadTimestamp { get => GetProperty<DateTimeOffset?>(); set => SetProperty(value); }

		public string ButtonContent { get => GetProperty<string>(); set => SetProperty(value); }

		public bool ObservingReadingChange { get => GetProperty<bool>(); set => SetProperty(value); }

		public double IlluminanceInLux { get => GetProperty<double>(); set => SetProperty(value); }


		public bool LightSensorAvailable => _lightSensor != null;

		public LightSensorSamplePageViewModel()
		{
			_lightSensor = LightSensor.GetDefault();
			
			if (_lightSensor != null)
			{
				ButtonContent = _startObservingContent;
			}
			else
			{
				ButtonContent = _sensorNotAvailable;
			}
		}

		public void StartObserveReadingChange()
		{
			_lightSensor.ReadingChanged += LightSensor_ReadingChanged;
			ButtonContent = _stopObservingContent;
			ObservingReadingChange = true;
		}

		public void StopObservingReadingChange()
		{
			_lightSensor.ReadingChanged -= LightSensor_ReadingChanged;
			ButtonContent = _startObservingContent;
			ObservingReadingChange = false;
		}

		private void LightSensor_ReadingChanged(LightSensor sender, LightSensorReadingChangedEventArgs args)
		{
			LastReadTimestamp = args.Reading.Timestamp;
			IlluminanceInLux = args.Reading.IlluminanceInLux;
		}
	}
}
