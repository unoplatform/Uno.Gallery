using System;
using System.Threading.Tasks;
using Uno.Gallery.ViewModels;
using Windows.Devices.Sensors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Hinge Angle Sensor", Description = "Represents a hinge angle sensor. This sensor exposes the angle of the device hinge.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.devices.sensors.hingeanglesensor?view=winrt-22621")]
	[SampleConditional(SampleConditionals.Droid, Reason = "Hinge Angle Sensor is currently only available on Android")]
	public sealed partial class HingeAngleSensorSamplePage : Page
	{
		public HingeAngleSensorSamplePage()
		{
			this.InitializeComponent();
		}

		private void ObserveReadingChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as Button)?.DataContext is HingeAngleSensorSamplePageViewModel viewModel)
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

	public class HingeAngleSensorSamplePageViewModel : ViewModelBase
	{
		private const string _startObservingContent = "Start observing hinge angle sensor reading changes";
		private const string _stopObservingContent = "Stop observing hinge angle sensor reading changes";
		private const string _sensorNotAvailableContent = "Hinge angle sensor is not available on this device";

		private HingeAngleSensor _sensor;

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

		public double HingeAngle
		{
			get => GetProperty<double>();
			set => SetProperty<double>(value);
		}

		public DateTimeOffset LastReadTimestamp
		{
			get => GetProperty<DateTimeOffset>();
			set => SetProperty<DateTimeOffset>(value);
		}

		public bool HingeAngleSensorAvailable
		{
			get => GetProperty<bool>();
			set => SetProperty<bool>(value);
		}

		public HingeAngleSensorSamplePageViewModel()
		{
			Task.Run(async () =>
			{
				_sensor = await HingeAngleSensor.GetDefaultAsync();

				if (_sensor != null)
				{
					ButtonContent = _startObservingContent;
					HingeAngleSensorAvailable = true;
				}
				else
				{
					ButtonContent = _sensorNotAvailableContent;
				}
			});
		}

		public void StartObserveReadingChange()
		{
			_sensor.ReadingChanged += _sensor_ReadingChanged;
			ButtonContent = _stopObservingContent;
			ObservingReadingChange = true;
		}

		private void _sensor_ReadingChanged(HingeAngleSensor sender, HingeAngleSensorReadingChangedEventArgs args)
		{
			LastReadTimestamp = args.Reading.Timestamp;
			HingeAngle = args.Reading.AngleInDegrees;
		}

		public void StopObservingReadingChange()
		{
			_sensor.ReadingChanged -= _sensor_ReadingChanged;
			ButtonContent = _startObservingContent;
			ObservingReadingChange = false;
		}
	}
}
