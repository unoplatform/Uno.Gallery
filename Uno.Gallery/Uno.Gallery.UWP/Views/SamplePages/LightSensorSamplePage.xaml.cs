using System;
using System.ComponentModel;
using Windows.Devices.Sensors;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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

	public class LightSensorSamplePageViewModel : INotifyPropertyChanged
	{
		private const string _startObservingContent = "Start observing light sensor reading changes";
		private const string _stopObservingContent = "Stop observing light sensor reading changes";

		private readonly LightSensor _lightSensor;
		private DateTimeOffset? _lastReadTimestamp;
		private string _buttonContent = "Light sensor is not available on this device/platform";
		private bool _observingReadingChange = false;
		private double _illuminanceInLux;

		public event PropertyChangedEventHandler PropertyChanged;

		public uint? ReportInterval
		{
			get => _lightSensor?.ReportInterval;
			set
			{
				if (_lightSensor != null && value != null)
				{
					_lightSensor.ReportInterval = (uint)value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReportInterval)));
				}
			}
		}
		
		public DateTimeOffset? LastReadTimestamp
		{
			get => _lastReadTimestamp;
			set
			{
				_lastReadTimestamp = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastReadTimestamp)));
			}
		}

		public string ButtonContent
		{
			get => _buttonContent;
			set
			{
				_buttonContent = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ButtonContent)));
			}
		}

		public bool ObservingReadingChange
		{
			get => _observingReadingChange;
			set
			{
				_observingReadingChange = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ObservingReadingChange)));
			}
		}

		public double IlluminanceInLux
		{
			get => _illuminanceInLux;
			set
			{
				_illuminanceInLux = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IlluminanceInLux)));
			}
		}


		public bool LightSensorAvailable => _lightSensor != null;

		public LightSensorSamplePageViewModel()
		{
			_lightSensor = LightSensor.GetDefault();
			
			if (_lightSensor != null)
			{
				ButtonContent = _startObservingContent;
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
