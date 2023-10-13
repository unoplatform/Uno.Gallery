using System;
using System.Linq;
using Uno.Gallery.ViewModels;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Devices.Sensors;
using Windows.UI.Core;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Threading.Tasks;

namespace Uno.Gallery.Views.Samples
{
	[SamplePage(SampleCategory.NonUIFeatures, "Proximity",
		Description = "Represents a Proximit sensor. This sensor returns distance.",
		DocumentationLink = "https://platform.uno/docs/articles/features/proximity-sensor.html",
		DataType = typeof(ProximitySamplePageViewModel))]
	public sealed partial class ProximitySamplePage : Page
	{
		public ProximitySamplePage ()
		{
			this.InitializeComponent();
			Loaded += ProximitySamplePage_Loaded;
		}

		private async void ProximitySamplePage_Loaded(object sender, RoutedEventArgs e)
		{
			if (!(((Sample)DataContext).Data is ProximitySamplePageViewModel viewModel))
				return;
			await viewModel.OnLoaded();
		}

		private void ObserveReadingChangeButton_Click(object sender, RoutedEventArgs e)
		{
			if (!(((Sample)DataContext).Data is ProximitySamplePageViewModel viewModel))
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

	public class ProximitySamplePageViewModel : ViewModelBase
	{
		private const string _startObservingContent = "Start observing proximity reading changes";
		private const string _stopObservingContent = "Stop observing proximity reading changes";
		private string _notAvailable = "Proximity is not available on this device/platform";

		private ProximitySensor _proximity = null;

		public string ButtonContent
		{
			get => GetProperty<string>();
			set => SetProperty<string>(value);
		}

		public bool IsProximityAvailable
		{
			get => GetProperty<bool>();
			set => SetProperty<bool>(value);
		}

		public bool ObservingReadingChange
		{
			get => GetProperty<bool>();
			set => SetProperty<bool>(value);
		}

		public uint? Distance
		{
			get => GetProperty<uint?>();
			set => SetProperty<uint?>(value);
		}

		public bool IsDetected
		{
			get => GetProperty<bool>();
			set => SetProperty<bool>(value);
		}

		public bool ProximityAvailable => _proximity != null;



		public ProximitySamplePageViewModel()
		{
			IsProximityAvailable = false;
			ButtonContent = _notAvailable;
		}

		public void StartObserveReadingChange()
		{
			_proximity.ReadingChanged += Proximity_ReadingChanged;
			ButtonContent = _stopObservingContent;
			ObservingReadingChange = true;
		}

		public void StopObservingReadingChange()
		{
			_proximity.ReadingChanged -= Proximity_ReadingChanged;
			ButtonContent = _startObservingContent;
			ObservingReadingChange = false;
		}

		private async void Proximity_ReadingChanged(ProximitySensor sender, ProximitySensorReadingChangedEventArgs args)
		{
			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				Distance = args.Reading.DistanceInMillimeters;
				IsDetected = args.Reading.IsDetected;
			});
		}

		internal async Task OnLoaded()
		{
			var selector = ProximitySensor.GetDeviceSelector();
			var devices = await DeviceInformation.FindAllAsync(selector);
			var device = devices.FirstOrDefault();
			if (device is not null)
			{

#if __ANDROID__
				_proximity = ProximitySensor.FromId(device.Id);
#endif
			}

			if (_proximity is not null)
			{
				await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					IsProximityAvailable = true;
					ButtonContent = _startObservingContent;
				});
				return;
			}

			await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				IsProximityAvailable = false;
				ButtonContent = _notAvailable;
			});
		}
		
	}
}
