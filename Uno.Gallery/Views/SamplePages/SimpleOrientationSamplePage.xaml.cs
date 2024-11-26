using System;
using System.ComponentModel;
using Uno.Gallery.ViewModels;
using Windows.Devices.Sensors;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(
		SampleCategory.NonUIFeatures,
		"Simple Orientation", Description = "This sensor detects the current quadrant orientation of the specified device as well as its face-up or face-down status.", DocumentationLink = "https://learn.microsoft.com/en-us/uwp/api/windows.devices.sensors.simpleorientationsensor")]
	public sealed partial class SimpleOrientationSamplePage : Page
	{
		public SimpleOrientationSamplePage()
		{
			this.InitializeComponent();
		}

		private void ObserveReadingChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if (sender is Button { DataContext: SimpleOrientationSamplePageViewModel viewModel })
			{
				if (viewModel.ObservingReadingChange)
				{
					viewModel.StopObservingReadingChange();
				}
				else
				{
					viewModel.StartObserveReadingChange();
				}
			}
		}
	}

	public class SimpleOrientationSamplePageViewModel : ViewModelBase
	{
		private const string _startObservingContent = "Start observing orientation changes";
		private const string _stopObservingContent = "Stop observing orientation changes";
		private const string _noSensorAvailableContent = "SimpleOrientationSensor is not available on this device/platform";
		private readonly SimpleOrientationSensor? _simpleOrientationSensor;

		public DateTimeOffset? LastReadTimestamp { get => GetProperty<DateTimeOffset?>(); set => SetProperty(value); }
		public string ButtonContent { get => GetProperty<string>(); set => SetProperty(value); }
		public bool ObservingReadingChange { get => GetProperty<bool>(); set => SetProperty(value); }
		public SimpleOrientation Orientation { get => GetProperty<SimpleOrientation>(); set => SetProperty(value); }

		public bool SimpleOrientationAvailable => _simpleOrientationSensor is not null;

		public SimpleOrientationSamplePageViewModel()
		{
			_simpleOrientationSensor = SimpleOrientationSensor.GetDefault();

			if (SimpleOrientationAvailable)
			{
				ButtonContent = _startObservingContent;
			}
			else
			{
				ButtonContent = _noSensorAvailableContent;
			}
		}

		public void StartObserveReadingChange()
		{
			if (_simpleOrientationSensor is null)
			{
				return;
			}

			_simpleOrientationSensor.OrientationChanged += SimpleOrientationSensor_OrientationChanged;
			ButtonContent = _stopObservingContent;
			ObservingReadingChange = true;
		}

		public void StopObservingReadingChange()
		{
			if (_simpleOrientationSensor is not null)
			{
				_simpleOrientationSensor.OrientationChanged -= SimpleOrientationSensor_OrientationChanged;
			}

			ButtonContent = _startObservingContent;
			ObservingReadingChange = false;
		}

		private async void SimpleOrientationSensor_OrientationChanged(SimpleOrientationSensor sender, SimpleOrientationSensorOrientationChangedEventArgs args)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				LastReadTimestamp = args.Timestamp;
				Orientation = args.Orientation;
			});
		}
	}
}
