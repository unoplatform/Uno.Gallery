using System;
using System.ComponentModel;
using Uno.Gallery.ViewModels;
using Windows.Devices.Sensors;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Simple Orientation", Description = "This sensor detects the current quadrant orientation of the specified device as well as its face-up or face-down status.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.devices.sensors.simpleorientationsensor")]
	public sealed partial class SimpleOrientationSamplePage : Page
	{
		public SimpleOrientationSamplePage()
		{
			this.InitializeComponent();
		}

		private void ObserveReadingChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if ((sender as Button)?.DataContext is SimpleOrientationSamplePageViewModel viewModel)
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

	public class SimpleOrientationSamplePageViewModel : ViewModelBase
	{
		private const string _startObservingContent = "Start observing orientation changes";
		private const string _stopObservingContent = "Stop observing orientation changes";
		private const string _noSensorAvailable = "SimpleOrientationSensor is not available on this device/platform";

		private readonly SimpleOrientationSensor _simpleOrientationSensor;

		public DateTimeOffset? LastReadTimestamp { get => GetProperty<DateTimeOffset?>(); set => SetProperty(value); }

		public string ButtonContent { get => GetProperty<string>(); set => SetProperty(value); }
		public bool ObservingReadingChange { get => GetProperty<bool>(); set => SetProperty(value); }
		public SimpleOrientation Orientation { get => GetProperty<SimpleOrientation>(); set => SetProperty(value); }

		public bool SimpleOrientationAvailable => _simpleOrientationSensor != null;

		public SimpleOrientationSamplePageViewModel()
		{
			_simpleOrientationSensor = SimpleOrientationSensor.GetDefault();

			if (_simpleOrientationSensor != null)
			{
				ButtonContent = _startObservingContent;
			}
			else
			{
				ButtonContent = _noSensorAvailable;
			}
		}

		public void StartObserveReadingChange()
		{
			_simpleOrientationSensor.OrientationChanged += SimpleOrientationSensor_OrientationChanged;
			ButtonContent = _stopObservingContent;
			ObservingReadingChange = true;
		}

		public void StopObservingReadingChange()
		{
			_simpleOrientationSensor.OrientationChanged -= SimpleOrientationSensor_OrientationChanged;
			ButtonContent = _startObservingContent;
			ObservingReadingChange = false;
		}

		private void SimpleOrientationSensor_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
		{
			LastReadTimestamp = args.Timestamp;
			Orientation = args.Orientation;
		}
	}
}
