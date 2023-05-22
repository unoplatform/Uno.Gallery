using System;
using Uno.Gallery.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Devices.Sensors;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Pedometer",
		Description = "Represents a Pedometer sensor. This sensor returns steps taken with the device.",
		DocumentationLink = "https://platform.uno/docs/articles/features/step-counter.html",
		DataType = typeof(PedometerSamplePageViewModel))]
	public sealed partial class PedometerSamplePage : Page
	{
		public PedometerSamplePage()
		{
			this.InitializeComponent();
		}

		private void ObserveReadingChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if (!(((Sample)DataContext).Data is PedometerSamplePageViewModel viewModel))
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

	public class PedometerSamplePageViewModel : ViewModelBase
	{
		private const string _startObservingContent = "Start observing pedometer reading changes";
		private const string _stopObservingContent = "Stop observing pedometer reading changes";
		private string _notAvailable = "Pedometer is not available on this device/platform";

		private Pedometer _pedometer;

		public double Steps { get => GetProperty<double>(); set => SetProperty(value); }
		public TimeSpan Duration { get => GetProperty<TimeSpan>(); set => SetProperty(value); }
		public string ButtonContent { get => GetProperty<string>(); set => SetProperty(value); }
		public DateTimeOffset? LastReadTimestamp { get => GetProperty<DateTimeOffset?>(); set => SetProperty(value); }
		public bool ObservingReadingChange { get => GetProperty<bool>(); set => SetProperty(value); }

		public bool PedometerAvailable { get => GetProperty<bool>(); set => SetProperty(value); }

		public PedometerSamplePageViewModel()
		{
			Init();
		}

		private async void Init()
		{
			_pedometer = await Pedometer.GetDefaultAsync();

			if (_pedometer != null)
			{
				_pedometer.ReportInterval = 10000;

				ButtonContent = _startObservingContent;
				PedometerAvailable = true;
				return;
			}

			ButtonContent = _notAvailable;
		}

		public void StartObserveReadingChange()
		{
			_pedometer.ReadingChanged += Pedometer_ReadingChanged;
			ButtonContent = _stopObservingContent;
			ObservingReadingChange = true;
		}

		public void StopObservingReadingChange()
		{
			_pedometer.ReadingChanged -= Pedometer_ReadingChanged;
			ButtonContent = _startObservingContent;
			ObservingReadingChange = false;
		}

		private async void Pedometer_ReadingChanged(Pedometer sender, PedometerReadingChangedEventArgs args)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				Steps = args.Reading.CumulativeSteps;
				Duration = args.Reading.CumulativeStepsDuration;
				LastReadTimestamp = args.Reading.Timestamp;
			});
		}
	}
}
