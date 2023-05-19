using System;
using Uno.Gallery.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Devices.Sensors;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Gyrometer",
		Description = "Represents a Gyrometer sensor that provides angular velocity values with respect to the x, y, and z axes.",
		DocumentationLink = "https://platform.uno/docs/articles/features/gyrometer.html",
		DataType = typeof(GyrometerSamplePageViewModel))]
	public sealed partial class GyrometerSamplePage : Page
	{
		public GyrometerSamplePage()
		{
			this.InitializeComponent();
		}

		private void ObserveReadingChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if (!(((Sample)DataContext).Data is GyrometerSamplePageViewModel viewModel))
				return;


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

	public class GyrometerSamplePageViewModel : ViewModelBase
	{
		private const string _startObservingContent = "Start observing gyrometer reading changes";
		private const string _stopObservingContent = "Stop observing gyrometer reading changes";
		private string _notAvailable = "Gyrometer is not available on this device/platform";

		private readonly Gyrometer _gyrometer;

		public double AngularVelocityX { get => GetProperty<double>(); set => SetProperty(value); }
		public double AngularVelocityY { get => GetProperty<double>(); set => SetProperty(value); }
		public double AngularVelocityZ { get => GetProperty<double>(); set => SetProperty(value); }
		public string ButtonContent { get => GetProperty<string>(); set => SetProperty(value); }
		public DateTimeOffset? LastReadTimestamp { get => GetProperty<DateTimeOffset?>(); set => SetProperty(value); }
		public bool ObservingReadingChange { get => GetProperty<bool>(); set => SetProperty(value); }

		public bool GyrometerAvailable => _gyrometer != null;

		public uint? ReportInterval
		{
			get => GetProperty<uint?>();
			set
			{
				if (_gyrometer != null && value != null)
				{
					_gyrometer.ReportInterval = (uint)value;
					SetProperty(value);
				}
			}
		}

		public GyrometerSamplePageViewModel()
		{
			_gyrometer = Gyrometer.GetDefault();

			if (GyrometerAvailable)
			{
				ReportInterval = 1000;
				ButtonContent = _startObservingContent;
				return;
			}

			ButtonContent = _notAvailable;
		}

		public void StartObserveReadingChange()
		{
			_gyrometer.ReadingChanged += Gyrometer_ReadingChanged;
			ButtonContent = _stopObservingContent;
			ObservingReadingChange = true;
		}

		public void StopObservingReadingChange()
		{
			_gyrometer.ReadingChanged -= Gyrometer_ReadingChanged;
			ButtonContent = _startObservingContent;
			ObservingReadingChange = false;
		}

		private async void Gyrometer_ReadingChanged(Gyrometer sender, GyrometerReadingChangedEventArgs args)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				AngularVelocityX = args.Reading.AngularVelocityX;
				AngularVelocityY = args.Reading.AngularVelocityY;
				AngularVelocityZ = args.Reading.AngularVelocityZ;
				LastReadTimestamp = args.Reading.Timestamp;
			});
		}
	}
}
