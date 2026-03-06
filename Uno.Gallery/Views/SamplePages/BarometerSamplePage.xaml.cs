using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using Uno.Gallery.ViewModels;
using Windows.Devices.Sensors;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Barometer",
		Description = "Represents a Barometer sensor. This sensor returns pressure in hectopascals (hPa).",
		DocumentationLink = "https://platform.uno/docs/articles/features/barometer.html",
		DataType = typeof(BarometerSamplePageViewModel))]
	public sealed partial class BarometerSamplePage : Page
	{
		public BarometerSamplePage()
		{
			this.InitializeComponent();
		}

		private void ObserveReadingChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if (!(((Sample)DataContext).Data is BarometerSamplePageViewModel viewModel))
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
}

[Microsoft.UI.Xaml.Data.Bindable]
public class BarometerSamplePageViewModel : ViewModelBase
{
	private const string _startObservingContent = "Start observing barometer reading changes";
	private const string _stopObservingContent = "Stop observing barometer reading changes";
	private string _notAvailable = "Barometer is not available on this device/platform";

	private readonly Barometer _barometer;

	public double Pressure { get => GetProperty<double>(); set => SetProperty(value); }
	public string ButtonContent { get => GetProperty<string>(); set => SetProperty(value); }
	public DateTimeOffset? LastReadTimestamp { get => GetProperty<DateTimeOffset?>(); set => SetProperty(value); }
	public bool ObservingReadingChange { get => GetProperty<bool>(); set => SetProperty(value); }

	public bool BarometerAvailable => _barometer != null;

	public BarometerSamplePageViewModel()
	{
		_barometer = Barometer.GetDefault();

		if (BarometerAvailable)
		{
			ButtonContent = _startObservingContent;
			return;
		}

		ButtonContent = _notAvailable;
	}

	public void StartObserveReadingChange()
	{
		_barometer.ReadingChanged += Barometer_ReadingChanged;
		ButtonContent = _stopObservingContent;
		ObservingReadingChange = true;
	}

	public void StopObservingReadingChange()
	{
		_barometer.ReadingChanged -= Barometer_ReadingChanged;
		ButtonContent = _startObservingContent;
		ObservingReadingChange = false;
	}

	private void Barometer_ReadingChanged(Barometer sender, BarometerReadingChangedEventArgs args)
	{
		LastReadTimestamp = args.Reading.Timestamp;
		Pressure = args.Reading.StationPressureInHectopascals;
	}
}
