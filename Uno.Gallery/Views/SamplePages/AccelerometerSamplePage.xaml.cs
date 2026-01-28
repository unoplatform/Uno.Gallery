using System;
using System.ComponentModel;
using Windows.ApplicationModel.Core;
using Windows.Devices.Sensors;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Accelerometer", Description = "Represents an accelerometer sensor. This sensor returns G-force values with respect to the x, y, and z axes.", DocumentationLink = "https://learn.microsoft.com/en-us/windows/uwp/devices-sensors/use-the-accelerometer")]
	public sealed partial class AccelerometerSamplePage : Page
	{
		public AccelerometerSamplePage()
		{
			this.InitializeComponent();
		}

		private void ObserveReadingChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as Button)?.DataContext is AccelerometerSamplePageViewModel viewModel)
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
	public class AccelerometerSamplePageViewModel: INotifyPropertyChanged
	{
		private const string _startObservingContent = "Start observing accelerometer reading changes";
		private const string _stopObservingContent = "Stop observing accelerometer reading changes";

		private readonly Accelerometer _accelerometer;
		private uint _shakeCount = 0;
		private DateTimeOffset? _lastShakeTimestamp;
		private DateTimeOffset? _lastReadTimestamp;
		private string _buttonContent = "Accelerometer is not available on this device/platform";
		private bool _observingReadingChange = false;
		private double _accelerationX;
		private double _accelerationY;
		private double _accelerationZ;

		public event PropertyChangedEventHandler PropertyChanged;

		public uint? ReportInterval
		{
			get => _accelerometer?.ReportInterval;
			set
			{
				if (_accelerometer != null && value != null)
				{
					_accelerometer.ReportInterval = (uint)value;
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReportInterval)));
				}
			}
		}

		public uint ShakeCount
		{
			get => _shakeCount;
			set
			{
				_shakeCount = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShakeCount)));
			}
		}

		public DateTimeOffset? LastShakeTimestamp
		{
			get => _lastShakeTimestamp;
			set
			{
				_lastShakeTimestamp = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastShakeTimestamp)));
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

		public double AccelerationX
		{
			get => _accelerationX;
			set
			{
				_accelerationX = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccelerationX)));
			}
		}

		public double AccelerationY
		{
			get => _accelerationY;
			set
			{
				_accelerationY = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccelerationY)));
			}
		}

		public double AccelerationZ
		{
			get => _accelerationZ;
			set
			{
				_accelerationZ = value;
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccelerationZ)));
			}
		}

		public bool AccelerometerAvailable => _accelerometer != null;

		public AccelerometerSamplePageViewModel()
		{
			_accelerometer = Accelerometer.GetDefault();
			
			if (_accelerometer != null)
			{
				_accelerometer.Shaken += Accelerometer_Shaken;
				ButtonContent = _startObservingContent;
			}
		}

		public void StartObserveReadingChange()
		{
			_accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
			ButtonContent = _stopObservingContent;
			ObservingReadingChange = true;
		}

		public void StopObservingReadingChange()
		{
			_accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
			ButtonContent = _startObservingContent;
			ObservingReadingChange = false;
		}

		private void Accelerometer_Shaken(Accelerometer sender, AccelerometerShakenEventArgs args)
		{
			LastShakeTimestamp = args.Timestamp;
			++ShakeCount;
		}

		private void Accelerometer_ReadingChanged(Accelerometer sender, AccelerometerReadingChangedEventArgs args)
		{
			_ = CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => {
				LastReadTimestamp = args.Reading.Timestamp;
				AccelerationX = args.Reading.AccelerationX;
				AccelerationY = args.Reading.AccelerationY;
				AccelerationZ = args.Reading.AccelerationZ;
			});
		}
	}
}
